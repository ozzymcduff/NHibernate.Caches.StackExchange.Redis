using System;
using StackExchange.Redis;

namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    public class RedisTest : IDisposable
    {
        protected RedisCacheConnectionSettings ConnectionSettings
        {
            get
            {
                return new RedisCacheConnectionSettings("127.0.0.1", 6379) { { "allowAdmin", "true" }, { "abortConnect", "false" } };
            }
        }
        private ConnectionMultiplexer connectionMultiplexer;
        protected IDatabase Redis { get; private set; }

        protected RedisTest()
        {
            RedisCacheProvider.ConnectionSettings = ConnectionSettings;
            connectionMultiplexer = ConnectionMultiplexer.Connect(ConnectionSettings.Render());
            Redis = connectionMultiplexer.GetDatabase();
            FlushDb();
        }

        public RedisCache GetRedisCacheWithRegion(string region) 
        {
            return new RedisCache("region", connectionMultiplexer, ConnectionSettings);
        }

        protected void FlushDb()
        {
            connectionMultiplexer.GetServer(ConnectionSettings.Host, ConnectionSettings.Port).FlushAllDatabases();
        }

        public void Dispose()
        {
            connectionMultiplexer.Dispose();
        }
    }
}
