using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

namespace GeneXus.Application
{
	public class TenantRedisCache : IDistributedCache
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<TenantRedisCache>();

		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly RedisCache _redis;

		public TenantRedisCache(IConnectionMultiplexer _redisMultiplexer, IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;

			var options = new RedisCacheOptions
			{
				ConnectionMultiplexerFactory = () => Task.FromResult(_redisMultiplexer),
			};
			_redis = new RedisCache(options);
		}
		private string GetTenantId()
		{
			return _httpContextAccessor.HttpContext?.Items[TenantMiddleware.TENANT_ID]?.ToString() ?? "default";
		}
		private string BuildKey(string key)
		{
			return $"{GetTenantId()}:{key}";
		}
		public byte[] Get(string key)
		{
			string realKey = BuildKey(key);
			log.LogDebug($"CacheGet: key={realKey}");
			return _redis.Get(realKey);
		}
		public Task<byte[]> GetAsync(string key, CancellationToken token = default)
		{
			string realKey = BuildKey(key);
			log.LogDebug($"CacheGetAsync: key={realKey}");
			return _redis.GetAsync(realKey, token);
		}
		public void Refresh(string key)
		{
			string realKey = BuildKey(key);
			log.LogDebug($"CacheRefresh: key={realKey}");
			_redis.Refresh(realKey);
		}
		public Task RefreshAsync(string key, CancellationToken token = default)
		{
			string realKey = BuildKey(key);
			log.LogDebug($"CacheRefreshAsync: key={realKey}");
			return _redis.RefreshAsync(realKey, token);
		}
		public void Remove(string key)
		{
			string realKey = BuildKey(key);
			log.LogDebug($"CacheRemove: key={realKey}");
			_redis.Remove(realKey);
		}
		public Task RemoveAsync(string key, CancellationToken token = default)
		{
			string realKey = BuildKey(key);
			log.LogDebug($"CacheRemoveAsync: key={realKey}");
			return _redis.RemoveAsync(realKey, token);
		}
		public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
		{
			string sliding = options?.SlidingExpiration?.ToString() ?? "none";
			string realKey = BuildKey(key);
			log.LogDebug($"CacheSet: key={realKey}, slidingExpiration={sliding}");
			_redis.Set(realKey, value, options);
		}
		public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
		{
			string sliding = options?.SlidingExpiration?.ToString() ?? "none";
			string realKey = BuildKey(key);
			log.LogDebug($"CacheSetAsync: key={realKey}, slidingExpiration={sliding}");
			return _redis.SetAsync(realKey, value, options, token);
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
