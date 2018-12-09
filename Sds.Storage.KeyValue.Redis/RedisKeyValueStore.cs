using Sds.Storage.KeyValue.Core;
using System;

namespace Sds.Storage.KeyValue.Redis
{
	public class RedisKeyValueStore : IKeyValueStore
	{
		public byte[] Load(string id)
		{
			var db = RedisConnection.GetDatabase();
			return db.StringGet(id);
		}

		public void Save(string id, byte[] value)
		{
			var db = RedisConnection.GetDatabase();
			db.StringSet(id, value);
		}

		public void Save(string id, string value)
		{
			var db = RedisConnection.GetDatabase();
			db.StringSet(id, value);
		}

		public void Delete(string id)
		{
			var db = RedisConnection.GetDatabase();
			db.KeyDelete(id);
		}

		public void SetExpiration(string id, TimeSpan expiry)
		{
			var db = RedisConnection.GetDatabase();
			db.KeyExpire(id, expiry);
		}
	}
}
