using System;
using System.Collections.Generic;
using System.Reflection;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using GeneXus.Messaging.Common;
using GeneXus.Services;
using GeneXus.Utils;

namespace GeneXus.Messaging.Queue
{
	public class AzureQueue : QueueBase, IQueue
	{

		public static String Name = "AZUREQUEUE";
		const string QUEUE_NAME = "QUEUENAME";
		const string QUEUE_CONNECTION_STRING = "CONNECTIONSTRING";

		QueueClient _queueClient { get; set; }
		private string _queueName { get; set; }
		private string _connectionString { get; set; }

		public AzureQueue() : this(null)
		{
		}

		public AzureQueue(GXService providerService) : base(providerService)
		{
			Initialize(providerService);
		}

		private void Initialize(GXService providerService)
		{
			ServiceSettings serviceSettings = new("QUEUE", Name, providerService);	
			_queueName = serviceSettings.GetEncryptedPropertyValue(QUEUE_NAME);
			_connectionString = serviceSettings.GetEncryptedPropertyValue(QUEUE_CONNECTION_STRING);

			QueueClientOptions queueClientOptions = new QueueClientOptions()
			{
				MessageEncoding = QueueMessageEncoding.Base64
			};

			_queueClient = new QueueClient(_connectionString, _queueName, queueClientOptions);
		}

		QueueClient QueueClient
		{
			get
			{
				if (_queueClient == null)
					_queueClient = new QueueClient(_connectionString, _queueName);
				return _queueClient;
			}
		}

		public AzureQueue(string connectionString, string queueName)
		{
			_queueName = queueName;
			_connectionString = connectionString;
		}

		//public AzureQueue(Uri uri, TokenCredential tokenCredential)
		//{
		//_queueClient = new QueueClient(uri, tokenCredential);
		//}

