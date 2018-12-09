using Sds.Core;
using Sds.Imaging.Domain.Contracts;
using Sds.Imaging.Processing;
using Sds.Storage.KeyValue.Core;
using Serilog;
using System;
using System.IO;

namespace Sds.Imaging.Worker
{
	public class Processor : IProcessor
	{
		private readonly IKeyValueRepository _repository;
		private readonly ImagingOptions _options;

		public Processor(
			IKeyValueRepository repository,
			ImagingOptions options)
		{
			_repository = repository;
			_options = options;
		}

		public void ProcessImage(SourceFileUploadedMessage message)
		{
			string extension = Path.GetExtension(message.SourceFileName);
			if (!FileRasterizer.Supports(extension))
			{
				var error = $"Unsupported file type {extension}";

				new ProcessingInfo
				{
					FileDescriptorId = message.ImageDescriptorId,
					ProcessingError = error
				}.SaveTo(_repository);

				Log.Information(error);
				return;
			}

			var format = _options.ImageFormat.ParseImageFormat();
			var imageBlobId = Guid.NewGuid().Encode();
			var imageFileName = $"{imageBlobId}.{format}";
			var rasterizer = new FileRasterizer();

			var sourceTempFilePath = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().Encode());

			try
			{
				using (var sourceStream = File.Create(sourceTempFilePath))
				{
					Log.Information($"Loading source {message.SourceDescriptorId}");
					_repository.LoadStream(message.SourceBlobId, sourceStream);

					Log.Information($"Rasterizing source {message.SourceDescriptorId}");
					sourceStream.Position = 0;
					var image = rasterizer.Rasterize(sourceStream, extension);
					image = image.Scale(message.ImageSize.Width, message.ImageSize.Height);

					Log.Information($"Saving image file {message.ImageDescriptorId} as {format}");

					using (var imageStream = new MemoryStream())
					{
						var imageData = image.Convert(format);
						imageStream.Write(imageData, 0, imageData.Length);

						_repository.SaveStream(imageBlobId, imageStream);

						new FileDescriptor
						{
							DescriptorId = message.ImageDescriptorId,
							FileName = imageFileName,
							BlobId = imageBlobId
						}.SaveTo(_repository);

						_repository.SetExpiration(message.ImageDescriptorId, message.SourceExpiry);
						_repository.SetStreamExpiration(imageBlobId, message.SourceExpiry);

						_repository.Delete(message.ProcessingInfoId);
					}

					Log.Information($"Image {message.ImageDescriptorId} processed");
				}
			}
			catch (Exception ex)
			{
				var error = $"Processing exception for source {message.SourceDescriptorId}: {ex.ToString()}";

				new ProcessingInfo
				{
					FileDescriptorId = message.ImageDescriptorId,
					ProcessingError = error
				}.SaveTo(_repository);

				Log.Information(error);
			}
			finally
			{
				if (File.Exists(sourceTempFilePath))
				{
					File.Delete(sourceTempFilePath);
				}
			}
		}
	}
}
