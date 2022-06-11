using GeneXus.Messaging.Common;
using GeneXus.Utils;

namespace GeneXus.Messaging.Queue
{
	public class AzureMessageQueueProvider
	{
		private const string AZUREQUEUE = "AZUREQUEUE";
		public SimpleMessageQueue Connect(string queueName, string queueURL, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)

		{
			MessageQueueProvider messageQueueProvider = new MessageQueueProvider();
			GXProperties properties = new GXProperties();
			properties.Add("QUEUE_AZUREQUEUE_QUEUENAME", queueName);
			properties.Add("QUEUE_AZUREQUEUE_CONNECTIONSTRING", queueURL);
			SimpleMessageQueue simpleMessageQueue = messageQueueProvider.Connect(AZUREQUEUE, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return simpleMessageQueue;
		}
	}
}
