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

		//AWS SQS

		public const string AWSSQS_QUEUE_PROVIDERTYPENAME = "GeneXus.Messaging.Queue.AWSQueue, GXAmazonSQS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		public const string QUEUE_AWSSQS_QUEUE_URL = "QUEUE_AWSSQS_QUEUE_URL";
		public const string QUEUE_AWSSQS_ACCESS_KEY = "QUEUE_AWSSQS_ACCESS_KEY";
		public const string QUEUE_AWSSQS_SECRET_KEY = "QUEUE_AWSSQS_SECRET_KEY";
		public const string QUEUE_AWSSQS_REGION = "QUEUE_AWSSQS_REGION";

	}
	public enum AuthenticationMethod
	{
		ActiveDirectory,
		Password
	}
}
