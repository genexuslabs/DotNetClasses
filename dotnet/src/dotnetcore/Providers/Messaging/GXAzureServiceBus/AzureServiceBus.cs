using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
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
		private const int MAX_MESSAGES_DEFAULT = 10;
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(AzureServiceBus));
		public static String Name = "AZURESB";

		private ConcurrentDictionary<string, ServiceBusReceivedMessage> m_messages = new ConcurrentDictionary<string, ServiceBusReceivedMessage>();
		ServiceBusClient _serviceBusClient { get; set; }
		private string _queueOrTopicName { get; set; }
		private string _connectionString { get; set; }
		private string _subscriptionName { get; set; }
		private ServiceBusSender _sender { get; set; }
		private ServiceBusReceiver _receiver { get; set; }
		private ServiceBusSessionReceiver _sessionReceiver { get; set; }
		private ServiceBusSessionReceiverOptions _sessionReceiverOptions { get; set; }
		private bool _sessionEnabled { get; set; }
		private string _sessionId { get; set; }

		public AzureServiceBus() : this(null)
		{
		}
		public AzureServiceBus(GXService providerService) : base(providerService)
		{
			Initialize(providerService);
		}
		private void Initialize(GXService providerService)
		{
			ServiceSettings serviceSettings = new(PropertyConstants.MESSAGE_BROKER, Name, providerService);
			_queueOrTopicName = serviceSettings.GetEncryptedPropertyValue(PropertyConstants.QUEUE_NAME);
			_connectionString = serviceSettings.GetEncryptedPropertyValue(PropertyConstants.QUEUE_CONNECTION_STRING);
			_subscriptionName = serviceSettings.GetEncryptedPropertyValue(PropertyConstants.TOPIC_SUBSCRIPTION);

			_sessionEnabled = Convert.ToBoolean(serviceSettings.GetEncryptedPropertyValue(PropertyConstants.SESSION_ENABLED));

			ServiceBusReceiverOptions serviceBusReceiverOptions = new ServiceBusReceiverOptions();
			_sessionReceiverOptions = new ServiceBusSessionReceiverOptions();

			string receiveMode = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.RECEIVE_MODE);
			string prefetchCount = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.PREFETCH_COUNT);
			string receiverIdentifier = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.RECEIVER_IDENTIFIER); ;
			_sessionId = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.RECEIVER_SESSIONID);
			
			if (!string.IsNullOrEmpty(receiveMode))
			{
				if (_sessionEnabled)
					_sessionReceiverOptions.ReceiveMode = (ServiceBusReceiveMode)Convert.ToInt16(receiveMode);
				else
					serviceBusReceiverOptions.ReceiveMode = (ServiceBusReceiveMode)Convert.ToInt16(receiveMode);
			}
			if (!string.IsNullOrEmpty(prefetchCount))
			{
				int prefetchcnt = Convert.ToInt32(prefetchCount);
				if (prefetchcnt != 0)
				{
					if (_sessionEnabled)
						_sessionReceiverOptions.PrefetchCount = prefetchcnt;
					else
						serviceBusReceiverOptions.PrefetchCount = prefetchcnt;
				}
			}
			if (!string.IsNullOrEmpty(receiverIdentifier))
			{
				if (_sessionEnabled)
					_sessionReceiverOptions.Identifier = receiverIdentifier;
				else
					serviceBusReceiverOptions.Identifier = receiverIdentifier;
			}

			string senderIdentifier = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.SENDER_IDENTIFIER);
		
			ServiceBusSenderOptions serviceBusSenderOptions = new ServiceBusSenderOptions();
			if (!string.IsNullOrEmpty(senderIdentifier))
				serviceBusSenderOptions.Identifier = senderIdentifier;

			//TO DO Consider connection options here
			//https://docs.microsoft.com/en-us/javascript/api/@azure/service-bus/servicebusclientoptions?view=azure-node-latest#@azure-service-bus-servicebusclientoptions-websocketoptions

			try
			{
				_serviceBusClient = new ServiceBusClient(_connectionString);
				if (_serviceBusClient != null)
				{
					_sender = _serviceBusClient.CreateSender(_queueOrTopicName, serviceBusSenderOptions);

					if (_sessionEnabled && _sender != null)
					{
						if (!string.IsNullOrEmpty(_sessionId))
						{
							if (string.IsNullOrEmpty(_subscriptionName))
							{
								Task<ServiceBusSessionReceiver> task;
								task = Task.Run<ServiceBusSessionReceiver>(async () => await _serviceBusClient.AcceptSessionAsync(_queueOrTopicName, _sessionId, _sessionReceiverOptions).ConfigureAwait(false));
								_sessionReceiver = task.Result;
							}
							else
							{
								Task<ServiceBusSessionReceiver> task;
								task = Task.Run<ServiceBusSessionReceiver>(async () => await _serviceBusClient.AcceptSessionAsync(_queueOrTopicName, _subscriptionName, _sessionId, _sessionReceiverOptions).ConfigureAwait(false));
								_sessionReceiver = task.Result;
							}
						}
						else
						{
							if (string.IsNullOrEmpty(_subscriptionName))
							{
								Task<ServiceBusSessionReceiver> task;
								task = Task.Run<ServiceBusSessionReceiver>(async () => await _serviceBusClient.AcceptNextSessionAsync(_queueOrTopicName, _sessionReceiverOptions).ConfigureAwait(false));
								_sessionReceiver = task.Result;
							}
							else
							{
								Task<ServiceBusSessionReceiver> task;
								task = Task.Run<ServiceBusSessionReceiver>(async () => await _serviceBusClient.AcceptNextSessionAsync(_queueOrTopicName, _subscriptionName, _sessionReceiverOptions).ConfigureAwait(false));
								_sessionReceiver = task.Result;
							}
						}
					}
					else
						if (string.IsNullOrEmpty(_subscriptionName))
						_receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, serviceBusReceiverOptions);

					else
						_receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, _subscriptionName, serviceBusReceiverOptions);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex.Message);
			}
		}
		public override string GetName()
		{
			return Name;
		}

		#region Async methods
		private async Task ServiceClientDisposeAsync()
		{
			await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
		}
		private async Task<bool> sendAsync(ServiceBusMessage serviceBusMessage, string options)
		{
			SendMessageOptions sendOptions = JSONHelper.Deserialize<SendMessageOptions>(options);
			if ((sendOptions != null) && (!string.IsNullOrEmpty(sendOptions.ScheduledEnqueueTime)))
			{
				try
				{
					await _sender.ScheduleMessageAsync(serviceBusMessage, DateTimeOffset.Parse(sendOptions.ScheduledEnqueueTime)).ConfigureAwait(false);
					return true;
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
			else
			{
				try
				{
					await _sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
					return true;
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}
		private async Task<bool> SendMessagesBatchAsync(IList<BrokerMessage> brokerMessages, string options)
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

				SendMessageOptions sendOptions = JSONHelper.Deserialize<SendMessageOptions>(options);
				if ((sendOptions != null) && (!string.IsNullOrEmpty(sendOptions.ScheduledEnqueueTime)))
				{
					try
					{
						await _sender.ScheduleMessagesAsync(serviceBusMessages, DateTimeOffset.Parse(sendOptions.ScheduledEnqueueTime)).ConfigureAwait(false);
						success = true;
					}
					catch (Exception ex)
					{
						GXLogging.Error(logger, ex.Message.ToString());
					}
				}
				else
				{
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
			}
			return success;
		}
		private async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesAsync(string options)
		{
			ReceiveMessageOptions receiveOptions = JSONHelper.Deserialize<ReceiveMessageOptions>(options);

			IReadOnlyList<ServiceBusReceivedMessage> receivedMessages;
			try
			{
				ServiceBusReceiverOptions serviceBusReceiverOptions = new ServiceBusReceiverOptions();

				if ((receiveOptions != null) && (!string.IsNullOrEmpty(receiveOptions.SessionId)) && _sessionEnabled && (receiveOptions.SessionId != _sessionId))
				{
					//Create new session receiver

					if (string.IsNullOrEmpty(_subscriptionName))
					{
						_sessionReceiver = await _serviceBusClient.AcceptSessionAsync(_queueOrTopicName, receiveOptions.SessionId, _sessionReceiverOptions).ConfigureAwait(false);
						_sessionId = receiveOptions.SessionId;
					}
					else
					{
						_sessionReceiver = await _serviceBusClient.AcceptSessionAsync(_queueOrTopicName, _subscriptionName, receiveOptions.SessionId, _sessionReceiverOptions).ConfigureAwait(false);
						_sessionId = receiveOptions.SessionId;
					}
				}

				int maxMessagesReceive = MAX_MESSAGES_DEFAULT;
				if ((receiveOptions != null) && (receiveOptions.MaxMessages != 0))
					maxMessagesReceive = receiveOptions.MaxMessages;

				TimeSpan maxWait = TimeSpan.Zero;
				if ((receiveOptions != null) && receiveOptions.MaxWaitTime != 0)
					maxWait = TimeSpan.FromSeconds(receiveOptions.MaxWaitTime);

				ServiceBusReceiver receiver = _receiver;
				if ((_sessionReceiver != null) && (_sessionEnabled))
					receiver = _sessionReceiver;

				if ((receiveOptions != null) && (receiveOptions.ReceiveDeferredSequenceNumbers != null) && (receiveOptions.ReceiveDeferredSequenceNumbers.Count > 0))
				{
					receivedMessages = await receiver.ReceiveDeferredMessagesAsync(receiveOptions.ReceiveDeferredSequenceNumbers).ConfigureAwait(false);
				}
				else
				{
					
				if (maxWait == TimeSpan.Zero)
					if ((receiveOptions != null) && (receiveOptions.PeekReceive != null) && (receiveOptions.PeekReceive.Peek))
						if (receiveOptions.PeekReceive.PeekFromSequenceNumber != 0)
							receivedMessages = await receiver.PeekMessagesAsync(maxMessages: maxMessagesReceive, receiveOptions.PeekReceive.PeekFromSequenceNumber).ConfigureAwait(false);
						else
							receivedMessages = await receiver.PeekMessagesAsync(maxMessages: maxMessagesReceive).ConfigureAwait(false);
					else
						receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages: maxMessagesReceive).ConfigureAwait(false);
				else
					receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages: maxMessagesReceive, maxWaitTime: maxWait).ConfigureAwait(false);
				}
				return receivedMessages;
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex.Message.ToString());
			}
			return null;
		}

		private async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(string options)
		{
			ReceiveMessageOptions receiveOptions = JSONHelper.Deserialize<ReceiveMessageOptions>(options);
			ServiceBusReceivedMessage receivedMessage;

			try
			{
				ServiceBusReceiverOptions serviceBusReceiverOptions = new ServiceBusReceiverOptions();

				if ((receiveOptions != null) && (!string.IsNullOrEmpty(receiveOptions.SessionId)) && _sessionEnabled && (receiveOptions.SessionId != _sessionId))
				{
					//Create new session receiver

					if (string.IsNullOrEmpty(_subscriptionName))
					{
						_sessionReceiver = await _serviceBusClient.AcceptSessionAsync(_queueOrTopicName, receiveOptions.SessionId, _sessionReceiverOptions).ConfigureAwait(false);
						_sessionId = receiveOptions.SessionId;
					}
					else
					{
						_sessionReceiver = await _serviceBusClient.AcceptSessionAsync(_queueOrTopicName, _subscriptionName, receiveOptions.SessionId, _sessionReceiverOptions).ConfigureAwait(false);
						_sessionId = receiveOptions.SessionId;
					}
				}

				TimeSpan maxWait = TimeSpan.Zero;
				if ((receiveOptions != null) && receiveOptions.MaxWaitTime != 0)
					maxWait = TimeSpan.FromSeconds(receiveOptions.MaxWaitTime);

				ServiceBusReceiver receiver = _receiver;
				if ((_sessionReceiver != null) && (_sessionEnabled))
					receiver = _sessionReceiver;

				if ((receiveOptions != null) && (receiveOptions.ReceiveDeferredSequenceNumbers != null) && (receiveOptions.ReceiveDeferredSequenceNumbers.Count > 0))
				{
					receivedMessage = await receiver.ReceiveDeferredMessageAsync(receiveOptions.ReceiveDeferredSequenceNumbers[0]).ConfigureAwait(false);
				}
				else
				{ 
					if (maxWait != TimeSpan.Zero)
						receivedMessage = await receiver.ReceiveMessageAsync(maxWaitTime: maxWait).ConfigureAwait(false);
					else
						if ((receiveOptions != null) && (receiveOptions.PeekReceive != null) && (receiveOptions.PeekReceive.Peek))
							if (receiveOptions.PeekReceive.PeekFromSequenceNumber != 0)
								receivedMessage = await receiver.PeekMessageAsync(receiveOptions.PeekReceive.PeekFromSequenceNumber).ConfigureAwait(false);
							else
							receivedMessage = await receiver.PeekMessageAsync().ConfigureAwait(false);
						else
							receivedMessage = await receiver.ReceiveMessageAsync().ConfigureAwait(false);
				}
				return receivedMessage;
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex.Message.ToString());
			}
			return null;
		}
		#endregion

		#region API Methods
		public bool SendMessage(BrokerMessage brokerMessage, string options)
		{
			bool success = false;
			ServiceBusMessage serviceBusMessage = BrokerMessageToServiceBusMessage(brokerMessage);
			try
			{
				Task<bool> task;
				if (_sender != null)
				{
					task = Task.Run<bool>(async () => await sendAsync(serviceBusMessage, options));
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
		bool IMessageBroker.SendMessages(IList<BrokerMessage> brokerMessages, string options)
		{
			bool success = false;
			try
			{
				Task<bool> task = Task<bool>.Run(async () => await SendMessagesBatchAsync(brokerMessages, options));
				success = task.Result;
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return success;
		}
		IList<BrokerMessage> IMessageBroker.GetMessages(string options, out bool success)
		{
			IList<BrokerMessage> brokerMessages = new List<BrokerMessage>();
			success = false;
			try
			{
				Task<IReadOnlyList<ServiceBusReceivedMessage>> receivedMessages = Task<IReadOnlyList<ServiceBusReceivedMessage>>.Run(async () => await ReceiveMessagesAsync(options));
				if (receivedMessages != null && receivedMessages.Result != null)
				{
					foreach (ServiceBusReceivedMessage serviceBusReceivedMessage in receivedMessages.Result)
					{
						if (serviceBusReceivedMessage != null)
							if (AddOrUpdateStoredServiceReceivedMessage(serviceBusReceivedMessage))
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

		public bool ConsumeMessage(BrokerMessage brokerMessage, string options)
		{
			ConsumeMessageOptions consumeOptions = JSONHelper.Deserialize<ConsumeMessageOptions>(options);
			if (consumeOptions != null)
			{
				ServiceBusReceiver receiver = _receiver;
				if ((_sessionReceiver != null) && (_sessionEnabled))
					receiver = _sessionReceiver;

				ServiceBusReceivedMessage serviceBusReceviedMessage = GetStoredServiceBusReceivedMessage(brokerMessage);
				if (serviceBusReceviedMessage != null)
				{
					try
					{
						Task task;
						switch (consumeOptions.ConsumeMode)
						{
							case ConsumeMessageOptions.ConsumeModeOpts.Complete:
								{
									task = Task.Run(async () => await receiver.CompleteMessageAsync(serviceBusReceviedMessage).ConfigureAwait(false));
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
									break;
								}
							case ConsumeMessageOptions.ConsumeModeOpts.Abandon:
								{
									task = Task.Run(async () => await receiver.AbandonMessageAsync(serviceBusReceviedMessage).ConfigureAwait(false));
									break;
								}
							case ConsumeMessageOptions.ConsumeModeOpts.DeadLetter:
								{
									task = Task.Run(async () => await receiver.DeadLetterMessageAsync(serviceBusReceviedMessage).ConfigureAwait(false));
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
									break;
								}
							case ConsumeMessageOptions.ConsumeModeOpts.Defer:
								{
									task = Task.Run(async () => await receiver.DeferMessageAsync(serviceBusReceviedMessage).ConfigureAwait(false));
									break;
								}
							case ConsumeMessageOptions.ConsumeModeOpts.RenewMessageLock:
								{
									task = Task.Run(async () => await receiver.RenewMessageLockAsync(serviceBusReceviedMessage).ConfigureAwait(false));
									break;
								}
						}
						return true;
					}
					catch (AggregateException ae)
					{
						throw ae;
					}
				}
			}
			return false;
		}

		public BrokerMessage GetMessage(string options, out bool success)
		{
			BrokerMessage brokerMessage = new BrokerMessage();
			success = false;
			try
			{
				Task<ServiceBusReceivedMessage> receivedMessage = Task<ServiceBusReceivedMessage>.Run(async () => await ReceiveMessageAsync(options));
				if (receivedMessage != null && receivedMessage.Result != null)
				{
					ServiceBusReceivedMessage serviceBusReceivedMessage = receivedMessage.Result;
					if (AddOrUpdateStoredServiceReceivedMessage(serviceBusReceivedMessage))
					{ 
						success = true;
						return (SBReceivedMessageToBrokerMessage(serviceBusReceivedMessage));
					}
				}
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return brokerMessage;
		}
		public void Dispose()
		{
			Task task = Task.Run(async () => await ServiceClientDisposeAsync().ConfigureAwait(false));
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
		#endregion

		#region Transformation Methods
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

		#endregion

		#region Data
		[DataContract]
		internal class SendMessageOptions
		{
			[DataMember] 
			internal string ScheduledEnqueueTime { get; set; }
		}

		[DataContract()]
		internal class ReceiveMessageOptions
		{
			int _maxmessages;
			int _maxwaittime;
			string _sessionid;
			PeekReceiveOpts _peekreceiveopts;
			IList<long> _receivedeferredsequencenumbers;

			[DataMember()]
			internal int MaxMessages { get => _maxmessages; set => _maxmessages = value; }

			[DataMember()]
			internal int MaxWaitTime { get => _maxwaittime; set => _maxwaittime = value; }

			[DataMember()]
			internal string SessionId { get => _sessionid; set => _sessionid = value ; }

			[DataMember()]
			internal PeekReceiveOpts PeekReceive { get => _peekreceiveopts; set => _peekreceiveopts = value; }

			[DataMember()]
			internal IList<long> ReceiveDeferredSequenceNumbers { get => _receivedeferredsequencenumbers; set => _receivedeferredsequencenumbers = value; }
		}

		[DataContract()]
		public class PeekReceiveOpts
		{
			bool _peek;
			long _peekfromsequencenumber;

			[DataMember()]
			internal bool Peek { get => _peek; set => _peek = value; }

			[DataMember()]
			internal long PeekFromSequenceNumber { get => _peekfromsequencenumber ; set => _peekfromsequencenumber = value; }
		}

		[DataContract]
		internal class ConsumeMessageOptions
		{
			[DataMember]
			internal ConsumeModeOpts ConsumeMode { get; set; }
			internal enum ConsumeModeOpts
			{
				Complete,
				Abandon,
				DeadLetter,
				Defer,
				RenewMessageLock
			}
		}

		#endregion

		#region helper methods

		private ServiceBusReceivedMessage GetStoredServiceBusReceivedMessage(BrokerMessage message)
		{
			string messageIdentifier = GetMessageIdentifier(message);
			if (m_messages.TryGetValue(messageIdentifier, out ServiceBusReceivedMessage serviceBusReceivedMessage))
				return serviceBusReceivedMessage;
			else
				return null;
		}
		private void RemoveStoredServiceBusReceivedMessage(BrokerMessage message)
		{
			string messageIdentifier = GetMessageIdentifier(message);
			lock (m_messages)
			{ 
				if (m_messages.TryGetValue(messageIdentifier, out ServiceBusReceivedMessage serviceBusReceivedMessage))
				{
					KeyValuePair<string, ServiceBusReceivedMessage> keyValuePair = new KeyValuePair<string, ServiceBusReceivedMessage>(messageIdentifier, serviceBusReceivedMessage);
					m_messages.TryRemove(keyValuePair);
				}
			}
		}
		private bool AddOrUpdateStoredServiceReceivedMessage(ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			string messageIdentifier = GetMessageIdentifierFromServiceBus(serviceBusReceivedMessage);
			if (!string.IsNullOrEmpty(messageIdentifier))
				lock (m_messages)
				{ 
					if (m_messages.TryGetValue(messageIdentifier, out ServiceBusReceivedMessage originalMessage))
					{
						return (m_messages.TryUpdate(messageIdentifier, serviceBusReceivedMessage, originalMessage));			
					}
					else
						return m_messages.TryAdd(messageIdentifier, serviceBusReceivedMessage);
				}
			return false;
		}

		private string GetMessageIdentifier(BrokerMessage message)
		{
			//The sequence number is a unique 64-bit integer assigned to a message as it is accepted and stored by the broker and functions as its true identifier.
			//For partitioned entities, the sequence number is issued relative to the partition.
			//https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing
			//Follow this to identify the message
			//https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-partitioning#using-a-partition-key


			string messageSequenceNumber = GetMessageSequenceNumber(message);
			string messageIdentifier = string.Empty;

			//Get SessionId of the message
			string messageSessionId = GetMessageSessionId(message);
			if (!string.IsNullOrEmpty(messageSessionId))
				messageIdentifier = $"{messageSequenceNumber}{messageSessionId}";		
			else
			{
				//Get PartitionKey of the message
				string messagePartitionKey = GetMessagePartitionKey(message);
				if (!string.IsNullOrEmpty(messagePartitionKey))
					messageIdentifier = $"{messageSequenceNumber}{messagePartitionKey}";
				else
					messageIdentifier = $"{messageSequenceNumber}{message.MessageId}";
			}
			return messageIdentifier.GetHashCode().ToString();
		}

		private string GetMessageIdentifierFromServiceBus(ServiceBusReceivedMessage message)
		{		
			string messageSequenceNumber = message.SequenceNumber.ToString();
			string messageIdentifier = string.Empty;
			//Get SessionId of the message
			string messageSessionId = message.SessionId;
			if (!string.IsNullOrEmpty(messageSessionId))
				messageIdentifier = $"{messageSequenceNumber}{messageSessionId}";
			else
			{
				//Get PartitionKey of the message
				string messagePartitionKey = message.PartitionKey;
				if (!string.IsNullOrEmpty(messagePartitionKey))
					messageIdentifier = $"{messageSequenceNumber}{messagePartitionKey}";
				else
					messageIdentifier = $"{messageSequenceNumber}{message.MessageId}";
			}
			return messageIdentifier.GetHashCode().ToString();
		}
		private string GetMessageSequenceNumber(BrokerMessage message)
		{
			string sequenceNumberValue = string.Empty;
			if (message != null)
			{
				if (message.MessageAttributes.ContainsKey("SequenceNumber"))
					sequenceNumberValue = message.MessageAttributes.Get("SequenceNumber");
			}
			return sequenceNumberValue;
		}
		private string GetMessageSessionId(BrokerMessage message)
		{
			string messageSessionId = string.Empty;
			if (message != null)
			{
				if (message.MessageAttributes.ContainsKey("SessionId"))
					messageSessionId = message.MessageAttributes.Get("SessionId");
			}
			return messageSessionId;
		}
		private string GetMessagePartitionKey(BrokerMessage message)
		{
			string messagePartitionKey = string.Empty;
			if (message != null)
			{
				if (message.MessageAttributes.ContainsKey("PartitionKey"))
					messagePartitionKey = message.MessageAttributes.Get("PartitionKey");
			}
			return messagePartitionKey;
		}
		private void LoadReceivedMessageProperties(ServiceBusReceivedMessage serviceBusReceivedMessage, ref BrokerMessage brokerMessage)
		{
			GXProperties properties = new GXProperties();

			if (serviceBusReceivedMessage != null)
			{
				brokerMessage.MessageAttributes = new GXProperties();
				Type t = serviceBusReceivedMessage.GetType();
				PropertyInfo[] props = t.GetProperties();
				foreach (PropertyInfo prop in props)
				{
					object value;
					if (prop.Name != "ApplicationProperties")
					{
						if (prop.GetIndexParameters().Length == 0 && serviceBusReceivedMessage != null)
						{
							value = prop.GetValue(serviceBusReceivedMessage);

							if (value != null)
								brokerMessage.MessageAttributes.Add(prop.Name, value.ToString());
						}
					}
				}
				//Application Properties
				foreach (KeyValuePair<string, Object> o in serviceBusReceivedMessage.ApplicationProperties)
				{
					brokerMessage.MessageAttributes.Add(o.Key, o.Value.ToString());
				}
			}

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



