using System;
using StackExchange.Redis;

namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    public class RedisTest : IDisposable
    {
        protected string Host() { return "127.0.0.1"; }
        protected string ValidHost() { return Host() + ":6379,allowAdmin=true,abortConnect=false"; }
        protected string InvalidHost() { return "unknown-host:6666,abortConnect=false"; }

        protected ConnectionMultiplexer ConnectionMultiplexer { get; private set; }
        protected IDatabase Redis { get; private set; }
        
        protected RedisTest()
        {
            ConnectionMultiplexer = ConnectionMultiplexer.Connect(ValidHost());
            Redis = ConnectionMultiplexer.GetDatabase();
            FlushDb();
        }

        protected void FlushDb()
        {
            ConnectionMultiplexer.GetServer(Host(), 6379).FlushAllDatabases();
        }

        public void Dispose()
        {
            ConnectionMultiplexer.Dispose();
        }
    }
}
