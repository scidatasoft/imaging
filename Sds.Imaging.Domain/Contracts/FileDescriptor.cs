using Newtonsoft.Json;

namespace Sds.Imaging.Domain.Contracts
{
	public class FileDescriptor
	{
		/// <summary>
		/// Serves as key
		/// </summary>
		[JsonProperty("descriptorId")]
		public string DescriptorId { get; set; }

		[JsonProperty("fileName")]
		public string FileName { get; set; }

		[JsonProperty("blobId")]
		public string BlobId { get; set; }

		[JsonProperty("processingInfoId")]
		public string ProcessingInfoId { get; set; }
	}
}
