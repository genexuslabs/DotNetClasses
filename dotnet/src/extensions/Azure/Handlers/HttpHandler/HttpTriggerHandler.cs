using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GxClasses.Web;
using GxClasses.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

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

		public HttpTriggerHandler(IGXRouting gxRouting)
		{
			_gxRouting = gxRouting;
		}
		public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
			FunctionContext executionContext)
		{

			var logger = executionContext.GetLogger("HttpTriggerHandler");
			logger.LogInformation($"GeneXus Http trigger handler. Function processed: {executionContext.FunctionDefinition.Name}.");

			var httpResponseData = req.CreateResponse();
			HttpContext httpAzureContextAccessor = new GXHttpAzureContextAccessor(req, httpResponseData);

			GXRouting.ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			GXRouting.AzureFunctionName = executionContext.FunctionDefinition.Name;

			_gxRouting.ProcessRestRequest(httpAzureContextAccessor);
			return httpResponseData;
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



