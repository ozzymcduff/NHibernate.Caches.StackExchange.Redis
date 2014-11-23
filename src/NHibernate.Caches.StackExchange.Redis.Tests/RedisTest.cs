using System;
using StackExchange.Redis;
using System.Security;
using NHibernate.Caches.StackExchange.Redis;

namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    public class RedisTest : IDisposable
    {
        private bool TryGetEnvironmentVariable(string name, out string value)
        {
            try
            {
                value = Environment.GetEnvironmentVariable(name);
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                    return false;
                }
                return true;
            }
            catch (SecurityException)
            {
                value = null;
                return false;
            }
        }

        private bool TryGetRedisAddr(out string addr)
        {
            return TryGetEnvironmentVariable("REDIS_ADDR", out addr);
        }

        private bool TryGetRedisPort(out int port)
        {
            string value;
            if (TryGetEnvironmentVariable("REDIS_PORT", out value))
            {
                port = Int32.Parse(value);
                return true;
            }
            port = -1;
            return false;
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
					{ "abortConnect", "false" },
					{ "connectTimeout", "5000" },
					{ "syncTimeout", "5000" }
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
