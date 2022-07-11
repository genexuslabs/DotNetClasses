using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using GeneXus.Messaging.Common;
using GeneXus.Services;
using GeneXus.Utils;
using log4net;

namespace GeneXus.Messaging.GXAzureServiceBus
{
	public class AzureServiceBus : MessageBrokerBase, IMessageBroker
	{

		public static String Name = "AZURESB";
		ServiceBusClient _serviceBusClient { get; set; }
		private string _queueOrTopicName { get; set; }
		private string _connectionString { get; set; }
		private ServiceBusSender _sender { get; set; }
		
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(AzureServiceBus));

		public AzureServiceBus() : this(null)
		{
		}

		public AzureServiceBus(GXService providerService) : base(providerService)
		{
			Initialize(providerService);
		}

		public void Dispose()
		{
			Task task = Task.Run(async () => await ServiceClientDisposeAsync());
		}
		private async Task ServiceClientDisposeAsync()
		{
			await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
		}

		private async Task RemoveMessageAsync(ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			ServiceBusReceiver receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName);
			await receiver.CompleteMessageAsync(serviceBusReceivedMessage).ConfigureAwait(false);
			await receiver.DisposeAsync().ConfigureAwait(false);
		}

		private void Initialize(GXService providerService)
		{
			ServiceSettings serviceSettings = new(PropertyConstants.MESSAGE_BROKER, Name, providerService);
			_queueOrTopicName = serviceSettings.GetEncryptedPropertyValue(PropertyConstants.QUEUE_NAME);
			_connectionString = serviceSettings.GetEncryptedPropertyValue(PropertyConstants.QUEUE_CONNECTION_STRING);

			//TO DO Consider connection options here
			try
			{
				_serviceBusClient = new ServiceBusClient(_connectionString);
				if (_serviceBusClient != null)
				{ 	
					_sender = _serviceBusClient.CreateSender(_queueOrTopicName);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex.Message);
			}		
		}

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

		public override string GetName()
		{
			return Name;
		}

		public bool SendMessage(BrokerMessage brokerMessage)
		{
			bool success = false;
			ServiceBusMessage serviceBusMessage = BrokerMessageToServiceBusMessage(brokerMessage);
			try
			{
				Task<bool> task;
				if (_sender != null)
				{
					task = Task.Run<bool>(async () => await sendAsync(serviceBusMessage));
					success = task.Result;
				}
				else
				{
					throw new Exception("There was an error at the Message Broker initialization.");
				}
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return success;
		}

		private async Task<bool> sendAsync(ServiceBusMessage serviceBusMessage)
		{
			try
			{ 
				await _sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
				return true;
			}
			catch (Exception ex)
			{
				return false;
				throw ex;
			}
		}
		bool IMessageBroker.SendMessages(IList<BrokerMessage> brokerMessages, BrokerMessageOptions messageQueueOptions)
		{
			bool success = false;
			try
			{
				Task<bool> task = Task<bool>.Run(async () => await SendMessagesBatchAsync(brokerMessages));
				success = task.Result;
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return success;
		}

		private async Task<bool> SendMessagesBatchAsync(IList<BrokerMessage> brokerMessages)
		{
			bool success = false;
			if (_sender == null)
			{
				throw new Exception("There was an error at the Message Broker initialization.");
			}
			else
			{
				ServiceBusMessage serviceBusMessage;
				IList<ServiceBusMessage> serviceBusMessages = new List<ServiceBusMessage>();
				foreach (BrokerMessage brokerMessage in brokerMessages)
				{
					serviceBusMessage = BrokerMessageToServiceBusMessage(brokerMessage);
					serviceBusMessages.Add(serviceBusMessage);
				}
				try
				{
					await _sender.SendMessagesAsync(serviceBusMessages).ConfigureAwait(false);
					success = true;
				}
				catch (Exception ex)
				{
					GXLogging.Error(logger, ex.Message.ToString());
				}			
			}
			return success;
		}
		IList<BrokerMessage> IMessageBroker.GetMessages(BrokerMessageOptions messageQueueOptions, out bool success)
		{
			IList<BrokerMessage> brokerMessages = new List<BrokerMessage>();
			success = false;
			try
			{
				Task<IReadOnlyList<ServiceBusReceivedMessage>> receivedMessages = Task<IReadOnlyList<ServiceBusReceivedMessage>>.Run(async () => await ReceiveMessagesAsync(messageQueueOptions));
				if (receivedMessages != null && receivedMessages.Result != null)
				{
					foreach (ServiceBusReceivedMessage serviceBusReceivedMessage in receivedMessages.Result)
					{
						brokerMessages.Add(SBReceivedMessageToBrokerMessage(serviceBusReceivedMessage));
					}
					success = true;
				}
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return brokerMessages;
		}

		private async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesAsync(BrokerMessageOptions messageQueueOptions)
		{
			IReadOnlyList<ServiceBusReceivedMessage> receivedMessages;
			try
			{
				ServiceBusReceiverOptions serviceBusReceiverOptions = new ServiceBusReceiverOptions();

				if (messageQueueOptions.ReceiveMode == 1) // Valid values : PeekLock (0), ReceiveAndDelete (1)
					serviceBusReceiverOptions.ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete;

				if (messageQueueOptions.PrefetchCount != 0) 
					serviceBusReceiverOptions.PrefetchCount = messageQueueOptions.PrefetchCount;

				ServiceBusReceiver receiver;

				if (!string.IsNullOrEmpty(messageQueueOptions.SubscriptionName))
				{
					receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, messageQueueOptions.SubscriptionName, serviceBusReceiverOptions);
				}
				else
				{ 
					receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, serviceBusReceiverOptions);
				}

				int maxMessagesReceive = 1;
				TimeSpan maxWaitTimeout = TimeSpan.Zero;
				if (messageQueueOptions != null && messageQueueOptions.WaitTimeout != 0)
					maxWaitTimeout = TimeSpan.FromSeconds(messageQueueOptions.WaitTimeout);

				if (messageQueueOptions != null && messageQueueOptions.MaxNumberOfMessages != 0)
					maxMessagesReceive = messageQueueOptions.MaxNumberOfMessages;

				if (maxWaitTimeout == TimeSpan.Zero)
					receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages: maxMessagesReceive).ConfigureAwait(false);
				else
					receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages: maxMessagesReceive, maxWaitTime: maxWaitTimeout).ConfigureAwait(false);

				await receiver.DisposeAsync().ConfigureAwait(false);

				return receivedMessages;
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex.Message.ToString());
			}
			return null;
		}

		#region Transform Methods
		private ServiceBusMessage BrokerMessageToServiceBusMessage(BrokerMessage brokerMessage)
		{
			ServiceBusMessage serviceBusMessage = new ServiceBusMessage(brokerMessage.MessageBody);
			serviceBusMessage.MessageId = brokerMessage.MessageId;

			GXProperties messageAttributes = brokerMessage.MessageAttributes;
			if (messageAttributes != null)
				LoadMessageProperties(messageAttributes, ref serviceBusMessage);

			return serviceBusMessage;
		}
		private BrokerMessage SBReceivedMessageToBrokerMessage(ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			BrokerMessage brokerMessage = new BrokerMessage();
			brokerMessage.MessageId = serviceBusReceivedMessage.MessageId;
			brokerMessage.MessageBody = serviceBusReceivedMessage.Body.ToString();
		
			LoadReceivedMessageProperties(serviceBusReceivedMessage, ref brokerMessage);
			return brokerMessage;
		}

		private void LoadReceivedMessageProperties(ServiceBusReceivedMessage serviceBusReceivedMessage, ref BrokerMessage brokerMessage)
		{
			GXProperties properties = new GXProperties();

			if (serviceBusReceivedMessage != null)
			{
				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.Subject))
					properties.Add("Subject", serviceBusReceivedMessage.Subject);

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.ReplyToSessionId))
					properties.Add("ReplyToSessionId", serviceBusReceivedMessage.ReplyToSessionId);

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.DeadLetterSource))
					properties.Add("DeadLetterSource", serviceBusReceivedMessage.DeadLetterSource);

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.ContentType))
					properties.Add("ContentType", serviceBusReceivedMessage.ContentType);

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.CorrelationId))
					properties.Add("CorrelationId", serviceBusReceivedMessage.CorrelationId);

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.DeadLetterErrorDescription))
					properties.Add("DeadLetterErrorDescription", serviceBusReceivedMessage.DeadLetterErrorDescription);

				if (string.IsNullOrEmpty(serviceBusReceivedMessage.DeadLetterReason))
					properties.Add("DeadLetterReason", serviceBusReceivedMessage.DeadLetterReason);

				if (serviceBusReceivedMessage.DeliveryCount != 0)
					properties.Add("DeliveryCount", serviceBusReceivedMessage.DeliveryCount.ToString());

				if (serviceBusReceivedMessage.EnqueuedSequenceNumber != 0)
					properties.Add("EnqueuedSequenceNumber", serviceBusReceivedMessage.EnqueuedSequenceNumber.ToString());

				properties.Add("EnqueuedTime", serviceBusReceivedMessage.EnqueuedTime.UtcDateTime.ToString());

				properties.Add("ExpiresAt", serviceBusReceivedMessage.ExpiresAt.UtcDateTime.ToString());
				properties.Add("LockedUntil", serviceBusReceivedMessage.LockedUntil.UtcDateTime.ToString());

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.LockToken))
					properties.Add("LockToken", serviceBusReceivedMessage.LockToken);

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.PartitionKey))
					properties.Add("PartitionKey", serviceBusReceivedMessage.PartitionKey);

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.ReplyTo))
					properties.Add("ReplyTo", serviceBusReceivedMessage.ReplyTo);

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.SessionId))
					properties.Add("SessionId", serviceBusReceivedMessage.SessionId);

				properties.Add("TimeToLive", serviceBusReceivedMessage.TimeToLive.ToString());

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.ReplyToSessionId))
					properties.Add("ReplyToSessionId", serviceBusReceivedMessage.ReplyToSessionId);

				properties.Add("ScheduledEnqueueTime", serviceBusReceivedMessage.ScheduledEnqueueTime.UtcDateTime.ToString());

				if (serviceBusReceivedMessage.SequenceNumber != 0)
					properties.Add("SequenceNumber", serviceBusReceivedMessage.SequenceNumber.ToString());

				properties.Add("State", serviceBusReceivedMessage.State.ToString());

				if (!string.IsNullOrEmpty(serviceBusReceivedMessage.TransactionPartitionKey))
					properties.Add("TransactionPartitionKey", serviceBusReceivedMessage.TransactionPartitionKey);

				//Application Properties
				brokerMessage.MessageAttributes = new GXProperties();
				Type t = serviceBusReceivedMessage.GetType();
				PropertyInfo[] props = t.GetProperties();
				foreach (PropertyInfo prop in props)
				{
					object value;
					if (prop.GetIndexParameters().Length == 0 && serviceBusReceivedMessage != null)
					{
						value = prop.GetValue(serviceBusReceivedMessage);
						if (value != null)
							brokerMessage.MessageAttributes.Add(prop.Name, value.ToString());
					}
				}
			}
			brokerMessage.MessageAttributes = properties;
		}
		private void LoadMessageProperties(GXProperties properties, ref ServiceBusMessage serviceBusMessage)
		{
			if (properties != null)
			{
				GxKeyValuePair messageAttribute = new GxKeyValuePair();
				messageAttribute = properties.GetFirst();
				while (!properties.Eof())
				{
					switch (messageAttribute.Key.ToLower())
					{
						case "timetolive":
							serviceBusMessage.TimeToLive = System.TimeSpan.Parse(messageAttribute.Value);
							break;
						case "to":
							serviceBusMessage.To = messageAttribute.Value;
							break;
						case "subject":
							serviceBusMessage.Subject = messageAttribute.Value;
							break;
						case "partitionkey":
							serviceBusMessage.PartitionKey = messageAttribute.Value;
							break;
						case "transactionpartitionkey":
							serviceBusMessage.TransactionPartitionKey = messageAttribute.Value;
							break;
						case "contenttype":
							serviceBusMessage.ContentType = messageAttribute.Value;
							break;
						case "correlationid":
							serviceBusMessage.CorrelationId = messageAttribute.Value;
							break;
						case "replyto":
							serviceBusMessage.ReplyTo = messageAttribute.Value;
							break;
						case "replytosessionid":
							serviceBusMessage.ReplyToSessionId = messageAttribute.Value;
							break;
						case "sessionid":
							serviceBusMessage.SessionId = messageAttribute.Value;
							break;
						case "scheduledenqueuetime":
							serviceBusMessage.ScheduledEnqueueTime = System.DateTimeOffset.Parse(messageAttribute.Value);
							break;
						default:
							serviceBusMessage.ApplicationProperties.Add(messageAttribute.Key, messageAttribute.Value);
							break;
					}
					messageAttribute = properties.GetNext();
				}
			}
		}

		#endregion
	}
}
