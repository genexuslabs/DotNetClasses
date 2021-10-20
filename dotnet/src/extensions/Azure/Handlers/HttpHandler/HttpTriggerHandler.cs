using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Cache;
using GxClasses.Web;
using GxClasses.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GeneXus.Deploy.AzureFunctions.HttpHandler

{
	public class HttpTriggerHandler
	{
		
		public static Dictionary<string, string> servicesPathUrl = new Dictionary<string, string>();
		public List<string> servicesBase = new List<string>();
		public static Dictionary<String, Dictionary<string, string>> servicesMap = new Dictionary<String, Dictionary<string, string>>();
		public static Dictionary<String, Dictionary<Tuple<string, string>, String>> servicesMapData = new Dictionary<String, Dictionary<Tuple<string, string>, string>>();
		public static Dictionary<string, List<string>> servicesValidPath = new Dictionary<string, List<string>>();

		private IGXRouting _gxRouting;
		private ICacheService2 _redis;

		public HttpTriggerHandler(IGXRouting gxRouting, ICacheService2 redis)
		{
			_gxRouting = gxRouting;
			if (redis != null & redis.GetType() == typeof(Redis))
				_redis = redis;
		}
		public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
			FunctionContext executionContext)
		{

			var logger = executionContext.GetLogger("HttpTriggerHandler");
			logger.LogInformation($"GeneXus Http trigger handler. Function processed: {executionContext.FunctionDefinition.Name}.");

			var httpResponseData = req.CreateResponse();
			HttpContext httpAzureContextAccessor = new GXHttpAzureContextAccessor(req, httpResponseData, _redis);

			GXRouting.ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			GXRouting.AzureFunctionName = executionContext.FunctionDefinition.Name;

			_gxRouting.ProcessRestRequest(httpAzureContextAccessor);
			return httpResponseData;
		}
	}

	public class RedisHttpSession : ISession
	{
		const int AZURE_SESSION_TIMEOUT_IN_MINUTES = 5;
		string _sessionId;
		private Redis _redis;
		public RedisHttpSession(ICacheService2 redis, string sessionId)
		{
			_redis = (Redis)redis;
			_sessionId = sessionId;
		}
		public bool IsAvailable => throw new NotImplementedException();

		public string Id => _sessionId;

		public IEnumerable<string> Keys => throw new NotImplementedException();
		
		private IEnumerable<string> convert<String>(IEnumerable<RedisKey> enumerable)
		{
			foreach (RedisKey key in enumerable)
				yield return key;
		}
		public void Clear()
		{
			_redis.ClearCache(Id);
		}

		public Task CommitAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task LoadAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public void Remove(string key)
		{
			_redis.ClearKey(key);
		}

		public void Set(string key, byte[] value)
		{
			if (_redis.redisSessionTimeout != 0)
				_redis.Set(Id, key, value, _redis.redisSessionTimeout);
			else
				_redis.Set(Id, key, value, AZURE_SESSION_TIMEOUT_IN_MINUTES);
		}

		public bool TryGetValue(string key, out byte[] value)
		{
			if (_redis.Get(Id,key,out value) && value != null)
			{
				return true;
			}
			value = null;
			return false;
		}
	}
	public class MockHttpSession : ISession
	{
		string _sessionId = Guid.NewGuid().ToString();
		readonly Dictionary<string, object> _sessionStorage = new Dictionary<string, object>();
		string ISession.Id => _sessionId;
		bool ISession.IsAvailable => throw new NotImplementedException();
		IEnumerable<string> ISession.Keys => _sessionStorage.Keys;
		void ISession.Clear()
		{
			_sessionStorage.Clear();
		}
		Task ISession.CommitAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
		Task ISession.LoadAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
		void ISession.Remove(string key)
		{
			_sessionStorage.Remove(key);
		}
		void ISession.Set(string key, byte[] value)
		{
			_sessionStorage[key] = Encoding.UTF8.GetString(value);
		}
		bool ISession.TryGetValue(string key, out byte[] value)
		{

			if (_sessionStorage.ContainsKey(key) && _sessionStorage[key] != null)
			{
				value = Encoding.ASCII.GetBytes(_sessionStorage[key].ToString());
				return true;
			}
			value = null;
			return false;
		}
	}
}



