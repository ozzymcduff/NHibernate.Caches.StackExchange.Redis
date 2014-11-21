using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.Caches.StackExchange.Redis
{
    public class RedisCacheConnectionSettings : IEnumerable<KeyValuePair<string, string>>
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public IDictionary<string, string> Properties { get; private set; }
        public RedisCacheConnectionSettings(string host, int port)
        {
            Host = host;
            Port = port;
            Properties = new Dictionary<string, string>();
        }
        public void Add(string key, string value)
        {
            Properties.Add(key, value);
        }
        internal static RedisCacheConnectionSettings Default()
        {
            return new RedisCacheConnectionSettings("127.0.0.1", 6379) { { "allowAdmin", "true" }, { "abortConnect", "false" } };
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return new Dictionary<string, string>(Properties) { { "host", Host }, { "port", Port.ToString() } }.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Render()
        {
            //"127.0.0.1:6379,allowAdmin=true,abortConnect=false"
            var list = new List<string>(){{string.Format("{0}:{1}", Host, Port)}};
            list.AddRange(Properties.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value)));
            return string.Join(",", list.ToArray());
        }
    }
}
