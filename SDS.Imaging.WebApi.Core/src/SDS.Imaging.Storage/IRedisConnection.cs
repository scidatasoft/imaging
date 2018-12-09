using StackExchange.Redis;

namespace SDS.Imaging.Storage
{
    public interface IRedisConnection
    {
        IDatabase GetDatabase();
    }
}