using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDS.Imaging.Storage
{
	public class SourceFileUploadedMessage
	{
		public string SourceDescriptorId { get; set; }
		public string SourceBlobId { get; set; }
		public string SourceFileName { get; set; }
		public string ImageDescriptorId { get; set; }
	}
}
