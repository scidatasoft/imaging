using Sds.Storage.KeyValue.Core;

namespace Sds.Imaging.Domain.Contracts
{
	public static class ContractExtensions
	{
		public static void SaveTo(this ProcessingInfo obj, IKeyValueRepository repository)
		{
			repository.SaveObject(obj.ProcessingInfoId, obj);
		}
		public static void SaveTo(this FileDescriptor obj, IKeyValueRepository repository)
		{
			repository.SaveObject(obj.DescriptorId, obj);
		}
	}
}
