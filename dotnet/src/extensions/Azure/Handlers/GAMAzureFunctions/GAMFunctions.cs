using System.Threading.Tasks;
using GeneXus.Cache;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GxClasses.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using HttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;

namespace GeneXus.Deploy.AzureFunctions.GAM
{
	public class GAMAzureFunctions
	{
		private IGXRouting _gxRouting;
		private ICacheService2 _redis;

		private ProxyServices _proxyServices;

		public GAMAzureFunctions(IGXRouting gxRouting, ICacheService2 redis)
		{
			_gxRouting = gxRouting;
			if (redis != null && redis.GetType() == typeof(Redis))
				_redis = redis;
			_proxyServices = new ProxyServices(_gxRouting, _redis);
		}


		[Function("oauth_access_token")]
		public async Task<HttpResponseData> Accesstoken([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/access_token")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_logout")]
		public async Task<HttpResponseData> Logout([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/logout")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_userinfo")]
		public async Task<HttpResponseData> UserInfo([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/userinfo")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_gam_signin")]
		public async Task<HttpResponseData> Signin([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/gam/signin")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_gam_callback")]
		public async Task<HttpResponseData> Callback([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/gam/callback")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_gam_access_token")]
		public async Task<HttpResponseData> GAMAccessToken([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/gam/access_token")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_gam_userinfo")]
		public async Task<HttpResponseData> GAMUserInfo([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/gam/userinfo")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_gam_signout")]
		public async Task<HttpResponseData> GAMSignout([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/gam/signout")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_RequestTokenService")]
		public async Task<HttpResponseData> RequestTokenService([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/RequestTokenService")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("oauth_QueryAccessToken")]
		public async Task<HttpResponseData> QueryAccessToken([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "oauth/QueryAccessToken")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("saml_gam_signout")]
		public async Task<HttpResponseData> SAMLGAMSignout([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "saml/gam/signout")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

		[Function("saml_gam_signin")]
		public async Task<HttpResponseData> SAMLGAMSigin([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "saml/gam/signin")] HttpRequestData req,
			FunctionContext executionContext)
		{
			return await _proxyServices.ExecuteHttpFunction(req, executionContext);
		}

	}

}