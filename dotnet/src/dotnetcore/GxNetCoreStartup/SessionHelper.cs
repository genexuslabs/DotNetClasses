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
		private readonly IServiceProvider _serviceProvider;
		private readonly ConcurrentDictionary<string, RedisCache> _redisCaches = new();

		public TenantRedisCache(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
		{
			_httpContextAccessor = httpContextAccessor;
			_serviceProvider = serviceProvider;
		}

		private IDistributedCache GetTenantCache()
		{
			string tenantId = _httpContextAccessor.HttpContext?.Items[TenantMiddleware.TENANT_ID]?.ToString() ?? "default";

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

		public CustomRedisSessionStore(string connectionString, TimeSpan idleTimeout, string instanceName)
		{
			var mux = ConnectionMultiplexer.Connect(connectionString);
			_db = mux.GetDatabase();
			_idleTimeout = idleTimeout;
			_refreshThreshold = TimeSpan.FromTicks((long)(idleTimeout.Ticks * 0.2));
			_instanceName = instanceName ?? string.Empty;
		}

		private string FormatKey(string key) => string.IsNullOrEmpty(_instanceName) ? key : $"{_instanceName}:{key}";

		public byte[] Get(string key) => _db.StringGet(FormatKey(key));

		public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
		{
			string redisKey = FormatKey(key);
			var value = await _db.StringGetAsync(redisKey);

			var ttl = await _db.KeyTimeToLiveAsync(redisKey);

			if (ttl.HasValue && ttl.Value < _refreshThreshold)
			{
				_ = _db.KeyExpireAsync(redisKey, _idleTimeout);
			}

			return value;
		}

		public void Refresh(string key)
		{
			string redisKey = FormatKey(key);
		}

		public Task RefreshAsync(string key, CancellationToken token = default)
		{
			return Task.CompletedTask;
		}

		public void Remove(string key) => _db.KeyDelete(FormatKey(key));

		public Task RemoveAsync(string key, CancellationToken token = default)
			=> _db.KeyDeleteAsync(FormatKey(key));

		public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
		{
			_db.StringSet(FormatKey(key), value, _idleTimeout);
		}

		public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
		{
			return _db.StringSetAsync(FormatKey(key), value, _idleTimeout);
		}
	}

	public class TenantMiddleware
	{
		internal const string TENANT_ID = "TenantId";
		private readonly RequestDelegate _next;

		public TenantMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			string host = context.Request.Host.Host;
			string subdomain = host.Split('.').FirstOrDefault();
			context.Items[TENANT_ID] = subdomain;

			await _next(context);
		}
	}
}
