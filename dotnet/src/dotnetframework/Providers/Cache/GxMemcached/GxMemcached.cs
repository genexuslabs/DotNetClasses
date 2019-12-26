using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneXus.Utils;
using System.Net;
using BeIT.MemCached;
using GeneXus.Configuration;
using log4net;
using GeneXus.Services;
using System.Security;

namespace GeneXus.Cache
{
	[SecuritySafeCritical]
	public class Memcached : ICacheService2
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Memcached));
		
		MemcachedClient _cache;
		public Memcached() 
		{
			_cache = InitCache();
		}
		MemcachedClient InitCache()
		{
            GXService providerService = ServiceFactory.GetGXServices().Get(GXServices.CACHE_SERVICE);
            String address = providerService.Properties.Get("CACHE_PROVIDER_ADDRESS");
			if (!String.IsNullOrEmpty(address))
			{
				MemcachedClient.Setup("instance", address.Split(',', ';', ' '));
			}
			else
			{
				MemcachedClient.Setup("instance", new string[] { "127.0.0.1:11211" });
			}
			return MemcachedClient.GetInstance("instance");
		}

		private bool Get<T>(string key, out T value)
		{
			if (default(T) == null)
			{
				value = (T)_cache.Get(key);
				if (value == null) GXLogging.Debug(log, "Get<T>, misses key '" + key + "'");

				return value != null;
			}
			else
			{
				object oValue = _cache.Get(key);
				if (oValue != null)
				{
					value = (T)Convert.ChangeType(oValue, typeof(T));
					return true;
				}
				else
				{
					GXLogging.Debug(log,"Get<T>, misses key '" + key + "'");
					value = default(T);
					return false;
				}
			}
		}
		[SecuritySafeCritical]
		public IDictionary<string, T> GetAll<T>(string cacheid, IEnumerable<string> keys)
		{
			if (keys != null)
			{
				var prefixedKeys = Key(cacheid, keys);
				string[] arrKeys = prefixedKeys.ToArray();
				object[] result = _cache.Get(arrKeys);
				Dictionary<string, T> dictionaryResult = new Dictionary<string, T>();
				int index = 0;
				foreach (string key in prefixedKeys)
				{
					if (result[index] == null)
						dictionaryResult.Add(key, default(T));
					else
						dictionaryResult.Add(key, (T)Convert.ChangeType(result[index], typeof(T)));
					index++;
				}
				return dictionaryResult;
			}
			else
			{
				return null;
			}
		}
		[SecuritySafeCritical]
		public void SetAll<T>(string cacheid, IEnumerable<string> keys, IEnumerable<T> values, int duration = 0)
		{
			if (keys != null && values!=null && keys.Count() == values.Count())
			{
				var prefixedKeys = Key(cacheid, keys);
				IDictionary<string, T> dictionary = new Dictionary<string, T>();
				IEnumerator<T> valuesEnumerator = values.GetEnumerator();
				foreach (string key in prefixedKeys)
				{
					if (valuesEnumerator.MoveNext())
					{
						_cache.Set(key, valuesEnumerator.Current, TimeSpan.FromMinutes(duration));
					}
				}
			}
		}

		private void Set<T>(string key, T value, int duration)
		{
			GXLogging.Debug(log,"Set<T> key:" + key + " value " + value + " valuetype:" + value.GetType());
            if (duration > 0)
			    _cache.Set(key, value, TimeSpan.FromMinutes(duration));
            else
                _cache.Set(key, value);
        }

		private void Set<T>(string key, T value)
		{
			_cache.Set(key, value);
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

		public void Clear(string cacheid, string key)
		{
			ClearKey(Key(cacheid, key));
		}

		public void ClearKey(string key)
		{
			_cache.Delete(key);
		}
		[SecuritySafeCritical]
		public void ClearCache(string cacheid)
		{
			Nullable<long> prefix = new Nullable<long>(KeyPrefix(cacheid).Value + 1);
			_cache.Set(cacheid, prefix);
		}

		public void ClearAllCaches()
		{
			_cache.FlushAll();
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
			return cacheid + prefix + GXUtil.GetHash(key);
		}

	}
}
