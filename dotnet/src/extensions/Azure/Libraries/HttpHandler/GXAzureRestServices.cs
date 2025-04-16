using System;
using System.Net.Http;
using GeneXus;
using GeneXus.Application;
using GeneXus.Cache;
using GeneXus.Deploy.AzureFunctions.HttpHandler;
using GeneXus.Utils;
using Microsoft.AspNetCore.Http;

namespace GxClasses.Web.Middleware
{
	public class GXAzureRestService : GxRestService
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXAzureRestService>();
		private readonly ICacheService2 _cacheService;
		private HttpContext _httpContext;

		protected override HttpContext GetHttpContext()
		{
			return _httpContext;
		}

		protected void setInitialization(HttpContext httpContext) 
		{
			_httpContext = httpContext;
			context = GxContext.CreateDefaultInstance();
			SetServiceSession(httpContext.Request, httpContext.Response, httpContext);

		}
		public GXAzureRestService(ICacheService2 redis) : base()
		{
			if (GxContext.IsAzureContext)
			{
				if (redis != null && redis.GetType() == typeof(Redis))
				{
					_cacheService = redis;
				}
				else
					_cacheService = new InProcessCache();
			}
			else
			{
				GXLogging.Debug(log, "Error: Not an Azure context.");
				throw new Exception("Operation Cancelled. Not an Azure context.");
			}
		}
		private void SetServiceSession(HttpRequest request, HttpResponse response, HttpContext httpContext)
		{
			if (GxContext.IsAzureContext)
			{
				if (_httpContext != null)
				{
					GXHttpAzureContext httpAzureContext = new GXHttpAzureContext(_httpContext.Request, _httpContext.Response, _cacheService);
					if (httpAzureContext != null && httpAzureContext.Session != null)
						_httpContext.Session = httpAzureContext.Session;
					else
						GXLogging.Debug(log, $"Error : Azure Serverless session could not be created.");
				}
			}
			else
			{
				GXLogging.Debug(log, "Error: Not an Azure context.");
				throw new Exception("Operation Cancelled. Not an Azure context.");
			}
		}
	}
}
