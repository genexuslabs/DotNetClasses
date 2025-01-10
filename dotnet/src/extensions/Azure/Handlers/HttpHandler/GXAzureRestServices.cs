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
			}
		}
		public void SetServiceSession(HttpRequest request, HttpResponse response, HttpContext httpContext)
		{
			if (GxContext.IsAzureContext)
			{
				GXHttpAzureContext httpAzureContext;
				if ((context != null & context.HttpContext != null) && (Request != null && Response != null))
				{
					httpAzureContext = new GXHttpAzureContext(Request, Response, _cacheService);
				}
				else
				{
					context.HttpContext = httpContext;
					httpAzureContext = new GXHttpAzureContext(request, response, _cacheService);

				}
				if (httpAzureContext != null && httpAzureContext.Session != null)
					context.HttpContext.Session = httpAzureContext.Session;
				else
					GXLogging.Debug(log, $"Azure Serverless: Session could not be created.");
			}
		}
	}
}
