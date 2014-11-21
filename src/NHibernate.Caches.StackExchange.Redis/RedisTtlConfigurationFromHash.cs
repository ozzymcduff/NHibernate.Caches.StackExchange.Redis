using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.Caches.StackExchange.Redis
{
    public class RedisTtlConfigurationFromHash
    {
        private readonly IDictionary<string, string> properties;
        public RedisTtlConfigurationFromHash(IDictionary<string, string> properties)
        {
            this.properties = properties;
        }

        public TimeSpan GetExpiryTimeSpan()
        {
            int expiry = 60 * 60;
            var expiryTimeSpan = TimeSpan.FromSeconds(expiry);

            if (properties != null)
            {
                var expirationString = GetExpirationString(properties);
                if (expirationString != null)
                {
                    expiry = Convert.ToInt32(expirationString);
                    expiryTimeSpan = TimeSpan.FromSeconds(expiry);
                }
            }
            return expiryTimeSpan;
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
    }
}
