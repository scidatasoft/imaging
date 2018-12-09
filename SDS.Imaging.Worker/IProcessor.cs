using Sds.Imaging.Domain.Contracts;

namespace Sds.Imaging.Worker
{
	public interface IProcessor
	{
		void ProcessImage(SourceFileUploadedMessage message);
	}
}