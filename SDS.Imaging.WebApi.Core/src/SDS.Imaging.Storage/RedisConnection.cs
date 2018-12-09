using StackExchange.Redis;

namespace SDS.Imaging.Storage
{
    public class RedisConnection : IRedisConnection
    {
        private readonly ConnectionMultiplexer _redis;

        public ConnectionMultiplexer GetConnection() { return _redis; }

        public IDatabase GetDatabase() { return _redis.GetDatabase(); }

        public RedisConnection(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
        }
    }
}
