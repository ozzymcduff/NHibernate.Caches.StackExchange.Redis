using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Cache;
using StackExchange.Redis;

namespace NHibernate.Caches.Redis
{
    public class RedisCache : ICache
    {
        private class ObjectSerializer
        {
            private readonly BinaryFormatter _bf = new BinaryFormatter();

            public virtual byte[] Serialize(object value)
            {
                if (value == null)
                    return null;
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Seek(0, 0);
                    _bf.Serialize(memoryStream, value);
                    memoryStream.Seek(0, 0);
                    var buffer = new byte[(int)memoryStream.Length];
                    memoryStream.Read(buffer, 0, (int)memoryStream.Length);
                    return buffer;
                }
            }

            public virtual object Deserialize(byte[] bytes)
            {
                if (bytes == null)
                    return null;

                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(bytes, 0, bytes.Length);
                    memoryStream.Seek(0, 0);
                    return _bf.Deserialize(memoryStream);
                }

            }
        }
        internal const string PrefixName = "nhib:";
        private readonly string _region;
        private readonly string _regionPrefix = "";
        private static readonly IInternalLogger Log;
        private readonly TimeSpan _expiryTimeSpan;

        static RedisCache()
        {
            Log = LoggerProvider.LoggerFor((typeof(RedisCache)));
        }

        private readonly ObjectSerializer _serializer;
        private readonly ConnectionMultiplexer _clientManager;
        [ThreadStatic] private static HashAlgorithm _hasher;
        [ThreadStatic] private static MD5 _md5;

        public string RegionName { get; private set; }
        public int Timeout { get { return Timestamper.OneMs * 60000; } }

        private static HashAlgorithm Hasher
        {
            get
            {
                if (_hasher == null)
                {
                    _hasher = HashAlgorithm.Create();
                }
                return _hasher;
            }
        }

        private static MD5 Md5
        {
            get
            {
                if (_md5 == null)
                {
                    _md5 = MD5.Create();
                }
                return _md5;
            }
        }

        /// <summary>
        /// Turn the key obj into a string, preperably using human readable
        /// string, and if the string is too long (>=250) it will be hashed
        /// </summary>
        public string KeyAsString(object key)
        {
            var fullKey = FullKeyAsString(key);
            if (fullKey.Length >= 250) //max key size for memcache
            {
                return ComputeHash(fullKey, Hasher);
            }
            else
            {
                return fullKey;
            }
        }

        /// <summary>
        /// Turn the key object into a human readable string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string FullKeyAsString(object key)
        {
            return String.Concat(PrefixName, _regionPrefix, _region, ":", key.ToString(), "@", key.GetHashCode());
        }

        /// <summary>
        /// Compute the hash of the full key string using the given hash algorithm
        /// </summary>
        /// <param name="fullKeyString">The full key return by call FullKeyAsString</param>
        /// <param name="hashAlgorithm">The hash algorithm used to hash the key</param>
        /// <returns>The hashed key as a string</returns>
        private static string ComputeHash(string fullKeyString, HashAlgorithm hashAlgorithm)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(fullKeyString);
            byte[] computedHash = hashAlgorithm.ComputeHash(bytes);
            return Convert.ToBase64String(computedHash);
        }

        /// <summary>
        /// Compute an alternate key hash; used as a check that the looked-up value is 
        /// in fact what has been put there in the first place.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The alternate key hash (using the MD5 algorithm)</returns>
        private string GetAlternateKeyHash(object key)
        {
            string fullKey = FullKeyAsString(key);
            if (fullKey.Length >= 250)
            {
                return ComputeHash(fullKey, Md5);
            }
            else
            {
                return fullKey;
            }
        }

        public RedisCache(string regionName, ConnectionMultiplexer clientManager)
            : this(regionName, new Dictionary<string, string>(), clientManager)
        {
        }

        public RedisCache(string regionName, IDictionary<string, string> properties, ConnectionMultiplexer clientManager)
        {
            _serializer = new ObjectSerializer();
            if (clientManager == null) throw new ArgumentNullException("clientManager");
            _clientManager = clientManager;
            RegionName = _region = regionName;
            int expiry = 60 * 60;
            _expiryTimeSpan = TimeSpan.FromSeconds(expiry);

            if (properties != null)
            {
                var expirationString = GetExpirationString(properties);
                if (expirationString != null)
                {
                    expiry = Convert.ToInt32(expirationString);
                    _expiryTimeSpan = TimeSpan.FromSeconds(expiry);
                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("using expiration of {0} seconds", expiry);
                    }
                }

                if (properties.ContainsKey("regionPrefix"))
                {
                    _regionPrefix = properties["regionPrefix"];

                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("new regionPrefix :{0}", _regionPrefix);
                    }
                }
                else
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("no regionPrefix value given, using defaults");
                    }
                }
            }
        }

        private static string GetExpirationString(IDictionary<string, string> props)
        {
            string result;
            if (!props.TryGetValue("expiration", out result))
            {
                props.TryGetValue(Cfg.Environment.CacheDefaultExpiration, out result);
            }
            return result;
        }


        public long NextTimestamp()
        {
            return Timestamper.Next();
        }

        public virtual void Put(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "null key not allowed");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value", "null value not allowed");
            }

            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("setting value for item {0}", key);
            }

            var client = _clientManager.GetDatabase();
            
            var cacheKey = KeyAsString(key);
            var returnOk = client.StringSet(cacheKey,
                _serializer.Serialize(new DictionaryEntry(GetAlternateKeyHash(key), value)), 
                    _expiryTimeSpan);
            if (!returnOk)
            {
                if (Log.IsWarnEnabled)
                {
                    Log.WarnFormat("could not save: {0} => {1}", key, value);
                }
            }
        }

        public virtual object Get(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "null key not allowed");
            }

            var client = _clientManager.GetDatabase();
            var transaction = client.CreateTransaction();
            var objectKey = KeyAsString(key);
            var task = transaction.StringGetAsync(objectKey)
                .ContinueWith(x=>ObserveExceptionT(x));
            transaction.KeyExpireAsync(objectKey, _expiryTimeSpan)
                .ContinueWith(ObserveException);
            transaction.Execute();

            if (!task.Result.HasValue)
            {
                return null;
            }

            var de = (DictionaryEntry)_serializer.Deserialize(task.Result);

            //we need to check here that the key that we stored is really the key that we got
            //the reason is that for long keys, we hash the value, and this mean that we may get
            //hash collisions. The chance is very low, but it is better to be safe
            var checkKeyHash = GetAlternateKeyHash(key);
            return checkKeyHash.Equals(de.Key) ? de.Value : null;
        }

        public virtual void Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("removing item {0}", key);
            }

            var client = _clientManager.GetDatabase();
            client.KeyDelete(KeyAsString(key)); 
        }

        public virtual void Clear()
        {
            var client = _clientManager.GetDatabase();
            var server = _clientManager.GetServer("127.0.0.1", 6379);
            var keys = server.Keys(pattern: String.Concat(PrefixName, _regionPrefix, _region, ":*"));
            client.KeyDelete(keys.ToArray());
        }

        public virtual void Destroy()
        {
            Clear();
        }

        public virtual void Lock(object key)
        {
            // do nothing
        }

        public virtual void Unlock(object key)
        {
            // do nothing
        }

        private static void ObserveException(System.Threading.Tasks.Task x)
        {
            if (x.Exception != null)
            {
                Log.Error(x.Exception);
            }
        }
        private static TRes ObserveExceptionT<TRes>(Task<TRes> x)
        {
            if (x.Exception != null)
            {
                Log.Error(x.Exception);
            }
            return x.Result;
        }

    }
}
