using System;
using System.Collections;
using GeneXus.Messaging.Common;
using GeneXus.Utils;

namespace GeneXus.Messaging.GXAzureServiceBus
{
	public class ServiceBusMessageBrokerProvider
	{
		public MessageQueue Connect(string queueName, string connectionString, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			System.Diagnostics.Debugger.Launch();
			MessageBrokerProvider messageBrokerProvider = new MessageBrokerProvider();
			GXProperties properties = new GXProperties();
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_QUEUENAME, queueName);
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_CONNECTIONSTRING, connectionString);

			MessageQueue messageQueue = messageBrokerProvider.Connect(PropertyConstants.AZURESERVICEBUS, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return messageQueue;
		}

		public MessageQueue Connect(string topicName, string subcriptionName, string connectionString, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			System.Diagnostics.Debugger.Launch();
			MessageBrokerProvider messageBrokerProvider = new MessageBrokerProvider();
			GXProperties properties = new GXProperties();
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_TOPICNAME, topicName);
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_SUBSCRIPTION_NAME, subcriptionName);
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_CONNECTIONSTRING, connectionString);

			MessageQueue messageQueue = messageBrokerProvider.Connect(PropertyConstants.AZURESERVICEBUS, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return messageQueue;
		}
		public MessageQueue Connect(string queueName, string connectionString, bool sessionEnabled, GxUserType receiverOptions, string senderIdentifier, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			System.Diagnostics.Debugger.Launch();

			MessageBrokerProvider messageBrokerProvider = new MessageBrokerProvider();

			ReceiverOptions options = TransformGXUserTypeToReceiverOptions(receiverOptions);

			GXProperties properties = new GXProperties();
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_QUEUENAME, queueName);
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_CONNECTIONSTRING, connectionString);
			properties.Add(PropertyConstants.SESSION_ENABLED, sessionEnabled.ToString());
			properties.Add(PropertyConstants.RECEIVE_MODE, options.ReceiveMode.ToString());
			properties.Add(PropertyConstants.PREFETCH_COUNT, options.PrefetchCount.ToString());
			properties.Add(PropertyConstants.RECEIVER_IDENTIFIER, options.Identifier);
			properties.Add(PropertyConstants.RECEIVER_SESSIONID, options.SessionId);
			properties.Add(PropertyConstants.SENDER_IDENTIFIER, senderIdentifier);

			MessageQueue messageQueue = messageBrokerProvider.Connect(PropertyConstants.AZURESERVICEBUS, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return messageQueue;
		}

		public MessageQueue Connect(string topicName, string subcriptionName, string connectionString, bool sessionEnabled, GxUserType receiverOptions, string senderIdentifier, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			System.Diagnostics.Debugger.Launch();
			MessageBrokerProvider messageBrokerProvider = new MessageBrokerProvider();
			GXProperties properties = new GXProperties();
			ReceiverOptions options = TransformGXUserTypeToReceiverOptions(receiverOptions);

			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_TOPICNAME, topicName);
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_SUBSCRIPTION_NAME, subcriptionName);
			properties.Add(PropertyConstants.MESSAGEBROKER_AZURESB_CONNECTIONSTRING, connectionString);
			properties.Add(PropertyConstants.SESSION_ENABLED, sessionEnabled.ToString());
			properties.Add(PropertyConstants.RECEIVE_MODE, options.ReceiveMode.ToString());
			properties.Add(PropertyConstants.PREFETCH_COUNT, options.PrefetchCount.ToString());
			properties.Add(PropertyConstants.RECEIVER_IDENTIFIER, options.Identifier);
			properties.Add(PropertyConstants.RECEIVER_SESSIONID, options.SessionId);
			properties.Add(PropertyConstants.SENDER_IDENTIFIER, senderIdentifier);
			
			MessageQueue messageQueue = messageBrokerProvider.Connect(PropertyConstants.AZURESERVICEBUS, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return messageQueue;
		}

		#region Transformation methods
		private ReceiverOptions TransformGXUserTypeToReceiverOptions(GxUserType options)
		{
			ReceiverOptions receiverOptions = new ReceiverOptions();
			if (options != null)
			{ 
				receiverOptions.ReceiveMode = options.GetPropertyValue<short>("Receivemode");
				receiverOptions.Identifier = options.GetPropertyValue<string>("Identifier");
				receiverOptions.PrefetchCount = options.GetPropertyValue<short>("Prefetchcount");
				receiverOptions.SessionId = options.GetPropertyValue<string>("Sessionid");
			}
			return receiverOptions;
		}
		#endregion
		public class ReceiverOptions : GxUserType
		{
			public short ReceiveMode { get; set; }
			public short PrefetchCount { get; set; }
			public string Identifier { get; set; }
			public string SessionId { get; set; }

			#region Json
			private static Hashtable mapper;
			public override String JsonMap(String value)
			{
				if (mapper == null)
				{
					mapper = new Hashtable();
				}
				return (String)mapper[value]; ;
			}

			public override void ToJSON()
			{
				ToJSON(true);
				return;
			}

			public override void ToJSON(bool includeState)
			{
				AddObjectProperty("ReceiveMode", ReceiveMode, false);
				AddObjectProperty("PrefetchCount", PrefetchCount, false);
				AddObjectProperty("Identifier", Identifier, false);
				AddObjectProperty("SessionId", Identifier, false);
				return;
			}

			#endregion
		}
	}
}
