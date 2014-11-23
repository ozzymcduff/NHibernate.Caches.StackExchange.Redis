using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    public class RedisCacheConnectionSettingsTests
    {
        [Fact]
        public void Test() 
        {
            var connstr = new RedisCacheConnection("localhost", 6379) { { "allowAdmin", "true" }, { "abortConnect", "false" } }.Render();
            Assert.Equal("localhost:6379,allowAdmin=true,abortConnect=false", connstr);
        }
    }
}
