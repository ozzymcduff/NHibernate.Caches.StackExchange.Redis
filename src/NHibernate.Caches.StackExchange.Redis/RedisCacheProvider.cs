using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Cache;
using StackExchange.Redis;
using System.Configuration;

namespace NHibernate.Caches.StackExchange.Redis
{
    public class RedisCacheProvider : ICacheProvider
    {
        private static readonly IInternalLogger Log = LoggerProvider.LoggerFor(typeof(RedisCacheProvider));
        private static ConnectionMultiplexer _clientManagerStatic;
        private static RedisCacheConnection _connectionSettings;
        public static RedisCacheConnection ConnectionSettings
        {
            get { return _connectionSettings; }
            set { _connectionSettings = value; _clientManagerStatic = null; }
        }
        private static readonly Dictionary<string, ICache> caches;

        static RedisCacheProvider()
        {
            caches = new Dictionary<string, ICache>();
        }

        public ICache BuildCache(string regionName, IDictionary<string, string> properties)
        {
            ICache result;
            if (caches.TryGetValue(regionName, out result))
            {
                return result;
            }

            if (_clientManagerStatic == null)
            {
                if (ConnectionSettings == null)
                {
                    ConnectionSettings = RedisCacheConnection.Default();
                }
                _clientManagerStatic = ConnectionMultiplexer.Connect(ConnectionSettings.Render());
            }

            if (Log.IsDebugEnabled)
            {
                var sb = new StringBuilder();
                foreach (var pair in properties)
                {
                    sb.Append(pair.Key);
                    sb.Append(" = '");
                    sb.Append(pair.Value);
                    sb.AppendLine("';");
                }
                Log.Debug(String.Format("building cache with region: {0}, properties: \n{1}", regionName, sb));
            }
            result = new RedisCache(regionName, properties, _clientManagerStatic, ConnectionSettings);
            caches.Add(regionName, result);
            return result;
        }

        public long NextTimestamp()
        {
            return Timestamper.Next();
        }

        public void Start(IDictionary<string, string> properties)
        {
            // No-op.
            Log.Debug("starting cache provider");
        }

        public void Stop()
        {
            // No-op.
            Log.Debug("stopping cache provider");
        }
    }
}
