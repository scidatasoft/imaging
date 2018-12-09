using System;
using System.IO;

namespace Sds.Storage.KeyValue.Core
{
	public interface IKeyValueRepository
	{
		byte[] LoadData(string id);
		void SaveData(string id, byte[] value);
		void SaveData(string id, string value);
		void SaveStream(string id, Stream stream);
		void LoadStream(string id, Stream stream);
		void DeleteStream(string id);
		T LoadObject<T>(string id) where T : class;
		void SaveObject<T>(string id, T descriptor) where T : class;
		void Delete(string id);
		void SetExpiration(string id, TimeSpan expiry);
		void SetStreamExpiration(string id, TimeSpan expiry);
	}
}
