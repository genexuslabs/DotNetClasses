using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GeneXus.Deploy.AzureFunctions.Handlers.Dummies
{
	public static class HttpTriggerCSharp1
	{
		[Function("HttpTriggerCSharp1")]
		public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
			FunctionContext executionContext)
		{
			
			var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

			response.WriteString("Welcome to Azure Functions!");

			return response;
		}
	}
}
