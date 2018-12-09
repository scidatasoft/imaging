using Newtonsoft.Json;

namespace Sds.Imaging.Domain.Contracts
{
	public class ProcessingInfo
	{
		/// <summary>
		/// Serves as key
		/// </summary>
		[JsonProperty("processingInfoId")]
		public string ProcessingInfoId { get; set; }

		[JsonProperty("processingError")]
		public string ProcessingError { get; set; }

		[JsonProperty("fileDescriptorId")]
		public string FileDescriptorId { get; set; }
	}
}
