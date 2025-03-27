using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Cache;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace GeneXus.Deploy.AzureFunctions.HttpHandler
{
	public class RedisHttpSession : ISession
	{
		const int SESSION_TIMEOUT_IN_MINUTES = 5;
		const string AzureRedisCacheId = "REDIS_CACHE_SESSION_ID";
		string _sessionId;
		private Redis _redis;
		public Dictionary<string, byte[]> data;
		public RedisHttpSession(ICacheService2 redis, string sessionId)
		{
			_redis = (Redis)redis;
			_sessionId = sessionId;
		}

		public bool IsAvailable => throw new NotImplementedException();

		public string Id => _sessionId;

		public IEnumerable<string> Keys => throw new NotImplementedException();

		private IEnumerable<string> convert<String>(IEnumerable<RedisKey> enumerable)
		{
			foreach (RedisKey key in enumerable)
				yield return key;
		}
		public void Clear()
		{
			_redis.ClearCache(AzureRedisCacheId);
		}

		public bool RefreshSession(string sessionId)
		{
			if (_redis.Get(AzureRedisCacheId, sessionId, out Dictionary<string, byte[]> value))
			{
				int refreshTimeout = (_redis.redisSessionTimeout == 0) ? SESSION_TIMEOUT_IN_MINUTES : _redis.redisSessionTimeout;
				if (value != null)
				{
					return (_redis.KeyExpire(AzureRedisCacheId, sessionId, TimeSpan.FromMinutes(refreshTimeout), CommandFlags.None));
				}
			}
			return false;
		}

		public Task CommitAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task LoadAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public void Remove(string key)
		{
			_redis.ClearKey(key);
		}

		public void Set(string key, byte[] value)
		{
			if (!_redis.Get(AzureRedisCacheId, Id, out Dictionary<string, byte[]> data))
				data = new Dictionary<string, byte[]>();
			data[key] = value;

			if (_redis.redisSessionTimeout != 0)
				_redis.Set(AzureRedisCacheId, Id, data, _redis.redisSessionTimeout);
			else
				_redis.Set(AzureRedisCacheId, Id, data, SESSION_TIMEOUT_IN_MINUTES);
		}

		public bool TryGetValue(string key, out byte[] value)
		{
			if (_redis.Get(AzureRedisCacheId, Id, out Dictionary<string, byte[]> data))
			{
				if (data != null)
				{
					if (data.TryGetValue(key, out byte[] keyvalue))
					{
						value = keyvalue;
						return true;
					}
				}
			}
			value = null;
			return false;
		}
		public bool SessionKeyExists(string sessionId)
		{
			return (_redis.KeyExists(AzureRedisCacheId, sessionId));
		}

	}
	public class MockHttpSession : ISession
	{
		string _sessionId = Guid.NewGuid().ToString();
		readonly Dictionary<string, object> _sessionStorage = new Dictionary<string, object>();
		string ISession.Id => _sessionId;
		bool ISession.IsAvailable => throw new NotImplementedException();
		IEnumerable<string> ISession.Keys => _sessionStorage.Keys;
		void ISession.Clear()
		{
			_sessionStorage.Clear();
		}
		Task ISession.CommitAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
		Task ISession.LoadAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
		void ISession.Remove(string key)
		{
			_sessionStorage.Remove(key);
		}
		void ISession.Set(string key, byte[] value)
		{
			_sessionStorage[key] = Encoding.UTF8.GetString(value);
		}
		bool ISession.TryGetValue(string key, out byte[] value)
		{

			if (_sessionStorage.ContainsKey(key) && _sessionStorage[key] != null)
			{
				value = Encoding.ASCII.GetBytes(_sessionStorage[key].ToString());
				return true;
			}
			value = null;
			return false;
		}
	}
}

