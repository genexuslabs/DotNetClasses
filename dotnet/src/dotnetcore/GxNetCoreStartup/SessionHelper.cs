using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace GeneXus.Application
{
	public class TenantRedisCache : IDistributedCache
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<TenantRedisCache>();

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
			RedisCache cache;
			bool existed = _redisCaches.TryGetValue(tenantId, out cache);
			if (existed)
			{
				log.LogDebug($"GetTenantCache: tenantId={tenantId}, cache reused");
				return cache;
			}
			else
			{
				log.LogDebug($"GetTenantCache: tenantId={tenantId}, cache created");
				cache = _redisCaches.GetOrAdd(tenantId, id =>
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
			return cache;
		}
		public byte[] Get(string key)
		{
			IDistributedCache cache = GetTenantCache();
			log.LogDebug($"CacheGet: key={key}");
			return cache.Get(key);
		}
		public Task<byte[]> GetAsync(string key, CancellationToken token = default)
		{
			IDistributedCache cache = GetTenantCache();
			log.LogDebug($"CacheGetAsync: key={key}");
			return cache.GetAsync(key, token);
		}
		public void Refresh(string key)
		{
			IDistributedCache cache = GetTenantCache();
			log.LogDebug($"CacheRefresh: key={key}");
			cache.Refresh(key);
		}
		public Task RefreshAsync(string key, CancellationToken token = default)
		{
			IDistributedCache cache = GetTenantCache();
			log.LogDebug($"CacheRefreshAsync: key={key}");
			return cache.RefreshAsync(key, token);
		}
		public void Remove(string key)
		{
			IDistributedCache cache = GetTenantCache();
			log.LogDebug($"CacheRemove: key={key}");
			cache.Remove(key);
		}
		public Task RemoveAsync(string key, CancellationToken token = default)
		{
			IDistributedCache cache = GetTenantCache();
			log.LogDebug($"CacheRemoveAsync: key={key}");
			return cache.RemoveAsync(key, token);
		}
		public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
		{
			string sliding = options?.SlidingExpiration?.ToString() ?? "none";
			IDistributedCache cache = GetTenantCache();
			log.LogDebug($"CacheSet: key={key}, slidingExpiration={sliding}");
			cache.Set(key, value, options);
		}
		public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
		{
			string sliding = options?.SlidingExpiration?.ToString() ?? "none";
			IDistributedCache cache = GetTenantCache();
			log.LogDebug($"CacheSetAsync: key={key}, slidingExpiration={sliding}");
			return cache.SetAsync(key, value, options, token);
		}
	}

	public class TenantMiddleware
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<TenantMiddleware>();

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

			log.LogDebug($"TenantMiddleware: host={host}, subdomain={subdomain}, path={context.Request.Path}");
			await _next(context);
		}
	}
}
