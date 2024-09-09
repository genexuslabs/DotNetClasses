using GeneXus.Messaging.Common;
using GeneXus.Utils;

namespace GeneXus.Messaging.Queue
{
	/// <summary>
	/// Implementation of AzureQueue.MessageQueueProvider external object.
	/// </summary>
	/// 
	public class AzureMessageQueueProvider
	{
		public SimpleMessageQueue Connect(string queueName, string connectionString, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{			
			MessageQueueProvider messageQueueProvider = new MessageQueueProvider();
			GXProperties properties = new GXProperties
			{
				{ PropertyConstants.QUEUE_AZUREQUEUE_QUEUENAME, queueName },
				{ PropertyConstants.QUEUE_AZUREQUEUE_CONNECTIONSTRING, connectionString },
				{ PropertyConstants.AUTHENTICATION_METHOD, AuthenticationMethod.Password.ToString()}
			};
			SimpleMessageQueue simpleMessageQueue = messageQueueProvider.Connect(PropertyConstants.AZURE_QUEUE_PROVIDERTYPENAME, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return simpleMessageQueue;
		}
		public SimpleMessageQueue Authenticate(string queueURI, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			MessageQueueProvider messageQueueProvider = new MessageQueueProvider();
			GXProperties properties = new GXProperties
			{
				{ PropertyConstants.QUEUE_AZUREQUEUE_QUEUEURI, queueURI },
				{ PropertyConstants.AUTHENTICATION_METHOD, AuthenticationMethod.ActiveDirectory.ToString()}
			};
			SimpleMessageQueue simpleMessageQueue = messageQueueProvider.Connect(PropertyConstants.AZURE_QUEUE_PROVIDERTYPENAME, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return simpleMessageQueue;
		}
	}
}