		public bool GetMessageFromException(Exception ex, SdtMessages_Message msg)
		{
			try
			{
				Azure.RequestFailedException az_ex = (Azure.RequestFailedException)ex;
				msg.gxTpr_Id = az_ex.ErrorCode;
				msg.gxTpr_Description = az_ex.Message; 
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Get the approximate number of messages in the queue
		/// </summary>
		/// <param name="success"></param>
		/// <returns></returns>
		public int GetQueueLength(out bool success)
		{
			int cachedMessagesCount = 0;
			success = false;
			if (_queueClient is QueueClient && _queueClient.Exists())
			{
				QueueProperties properties = _queueClient.GetProperties();

				// Retrieve the cached approximate message count.
				cachedMessagesCount = properties.ApproximateMessagesCount;
				success = true;
			}
			return cachedMessagesCount;
		}

		/// <summary>
		/// Deletes all messages from a queue.
		/// </summary>
		public void Clear(out bool success)
		{
			success = false;
			if (_queueClient is QueueClient && _queueClient.Exists())
			{
				Azure.Response result = _queueClient.ClearMessages();
				success = !result.IsError;
			}
		}

		/// <summary>
		/// Permanently removes the first message dequeued.
		/// </summary>

		public MessageQueueResult DeleteMessage(out bool success)
		{
			success=false;
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			if (_queueClient is QueueClient && _queueClient.Exists())
			{
				Azure.Response<QueueMessage> receivedMessage = _queueClient.ReceiveMessage();

				if ((receivedMessage != null) && (!receivedMessage.GetRawResponse().IsError) && (receivedMessage.Value != null))
				{
					Azure.Response deleteResult = _queueClient.DeleteMessage(receivedMessage.Value.MessageId, receivedMessage.Value.PopReceipt);

					success = !deleteResult.IsError;
					if (success)
					{
						return (AzQueueMessageToMessageQueueResult(receivedMessage.Value, MessageQueueResultStatus.Deleted));
					}
				}
			}
			return messageQueueResult;
		}

		/// <summary>
		/// Deletes permanently the messages given on the list.
		/// </summary>
		
		public IList<MessageQueueResult> DeleteMessages(List<string> messageHandleId, MessageQueueOptions messageQueueOptions,out bool success)
		{
			success = false;
			IList<MessageQueueResult> messageQueueResults = new List<MessageQueueResult>();
			if (_queueClient is QueueClient && _queueClient.Exists())
			{
				QueueProperties properties = _queueClient.GetProperties();

				QueueMessage[] receivedMessages = _queueClient.ReceiveMessages(messageQueueOptions.MaxNumberOfMessages);
				Azure.Response deleteResult;
				foreach (QueueMessage message in receivedMessages)
				{
					if (messageHandleId.Contains(message.MessageId) )
					{ 
						deleteResult = _queueClient.DeleteMessage(message?.MessageId, message?.PopReceipt);
						if ((deleteResult != null) && (!deleteResult.IsError) && message is QueueMessage)
							messageQueueResults.Add(AzQueueMessageToMessageQueueResult(queueMessage: message, status: MessageQueueResultStatus.Deleted));
					}
				}
				success = true;
			}
			return messageQueueResults;
		}

		/// <summary>
		/// Retrieves all the messages from the queue.
		/// </summary>
		public IList<SimpleQueueMessage> GetMessages(out bool success)
		{
			success = false;
			IList<SimpleQueueMessage> simpleQueueMessages = new List<SimpleQueueMessage>();
			if (_queueClient is QueueClient && _queueClient.Exists())
			{
				Azure.Response<QueueMessage[]> messageResponse;
				
				messageResponse = _queueClient.ReceiveMessages();
				
				if (messageResponse.Value != null)
				{
					foreach (QueueMessage message in messageResponse.Value)
					{
						simpleQueueMessages.Add(AzQueueMessageToSimpleQueueMessage(message));

					}
				}
				success = true;
			}
			return simpleQueueMessages;
		}

		/// <summary>
		/// Retrieves mesages from the queue, given some options.
		/// </summary>
		public IList<SimpleQueueMessage> GetMessages(MessageQueueOptions messageQueueOptions, out bool success)
		{
			success = false;
			bool deleteSuccess = true;
			IList<SimpleQueueMessage> simpleQueueMessages = new List<SimpleQueueMessage>();
			if (_queueClient is QueueClient && _queueClient.Exists())
			{
				Azure.Response<QueueMessage[]> messageResponse;
				if (messageQueueOptions.MaxNumberOfMessages != 0)

					messageResponse = _queueClient.ReceiveMessages(messageQueueOptions.MaxNumberOfMessages, TimeSpan.FromSeconds(messageQueueOptions.VisibilityTimeout));
				else
					messageResponse = _queueClient.ReceiveMessages(visibilityTimeout: TimeSpan.FromSeconds(messageQueueOptions.VisibilityTimeout));

				if (messageResponse.Value != null)
				{
					foreach (QueueMessage message in messageResponse.Value)
					{
						simpleQueueMessages.Add(AzQueueMessageToSimpleQueueMessage(message));
						if (messageQueueOptions.DeleteConsumedMessages)
						{
							Azure.Response deleteResponse = _queueClient.DeleteMessage(message.MessageId, message.PopReceipt);
							if (deleteResponse != null && deleteResponse.IsError)
								deleteSuccess = false;
						}
					}
				}
				success = deleteSuccess;
			}
			return simpleQueueMessages;
		}

		public MessageQueueResult SendMessage(SimpleQueueMessage simpleQueueMessage, MessageQueueOptions messageQueueOptions, out bool success)
		{
			success = false;
			Azure.Response<SendReceipt> sendReceipt;
			MessageQueueResult queueMessageResult = new MessageQueueResult();
			
			if (_queueClient is QueueClient && _queueClient.Exists())
			{
				sendReceipt = _queueClient.SendMessage(simpleQueueMessage.MessageBody, TimeSpan.FromSeconds(messageQueueOptions.VisibilityTimeout), TimeSpan.FromSeconds(messageQueueOptions.TimetoLive));
				if ((sendReceipt != null) && (sendReceipt.Value != null))
				{
					MessageQueueResult result = new MessageQueueResult()
					{
						MessageId = simpleQueueMessage.MessageId,
						ServerMessageId = sendReceipt.Value.MessageId,
						MessageStatus = MessageQueueResultStatus.Sent,
						MessageAttributes = new GXProperties()

					};
					Type t = sendReceipt.GetType();
					PropertyInfo[] props = t.GetProperties();
					foreach (PropertyInfo prop in props)
					{
						object value;
						if (prop.GetIndexParameters().Length == 0 && sendReceipt != null)
						{
							value = prop.GetValue(sendReceipt);
							if (value != null)
								result.MessageAttributes.Add(prop.Name, value.ToString());
						}
					}
					success = true;
					return result;
				}
			}
			return queueMessageResult;

		}
		public MessageQueueResult SendMessage(SimpleQueueMessage simpleQueueMessage, out bool success)
		{
			success = false;
			Azure.Response<SendReceipt> sendReceipt;
			MessageQueueResult queueMessageResult = new MessageQueueResult();
			if (_queueClient is QueueClient &&  _queueClient.Exists())
			{
				sendReceipt =  _queueClient.SendMessage(simpleQueueMessage.MessageBody);
				if ((sendReceipt != null) && (sendReceipt.Value != null))
				{
					MessageQueueResult result = new MessageQueueResult()
					{
						MessageId = simpleQueueMessage.MessageId,
						ServerMessageId = sendReceipt.Value.MessageId,
						MessageStatus = MessageQueueResultStatus.Sent,
						MessageAttributes = new GXProperties()

					};
					Type t = sendReceipt.Value.GetType();
					PropertyInfo[] props = t.GetProperties();
					foreach (PropertyInfo prop in props)
					{
						object value;
						if (prop.GetIndexParameters().Length == 0 && sendReceipt.Value != null)
						{
							value = prop.GetValue(sendReceipt.Value);
							if (value != null)
								result.MessageAttributes.Add(prop.Name, value.ToString());
						}
					}
					success = true;
					return result;
				}	
			}
			return queueMessageResult;

		}

		/// <summary>
		/// Send messages using Queue options 
		/// </summary>
		
		public IList<MessageQueueResult> SendMessages(IList<SimpleQueueMessage> simpleQueueMessages, MessageQueueOptions messageQueueOptions, out bool success)
		{
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			IList<MessageQueueResult> messageQueueResults = new List<MessageQueueResult>();

			bool sendError = false;
			bool successSend = false;
			foreach (SimpleQueueMessage simpleQueueMessage in simpleQueueMessages)
			{
				messageQueueResult = SendMessage(simpleQueueMessage, messageQueueOptions, out successSend);
				if (successSend)
					messageQueueResults.Add(messageQueueResult);
				else
				{
					sendError = true;
				}
			}
			success = !sendError;
			return messageQueueResults;
		}

		private MessageQueueResult AzQueueMessageToMessageQueueResult(QueueMessage queueMessage, string status)
		{
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			if (queueMessage != null)
			{ 
				messageQueueResult.MessageId = queueMessage.MessageId;
				messageQueueResult.ServerMessageId = queueMessage.MessageId;
				messageQueueResult.MessageStatus = status;
				messageQueueResult.MessageAttributes = new GXProperties();


				Type t = queueMessage.GetType();
				PropertyInfo[] props = t.GetProperties();
				foreach (PropertyInfo prop in props)
				{
					object value;
					if (prop.GetIndexParameters().Length == 0 && queueMessage != null)
					{
						value = prop.GetValue(queueMessage);
						if (value != null)
							messageQueueResult.MessageAttributes.Add(prop.Name, value.ToString());
					}
				}		
			}
			return messageQueueResult;
		}

		private SimpleQueueMessage AzPeekedMessageToSimpleQueueMessage(PeekedMessage peekedMessage)
		{
			SimpleQueueMessage simpleQueueMessage = new SimpleQueueMessage();
			if (peekedMessage != null)
			{
				simpleQueueMessage.MessageId = peekedMessage.MessageId;
				simpleQueueMessage.MessageBody = peekedMessage.Body.ToString();
				simpleQueueMessage.MessageAttributes = new GXProperties();
		
				Type t = peekedMessage.GetType();
				PropertyInfo[] props = t.GetProperties();
				foreach (PropertyInfo prop in props)
				{
					object value;
					if (prop.GetIndexParameters().Length == 0 && peekedMessage != null)
					{
						value = prop.GetValue(peekedMessage);
						if (value != null)
							simpleQueueMessage.MessageAttributes.Add(prop.Name, value.ToString());
					}
				}
			}
			return simpleQueueMessage;
		}

		private SimpleQueueMessage AzQueueMessageToSimpleQueueMessage(QueueMessage queueMessage)
		{
			SimpleQueueMessage simpleQueueMessage = new SimpleQueueMessage();
			if (queueMessage != null)
			{
				simpleQueueMessage.MessageId = queueMessage.MessageId;
				simpleQueueMessage.MessageBody = queueMessage.Body.ToString();
				simpleQueueMessage.MessageAttributes = new GXProperties();

				Type t = queueMessage.GetType();
				PropertyInfo[] props = t.GetProperties();
				foreach (PropertyInfo prop in props)
				{
					object value;
					if (prop.GetIndexParameters().Length == 0 && queueMessage != null)
					{
						value = prop.GetValue(queueMessage);
						if (value != null)
							simpleQueueMessage.MessageAttributes.Add(prop.Name, value.ToString());
					}
				}
			}
			return simpleQueueMessage;
		}
		public override string GetName()
		{
			return Name;
		}
	}

}