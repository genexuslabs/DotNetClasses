using System;
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
		public void SetServiceSession(HttpRequest request, HttpResponse response, HttpContext httpContext)
		{
			if (GxContext.IsAzureContext)
			{
				if ((context != null && context.HttpContext != null) && (Request != null && Response != null))
				{
					GXHttpAzureContext httpAzureContext = new GXHttpAzureContext(Request, Response, _cacheService);
					if (httpAzureContext != null && httpAzureContext.Session != null && context != null && context.HttpContext != null)
						context.HttpContext.Session = httpAzureContext.Session;
					else
						GXLogging.Debug(log, $"Error : Azure Serverless session could not be created.");
				}
				else
				{
					if (context != null)
					{
						context.HttpContext = httpContext;
						GXHttpAzureContext httpAzureContext = new GXHttpAzureContext(request, response, _cacheService);
						if (httpAzureContext != null && httpAzureContext.Session != null && context != null && context.HttpContext != null)
							context.HttpContext.Session = httpAzureContext.Session;
						else
							GXLogging.Debug(log, $"Error : Azure Serverless session could not be created.");
					}
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
