using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Cache;
using StackExchange.Redis;

namespace NHibernate.Caches.StackExchange.Redis
{
    public class RedisCacheProvider : ICacheProvider
    {
        private static readonly IInternalLogger Log = LoggerProvider.LoggerFor(typeof(RedisCacheProvider));
        private static ConnectionMultiplexer _clientManagerStatic;
        private static RedisCacheConnectionSettings _connectionSettings;
        public static RedisCacheConnectionSettings ConnectionSettings { get { return _connectionSettings; } set { _connectionSettings = value; _clientManagerStatic = null; } }

        public ICache BuildCache(string regionName, IDictionary<string, string> properties)
        {
            if (_clientManagerStatic == null)
            {
                if (ConnectionSettings == null)
                {
                    ConnectionSettings = RedisCacheConnectionSettings.Default();
                }
                _clientManagerStatic = ConnectionMultiplexer.Connect(ConnectionSettings.Render());
            }

            if (Log.IsDebugEnabled)
            {
                var sb = new StringBuilder();
                foreach (var pair in properties)
                {
                    sb.Append("name=");
                    sb.Append(pair.Key);
                    sb.Append("&value=");
                    sb.Append(pair.Value);
                    sb.Append(";");
                }
                Log.Debug("building cache with region: " + regionName + ", properties: " + sb);
            }

            return new RedisCache(regionName, properties, _clientManagerStatic, ConnectionSettings);
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
