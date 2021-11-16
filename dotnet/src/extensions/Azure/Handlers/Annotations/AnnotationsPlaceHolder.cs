using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AllFunctions
{
	// This is a dummy class.
	// The purpose of this class is to force the Net SDK to generate all the configuration files
	// needed for running under Net 5 framework.
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
	public static class QueueTriggerDummy
	{
		[Function("QueueTrigger1")]
		public static void Run([QueueTrigger("myqueue-items", Connection = "")] string myQueueItem,
			FunctionContext context)
		{

		}
	}
	public static class ServiceBusQueueDummy
	{
		[Function("ServiceBusQueueTrigger1")]
		public static void Run([ServiceBusTrigger("myqueue", Connection = "")] string myQueueItem, FunctionContext context)
		{

		}
	}
	public static class TimerTriggerDummy
	{
		[Function("TimerTrigger1")]
		public static void Run([TimerTrigger("0 */5 * * * *")] string myTimer, FunctionContext context)
		{
		}
	}
}
