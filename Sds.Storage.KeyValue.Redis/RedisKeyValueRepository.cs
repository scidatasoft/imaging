using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Sds.Storage.KeyValue.Core;
using Sds.Core;

namespace Sds.Storage.KeyValue.Redis
{
	public class RedisKeyValueRepository : IKeyValueRepository
	{
		private class PortionInfo
		{
			public int Order { get; set; }
			public string BlobId { get; set; }
		}

		private const int StreamPortionSize = 15 * 1024 * 1024;

		private readonly IKeyValueStore _rawStore;

		public RedisKeyValueRepository(IKeyValueStore rawStore)
		{
			_rawStore = rawStore;
		}

		public byte[] LoadData(string id)
		{
			return _rawStore.Load(id);
		}

		public void SaveData(string id, string value)
		{
			_rawStore.Save(id, value);
		}

		public void SaveData(string id, byte[] value)
		{
			_rawStore.Save(id, value);
		}

		public void SaveStream(string id, Stream stream)
		{
			var portions = new List<PortionInfo>();

			stream.Position = 0;
			while (stream.Position != stream.Length)
			{
				var portionInfo = new PortionInfo
				{
					BlobId = $"{id}-{Guid.NewGuid().Encode()}",
					Order = portions.Count
				};

				var portionSize = (int)Math.Min(stream.Length - stream.Position, StreamPortionSize);
				var buffer = new byte[portionSize];
				stream.Read(buffer, 0, portionSize);

				SaveData(portionInfo.BlobId, buffer);
				portions.Add(portionInfo);
			}

			var rawPortions = JsonConvert.SerializeObject(portions);
			SaveData(id, rawPortions);
		}

		public void LoadStream(string id, Stream stream)
		{
			var rawPortions = LoadData(id);
			if (rawPortions == null)
			{
				return;
			}

			var portions = JsonConvert.DeserializeObject<List<PortionInfo>>(Encoding.UTF8.GetString(rawPortions));
			foreach (var portionInfo in portions.OrderBy(x => x.Order))
			{
				var portion = LoadData(portionInfo.BlobId);
				stream.Write(portion, 0, portion.Length);
			}
		}

		public void DeleteStream(string id)
		{
			var rawPortions = LoadData(id);
			if (rawPortions == null)
			{
				return;
			}

			var portions = JsonConvert.DeserializeObject<List<PortionInfo>>(Encoding.UTF8.GetString(rawPortions));
			foreach (var portionInfo in portions)
			{
				Delete(portionInfo.BlobId);
			}

			Delete(id);
		}

		public T LoadObject<T>(string id) where T : class
		{
			var data = LoadData(id);
			if (data == null)
			{
				return null;
			}

			return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
		}

		public void SaveObject<T>(string id, T obj) where T : class
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException(nameof(id));
			}

			var value = JsonConvert.SerializeObject(obj);

			SaveData(id, value);
		}

		public void Delete(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException(nameof(id));
			}

			_rawStore.Delete(id);
		}

		public void SetExpiration(string id, TimeSpan expiry)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException(nameof(id));
			}

			_rawStore.SetExpiration(id, expiry);
		}

		public void SetStreamExpiration(string id, TimeSpan expiry)
		{
			var rawPortions = LoadData(id);
			if (rawPortions == null)
			{
				return;
			}

			var portions = JsonConvert.DeserializeObject<List<PortionInfo>>(Encoding.UTF8.GetString(rawPortions));
			foreach (var portionInfo in portions)
			{
				_rawStore.SetExpiration(portionInfo.BlobId, expiry);
			}

			_rawStore.SetExpiration(id, expiry);
		}

	}
}
