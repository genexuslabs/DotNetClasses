using GeneXus.Messaging.Common;
using GeneXus.Utils;

namespace GeneXus.Messaging.GXAzureServiceBus
{
	public class ServiceBusMessageBrokerProvider
	{
		public MessageQueue Connect(string queueName, string queueConnection, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			MessageBrokerProvider messageBrokerProvider = new MessageBrokerProvider();
			GXProperties properties = new GXProperties();
			properties.Add(PropertyConstants. MESSAGEBROKER_AZURESB_QUEUENAME, queueName);
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_QUEUECONNECTION, queueConnection);
			MessageQueue messageQueue = messageBrokerProvider.Connect(PropertyConstants.AZURESERVICEBUS, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return messageQueue;
		}
	}
}
