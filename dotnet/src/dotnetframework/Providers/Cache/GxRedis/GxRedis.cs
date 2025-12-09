using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
#if NETCORE
using GeneXus.Application;
using GxClasses.Helpers;
using Microsoft.Extensions.Caching.Memory;
#endif
using GeneXus.Encryption;
using GeneXus.Services;
using GeneXus.Utils;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;
using GeneXus.Configuration;

namespace GeneXus.Cache
{
	public sealed class Redis : ICacheService2
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<Redis>();

		ConnectionMultiplexer _redisConnection;
		IDatabase _redisDatabase;
#if NETCORE
		MemoryCache _localCache;
		private const double DEFAULT_LOCAL_CACHE_FACTOR = 0.2;
		private TimeSpan MAX_LOCAL_CACHE_TTL;
		private long MAX_LOCAL_CACHE_TTL_TICKS;
		private const int MAX_LOCAL_CACHE_TTL_DEFAULT_MIMUTES = 5;

#endif
		ConfigurationOptions _redisConnectionOptions;
		private const int REDIS_DEFAULT_PORT = 6379;
		public int redisSessionTimeout;

		public Redis(string connectionString)
		{
			_redisConnectionOptions = ConfigurationOptions.Parse(connectionString);
			_redisConnectionOptions.AllowAdmin = true;
		}

