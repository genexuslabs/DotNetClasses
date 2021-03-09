using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneXus.Configuration;
using GeneXus.Utils;
using log4net;
using ServiceStack.Caching;
using ServiceStack.Redis;
using GeneXus.Services;
using ServiceStack.Text;

namespace GeneXus.Cache
{
    public sealed class Redis : ICacheService2
	{
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Redis));
        RedisClient _cache;
		private const int REDIS_DEFAULT_PORT = 6379;

		public Redis()
        {
            GXService providerService = ServiceFactory.GetGXServices().Get(GXServices.CACHE_SERVICE);
            String address, password;
            address = providerService.Properties.Get("CACHE_PROVIDER_ADDRESS");
            password = providerService.Properties.Get("CACHE_PROVIDER_PASSWORD");

            if (!String.IsNullOrEmpty(address))
            {
                if (!String.IsNullOrEmpty(password))
                {
					if (!address.Contains(':'))
					{
						address = $"{address}:{REDIS_DEFAULT_PORT}";
					}
					address = String.Format("redis://clientid:{0}@{1}", password.Trim(), address.Trim());
					_cache = new RedisClient(new Uri(address));
				}
				else
				{
					_cache = new RedisClient(address);
				}
			}
            else
                _cache = new RedisClient("localhost", REDIS_DEFAULT_PORT);
			JsConfig.DateHandler = DateHandler.ISO8601;
		}

        public void Clear(string cacheid, string key)
        {
            ClearKey(Key(cacheid, key));
        }

        public void ClearKey(string key)
        {
            _cache.Remove(key);
        }

        public void ClearCache(string cacheid)
        {
            _cache.Increment(cacheid, 1);
        }

        public void ClearAllCaches()
        {
            _cache.FlushAll();
        }

        private bool Get<T>(string key, out T value)
        {
            if (default(T) == null)
            {
                value = _cache.Get<T>(key);
                if (value == null) GXLogging.Debug(log, "Get<T>, misses key '" + key + "'");
                return value != null;
            }
            else {
                if (_cache.ContainsKey(key))
                {
                    value = _cache.Get<T>(key);
                    return true;
                }
                else
                {
                    GXLogging.Debug(log, "Get<T>, misses key '" + key + "'");
                    value = default(T);
                    return false;
                }
            }
        }

		public IDictionary<string, T> GetAll<T>(string cacheid, IEnumerable<string> keys)
		{
			if (keys != null)
			{
				var prefixedKeys = Key(cacheid, keys);
				return _cache.GetAll<T>(prefixedKeys);
			}
			else
			{
				return null;
			}
		}

		public void SetAll<T>(string cacheid, IEnumerable<string> keys, IEnumerable<T> values, int duration=0)
		{
			if (keys != null && values!=null && keys.Count() == values.Count())
			{
				var prefixedKeys = Key(cacheid, keys);
				IDictionary<string, T> dictionary = new Dictionary<string, T>();
				IEnumerator<T> valuesEnumerator = values.GetEnumerator();
				foreach (string key in prefixedKeys)
				{
					if (valuesEnumerator.MoveNext())
						dictionary.Add(key, valuesEnumerator.Current);
				}
				_cache.SetAll<T>(dictionary);
			}
		}

		private void Set<T>(string key, T value, int duration)
        {
            GXLogging.Debug(log,"Set<T> key:" + key + " value " + value + " valuetype:" + value.GetType());
            if (duration > 0)
                _cache.Set<T>(key, value, TimeSpan.FromMinutes(duration));
            else
                _cache.Set<T>(key, value);
        }

        private void Set<T>(string key, T value)
        {
            _cache.Set<T>(key, value);
        }

        public bool Get<T>(string cacheid, string key, out T value)
        {
            GXLogging.Debug(log,"Get<T> cacheid:" + cacheid + " key:" + key);
            return Get<T>(Key(cacheid, key), out value);
        }

        public void Set<T>(string cacheid, string key, T value)
        {
            Set<T>(Key(cacheid, key), value);
        }

        public void Set<T>(string cacheid, string key, T value, int durationMinutes)
        {
            Set<T>(Key(cacheid, key), value, durationMinutes);
        }

        private string Key(string cacheid, string key)
        {
			return FormatKey(cacheid, key, KeyPrefix(cacheid));
        }
		private IEnumerable<string> Key(string cacheid, IEnumerable<string> key)
		{
			var prefix = KeyPrefix(cacheid);
			return key.Select(k => FormatKey(cacheid, k, prefix));
		}
		private string FormatKey(string cacheid, string key, Nullable<long> prefix)
		{
			return String.Format("{0}_{1}_{2}", cacheid, prefix, GXUtil.GetHash(key));
		}
		private Nullable<long> KeyPrefix(string cacheid)
		{
			Nullable<long> prefix;
			if (!Get<Nullable<long>>(cacheid, out prefix))
			{
				prefix = DateTime.Now.Ticks;
				Set<Nullable<long>>(cacheid, prefix);
			}
			return prefix;
		}

    }
}