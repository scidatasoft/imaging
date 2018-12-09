namespace SDS.Imaging.Storage
{
    public class RedisStore : IKeyValueStore
    {
        private readonly IRedisConnection _redisConnection;

        public RedisStore(IRedisConnection redisConnection)
        {
            _redisConnection = redisConnection;
        }

        public void Save(string id, string value)
        {
            var db = _redisConnection.GetDatabase();
            db.StringSet(id, value);
        }

        public void Save(string id, byte[] value)
        {
            var db = _redisConnection.GetDatabase();
            db.StringSet(id, value);
        }

        public byte[] Load(string id)
        {
            var db = _redisConnection.GetDatabase();
            return db.StringGet(id);
        }
    }
}
