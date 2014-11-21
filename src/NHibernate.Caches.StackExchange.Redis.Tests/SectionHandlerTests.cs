using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    [TestFixture]
    public class SectionHandlerTests
    {
        public XmlNode GetConfigurationSection(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }

        [Test]
        public void TestGetConfigNullSection()
        {
            var handler = new SectionHandler();
            var section = new XmlDocument();
            object result = handler.Create(null, null, section);
            Assert.IsNotNull(result);
            Assert.IsTrue(result is CacheConfig[]);
            var caches = result as CacheConfig[];
            Assert.AreEqual(0, caches.Length);
        }

        [Test]
        public void TestGetConfigFromFile()
        {
            const string xmlSimple = "<rediscache><cache region=\"foo\" expiration=\"500\" priority=\"4\" /></rediscache>";

            var handler = new SectionHandler();
            XmlNode section = GetConfigurationSection(xmlSimple);
            object result = handler.Create(null, null, section);
            Assert.IsNotNull(result);
            Assert.IsTrue(result is CacheConfig[]);
            var caches = result as CacheConfig[];
            Assert.AreEqual(1, caches.Length);
            Assert.That(caches[0].Properties, Is.EquivalentTo(new Dictionary<string, string>() { { "expiration", "500" }, { "priority", "4" } }));
        }

        [Test]
        public void TestGetConfigAppConfig()
        {
            var handler = new SectionHandler();
            var section = ConfigurationManager.GetSection("rediscache");
            Assert.IsNotNull(section);
            Assert.IsTrue(section is CacheConfig[]);
            var caches = section as CacheConfig[];
            Assert.AreEqual(1, caches.Length);
            Assert.AreEqual("foo_bar", caches[0].Region);
            Assert.That(caches[0].Properties, Is.EquivalentTo(new Dictionary<string, string>() { { "expiration", "999" }, { "priority", "4" } }));
        }
    }
}
