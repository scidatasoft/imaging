using System.Configuration;
using StackExchange.Redis;

namespace Sds.Storage.KeyValue.Redis
{
	internal static class RedisConnection
	{
		private readonly static ConnectionMultiplexer _redis = GetConnectionMultiplexer();

		public static ConnectionMultiplexer GetConnection() { return _redis; }

		public static IDatabase GetDatabase() { return _redis.GetDatabase(); }

		private static ConnectionMultiplexer GetConnectionMultiplexer()
		{
			var config = ConfigurationOptions.Parse(ConfigurationManager.AppSettings["RedisConnection"].ToString());
			config.SyncTimeout = 10000;
			config.ConnectTimeout = 10000;
			config.ResponseTimeout = 10000;

			return ConnectionMultiplexer.Connect(config);
		}
	}
}
