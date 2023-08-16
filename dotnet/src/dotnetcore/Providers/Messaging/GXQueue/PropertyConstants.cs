namespace GeneXus.Messaging.Common
{
	public static class PropertyConstants
	{
		//Azure Storage Queue
		internal const string AZURE_QUEUE_CLASSNAME = "GeneXus.Messaging.Queue.AzureQueue";
		internal const string AZURE_QUEUE_PROVIDER_CLASSNAME = "GeneXus.Messaging.Queue.AzureQueue, GXAzureQueue, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		public const string AZURE_QUEUE_PROVIDERTYPENAME = "AZUREQUEUE";

		public const string QUEUE_SERVICE_NAME = "QUEUE";

		public const string QUEUE_AZUREQUEUE_QUEUENAME = "QUEUE_AZUREQUEUE_QUEUENAME";
		public const string QUEUE_AZUREQUEUE_CONNECTIONSTRING = "QUEUE_AZUREQUEUE_CONNECTIONSTRING";
		public const string QUEUE_AZUREQUEUE_QUEUEURI = "QUEUE_AZUREQUEUE_QUEUEURI";

		public const string QUEUENAME = "QUEUENAME";
		public const string CONNECTIONSTRING = "CONNECTIONSTRING";
		public const string QUEUEURI = "QUEUEURI";
		public const string AUTHENTICATION_METHOD = "AUTHENTICATION_METHOD";

	}
	public enum AuthenticationMethod
	{
		ActiveDirectory,
		Password
	}
}
