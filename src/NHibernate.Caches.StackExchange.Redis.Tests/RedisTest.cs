﻿using System;
using StackExchange.Redis;

namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    public class RedisTest : IDisposable
    {
        protected const string ValidHost = "127.0.0.1:6379,allowAdmin=true,abortConnect=false";
        protected const string InvalidHost = "unknown-host:6666,abortConnect=false";

        protected ConnectionMultiplexer ClientManager { get; private set; }
        protected IDatabase Redis { get; private set; }
        
        protected RedisTest()
        {
            ClientManager = ConnectionMultiplexer.Connect(ValidHost);
            Redis = ClientManager.GetDatabase();
            FlushDb();
        }

        protected void FlushDb()
        {
            ClientManager.GetServer("127.0.0.1", 6379).FlushAllDatabases();
        }

        public void Dispose()
        {
            ClientManager.Dispose();
        }
    }
}