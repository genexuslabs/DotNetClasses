namespace GeneXus.Messaging.Common
{
	public static class PropertyConstants
	{
		//Azure Service Bus
		internal const string AZURE_SB_CLASSNAME = "GeneXus.Messaging.GXAzureServiceBus.AzureServiceBus";
		internal const string AZURE_SB_PROVIDER_CLASSNAME = "GeneXus.Messaging.GXAzureServiceBus.AzureServiceBus, GXAzureServiceBus, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		public const string AZURESERVICEBUS = "AZURESERVICEBUS";
		public const string RECEIVE_MODE = "RECEIVE_MODE";
		public const string PREFETCH_COUNT = "PREFETCH_COUNT";
		public const string RECEIVER_IDENTIFIER = "RECEIVER_IDENTIFIER";
		public const string RECEIVER_SESSIONID = "RECEIVER_SESSIONID";
		public const string SENDER_IDENTIFIER = "SENDER_IDENTIFIER";

		public const string MESSAGEBROKER_AZURESB_QUEUENAME = "MESSAGEBROKER_AZURESB_QUEUENAME";
		public const string MESSAGEBROKER_AZURESB_TOPICNAME = "MESSAGEBROKER_AZURESB_TOPICNAME";
		public const string MESSAGEBROKER_AZURESB_SUBSCRIPTION_NAME = "MESSAGEBROKER_AZURESB_SUBSCRIPTION";
		public const string MESSAGEBROKER_AZURESB_CONNECTIONSTRING = "MESSAGEBROKER_AZURESB_QUEUECONNECTION";
		public const string QUEUE_NAME = "QUEUENAME";
		public const string QUEUE_CONNECTION_STRING = "QUEUECONNECTION";
		public const string TOPIC_SUBSCRIPTION = "SUBSCRIPTION";
		public const string MESSAGE_BROKER = "MESSAGEBROKER";
		public const string SESSION_ENABLED = "SESSION_ENABLED";
	}
}
