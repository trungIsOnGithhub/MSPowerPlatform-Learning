using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using Microsoft.WindowsAzure;
using Microsoft.Azure;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Configuration;

namespace PAS.Common
{
    public class RedisCache
    {
        private static RetryPolicy _retryPolicy;

        private static readonly string RedisConenctionString = ConfigurationManager.AppSettings["RedisConnectionString"];

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(RedisConenctionString);
        });

        public static bool IsValid()
        {
            if (string.IsNullOrEmpty(RedisConenctionString))
                return false;
            return true;
        }

        private static IDatabase Cache
        {
            get
            {
                return Connection.GetDatabase();
            }
        }

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public static void AddToCache(string key, RedisValue value)
        {
            AddToCache(key, value, TimeSpan.MaxValue);
        }

        public static void AddToCache(string key, RedisValue value, TimeSpan timeSpan)
        {
            Cache.StringSet(key, value, timeSpan);
        }

        public static bool CacheContain(string key)
        {
            return Cache.KeyExists(key);
        }

        public static RedisValue GetCachedObject(string key, bool isNotEmpty = false)
        {
            // retry strategy with a specified number of retry attempts(3) and fixed time interval between retries(500)
            var retryStrategy = new FixedInterval(3, TimeSpan.FromMilliseconds(500));
            // Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling


            _retryPolicy = new RetryPolicy<RedisCacheTransientErrorDetectionStrategy>(retryStrategy);

            return _retryPolicy.ExecuteAction(() => Cache.StringGet(key));
        }

        public static void ClearItem(string key)
        {
            Cache.KeyDelete(key);
        }
    }
}
