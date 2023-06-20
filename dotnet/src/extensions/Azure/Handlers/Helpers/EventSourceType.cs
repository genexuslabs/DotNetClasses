namespace GeneXus.Deploy.AzureFunctions.Handlers.Helpers
{
	public class EventSourceType
	{
		public const string QueueMessage = "QueueMessage";
		public const string ServiceBusMessage = "ServiceBusMessage";
		public const string Timer = "Timer";
		public const string CosmosDB = "CosmosDB";
		public const string Blob = "Blob";
	}
}
