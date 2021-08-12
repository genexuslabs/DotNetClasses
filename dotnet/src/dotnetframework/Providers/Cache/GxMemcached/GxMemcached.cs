using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using GeneXus.Services;
using GeneXus.Utils;

namespace GeneXus.Cache
{
	[SecuritySafeCritical]
	public class Memcached : ICacheService2
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Memcached));

		MemcachedClient _cache;
		const int DEFAULT_MEMCACHED_PORT = 11211;
		public Memcached() 
		{
			_cache = InitCache();
		}
		MemcachedClient InitCache()
		{
			GXServices services = ServiceFactory.GetGXServices();
			String address = string.Empty;
			if (services != null)
			{
				GXService providerService = ServiceFactory.GetGXServices().Get(GXServices.CACHE_SERVICE);
				address = providerService.Properties.Get("CACHE_PROVIDER_ADDRESS");
			}

#if NETCORE
			var loggerFactory = new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory();
			MemcachedClientConfiguration config = new MemcachedClientConfiguration(loggerFactory, new MemcachedClientOptions());
#else
			MemcachedClientConfiguration config = new MemcachedClientConfiguration();
#endif
			if (!String.IsNullOrEmpty(address))
			{
				foreach (string host in address.Split(',', ';', ' ')) {
					if (!host.Contains(':'))
					{
						config.AddServer(host, DEFAULT_MEMCACHED_PORT);
					}
					else
					{
						config.AddServer(host);
					}
				}
				config.Protocol = MemcachedProtocol.Binary;
			}
			else
			{
				config.AddServer("127.0.0.1", 11211);
			}
#if NETCORE
			return new MemcachedClient(loggerFactory, config);
#else
			return new MemcachedClient(config);
#endif
		}
		[SecurityCritical]
		private bool Get<T>(string key, out T value)
		{
			if (default(T) == null)
			{
				value = Deserialize<T>(_cache.Get(key) as string);
				if (value == null) GXLogging.Debug(log, "Get<T>, misses key '" + key + "'");

				return value != null;
			}
			else
			{
				object oValue = Deserialize<T>(_cache.Get(key) as string);
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

		public IDictionary<string, T> GetAll<T>(string cacheid, IEnumerable<string> keys)
		{
			if (keys != null)
			{
				var prefixedKeys = Key(cacheid, keys);
#if NETCORE
				IDictionary<string, string> result= _cache.Get<string>(prefixedKeys);
#else
				IDictionary<string, object> result= _cache.Get(prefixedKeys);
#endif
				Dictionary<string, T> dictionaryResult = new Dictionary<string, T>();
				int index = 0;
				foreach (string key in prefixedKeys)
				{
					if (!result.ContainsKey(key))
						dictionaryResult.Add(key, default(T));
					else
						dictionaryResult.Add(key, Deserialize<T>(result[key] as string));
					index++;
				}
				return dictionaryResult;
			}
			else
			{
				return null;
			}

		}


		public void SetAll<T>(string cacheid, IEnumerable<string> keys, IEnumerable<T> values, int duration = 0)
		{
			if (keys != null && values!=null && keys.Count() == values.Count())
			{
				var prefixedKeys = Key(cacheid, keys);
				IEnumerator<T> valuesEnumerator = values.GetEnumerator();
				foreach (string key in prefixedKeys)
				{
					if (valuesEnumerator.MoveNext())
					{
						_cache.Store(StoreMode.Set, key, Serialize(valuesEnumerator.Current), TimeSpan.FromMinutes(duration));
					}
				}
			}
		}
	
		private void Set<T>(string key, T value, int duration)
		{
			GXLogging.Debug(log,"Set<T> key:" + key + " value " + value + " valuetype:" + value.GetType());
            if (duration > 0)
				_cache.Store(StoreMode.Set, key, Serialize(value), TimeSpan.FromMinutes(duration));
            else
				_cache.Store(StoreMode.Set, key, Serialize(value));
		}

		private void Set<T>(string key, T value)
		{
			_cache.Store(StoreMode.Set, key, Serialize(value));
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
		[SecuritySafeCritical]
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
			_cache.Remove(key);
		}
		public void ClearCache(string cacheid)
		{
			Nullable<long> prefix = new Nullable<long>(KeyPrefix(cacheid).Value + 1);
			_cache.Store(StoreMode.Set, cacheid, Serialize(prefix));
		}
		public void ClearAllCaches()
		{
			_cache.FlushAll();
		}
		[SecurityCritical]
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
		[SecurityCritical]
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
		static string Serialize(object o)
		{
			if (o == null)
			{
				return null;
			}
			return JsonSerializer.Serialize(o);
		}
		[SecurityCritical]
		static T Deserialize<T>(string value)
		{
			if (value == null)
			{
				return default(T);
			}
			JsonSerializerOptions opts = new JsonSerializerOptions();
			opts.Converters.Add(new ObjectToInferredTypesConverter());
			return JsonSerializer.Deserialize<T>(value, opts);
		}
	}
	[SecurityCritical]
	public class ObjectToInferredTypesConverter : JsonConverter<object>
	{
		[SecurityCritical]
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(object) == typeToConvert;
		}
		[SecurityCritical]
		public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.True:
					return true;
				case JsonTokenType.False:
					return false;
				case JsonTokenType.Number:
					if (reader.TryGetInt64(out long l))
						return l;
					else return reader.GetDouble();
				case JsonTokenType.String:
					if (reader.TryGetDateTime(out DateTime datetime))
						return datetime;
					else return reader.GetString();
				default:
					using (JsonDocument document = JsonDocument.ParseValue(ref reader))
					{
						return document.RootElement.Clone().ToString();
					}
			}
		}
		[SecurityCritical]
		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}

}
