using System;

namespace Sds.Storage.KeyValue.Core
{
	public interface IKeyValueStore
	{
		byte[] Load(string id);
		void Save(string id, string value);
		void Save(string id, byte[] value);
		void Delete(string id);
		void SetExpiration(string id, TimeSpan expiry);
	}
}