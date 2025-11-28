using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace GeneXus.Application
{

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
}
