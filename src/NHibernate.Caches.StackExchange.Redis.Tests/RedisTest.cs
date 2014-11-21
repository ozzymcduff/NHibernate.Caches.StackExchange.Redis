using System;
using StackExchange.Redis;
using System.Security;
namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    public class RedisTest : IDisposable
    {
        private bool TryGetRedisAddr(out string addr)
        {
            try
            {
                addr = Environment.GetEnvironmentVariable("REDIS_ADDR");
                return true;
            }
            catch (SecurityException)
            {
                addr = null;
                return false;
            }
        }
        private bool TryGetRedisPort(out int port)
        {
            try
            {
                port = Int32.Parse(Environment.GetEnvironmentVariable("REDIS_PORT"));
                return true;
            }
            catch (SecurityException)
            {
                port = -1;
                return false;
            }
        }
        protected RedisCacheConnection ConnectionSettings
        {
            get
            {
                string addr = null;
                int port = 0;
                return new RedisCacheConnection(
                    TryGetRedisAddr(out addr) ? addr : "127.0.0.1",
                    TryGetRedisPort(out port) ? port : 6379
                    ) 
                {
                    { "allowAdmin", "true" }, 
                    { "abortConnect", "false" } 
                };
            }
        }
        private ConnectionMultiplexer _connectionMultiplexer;
        private ConnectionMultiplexer connectionMultiplexer
        {
            get
            {
                if (_connectionMultiplexer == null)
                {
                    _connectionMultiplexer = ConnectionMultiplexer.Connect(ConnectionSettings.Render());
                }
                return _connectionMultiplexer;
            }
        }
        protected IDatabase Redis { get { return connectionMultiplexer.GetDatabase(); } }

        protected RedisTest()
        {
            RedisCacheProvider.ConnectionSettings = ConnectionSettings;
            FlushDb();
        }

        public RedisCache GetRedisCacheWithRegion(string region)
        {
            return new RedisCache(region, connectionMultiplexer, ConnectionSettings);
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
