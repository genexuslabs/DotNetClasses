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
		private const short LOCK_DURATION = 5;
		public static String Name = "AZURESB";

		private ConcurrentDictionary<string, Tuple<DateTime, ServiceBusReceivedMessage>> m_messages = new ConcurrentDictionary<string, Tuple<DateTime,ServiceBusReceivedMessage>>();
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
		private string receiveMode { get; set; }
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

			receiveMode = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.RECEIVE_MODE);
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
						//These methods throw an exception when the service bus is empty:
						//ServiceBusException: The operation did not complete within the allocated time (ServiceTimeout) 
						//so I remove them for now
						/*
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
						}*/
						throw new Exception("ServiceBus: Specify a session to establish a service bus receiver for the session-enabled queue or topic.");
					}
				}
				else
					if (string.IsNullOrEmpty(_subscriptionName))
					_receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, serviceBusReceiverOptions);

				else
					_receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, _subscriptionName, serviceBusReceiverOptions);
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

		private async Task<bool> CancelScheduleAsync(long sequenceNumber)
		{
			try
			{
				await _sender.CancelScheduledMessageAsync(sequenceNumber).ConfigureAwait(false);
				return true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		private async Task<long> ScheduleMessageAsync(ServiceBusMessage serviceBusMessage, string options)
		{
			ScheduleMessageOptions scheduleOptions = JSONHelper.Deserialize<ScheduleMessageOptions>(options);
			if ((serviceBusMessage != null) && (scheduleOptions != null) && (!string.IsNullOrEmpty(scheduleOptions.ScheduledEnqueueTime)))
			{
				try
				{
					return (await _sender.ScheduleMessageAsync(serviceBusMessage, DateTime.Parse(scheduleOptions.ScheduledEnqueueTime)).ConfigureAwait(false));
	
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
			return 0;
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
				try
				{ 
					await _sender.SendMessagesAsync(serviceBusMessages).ConfigureAwait(false);
					success = true;
				}
				catch (Exception ex)
				{
					throw ex;
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
				throw ex;
			}
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
				throw ex;
			}
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
					ClearServiceBusAuxiliaryStorage();
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
				ClearServiceBusAuxiliaryStorage();
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
					ClearServiceBusAuxiliaryStorage();
					foreach (ServiceBusReceivedMessage serviceBusReceivedMessage in receivedMessages.Result)
					{
						if (serviceBusReceivedMessage != null)
							brokerMessages.Add(SBReceivedMessageToBrokerMessage(serviceBusReceivedMessage));

							//If receive Mode = Peek Lock, save the messages to be retrieved later
							if (!string.IsNullOrEmpty(receiveMode) && (Convert.ToInt16(receiveMode) == 0))
							{ 
								if (!AddOrUpdateStoredServiceReceivedMessage(serviceBusReceivedMessage))
								{
								throw new Exception("Invalid operation.");
								}
							}
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

				ClearServiceBusAuxiliaryStorage();
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
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
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
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
									break;
								}
							case ConsumeMessageOptions.ConsumeModeOpts.RenewMessageLock:
								{
									task = Task.Run(async () => await receiver.RenewMessageLockAsync(serviceBusReceviedMessage).ConfigureAwait(false));
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
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
				else
				{
					throw new Exception("Invalid operation.");
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
				ClearServiceBusAuxiliaryStorage();
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
			m_messages.Clear();
		}
		public long ScheduleMessage(BrokerMessage brokerMessage, string options)
		{
			long sequenceNumber = 0;
			ServiceBusMessage serviceBusMessage = BrokerMessageToServiceBusMessage(brokerMessage);
			try
			{
				Task<long> task;
				if (_sender != null)
				{
					task = Task.Run<long>(async () => await ScheduleMessageAsync(serviceBusMessage, options));
					sequenceNumber = task.Result;
					ClearServiceBusAuxiliaryStorage();
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
			return sequenceNumber;
		}
		public bool CancelSchedule(long sequenceNumber)
		{
			bool success = false;
			try
			{
				Task<bool> task;
				if (_sender != null)
				{
					task = Task.Run<bool>(async () => await CancelScheduleAsync(sequenceNumber));
					success = task.Result;
					ClearServiceBusAuxiliaryStorage();
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
			return false;
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
			if (brokerMessage != null)
			{ 
				ServiceBusMessage serviceBusMessage = new ServiceBusMessage(brokerMessage.MessageBody);
				serviceBusMessage.MessageId = brokerMessage.MessageId;

				GXProperties messageAttributes = brokerMessage.MessageAttributes;
				if (messageAttributes != null)
					LoadMessageProperties(messageAttributes, ref serviceBusMessage);

				return serviceBusMessage;
			}
			return null;
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
		[DataContract()]
		internal class ScheduleMessageOptions
		{
			long _cancelSequenceNumber;
			string _scheduledEnqueueTime;

			[DataMember()] 
			internal string ScheduledEnqueueTime { get => _scheduledEnqueueTime; set => _scheduledEnqueueTime = value; }

			[DataMember()]
			internal long CancelSequenceNumber { get => _cancelSequenceNumber; set => _cancelSequenceNumber = value; }
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

		#region Helper methods

		private ServiceBusReceivedMessage GetStoredServiceBusReceivedMessage(BrokerMessage message)
		{
			string messageIdentifier = GetMessageIdentifier(message);
			if (m_messages.TryGetValue(messageIdentifier, out Tuple<DateTime, ServiceBusReceivedMessage> messageStored))
			{ 
				return messageStored.Item2;
			}
			else
				return null;
		}
		private void RemoveStoredServiceBusReceivedMessage(BrokerMessage message)
		{
			string messageIdentifier = GetMessageIdentifier(message);
			m_messages.TryRemove(messageIdentifier, out _);
		}
		private bool AddOrUpdateStoredServiceReceivedMessage(ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			string messageIdentifier = GetMessageIdentifierFromServiceBus(serviceBusReceivedMessage);
			if (!string.IsNullOrEmpty(messageIdentifier))
			{
				Tuple<DateTime, ServiceBusReceivedMessage> messageStored = new Tuple<DateTime, ServiceBusReceivedMessage>(DateTime.UtcNow, serviceBusReceivedMessage);
				m_messages[messageIdentifier] = messageStored;
				return true;
			}
			return false;
		}

		private void ClearServiceBusAuxiliaryStorage()
		{
			//Clear all messages older than 5 minutes
			//When a consumer locks a message, the broker temporarily hides it from other consumers (LockDuration).
			//However, the lock on the message has a timeout, which is 5 mins maximum

			foreach (KeyValuePair<string, Tuple<DateTime, ServiceBusReceivedMessage>> entry in m_messages)
			{
				if (entry.Value.Item1.AddMinutes(LOCK_DURATION) < DateTime.UtcNow)
				{
					m_messages.TryRemove(entry.Key, out _);
				}
			}
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
				messageIdentifier = $"{messageSequenceNumber}_{messageSessionId}";		
			else
			{
				//Get PartitionKey of the message
				string messagePartitionKey = GetMessagePartitionKey(message);
				if (!string.IsNullOrEmpty(messagePartitionKey))
					messageIdentifier = $"{messageSequenceNumber}_{messagePartitionKey}";
				else
					messageIdentifier = $"{messageSequenceNumber}_{message.MessageId}";
			}
			return messageIdentifier;
		}

		private string GetMessageIdentifierFromServiceBus(ServiceBusReceivedMessage message)
		{		
			string messageSequenceNumber = message.SequenceNumber.ToString();
			string messageIdentifier = string.Empty;
			//Get SessionId of the message
			string messageSessionId = message.SessionId;
			if (!string.IsNullOrEmpty(messageSessionId))
				messageIdentifier = $"{messageSequenceNumber}_{messageSessionId}";
			else
			{
				//Get PartitionKey of the message
				string messagePartitionKey = message.PartitionKey;
				if (!string.IsNullOrEmpty(messagePartitionKey))
					messageIdentifier = $"{messageSequenceNumber}_{messagePartitionKey}";
				else
					messageIdentifier = $"{messageSequenceNumber}_{message.MessageId}";
			}
			return messageIdentifier;
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



