using Azure.Messaging;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using Microsoft.Azure.Functions.Worker;

namespace EventGridTriggerDummy
{
	public static class EventGridFunction
	{
		[Function("EventGridFunction")]
		public static void Run([EventGridTrigger] CustomEventType input, FunctionContext context)
		{

		}
		[Function("EventGridFunctionCloudSchema")]
		public static void Run([EventGridTrigger] CloudEvent input, FunctionContext context)
		{

		}
	}
}