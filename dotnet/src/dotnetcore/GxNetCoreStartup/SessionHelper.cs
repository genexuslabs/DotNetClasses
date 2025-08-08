using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;
using GeneXus.Services;
using System.Linq;

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
			string tenantId = _httpContextAccessor.HttpContext?.Items[AppContext.TENANT_ID]?.ToString() ?? "default";

			return _redisCaches.GetOrAdd(tenantId, id =>
			{
				ISessionService sessionService = GXSessionServiceFactory.GetProvider();
				var options = new RedisCacheOptions
				{
					Configuration = sessionService.ConnectionString,
					InstanceName = $"{id}:"
				};
				return new RedisCache(options);
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
