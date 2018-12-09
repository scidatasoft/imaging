using System;
using System.Drawing;

namespace Sds.Imaging.Domain.Contracts
{
	public class SourceFileUploadedMessage
	{
		public string SourceDescriptorId { get; set; }
		public string SourceBlobId { get; set; }
		public string SourceFileName { get; set; }
		public TimeSpan SourceExpiry { get; set; }
		public string ImageDescriptorId { get; set; }
		public Size ImageSize { get; set; }
		public string ProcessingInfoId { get; set; }
	}
}
