using Azure.Messaging;
using Azure.Messaging.EventGrid;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using Microsoft.Azure.Functions.Worker;

namespace EventGridTriggerDummy
{
	public static class EventGridFunction
	{
		
		[Function("EventGridFunctionCloudSchema")]
		public static void Run([EventGridTrigger] CloudEvent input, FunctionContext context)
		{

		}
		[Function("EventGridFunctionAzureSchema")]
		public static void Run([EventGridTrigger] EventGridEvent input, FunctionContext context)
		{

		}
	}
}