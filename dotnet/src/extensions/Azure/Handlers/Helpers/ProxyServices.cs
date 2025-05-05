using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Cache;
using GeneXus.Deploy.AzureFunctions.HttpHandler;
using GxClasses.Web;
using GxClasses.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GeneXus.Deploy.AzureFunctions.Handlers.Helpers
{
	public class ProxyServices
	{
		private IGXRouting _gxRouting;
		private ICacheService2 _redis;

		public ProxyServices(IGXRouting gxRouting, ICacheService2 redis)
		{
			_gxRouting = gxRouting;
			_redis = redis;
		}

		public async Task<HttpResponseData> ExecuteHttpFunction(HttpRequestData req, FunctionContext executionContext)
		{

			HttpResponseData httpResponseData = req.CreateResponse();
			HttpContext httpAzureContextAccessor = new GXHttpAzureContextAccessor(req, httpResponseData, _redis);

			GXRouting.ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			GXRouting.AzureFunctionName = executionContext.FunctionDefinition.Name;

			await _gxRouting.ProcessRestRequest(httpAzureContextAccessor);
			return httpResponseData;
		}
	}
}
