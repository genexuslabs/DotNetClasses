using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Cache;
using GeneXus.Utils;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace GeneXus.Deploy.AzureFunctions.HttpHandler
{
	public class GXHttpAzureContext
	{
		private ICacheService2 _redis;
		private string sessionId;
		private ISession session;
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXHttpAzureContext>();
		internal const string AzureSessionId = "GX_AZURE_SESSIONID";

		public ISession Session => session;
		
		public GXHttpAzureContext( HttpRequest request, HttpResponse response, ICacheService2 redis)
		{			
			bool isSecure = IsSecureConnection(request);

			if (request != null && request.Cookies != null && request.Cookies[AzureSessionId] != null) 
				sessionId = request.Cookies[AzureSessionId];
			
			if (redis != null && redis.GetType() == typeof(Redis))
				_redis = redis;
			
			if (string.IsNullOrEmpty(sessionId) && request != null)
				CreateSessionId(isSecure, response, request);
			
			if ((_redis != null) && (sessionId != null))
				session = new RedisHttpSession(_redis, sessionId);
			else
				session = new MockHttpSession();

			if (!string.IsNullOrEmpty(sessionId))
			{//Refresh the session timestamp
				if (session is RedisHttpSession)
				{
					RedisHttpSession redisHttpSession = (RedisHttpSession)session;
					//Check if session is in cache
					if (redisHttpSession.SessionKeyExists(sessionId))
					{
						bool success = redisHttpSession.RefreshSession(sessionId);
						if (!success)
							GXLogging.Debug(log, $"Azure Serverless: Session could not be refreshed :{sessionId}");
					}
				}
			}

		}
		private bool GetSecureConnection(string headerKey, string headerValue)
		{
			if ((headerKey == "Front-End-Https") & (headerValue == "on"))
				return true;

			if ((headerKey == "X-Forwarded-Proto") & (headerValue == "https"))
				return true;

			return false;
		}

		private bool IsSecureConnection(HttpRequest request)
		{
			if ((request.Cookies["Front-End-Https"] == "on") || (request.Cookies["X-Forwarded-Proto"] == "https"))
				return true;
			else
				return false;
		}
		private void CreateSessionId(bool isSecure, HttpResponse response, HttpRequest request)
		{
			sessionId = Guid.NewGuid().ToString();
			
			if (!isSecure)
				isSecure = request.IsHttps;

			CookieOptions cookieOptions = new CookieOptions();

			if (!DateTime.MinValue.Equals(DateTimeUtil.NullDate()))
				cookieOptions.Expires = DateTime.MinValue;
			cookieOptions.Path = "";
			cookieOptions.Domain = "";
			cookieOptions.HttpOnly = true;
			cookieOptions.Secure = isSecure;
			
			if (response.Cookies != null)
				response.Cookies.Append(AzureSessionId,sessionId,cookieOptions);
			GXLogging.Debug(log, $"Create new Azure Session Id :{sessionId}");
		}
		public class MockHttpSession : ISession
		{
			string _sessionId = Guid.NewGuid().ToString();
			readonly ConcurrentDictionary<string, object> _sessionStorage = new ConcurrentDictionary<string, object>();
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
				_sessionStorage.TryRemove(key, out Object value);
			}
			void ISession.Set(string key, byte[] value)
			{
				_sessionStorage[key] = Encoding.UTF8.GetString(value);
			}
			bool ISession.TryGetValue(string key, out byte[] value)
			{
				value = Array.Empty<byte>();
				try
				{ 
					if (_sessionStorage != null && _sessionStorage.ContainsKey(key) && _sessionStorage[key] != null)
					{
						value = Encoding.ASCII.GetBytes(_sessionStorage[key].ToString());
						return true;
					}
					else
					{ 
						value = Array.Empty<byte>();
						return false;
					}
				}
				catch (Exception)
				{
					throw;
				}			
			}
		}
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
				value = Array.Empty<byte>();
				return false;
			}
			public bool SessionKeyExists(string sessionId)
			{
				return (_redis.KeyExists(AzureRedisCacheId, sessionId));
			}

		}
	}

}
