using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace GeneXus.Application
{
	public class TenantRedisCache : IDistributedCache
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ConcurrentDictionary<string, IDistributedCache> _redisCaches = new();

		public TenantRedisCache(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		private IDistributedCache GetTenantCache()
		{
			string tenantId = _httpContextAccessor.HttpContext?.Items[AppContext.TENANT_ID]?.ToString() ?? "default";

			return _redisCaches.GetOrAdd(tenantId, id =>
			{
				ISessionService sessionService = GXSessionServiceFactory.GetProvider();
				return new CustomRedisSessionStore(sessionService.ConnectionString, TimeSpan.FromMinutes(sessionService.SessionTimeout), id);
			});
		}

		public byte[] Get(string key) => GetTenantCache().Get(key);
		public Task<byte[]> GetAsync(string key, CancellationToken token = default) => GetTenantCache().GetAsync(key, token);
		public void Refresh(string key) => GetTenantCache().Refresh(key);
		public Task RefreshAsync(string key, CancellationToken token = default) => GetTenantCache().RefreshAsync(key, token);
		public void Remove(string key) => GetTenantCache().Remove(key);
		public Task RemoveAsync(string key, CancellationToken token = default) => GetTenantCache().RemoveAsync(key, token);
		public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => GetTenantCache().Set(key, value, options);
		public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => GetTenantCache().SetAsync(key, value, options, token);
	}

	public class CustomRedisSessionStore : IDistributedCache
	{
		private readonly IDatabase _db;
		private readonly TimeSpan _idleTimeout;
		private readonly TimeSpan _refreshThreshold;
		private readonly string _instanceName;
		// Refresh only if remaining TTL is below 35% of the original idle timeout.
		private const double IdleTimeoutRefreshRatio= 0.3; // 30%
		public CustomRedisSessionStore(string connectionString, TimeSpan idleTimeout, string instanceName)
		{
			var mux = ConnectionMultiplexer.Connect(connectionString);
			_db = mux.GetDatabase();
			_idleTimeout = idleTimeout;
			_refreshThreshold = TimeSpan.FromTicks((long)(idleTimeout.Ticks * IdleTimeoutRefreshRatio));
			_instanceName = instanceName ?? string.Empty;
		}

		private string FormatKey(string key) => string.IsNullOrEmpty(_instanceName) ? key : $"{_instanceName}:{key}";

		public byte[] Get(string key)
		{
			string redisKey = FormatKey(key);
			var value = _db.StringGet(redisKey);

			if (value.HasValue)
				RefreshKeyIfNeeded(redisKey);

			return value;
		}
		public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
		{
			string redisKey = FormatKey(key);
			var value = await _db.StringGetAsync(redisKey);

			if (value.HasValue)
			{
				await RefreshKeyIfNeededAsync(redisKey);
			}

			return value;
		}
		public void Refresh(string key)
		{
			string redisKey = FormatKey(key);
			RefreshKeyIfNeeded(redisKey);
		}
		private bool ShouldRefreshKey(TimeSpan? ttl)
		{
			return ttl.HasValue && ttl.Value < _refreshThreshold && ttl.Value > TimeSpan.Zero; ;
		}
		public async Task RefreshAsync(string key, CancellationToken token = default)
		{
			string redisKey = FormatKey(key);
			await RefreshKeyIfNeededAsync(redisKey);
		}
		private void RefreshKeyIfNeeded(string redisKey)
		{
			var ttl = _db.KeyTimeToLive(redisKey);
			if (ShouldRefreshKey(ttl))
			{
				_db.KeyExpire(redisKey, _idleTimeout);
			}
		}
		private async Task RefreshKeyIfNeededAsync(string redisKey)
		{
			var ttl = await _db.KeyTimeToLiveAsync(redisKey);

			if (ShouldRefreshKey(ttl))
			{
				await _db.KeyExpireAsync(redisKey, _idleTimeout);
			}
		}
		public void Remove(string key) => _db.KeyDelete(FormatKey(key));

		public Task RemoveAsync(string key, CancellationToken token = default)
			=> _db.KeyDeleteAsync(FormatKey(key));

		public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
		{
			TimeSpan expiry = options?.AbsoluteExpirationRelativeToNow ?? _idleTimeout;
			_db.StringSet(FormatKey(key), value, expiry);
		}
		public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
		{
			TimeSpan expiry = options?.AbsoluteExpirationRelativeToNow ?? _idleTimeout;
			return _db.StringSetAsync(FormatKey(key), value, expiry);
		}
	}

	public class TenantMiddleware
	{
		private readonly RequestDelegate _next;

		public TenantMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			string host = context?.Request?.Host.Host ?? string.Empty;
			string subdomain;

			if (!string.IsNullOrEmpty(host) && host.Contains('.'))
			{
				subdomain = host.Split('.').FirstOrDefault();
				if (!string.IsNullOrEmpty(subdomain))
					context.Items[AppContext.TENANT_ID] = subdomain;
			}

			await _next(context);
		}
	}
}
