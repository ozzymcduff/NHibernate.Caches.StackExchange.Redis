using Xunit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    public class SectionHandlerTests
    {
        public XmlNode GetConfigurationSection(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }

		[Fact]
        public void TestGetConfigNullSection()
        {
            var handler = new SectionHandler();
            var section = new XmlDocument();
            object result = handler.Create(null, null, section);
            Assert.NotNull(result);
            Assert.True(result is CacheConfig[]);
            var caches = result as CacheConfig[];
            Assert.Equal(0, caches.Length);
        }

		[Fact]
        public void TestGetConfigFromFile()
        {
            const string xmlSimple = "<rediscache><cache region=\"foo\" expiration=\"500\" priority=\"4\" /></rediscache>";

            var handler = new SectionHandler();
            XmlNode section = GetConfigurationSection(xmlSimple);
            object result = handler.Create(null, null, section);
            Assert.NotNull(result);
            Assert.True(result is CacheConfig[]);
            var caches = result as CacheConfig[];
            Assert.Equal(1, caches.Length);
			Assert.Equal(new Dictionary<string, string>() { { "expiration", "500" }, { "priority", "4" } },
				caches[0].Properties);
        }

		[Fact]
        public void TestGetConfigAppConfig()
        {
            var section = ConfigurationManager.GetSection("rediscache");
            Assert.NotNull(section);
            Assert.True(section is CacheConfig[]);
            var caches = section as CacheConfig[];
            Assert.Equal(1, caches.Length);
            Assert.Equal("foo_bar", caches[0].Region);
			Assert.Equal(new Dictionary<string, string>() { { "expiration", "999" }, { "priority", "4" } },
				caches[0].Properties);
        }
    }
}
