﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SDS.Imaging.Storage;
using SDS.Imaging.WebApi.Filters;
using SDS.Imaging.WebApi.Models;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SDS.Imaging.WebApi.Controllers
{
    /// <summary>
    /// Based on https://github.com/aspnet/Entropy/tree/1.0.0/samples/Mvc.FileUpload
    /// </summary>

    public class FilesController : Controller
    {
        private readonly IKeyValueStore _store;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<FilesController> _logger;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public FilesController(
            IKeyValueStore store, IHostingEnvironment hostingEnvironment, ILogger<FilesController> logger)
        {
            _store = store;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        [HttpGet]
        [GenerateAntiforgeryTokenCookieForAjax]
        public IActionResult FormToUpload()
        {
            return View();
        }

        // GET api/images/5
        [HttpGet("{id}")]
        public FileResult Get(string imageId)
        {
            //// TODO: 
            //// 1. Load descripor by image id
            //// 2. Load binary by 
            //FileDescriptor fileDescriptor = JsonConvert.DeserializeObject(_store.Load(imageId));

            //return File(_store.Load(fileDescriptor.BlobId), "application/x-msdownload", fileDescriptor.FileName);

            return null;
        }

        // DELETE api/images/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
        }


        // 1. We disable the form value model binding here to take control of handling potentially large files.
        // 2. Typically antiforgery tokens are sent in request body, but since we do not want to read the request body
        //    early, the tokens are made to be sent via headers. The antiforgery token filter first looks for tokens
        //    in the request header and then falls back to reading the body.
        [HttpPost]
        //[DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Upload()
        public async Task<IActionResult> Upload()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got '{Request.ContentType}'.");
            }

            // Used to accumulate all the form url encoded key value pairs in the request.
            var formAccumulator = new KeyValueAccumulator();
            string targetFilePath = null;

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    var name = HeaderUtilities.RemoveQuotes(contentDisposition.Name) ?? string.Empty;
                    var fileName = HeaderUtilities.RemoveQuotes(contentDisposition.FileName) ?? string.Empty;

                    // Here the uploaded file is being copied to local disk but you can also for example, copy the
                    // stream directly to let's say Azure blob storage
                    targetFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, Guid.NewGuid().ToString());
                    using (var targetStream = System.IO.File.Create(targetFilePath))
                    {
                        await section.Body.CopyToAsync(targetStream);

                        _logger.LogInformation($"Copied the uploaded file '{fileName}' to '{targetFilePath}'.");
                    }
                }
                else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                {
                    // Content-Disposition: form-data; name="key"
                    //
                    // value

                    // Do not limit the key name length here because the mulipart headers length
                    // limit is already in effect.
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                    MediaTypeHeaderValue mediaType;
                    MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
                    var encoding = FilterEncoding(mediaType?.Encoding);
                    using (var streamReader = new StreamReader(
                        section.Body,
                        encoding,
                        detectEncodingFromByteOrderMarks: true,
                        bufferSize: 1024,
                        leaveOpen: true))
                    {
                        // The value length limit is enforced by MultipartBodyLengthLimit
                        var value = await streamReader.ReadToEndAsync();
                        formAccumulator.Append(key, value);

                        if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                        {
                            throw new InvalidDataException(
                                $"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            // Bind form data to a model
            //var user = new User();
            //var formValueProvider = new FormValueProvider(
            //    BindingSource.Form,
            //    new FormCollection(formAccumulator.GetResults()),
            //    CultureInfo.CurrentCulture);

            //var bindingSuccessful = await TryUpdateModelAsync(user, prefix: "", valueProvider: formValueProvider);
            //if (!bindingSuccessful)
            //{
            //    if (!ModelState.IsValid)
            //    {
            //        return BadRequest(ModelState);
            //    }
            //}

            var uploadedData = new UploadedData()
            {
                //Name = user.Name,
                FilePath = targetFilePath
            };

            return Ok(uploadedData);
        }

        private static Encoding FilterEncoding(Encoding encoding)
        {
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed for most cases.
            if (encoding == null || Encoding.UTF7.Equals(encoding))
            {
                return Encoding.UTF8;
            }
            return encoding;
        }
    }
}
