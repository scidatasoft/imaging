using MassTransit;
using MimeTypes;
using Sds.Core;
using Sds.Imaging.Domain.Contracts;
using Sds.Imaging.WebApi.Attributes;
using Sds.Imaging.WebApi.Models.Files;
using Sds.Storage.KeyValue.Core;
using Serilog;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Sds.Imaging.WebApi.Controllers
{
	[EnableCors("*", "*", "*")]
	public class FilesController : ApiController
	{
		private const string KEY_IMAGE_WIDTH = "ImageWidth";
		private const string KEY_IMAGE_HEIGHT = "ImageHeight";

		private readonly IKeyValueRepository _repository;
		private readonly IBusControl _bus;
		private readonly CommonSettings _commonSettings;

		public FilesController(
			IKeyValueRepository repository,
			IBusControl bus,
			CommonSettings commonSettings)
		{
			_repository = repository;
			_bus = bus;
			_commonSettings = commonSettings;
		}

		[HttpGet]
		[Route("api/version")]
		public string Version()
		{
			Log.Information("Getting version.");
			return ConfigurationManager.AppSettings["Version"].ToString();
		}

		[HttpGet]
		[Route("api/files/{id}")]
		[CacheControl]
		public HttpResponseMessage Get(string id)
		{
			Log.Information($"Getting file {id}");
			var descriptor = _repository.LoadObject<FileDescriptor>(id);
			if (descriptor == null)
			{
				Log.Information($"File's {id} descriptor not found.");
				return Request.CreateResponse(HttpStatusCode.NotFound, "File with same id not found");
			}

			if (!string.IsNullOrEmpty(descriptor.ProcessingInfoId))
			{
				var processingInfo = _repository.LoadObject<ProcessingInfo>(descriptor.ProcessingInfoId);
				if (processingInfo != null)
				{
					if (string.IsNullOrEmpty(processingInfo.ProcessingError))
					{
						Log.Information($"File {id} is being processed.");
						return Request.CreateResponse(HttpStatusCode.Accepted, $"Image {id} is being processed");
					}
					else
					{
						// The file processing resulted in an error
						Log.Information($"File {id} processed with error: {processingInfo.ProcessingError}.");
						return Request.CreateResponse(
							HttpStatusCode.NoContent, $"Image {id} is not available. {processingInfo.ProcessingError}");
					}
				}
				else
				{
					// We have processing info ID but couldn't find the ProcessingInfo object
					return Request.CreateResponse(
						HttpStatusCode.Conflict, $"Processing data incosistency. Please delete file {id} and retry.");
				}
			}

			if (string.IsNullOrEmpty(descriptor.BlobId))
			{
				// We have failed to produce a blob even though there is no processing error
				Log.Information($"File {id} is being processed.");
				return Request.CreateResponse(
					HttpStatusCode.Conflict, $"Processing data incosistency. Please delete file {id} and retry.");
			}

			// The file processing was successfuly and we have the blob
			Log.Information($"File {id} found. Sending...");
			return HttpResponseWithStream(descriptor.DescriptorId, descriptor.BlobId, descriptor.FileName);
		}

		[HttpPost]
		[Route("api/files")]
		public async Task<IHttpActionResult> Post()
		{
			if (!Request.Content.IsMimeMultipartContent())
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

			var provider = new MultipartFileStreamProvider(HttpRuntime.AppDomainAppPath);
			await Request.Content.ReadAsMultipartAsync(provider);

			var filesInfo = provider.FileData.Select(file => SaveFile(file));
			return Content(HttpStatusCode.Created, filesInfo);
		}

		[HttpDelete]
		[Route("api/files/{id}")]
		public IHttpActionResult Delete(string id)
		{
			Log.Information($"Deleting file {id}.");
			var descriptor = _repository.LoadObject<FileDescriptor>(id);
			if (descriptor == null)
			{
				Log.Information($"File {id} not found.");
				return Ok(); // DELETE is indempotent
			}

			_repository.Delete(descriptor.ProcessingInfoId);
			_repository.DeleteStream(descriptor.BlobId);
			_repository.Delete(id);

			Log.Information($"File {id} deleted.");
			return Ok();
		}

		private HttpResponseMessage HttpResponseWithStream(string fileId, string blobId, string fileName)
		{
			var mediaType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName));

			var response = Request.CreateResponse(HttpStatusCode.OK);
			response.Content = new PushStreamContent((stream, httpContent, transportContext) =>
			{
				try
				{
					Log.Information($"Sending file {fileId} finised success.");
					_repository.LoadStream(blobId, stream);
				}
				catch (HttpException ex)
				{
					Log.Warning(ex, $"Sending file {fileId} finised with error.");

					// TODO: Magic number use - need to create a constant for this error
					if (ex.ErrorCode == -2147023667) // The remote host closed the connection. 
					{
						return;
					}
				}
				finally
				{
					// Close output stream as we are done
					stream.Close();
				}
			});
			response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
			response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = fileName
			};

			return response;
		}

		private UploadedFileInfo SaveFile(MultipartFileData file)
		{
			var fileName = file.Headers.ContentDisposition.FileName ?? file.Headers.ContentDisposition.Name;
			fileName = fileName?.Trim('"');
			var fileInfo = new UploadedFileInfo { FileName = fileName };

			var sourceDescriptorId = Guid.NewGuid().Encode();
			var imageDescriptorId = Guid.NewGuid().Encode();
			var processingInfoId = Guid.NewGuid().Encode();

			try
			{
				Log.Information($"Uploading file {sourceDescriptorId}");
				var sourceBlobId = UploadFileData(file.LocalFileName);
				var fileExpiry = GetFileExpiry(file);

				// save source descriptor
				Log.Information($"Saving source desctiptor {sourceDescriptorId}");
				new FileDescriptor
				{
					DescriptorId = sourceDescriptorId,
					FileName = fileName,
					BlobId = sourceBlobId,
				}.SaveTo(_repository);
				_repository.SetExpiration(sourceDescriptorId, fileExpiry);
				_repository.SetStreamExpiration(sourceBlobId, fileExpiry);

				// save image descriptor
				Log.Information($"Saving image desctiptor {imageDescriptorId}");
				new FileDescriptor
				{
					DescriptorId = imageDescriptorId,
					ProcessingInfoId = processingInfoId,
				}.SaveTo(_repository);
				_repository.SetExpiration(imageDescriptorId, fileExpiry);

				// save processing info
				Log.Information($"Saving processing info {processingInfoId}");
				new ProcessingInfo()
				{
					ProcessingInfoId = processingInfoId,
					FileDescriptorId = imageDescriptorId,
				}.SaveTo(_repository);
				_repository.SetExpiration(processingInfoId, fileExpiry);

				// publish message for processing
				Log.Information($"Publishing processing message for source {sourceDescriptorId}");
				_bus.Publish(new SourceFileUploadedMessage
				{
					SourceDescriptorId = sourceDescriptorId,
					SourceBlobId = sourceBlobId,
					SourceFileName = fileName,
					ImageDescriptorId = imageDescriptorId,
					ProcessingInfoId = processingInfoId,
					SourceExpiry = fileExpiry,
					ImageSize = GetImageSize(file)
				});

				fileInfo.SourceId = sourceDescriptorId;
				fileInfo.ImageId = imageDescriptorId;
			}
			catch (Exception ex)
			{
				Log.Warning(ex, $"Saving file {sourceDescriptorId} with error.");
				fileInfo.Error = ex.Message;
			}

			File.Delete(file.LocalFileName);
			return fileInfo;
		}

		private string UploadFileData(string fileName)
		{
			var blobId = Guid.NewGuid().Encode();

			using (var filestream = File.OpenRead(fileName))
			{
				_repository.SaveStream(blobId, filestream);
			}

			return blobId;
		}

		private TimeSpan GetFileExpiry(MultipartFileData file)
		{
			if (!file.Headers.Expires.HasValue)
			{
				return _commonSettings.FileExpiry;
			}

			return file.Headers.Expires.Value - DateTimeOffset.Now;
		}

		private Size GetImageSize(MultipartFileData file)
		{
			if (!file.Headers.Contains(KEY_IMAGE_WIDTH) || !file.Headers.Contains(KEY_IMAGE_HEIGHT))
			{
				return _commonSettings.ImageSize;
			}

			int width = 0;
			int height = 0;
			if (int.TryParse(file.Headers.GetValues(KEY_IMAGE_WIDTH).FirstOrDefault(), out width) &&
				int.TryParse(file.Headers.GetValues(KEY_IMAGE_HEIGHT).FirstOrDefault(), out height))
			{
				return new Size(width, height);
			}

			return _commonSettings.ImageSize;
		}
	}
}