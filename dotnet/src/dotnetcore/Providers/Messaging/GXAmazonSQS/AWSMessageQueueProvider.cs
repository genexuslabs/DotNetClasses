using GeneXus.Messaging.Common;
using GeneXus.Utils;

namespace GeneXus.Messaging.Queue
{
	public class AWSMessageQueueProvider
	{
		private const string AWS_SQS = "AWS_SQS";
		public SimpleMessageQueue Connect(GxUserType awsCredentials, string queueURL, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)

		{
			MessageQueueProvider messageQueueProvider = new MessageQueueProvider();
			GXProperties properties = TransformAWSCredentials(awsCredentials);
			properties.Add("QUEUE_AWSSQS_QUEUE_URL", queueURL);
			SimpleMessageQueue simpleMessageQueue = messageQueueProvider.Connect(AWS_SQS, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return simpleMessageQueue;
		}

		public GXProperties TransformAWSCredentials(GxUserType awsCredentials)
		{
			GXProperties properties = new GXProperties();
			properties.Add("QUEUE_AWSSQS_ACCESS_KEY", awsCredentials.GetPropertyValue<string>("Access_key"));
			properties.Add("QUEUE_AWSSQS_SECRET_KEY", awsCredentials.GetPropertyValue<string>("Secret_access_key"));
			properties.Add("QUEUE_AWSSQS_REGION", awsCredentials.GetPropertyValue<string>("Region"));
			properties.Add("QUEUE_AWSSQ_IAM_Role", awsCredentials.GetPropertyValue<string>("Iam_role"));
			return properties;
		}
	}
}
