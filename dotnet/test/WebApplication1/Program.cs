using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;

namespace WebApplication1
{
	public delegate IDistributedCache CacheResolver(string connectionString);

	public class CustomMiddleware
	{
		private readonly RequestDelegate _next;

		public CustomMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context, ISessionStore sessionStore)
		{
			await _next(context);
		}
	}
	public class Program
	{
		public static void Main(string[] args)
		{
			WebHost.CreateDefaultBuilder(args)
		   .UseStartup<Program>()
		   .Build().Run();
		}
		public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
		{
			app.UseMiddleware<CustomMiddleware>();
			app.UseSession();
			// Configure the HTTP request pipeline.

			app.UseSwagger();
			app.UseSwaggerUI();

			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
			app.UseHttpsRedirection();
			//app.UseAuthorization();


		}
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddTransient<CacheResolver>(_ => connectionString =>
			{
				Action<SqlServerCacheOptions> cacheConfigOptions = options =>
				{
					options.ConnectionString = connectionString;
					options.SchemaName = "dbo";
					options.TableName = "SessionData";
				};
				services.AddOptions();
				services.Configure(cacheConfigOptions);

				services.AddTransient<SqlServerCache>();

#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
				return services.BuildServiceProvider().GetService<SqlServerCache>();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
			});



			services.AddTransient<IDistributedCache, CustomCacheProvider>();
			services.AddSession();
			
			services.AddHttpContextAccessor();

			//services.AddDistributedMemoryCache();
			services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
		}
	}

	public class CustomCacheProvider : IDistributedCache
	{
		IHttpContextAccessor _httpContextAccessor;
		IDistributedCache _defaultCache;
		IDistributedCache _cache
		{
			get
			{
				IDistributedCache ctxCache =  _httpContextAccessor.HttpContext.Features.Get<IDistributedCache>();
				if (ctxCache != null)
					return ctxCache;
				else
					return _defaultCache;
			}
			
		}
		public CustomCacheProvider(CacheResolver resolver, IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;

			_defaultCache = resolver("Data Source=.\\SQLEXPRESS2022;Database=test;Integrated Security=true;TrustServerCertificate=true");
			if (httpContextAccessor!=null && httpContextAccessor.HttpContext != null)
			{
				httpContextAccessor.HttpContext.Features.Set<IDistributedCache>(_defaultCache);
			}
		}
		
		byte[] IDistributedCache.Get(string key)
		{
			return _cache.Get(key);
		}

		Task<byte[]> IDistributedCache.GetAsync(string key, CancellationToken token)
		{
			return _cache.GetAsync(key, token);
		}

		void IDistributedCache.Refresh(string key)
		{
			_cache.Refresh(key);
		}

		Task IDistributedCache.RefreshAsync(string key, CancellationToken token)
		{
			return _cache.RefreshAsync(key, token);
		}

		void IDistributedCache.Remove(string key)
		{
			_cache.Remove(key);
		}

		Task IDistributedCache.RemoveAsync(string key, CancellationToken token)
		{
			return _cache.RefreshAsync(key, token);
		}

		void IDistributedCache.Set(string key, byte[] value, DistributedCacheEntryOptions options)
		{
			_cache.Set(key, value, options);
		}

		Task IDistributedCache.SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token)
		{
			return _cache.SetAsync(key, value, options, token);
		}

	}
}
