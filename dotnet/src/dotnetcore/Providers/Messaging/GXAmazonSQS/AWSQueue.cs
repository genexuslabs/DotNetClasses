using System;
using System.Collections.Generic;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using GeneXus.Messaging.Common;
using GeneXus.Services;
using GeneXus.Utils;
using log4net;
using System.Threading.Tasks;
using System.Reflection;

namespace GeneXus.Messaging.Queue
{
	public class AWSQueue : QueueBase, IQueue
	{

		public static String Name = "AWSSQS";
		const string ACCESS_KEY = "ACCESS_KEY";
		const string SECRET_ACCESS_KEY = "SECRET_KEY";
		const string REGION = "REGION";
		const string QUEUE_URL = "QUEUE_URL";

		AmazonSQSClient _sqsClient;
		private string _accessKey;
		private string _secret;
		private string _awsregion;
		private string _queueURL;
		private bool _isFIFO;
		public const string MESSSAGE_GROUP_ID = "MessageGroupId";
		public const string MESSSAGE_DEDUPLICATION_ID = "MessageDeduplicationId";

		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(AWSQueue));

		public AWSQueue() : this(null)
		{
		}

		public AWSQueue(GXService providerService) : base(providerService)
		{
			Initialize(providerService);
			BasicAWSCredentials basicCredentials;
			RegionEndpoint region = RegionEndpoint.GetBySystemName(_awsregion);

			if ((_accessKey != null) && (_secret != null))
			{ 
				basicCredentials = new BasicAWSCredentials(_accessKey, _secret);
				_sqsClient = new AmazonSQSClient(basicCredentials, region);
			}
			else //Use IAM Role
			{
				_sqsClient = new AmazonSQSClient(region);
			}
		}

		private void Initialize(GXService providerService)
		{
			ServiceSettings serviceSettings = new("QUEUE", Name, providerService);

			_queueURL = serviceSettings.GetEncryptedPropertyValue(QUEUE_URL);
			_accessKey = serviceSettings.GetEncryptedPropertyValue(ACCESS_KEY);
			_secret = serviceSettings.GetEncryptedPropertyValue(SECRET_ACCESS_KEY);
			_awsregion = serviceSettings.GetEncryptedPropertyValue(REGION);

			_isFIFO = _queueURL.EndsWith(".fifo");

		}
		public void Clear(out bool success)
		{
			try
			{
				Task<PurgeQueueResponse> task = Task.Run<PurgeQueueResponse>(async () => await PurgeQueueAsync());
				PurgeQueueResponse response = task.Result;
				success = response != null;

			}
			catch (AggregateException ae)
			{
				throw ae;
			}
		}

