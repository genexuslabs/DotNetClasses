using System.Threading.Tasks;
using GeneXus.Cache;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GxClasses.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GeneXus.Deploy.AzureFunctions.Handlers.BackendServices
{
	public class BackendServices
	{
		private IGXRouting _gxRouting;
		private ICacheService2 _redis;
		private ProxyServices _proxyServices;

		public BackendServices(IGXRouting gxRouting, ICacheService2 redis)
		{
			_gxRouting = gxRouting;
			if (redis != null && redis.GetType() == typeof(Redis))
				_redis = redis;
			_proxyServices = new ProxyServices(_gxRouting, _redis);
		}

		[Function("gxmulticall")]
		public async Task<HttpResponseData> GXMulticall([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gxmulticall")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}
	}
}
