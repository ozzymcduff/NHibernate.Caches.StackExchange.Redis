using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Cache;
using StackExchange.Redis;

namespace NHibernate.Caches.Redis
{
    public class RedisCacheProvider : ICacheProvider
    {
        private static readonly IInternalLogger Log = LoggerProvider.LoggerFor(typeof(RedisCacheProvider));
        private static ConnectionMultiplexer _clientManagerStatic;

        static RedisCacheProvider()
        {
        }

        public static ConnectionMultiplexer ConnectionMultiplexer
        {
            set
            {
                if (_clientManagerStatic != null)
                    throw new InvalidOperationException("The client manager can only be configured once.");

                if (value == null) throw new ArgumentNullException();
                _clientManagerStatic = value;
            }
        }

        internal static void InternalSetClientManager(ConnectionMultiplexer clientManager)
        {
            _clientManagerStatic = clientManager;
        }

        public ICache BuildCache(string regionName, IDictionary<string, string> properties)
        {
            if (_clientManagerStatic == null)
            {
                throw new InvalidOperationException(
                    "An 'IRedisClientsManager' must be configured with SetClientManager(). " + 
                    "For example, call 'RedisCacheProvider(new PooledRedisClientManager())' " +
                    "before creating the ISessionFactory.");
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

            return new RedisCache(regionName, properties, _clientManagerStatic);
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