		public IList<MessageQueueResult> DeleteMessages(IList<SimpleQueueMessage> simpleQueueMessages, out bool success)
		{
			return RemoveMessages(simpleQueueMessages, out success);
		}
		private IList<MessageQueueResult> RemoveMessages(IList<SimpleQueueMessage> simpleQueueMessages, out bool success)
		{
			IList<MessageQueueResult> messageQueueResults = new List<MessageQueueResult>();
			List<string> messageHandleIds = new List<string>();
			success = false;
			try
			{
				foreach (SimpleQueueMessage simpleQueueMessage in simpleQueueMessages)
				{
					messageHandleIds.Add(simpleQueueMessage.MessageHandleId);
				}
				Task<DeleteMessageBatchResponse> task = Task.Run<DeleteMessageBatchResponse>(async () => await DeleteQueueMessageBatchAsync(messageHandleIds));

				DeleteMessageBatchResponse deleteMessageBatchResponse = task.Result;
				if (deleteMessageBatchResponse != null)
					success = (deleteMessageBatchResponse.Failed.Count == 0);

				foreach (BatchResultErrorEntry entry in deleteMessageBatchResponse.Failed)
				{
					MessageQueueResult messageQueueResult = SetupMessageQueueResult(entry);
					messageQueueResults.Add(messageQueueResult);
				}

				foreach (DeleteMessageBatchResultEntry entry in deleteMessageBatchResponse.Successful)
				{
					MessageQueueResult messageQueueResult = SetupMessageQueueResult(entry);
					messageQueueResults.Add(messageQueueResult);
				}
				
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return messageQueueResults;
		}

		public IList<SimpleQueueMessage> GetMessages(out bool success)
		{
			return RetrieveMessages(success : out success);
		}

		public IList<SimpleQueueMessage> GetMessages(MessageQueueOptions messageQueueOptions, out bool success)
		{
			return RetrieveMessages(out success, messageQueueOptions);
		}
		private IList<SimpleQueueMessage> RetrieveMessages(out bool success, MessageQueueOptions messageQueueOptions = null)
		{
			success = false;
			IList<SimpleQueueMessage> simpleQueueMessages = new List<SimpleQueueMessage>();
			try
			{
				Task<ReceiveMessageResponse> task = Task.Run<ReceiveMessageResponse>(async () => await GetMessageAsync(messageQueueOptions));

				ReceiveMessageResponse response = task.Result;
				success = response != null;
				if (success)
				{ 
					List<Message> messagesList = response.Messages;

					foreach (Message message in messagesList)
					{
				 		SimpleQueueMessage simpleQueueMessage = SetupSimpleQueueMessage(message);
						simpleQueueMessages.Add(simpleQueueMessage);	
					}
				}
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return simpleQueueMessages;
		}

		public int GetQueueLength(out bool success)
		{
			int approxNumberMessages = 0;
			success = false;
			try
			{
				List<string> attributes = new List<string> { "ApproximateNumberOfMessages" };
				Task<GetQueueAttributesResponse> task = Task.Run<GetQueueAttributesResponse>(async () => await GetQueueAttributeAsync(attributes).ConfigureAwait(false));
				GetQueueAttributesResponse response = task.Result;
				success = response != null;
				if (success)
				{
					return (response.ApproximateNumberOfMessages);
				}

			}
			catch (Exception ex)
			{
				throw ex;
			}
			return approxNumberMessages;
		}

		public MessageQueueResult SendMessage(SimpleQueueMessage simpleQueueMessage, out bool success)
		{
			success = false;
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			List<SimpleQueueMessage> simpleQueueMessages = new List<SimpleQueueMessage>() { simpleQueueMessage };
			try
			{
				Task<SendMessageBatchResponse> task = Task.Run<SendMessageBatchResponse>(async () => await SendMessageBatchAsync(simpleQueueMessages));
				SendMessageBatchResponse sendMessageBatchResponse = task.Result;
				if (sendMessageBatchResponse != null)
					success = (sendMessageBatchResponse.Failed.Count == 0);

				foreach (BatchResultErrorEntry entry in sendMessageBatchResponse.Failed)
				{
					messageQueueResult = SetupMessageQueueResult(entry);
				}

				foreach (SendMessageBatchResultEntry entry in sendMessageBatchResponse.Successful)
				{
					messageQueueResult = SetupMessageQueueResult(entry);
				}

			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return messageQueueResult;
		}

		protected MessageQueueResult SendMessage(SimpleQueueMessage simpleQueueMessage, MessageQueueOptions messageQueueOptions, out bool success)
		{
			success = false;
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			List<SimpleQueueMessage> simpleQueueMessages = new List<SimpleQueueMessage>() { simpleQueueMessage };
			try
			{
				Task<SendMessageBatchResponse> task = Task.Run<SendMessageBatchResponse>(async () => await SendMessageBatchAsync(simpleQueueMessages, messageQueueOptions));
				SendMessageBatchResponse sendMessageBatchResponse = task.Result;
				if (sendMessageBatchResponse != null)
					success = (sendMessageBatchResponse.Failed.Count == 0);

				foreach (BatchResultErrorEntry entry in sendMessageBatchResponse.Failed)
				{
					 messageQueueResult = SetupMessageQueueResult(entry);
				}

				foreach (SendMessageBatchResultEntry entry in sendMessageBatchResponse.Successful)
				{
					 messageQueueResult = SetupMessageQueueResult(entry);
				}

			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return messageQueueResult;
		}

		public IList<MessageQueueResult> SendMessages(IList<SimpleQueueMessage> simpleQueueMessages, MessageQueueOptions messageQueueOptions, out bool success)
		{
			success = false;
			IList<MessageQueueResult> messageQueueResults = new List<MessageQueueResult>();
			try
			{
				Task<SendMessageBatchResponse> task = Task.Run<SendMessageBatchResponse>(async () => await SendMessageBatchAsync(simpleQueueMessages, messageQueueOptions));
				SendMessageBatchResponse sendMessageBatchResponse = task.Result;
				if (sendMessageBatchResponse != null)
					success = (sendMessageBatchResponse.Failed.Count == 0);

				foreach (BatchResultErrorEntry entry in sendMessageBatchResponse.Failed)
				{
					MessageQueueResult messageQueueResult = SetupMessageQueueResult(entry);
					messageQueueResults.Add(messageQueueResult);
				}

				foreach (SendMessageBatchResultEntry entry in sendMessageBatchResponse.Successful)
				{
					MessageQueueResult messageQueueResult = SetupMessageQueueResult(entry);
					messageQueueResults.Add(messageQueueResult);
				}

			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return messageQueueResults;
		}
		
		public override string GetName()
		{
			return Name;
		}

		public bool GetMessageFromException(Exception ex, SdtMessages_Message msg)
		{
			try
			{
				AmazonSQSException sqs_ex = (AmazonSQSException)ex;
				msg.gxTpr_Id = sqs_ex.ErrorCode;
				msg.gxTpr_Description = sqs_ex.Message;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private MessageQueueResult SetupMessageQueueResult(SendMessageResponse response)
		{
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			messageQueueResult.MessageId = response.MessageId;
			messageQueueResult.MessageStatus = MessageQueueResultStatus.Sent;

			messageQueueResult.MessageAttributes = new GXProperties();

			messageQueueResult.MessageAttributes.Add("MD5OfMessageSystemAttributes", response.MD5OfMessageSystemAttributes);
			messageQueueResult.MessageAttributes.Add("MD5OfMessageAttributes", response.MD5OfMessageAttributes);
			messageQueueResult.MessageAttributes.Add("ContentLength", response.ContentLength.ToString());
			messageQueueResult.MessageAttributes.Add("MD5OfMessageBody", response.MD5OfMessageBody);
			messageQueueResult.MessageAttributes.Add("SequenceNumber", response.SequenceNumber);

			Type t = response.ResponseMetadata.GetType();
			PropertyInfo[] props = t.GetProperties();

			foreach (PropertyInfo prop in props)
			{
				object value;
				if (prop.GetIndexParameters().Length == 0 && response.ResponseMetadata != null)
				{
					value = prop.GetValue(response.ResponseMetadata);
					if (value != null)
						messageQueueResult.MessageAttributes.Add(prop.Name, value.ToString());
				}
			}
			return messageQueueResult;
		}
		private MessageQueueResult SetupMessageQueueResult(SendMessageBatchResultEntry response)
		{
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			messageQueueResult.MessageId = response.Id;
			messageQueueResult.MessageStatus = MessageQueueResultStatus.Sent;

			messageQueueResult.MessageAttributes = new GXProperties();

			messageQueueResult.MessageAttributes.Add("MD5OfMessageSystemAttributes", response.MD5OfMessageSystemAttributes);
			messageQueueResult.MessageAttributes.Add("MD5OfMessageAttributes", response.MD5OfMessageAttributes);
			messageQueueResult.MessageAttributes.Add("MD5OfMessageBody", response.MD5OfMessageBody);
			messageQueueResult.MessageAttributes.Add("SequenceNumber", response.SequenceNumber);
			return messageQueueResult;
		}

		private MessageQueueResult SetupMessageQueueResult(DeleteMessageBatchResultEntry response)
		{
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			messageQueueResult.MessageStatus = MessageQueueResultStatus.Deleted;
			messageQueueResult.MessageId = response.Id;
			return messageQueueResult;
		}

		private MessageQueueResult SetupMessageQueueResult(BatchResultErrorEntry response)
		{
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			messageQueueResult.MessageStatus = MessageQueueResultStatus.Failed;
			messageQueueResult.MessageId = response.Id;

			//Write error codes to log in debug mode
			GXLogging.Debug(logger, $"Error processing SQS. Message: {response.Id}. Error: {response.Message}({response.Code})");

			return messageQueueResult;
		}

		private SimpleQueueMessage SetupSimpleQueueMessage(Message response)
		{
			SimpleQueueMessage simpleQueueMessage = new SimpleQueueMessage();
			simpleQueueMessage.MessageId = response.MessageId;
			simpleQueueMessage.MessageBody = response.Body;
			simpleQueueMessage.MessageHandleId = response.ReceiptHandle;

			simpleQueueMessage.MessageAttributes = new GXProperties();
			
			simpleQueueMessage.MessageAttributes.Add("MD5OfMessageAttributes", response.MD5OfMessageAttributes);
			simpleQueueMessage.MessageAttributes.Add("MD5OfBody", response.MD5OfBody);
			
			foreach (var messageAttribute in response.MessageAttributes)
			{
				MessageAttributeValue messageAttributeValue = messageAttribute.Value;
				simpleQueueMessage.MessageAttributes.Add(messageAttribute.Key, messageAttribute.Value.StringValue);
			}

			foreach (var attribute in response.Attributes)
			{
				simpleQueueMessage.MessageAttributes.Add(attribute.Key, attribute.Value);
			}

			return simpleQueueMessage;
		}

		private async Task<SendMessageResponse> SendMessageAsync(SimpleQueueMessage simpleQueueMessage, MessageQueueOptions messageQueueOptions = null)
		{
			SendMessageResponse sendMessageResponse = new SendMessageResponse();

			if (simpleQueueMessage != null)
			{
				GXProperties messageAttributes = simpleQueueMessage.MessageAttributes;
				Dictionary<string, MessageAttributeValue> properties = new Dictionary<string, MessageAttributeValue>();

				GxKeyValuePair messageAttribute = new GxKeyValuePair();
				if (messageAttributes != null)
					messageAttribute = messageAttributes.GetFirst();
				while (!messageAttributes.Eof())
				{
					properties.Add(messageAttribute.Key, new MessageAttributeValue() { DataType = "String", StringValue = messageAttribute.Value });
					messageAttribute = messageAttributes.GetNext();
				}

				SendMessageRequest sendMessageRequest = new SendMessageRequest
				{
					QueueUrl = _queueURL,
					MessageBody = simpleQueueMessage.MessageBody,
					MessageAttributes = properties
				};

				if (messageQueueOptions != null && messageQueueOptions.DelaySeconds != 0)
					sendMessageRequest.DelaySeconds = messageQueueOptions.DelaySeconds;

				if ((messageAttributes != null) && (_isFIFO))
				{
					string mesageGroupId = messageAttributes.Get(MESSSAGE_GROUP_ID);
					string messageDeduplicationId = messageAttributes.Get(MESSSAGE_DEDUPLICATION_ID);

					if ((mesageGroupId != null) && (messageDeduplicationId != null))
					{
						sendMessageRequest.MessageGroupId = mesageGroupId;
						sendMessageRequest.MessageDeduplicationId = messageDeduplicationId;

					}
				}
				sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest).ConfigureAwait(false);
			}
			return sendMessageResponse;
		}

		private async Task<SendMessageBatchResponse> SendMessageBatchAsync(IList<SimpleQueueMessage> simpleQueueMessages, MessageQueueOptions messageQueueOptions=null)
		{
			List<SendMessageBatchRequestEntry> messageBatchRequestEntries = new List<SendMessageBatchRequestEntry>();
			SendMessageBatchResponse responseSendBatch = new SendMessageBatchResponse();

			foreach (SimpleQueueMessage simpleQueueMessage in simpleQueueMessages)
			{
				SendMessageBatchRequestEntry requestEntry = new SendMessageBatchRequestEntry();
				GXProperties messageAttributes = simpleQueueMessage.MessageAttributes;
				Dictionary<string, MessageAttributeValue> properties = new Dictionary<string, MessageAttributeValue>();

				GxKeyValuePair messageAttribute = new GxKeyValuePair();
				if (messageAttributes != null)
					messageAttribute = messageAttributes.GetFirst();
				while (!messageAttributes.Eof())
				{
					properties.Add(messageAttribute.Key, new MessageAttributeValue() { DataType = "String", StringValue = messageAttribute.Value });
					messageAttribute = messageAttributes.GetNext();
				}

				requestEntry.MessageBody = simpleQueueMessage.MessageBody;
				requestEntry.Id = simpleQueueMessage.MessageId;
				requestEntry.MessageAttributes = properties;
				if ((messageQueueOptions != null) && (messageQueueOptions.DelaySeconds != 0))
					requestEntry.DelaySeconds = messageQueueOptions.DelaySeconds;

				if ((messageAttributes != null) && (_isFIFO))
				{
					string mesageGroupId = messageAttributes.Get(MESSSAGE_GROUP_ID);
					string messageDeduplicationId = messageAttributes.Get(MESSSAGE_DEDUPLICATION_ID);

					if ((mesageGroupId != null) && (mesageGroupId != null))
					{
						requestEntry.MessageGroupId = mesageGroupId;
						requestEntry.MessageDeduplicationId = messageDeduplicationId;
					}
				}

				messageBatchRequestEntries.Add(requestEntry);
			}
			if (messageBatchRequestEntries.Count > 0)
				responseSendBatch = await _sqsClient.SendMessageBatchAsync(_queueURL, messageBatchRequestEntries).ConfigureAwait(false);

			return (responseSendBatch);

		}

		private async Task<ReceiveMessageResponse> GetMessageAsync(MessageQueueOptions messageQueueOptions = null)
		{
			ReceiveMessageResponse receiveMessageResponse = new ReceiveMessageResponse();
			try
			{

				ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest();
				receiveMessageRequest.QueueUrl = _queueURL;
				if (messageQueueOptions != null)
				{
					if (messageQueueOptions.MaxNumberOfMessages != 0)
						receiveMessageRequest.MaxNumberOfMessages = messageQueueOptions.MaxNumberOfMessages;
					if (messageQueueOptions.VisibilityTimeout != 0)
						receiveMessageRequest.VisibilityTimeout = messageQueueOptions.VisibilityTimeout;
					if (messageQueueOptions.WaitTimeout != 0)
						receiveMessageRequest.WaitTimeSeconds = messageQueueOptions.WaitTimeout;
					if (! string.IsNullOrEmpty(messageQueueOptions.ReceiveRequestAttemptId))
						receiveMessageRequest.ReceiveRequestAttemptId = messageQueueOptions.ReceiveRequestAttemptId;
					// TO DO : Check only for specific attrributes in the list

					if (messageQueueOptions.ReceiveMessageAttributes)
					{
						receiveMessageRequest.AttributeNames = new List<string> { "All" };
						receiveMessageRequest.MessageAttributeNames = new List<string>() { "All" };
					}
				}

				return await _sqsClient.ReceiveMessageAsync(receiveMessageRequest).ConfigureAwait(false);
			}
			catch (TaskCanceledException)
			{
				GXLogging.Debug(logger, $"Get Message Operation cancelled for SQS {_queueURL}.");
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			return receiveMessageResponse;
		}

		private async Task<DeleteMessageBatchResponse> DeleteQueueMessageBatchAsync(List<string> messageHandleId)
		{
			DeleteMessageBatchResponse deleteMessageBatchResponse = new DeleteMessageBatchResponse();
			try
			{

				List<DeleteMessageBatchRequestEntry> deleteMessageBatchRequestEntries = new List<DeleteMessageBatchRequestEntry>();

				foreach (string handleId in messageHandleId)
				{
					DeleteMessageBatchRequestEntry deleteMessageBatchRequestEntry = new DeleteMessageBatchRequestEntry();
					deleteMessageBatchRequestEntry.ReceiptHandle = handleId;
					deleteMessageBatchRequestEntry.Id = Guid.NewGuid().ToString();
					deleteMessageBatchRequestEntries.Add(deleteMessageBatchRequestEntry);
				}

				deleteMessageBatchResponse = await _sqsClient.DeleteMessageBatchAsync(_queueURL, deleteMessageBatchRequestEntries).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			return deleteMessageBatchResponse;
		}

		private async Task<PurgeQueueResponse> PurgeQueueAsync()
		{
			PurgeQueueResponse purgeQueueResponse = new PurgeQueueResponse();
			try
			{
				PurgeQueueRequest purgeQueueRequest = new PurgeQueueRequest();
				purgeQueueRequest.QueueUrl = _queueURL;

				purgeQueueResponse= await _sqsClient.PurgeQueueAsync(purgeQueueRequest).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			return purgeQueueResponse;
		}

		private async Task<GetQueueAttributesResponse> GetQueueAttributeAsync(List<string> attributesName)
		{
			GetQueueAttributesResponse getQueueAttributesResponse = new GetQueueAttributesResponse();
			GetQueueAttributesRequest getQueueAttributesRequest = new GetQueueAttributesRequest();
			try
			{
				getQueueAttributesRequest.QueueUrl= _queueURL;
				if (attributesName == null)
					getQueueAttributesRequest.AttributeNames.Add("All");
				foreach (string name in attributesName)
					getQueueAttributesRequest.AttributeNames.Add(name);
				getQueueAttributesResponse = await _sqsClient.GetQueueAttributesAsync(getQueueAttributesRequest).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			return getQueueAttributesResponse;

		}
	}

}