		public Redis(string connectionString, int sessionTimeout):this(connectionString)
		{
			redisSessionTimeout = sessionTimeout;
		}
		public Redis()
		{
			GXService providerService = ServiceFactory.GetGXServices()?.Get(GXServices.CACHE_SERVICE);
			if (providerService != null)
			{
				string address, password;
				address = providerService.Properties.Get("CACHE_PROVIDER_ADDRESS");
				password = providerService.Properties.Get("CACHE_PROVIDER_PASSWORD");

				if (string.IsNullOrEmpty(address))
					address = String.Format("localhost:{0}", REDIS_DEFAULT_PORT);

				if (!string.IsNullOrEmpty(password))
				{
					if (!address.Contains(':'))
					{
						address = $"{address}:{REDIS_DEFAULT_PORT}";
					}
					address = string.Format("{0},password={1}", address.Trim(), password.Trim());
					_redisConnectionOptions = ConfigurationOptions.Parse(address);
				}
				else
				{
					_redisConnectionOptions = ConfigurationOptions.Parse(address);
				}
				_redisConnectionOptions.AllowAdmin = true;
				InitLocalCache(providerService);
			}
		}
		private void InitLocalCache(GXService providerService)
		{
#if NETCORE
			string localCache = providerService.Properties.Get("ENABLE_MEMORY_CACHE");
			if (!string.IsNullOrEmpty(localCache) && localCache.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
			{
				GXLogging.Debug(log, "Using Redis Hybrid mode with local memory cache.");
				_localCache = new MemoryCache(new MemoryCacheOptions());
				if (Config.GetValueOrEnvironmentVarOf("MAX_LOCAL_CACHE_TTL", out string maxCacheTtlMinutesStr) && long.TryParse(maxCacheTtlMinutesStr, out long maxCacheTtlMinutes))
				{
					MAX_LOCAL_CACHE_TTL = TimeSpan.FromMinutes(maxCacheTtlMinutes);
					GXLogging.Debug(log, $"MAX_LOCAL_CACHE_TTL read from config: {MAX_LOCAL_CACHE_TTL}");
				}
				else
				{
					MAX_LOCAL_CACHE_TTL = TimeSpan.FromMinutes(MAX_LOCAL_CACHE_TTL_DEFAULT_MIMUTES);
					GXLogging.Debug(log, $"MAX_LOCAL_CACHE_TTL using default value: {MAX_LOCAL_CACHE_TTL}");
				}

				MAX_LOCAL_CACHE_TTL_TICKS = MAX_LOCAL_CACHE_TTL.Ticks;
			}
			else
			{
				GXLogging.Debug(log, "Using Redis only mode without local memory cache.");
			}
#endif
		}

		IDatabase RedisDatabase
		{
			get
			{
				if (_redisDatabase == null)
				{
					_redisConnection = ConnectionMultiplexer.Connect(_redisConnectionOptions);
					_redisDatabase = _redisConnection.GetDatabase();
				}
				return _redisDatabase;
			}
		}
		public void Clear(string cacheid, string key)
		{
			ClearKey(Key(cacheid, key));
		}

		public void ClearKey(string key)
		{
			RedisDatabase.KeyDelete(key);
			ClearKeyLocal(key);
		}

		public void ClearCache(string cacheid)
		{
			Nullable<long> prefix = new Nullable<long>(KeyPrefix(cacheid).Value + 1);
			RedisDatabase.StringSet(cacheid, prefix);
			SetPersistentLocal(cacheid, prefix);
		}

		public void ClearAllCaches()
		{
			IConnectionMultiplexer multiplexer = RedisDatabase.Multiplexer;
			System.Net.EndPoint[] endpoints = multiplexer.GetEndPoints(true);
			foreach (var endpoint in endpoints)
			{
				var server = multiplexer.GetServer(endpoint);
				server.FlushAllDatabases();
			}
			ClearAllCachesLocal();
		}

		public bool KeyExpire(string cacheid, string key, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
		{
			string fullKey = Key(cacheid, key);
			bool expirationSaved = RedisDatabase.KeyExpire(fullKey, expiry, flags);
			if (expirationSaved)
				KeyExpireLocal(fullKey);
			return expirationSaved;
		}

		public bool KeyExists(string cacheid, string key)
		{
			string fullKey = Key(cacheid, key);

			if (KeyExistsLocal(fullKey))
			{
				GXLogging.Debug(log, $"KeyExists hit local cache {fullKey}");
				return true;
			}

			return RedisDatabase.KeyExists(fullKey);
		}

		private bool Get<T>(string key, out T value)
		{
			if (GetLocal(key, out value))
			{
				GXLogging.Debug(log, $"Get<T> hit local cache {key}");
				return true;
			}

			if (default(T) == null)
			{
				value = Deserialize<T>(RedisDatabase.StringGet(key));
				if (value == null)
					GXLogging.Debug(log, "Get<T>, misses key '" + key + "'");
				else
					SetLocal(key, value);
				return value != null;
			}
			else
			{
				if (RedisDatabase.KeyExists(key))
				{
					value = Deserialize<T>(RedisDatabase.StringGet(key));
					SetLocal(key, value);
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

#if NETCORE
		public IDictionary<string, T> GetAll<T>(string cacheid, IEnumerable<string> keys)
		{
			if (keys == null) return null;

			var results = new Dictionary<string, T>();
			var keysToFetch = new List<string>();

			foreach (string k in keys)
			{
				string fullKey = Key(cacheid, k);
				if (GetLocal<T>(fullKey, out T value))
				{
					GXLogging.Debug(log, $"Get<T> hit local cache {fullKey}");
					results[k] = value;
				}
				else
				{
					keysToFetch.Add(k);
				}
			}

			if (keysToFetch.Count > 0)
			{
				var prefixedKeys = Key(cacheid, keysToFetch);
				RedisValue[] values = RedisDatabase.StringGet(prefixedKeys.ToArray());

				int i = 0;
				foreach (string k in keysToFetch)
				{
					string fullKey = Key(cacheid, k);
					T value = Deserialize<T>(values[i]);
					results[k] = value;

					SetLocal(fullKey, value);
					i++;
				}
			}

			return results;
		}
		public void SetAll<T>(string cacheid, IEnumerable<string> keys, IEnumerable<T> values, int duration = 0)
		{
			if (keys == null || values == null || keys.Count() != values.Count())
				return;

			IEnumerable<RedisKey> prefixedKeys = Key(cacheid, keys);
			IEnumerator<T> valuesEnumerator = values.GetEnumerator();
			KeyValuePair<RedisKey, RedisValue>[] redisBatch = new KeyValuePair<RedisKey, RedisValue>[prefixedKeys.Count()];

			int i = 0;
			foreach (RedisKey redisKey in prefixedKeys)
			{
				if (valuesEnumerator.MoveNext())
				{
					T value = valuesEnumerator.Current;
					redisBatch[i] = new KeyValuePair<RedisKey, RedisValue>(redisKey, Serialize(value));
					SetLocal<T>(redisKey.ToString(), value, duration);
				}
				i++;
			}
			if (redisBatch.Length > 0)
			{
				if (duration > 0)
				{
					foreach (var pair in redisBatch)
						RedisDatabase.StringSet(pair.Key, pair.Value, TimeSpan.FromMinutes(duration));
				}
				else
				{
					RedisDatabase.StringSet(redisBatch);
				}
			}
		}
#else
		public IDictionary<string, T> GetAll<T>(string cacheid, IEnumerable<string> keys)
		{
			if (keys != null)
			{
				var prefixedKeys = Key(cacheid, keys);
				RedisValue[] values = RedisDatabase.StringGet(prefixedKeys.ToArray());
				IDictionary<string, T> results = new Dictionary<string, T>();
				int i = 0;
				foreach (RedisKey key in prefixedKeys)
				{
					Get<T>(key, out T result);
					results.Add(key, Deserialize<T>(values[i]));
					i++;
				}
				return results;
			}
			else
			{
				return null;
			}
		}
		public void SetAll<T>(string cacheid, IEnumerable<string> keys, IEnumerable<T> values, int duration = 0)
		{
			if (keys != null && values != null && keys.Count() == values.Count())
			{
				var prefixedKeys = Key(cacheid, keys);
				IEnumerator<T> valuesEnumerator = values.GetEnumerator();
				KeyValuePair<RedisKey, RedisValue>[] dictionary = new KeyValuePair<RedisKey, RedisValue>[prefixedKeys.Count()];
				int i = 0;
				foreach (string key in prefixedKeys)
				{
					if (valuesEnumerator.MoveNext())
						dictionary[i] = new KeyValuePair<RedisKey, RedisValue>(key, Serialize(valuesEnumerator.Current));
				}
				RedisDatabase.StringSet(dictionary);
			}
		}
#endif
		private void Set<T>(string key, T value, int duration)
		{
			GXLogging.Debug(log, "Set<T> key:" + key + " value " + value + " valuetype:" + value.GetType());		
			if (duration > 0)
				RedisDatabase.StringSet(key, Serialize(value), TimeSpan.FromMinutes(duration));
			else
				RedisDatabase.StringSet(key, Serialize(value));
			SetLocal(key, value, duration);
		}

		private void Set<T>(string key, T value)
		{
			Set<T>(key, value, 0);
		}

		public bool Get<T>(string cacheid, string key, out T value)
		{
			GXLogging.Debug(log, "Get<T> cacheid:" + cacheid + " key:" + key);
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
		private IEnumerable<RedisKey> Key(string cacheid, IEnumerable<string> key)
		{
			long? prefix = KeyPrefix(cacheid);
			return key.Select(k => new RedisKey().Append(FormatKey(cacheid, k, prefix)));
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
		static string Serialize(object o)
		{
			if (o == null)
			{
				return null;
			}
			JsonSerializerOptions opts = new JsonSerializerOptions();
			opts.Converters.Add(new DBNullConverter());
			return JsonSerializer.Serialize(o, opts);
		}

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
#if NETCORE
		private TimeSpan LocalCacheTTL(int durationMinutes)
		{
			return LocalCacheTTL(durationMinutes > 0 ? TimeSpan.FromMinutes(durationMinutes) : (TimeSpan?)null);
		}
		private TimeSpan LocalCacheTTL(TimeSpan? ttl)
		{
			if (ttl.HasValue)
			{
				double ttlTicks = ttl.Value.Ticks * DEFAULT_LOCAL_CACHE_FACTOR;
				if (ttlTicks < MAX_LOCAL_CACHE_TTL_TICKS)
					return ttl.Value;
			}
			return MAX_LOCAL_CACHE_TTL;
		}
#endif
		private void ClearKeyLocal(string key)
		{
#if NETCORE
			_localCache?.Remove(key);
#endif
		}
		void ClearAllCachesLocal()
		{
#if NETCORE
			_localCache?.Compact(1.0);
#endif
		}

		private void KeyExpireLocal(string fullKey)
		{
#if NETCORE
			_localCache?.Remove(fullKey);
#endif
		}
		private bool KeyExistsLocal(string fullKey)
		{
#if NETCORE
			return _localCache?.TryGetValue(fullKey, out _) ?? false;
#else
			return false;
#endif
		}

		private void SetLocal<T>(string key, T value)
		{
#if NETCORE
			if (_localCache != null)
			{
				TimeSpan? redisTTL = RedisDatabase.KeyTimeToLive(key);
				_localCache.Set(key, value, LocalCacheTTL(redisTTL));
			}
#endif
		}
		private void SetPersistentLocal(string cacheid, long? prefix)
		{
#if NETCORE
			_localCache?.Set(cacheid, prefix, LocalCacheTTL(MAX_LOCAL_CACHE_TTL));
#endif
		}
		private void SetLocal<T>(string key, T value, int duration)
		{
#if NETCORE
			_localCache?.Set(key, value, LocalCacheTTL(duration));
#endif
		}
		private bool GetLocal<T>(string key, out T value)
		{
#if NETCORE
			if (_localCache == null)
			{
				value = default(T);
				return false;
			}
			return _localCache.TryGetValue(key, out value);
#else
				value = default(T);
				return false;
#endif
		}
	}
}