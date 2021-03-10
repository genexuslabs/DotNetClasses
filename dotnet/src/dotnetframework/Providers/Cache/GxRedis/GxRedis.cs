using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Runtime.Serialization.Formatters.Binary;
using GeneXus.Services;
using GeneXus.Utils;
using log4net;
using StackExchange.Redis;
using System.Reflection;
using System.Security;

namespace GeneXus.Cache
{
	public sealed class Redis : ICacheService2
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Redis));
		ConnectionMultiplexer _redisConnection;
		IDatabase _redis;
		private const int REDIS_DEFAULT_PORT = 6379;
		public Redis()
		{
			GXService providerService = ServiceFactory.GetGXServices().Get(GXServices.CACHE_SERVICE);
			string address, password;
			address = providerService.Properties.Get("CACHE_PROVIDER_ADDRESS");
			password = providerService.Properties.Get("CACHE_PROVIDER_PASSWORD");
			ConfigurationOptions options;


			if (!string.IsNullOrEmpty(address))
			{
				if (!string.IsNullOrEmpty(password))
				{
					if (!address.Contains(':'))
					{
						address = $"{address}:{REDIS_DEFAULT_PORT}";
					}
					address = string.Format("{0},password={1}", address.Trim(), password.Trim());
					options = ConfigurationOptions.Parse(address);
				}
				else
				{
					options = ConfigurationOptions.Parse(address);
				}
			}
			else
			{
				options = ConfigurationOptions.Parse(String.Format("localhost:{0}", REDIS_DEFAULT_PORT));
			}

			options.AllowAdmin = true;
			_redisConnection = ConnectionMultiplexer.Connect(options);

			_redis = _redisConnection.GetDatabase();
		}

		public void Clear(string cacheid, string key)
		{
			ClearKey(Key(cacheid, key));
		}

		public void ClearKey(string key)
		{
			_redis.KeyDelete(key);
		}

		public void ClearCache(string cacheid)
		{
			Nullable<long> prefix = new Nullable<long>(KeyPrefix(cacheid).Value + 1);
			_redis.StringSet(cacheid, prefix);
		}

		public void ClearAllCaches()
		{
			var endpoints = _redisConnection.GetEndPoints(true);
			foreach (var endpoint in endpoints)
			{
				var server = _redisConnection.GetServer(endpoint);
				server.FlushAllDatabases();
			}
		}

		private bool Get<T>(string key, out T value)
		{
			if (default(T) == null)
			{
				value = Deserialize<T>(_redis.StringGet(key));
				if (value == null) GXLogging.Debug(log, "Get<T>, misses key '" + key + "'");
				return value != null;
			}
			else
			{
				if (_redis.KeyExists(key))
				{
					value = Deserialize<T>(_redis.StringGet(key));
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
				RedisValue[] values = _redis.StringGet(prefixedKeys.ToArray());
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
				_redis.StringSet(dictionary);
			}
		}

		private void Set<T>(string key, T value, int duration)
		{
			GXLogging.Debug(log, "Set<T> key:" + key + " value " + value + " valuetype:" + value.GetType());
			if (duration > 0)
				_redis.StringSet(key, Serialize(value), TimeSpan.FromMinutes(duration));
			else
				_redis.StringSet(key, Serialize(value));
		}

		private void Set<T>(string key, T value)
		{
			_redis.StringSet(key, Serialize(value));
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
			var prefix = KeyPrefix(cacheid);
			return key.Select(k => new RedisKey(FormatKey(cacheid, k, prefix)));
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
			return JsonSerializer.Serialize(o);
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
	}
	public class ObjectToInferredTypesConverter: JsonConverter<object>
	{
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(object) == typeToConvert;
		}
		public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.True:
					return false;
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

		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}