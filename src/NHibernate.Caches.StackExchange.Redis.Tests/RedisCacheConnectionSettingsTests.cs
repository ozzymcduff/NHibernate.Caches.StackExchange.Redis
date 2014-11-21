using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    [TestFixture]
    public class RedisCacheConnectionSettingsTests
    {
        [Test]
        public void Test() 
        {
            var connstr = new RedisCacheConnection("localhost", 6379) { { "allowAdmin", "true" }, { "abortConnect", "false" } }.Render();
            Assert.AreEqual("localhost:6379,allowAdmin=true,abortConnect=false", connstr);
        }
    }
}
