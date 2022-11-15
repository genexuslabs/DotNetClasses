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

namespace GeneXus.Messaging.GXAzureServiceBus
{
	public class AzureServiceBus : MessageBrokerBase, IMessageBroker
	{
		private const int MAX_MESSAGES_DEFAULT = 10;
		private const short LOCK_DURATION = 5;
		public static String Name = "AZURESB";

		private ConcurrentDictionary<string, Tuple<DateTime, ServiceBusReceivedMessage>> m_messages = new ConcurrentDictionary<string, Tuple<DateTime, ServiceBusReceivedMessage>>();
		ServiceBusClient _serviceBusClient { get; set; }
		private string _queueOrTopicName { get; set; }
		private string _connectionString { get; set; }
		private string _subscriptionName { get; set; }
		private ServiceBusSender _sender { get; set; }
		private ServiceBusReceiver _receiver { get; set; }
		private ServiceBusSessionReceiver _sessionReceiver { get; set; }
		private ServiceBusReceiverOptions _serviceBusReceiverOptions { get; set; }
		private bool _sessionEnabled { get; set; }
		ServiceBusReceiveMode _sessionEnabledQueueReceiveMode { get; set; }
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
				if (!_sessionEnabled && _sender != null)
				{
					_serviceBusReceiverOptions = new ServiceBusReceiverOptions();

					string receiveMode = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.RECEIVE_MODE);
					string prefetchCount = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.PREFETCH_COUNT);
					string receiverIdentifier = serviceSettings.GetEncryptedOptPropertyValue(PropertyConstants.RECEIVER_IDENTIFIER);

					if (!string.IsNullOrEmpty(receiveMode))
						_serviceBusReceiverOptions.ReceiveMode = (ServiceBusReceiveMode)Convert.ToInt16(receiveMode);

					if (!string.IsNullOrEmpty(prefetchCount))
					{
						int prefetchcnt = Convert.ToInt32(prefetchCount);
						if (prefetchcnt != 0)
							_serviceBusReceiverOptions.PrefetchCount = prefetchcnt;

					}
					if (!string.IsNullOrEmpty(receiverIdentifier))
						_serviceBusReceiverOptions.Identifier = receiverIdentifier;
					else
						_serviceBusReceiverOptions.Identifier = String.Empty;

					if (string.IsNullOrEmpty(_subscriptionName))
						_receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, _serviceBusReceiverOptions);
					else
						_receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, _subscriptionName, _serviceBusReceiverOptions);
				}
			}
		}
		public override string GetName()
		{
			return Name;
		}

		#region Async methods

		private async Task CreateReceiverAsync()
		{
			//Release resources of previous receiver
			if (_receiver != null)
				await _receiver.CloseAsync().ConfigureAwait(false);

			if (string.IsNullOrEmpty(_subscriptionName))
				_receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, _serviceBusReceiverOptions);
			else
				_receiver = _serviceBusClient.CreateReceiver(_queueOrTopicName, _subscriptionName, _serviceBusReceiverOptions);
		}
		private async Task<ServiceBusSessionReceiver> AcceptSessionAsync(string sessionId, ServiceBusSessionReceiverOptions sessionReceiverOptions)
		{
			if (_sessionEnabled && (!string.IsNullOrEmpty(sessionId)))
			{
				//Create new session receiver
				ServiceBusSessionReceiver sessionReceiver;
				if (string.IsNullOrEmpty(_subscriptionName))
					sessionReceiver = await _serviceBusClient.AcceptSessionAsync(_queueOrTopicName, sessionId, sessionReceiverOptions).ConfigureAwait(false);
				else
					sessionReceiver = await _serviceBusClient.AcceptSessionAsync(_queueOrTopicName, _subscriptionName, sessionId, sessionReceiverOptions).ConfigureAwait(false);
				return sessionReceiver;
			}
			return null;
		}
		private async Task ServiceClientDisposeAsync()
		{
			await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
			if (_receiver != null)
				await _receiver.DisposeAsync().ConfigureAwait(false);
			await _sender.DisposeAsync().ConfigureAwait(false);
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
		private async Task InitializeReceiversForReceiveMthAsync(BrokerReceiverOpts brokerReceiverOptions)
		{

			if (_sessionEnabled && (brokerReceiverOptions == null))
			{
				throw new Exception("SessionId cannot be null for session-enabled queue or topic.");
			}
			else if (_sessionEnabled && (brokerReceiverOptions != null))
			{
				ServiceBusSessionReceiverOptions serviceBusSessionReceiverOptions = BrokerRecOptsToServiceBusSessionRecOpts(brokerReceiverOptions);

				if ((serviceBusSessionReceiverOptions != null) && (!string.IsNullOrEmpty(brokerReceiverOptions.SessionId)))
				{
					_sessionEnabledQueueReceiveMode = serviceBusSessionReceiverOptions.ReceiveMode;
					//Store receiver in case that the message has to be settled
					if (_sessionEnabledQueueReceiveMode == ServiceBusReceiveMode.PeekLock)
					{
						if (_sessionReceiver != null)
							await _sessionReceiver.CloseAsync().ConfigureAwait(false);
						_sessionReceiver = await AcceptSessionAsync(brokerReceiverOptions.SessionId, serviceBusSessionReceiverOptions).ConfigureAwait(false);
					}
					else
						_sessionReceiver = await AcceptSessionAsync(brokerReceiverOptions.SessionId, serviceBusSessionReceiverOptions).ConfigureAwait(false);
				}
				else
				{
					throw new Exception("SessionId cannot be null for session-enabled queue or topic.");
				}
			}
			else if (!_sessionEnabled && (brokerReceiverOptions != null))
			{
				//Check if a new receiver must be defined using new settings
				if ((_serviceBusReceiverOptions.ReceiveMode != brokerReceiverOptions.ReceiveMode) || (_serviceBusReceiverOptions.Identifier != brokerReceiverOptions.Identifier) || (_serviceBusReceiverOptions.PrefetchCount != brokerReceiverOptions.PrefetchCount))
				{
					_serviceBusReceiverOptions.ReceiveMode = brokerReceiverOptions.ReceiveMode;
					_serviceBusReceiverOptions.Identifier = brokerReceiverOptions.Identifier;
					_serviceBusReceiverOptions.PrefetchCount = brokerReceiverOptions.PrefetchCount;

					await CreateReceiverAsync().ConfigureAwait(false);
				}
			}
		}
		private async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesNotSessionAsync(ReceiveMessageOptions receiveOptions)
		{
			IReadOnlyList<ServiceBusReceivedMessage> receivedMessages;
			int maxMessagesReceive = MAX_MESSAGES_DEFAULT;
			if ((receiveOptions != null) && (receiveOptions.MaxMessages != 0))
				maxMessagesReceive = receiveOptions.MaxMessages;

			TimeSpan maxWait = TimeSpan.Zero;
			if ((receiveOptions != null) && receiveOptions.MaxWaitTime != 0)
				maxWait = TimeSpan.FromSeconds(receiveOptions.MaxWaitTime);

			if (_receiver != null)
			{
				if ((receiveOptions != null) && (receiveOptions.ReceiveDeferredSequenceNumbers != null) && (receiveOptions.ReceiveDeferredSequenceNumbers.Count > 0))
				{
					receivedMessages = await _receiver.ReceiveDeferredMessagesAsync(receiveOptions.ReceiveDeferredSequenceNumbers).ConfigureAwait(false);
				}
				else
				{
					if (maxWait == TimeSpan.Zero)
						if ((receiveOptions != null) && (receiveOptions.PeekReceive != null) && (receiveOptions.PeekReceive.Peek))
							if (receiveOptions.PeekReceive.PeekFromSequenceNumber != 0)
								receivedMessages = await _receiver.PeekMessagesAsync(maxMessages: maxMessagesReceive, receiveOptions.PeekReceive.PeekFromSequenceNumber).ConfigureAwait(false);
							else
								receivedMessages = await _receiver.PeekMessagesAsync(maxMessages: maxMessagesReceive).ConfigureAwait(false);
						else
							receivedMessages = await _receiver.ReceiveMessagesAsync(maxMessages: maxMessagesReceive).ConfigureAwait(false);
					else
						receivedMessages = await _receiver.ReceiveMessagesAsync(maxMessages: maxMessagesReceive, maxWaitTime: maxWait).ConfigureAwait(false);
				}
			}
			else
			{
				throw new Exception("Invalid Operation. No valid receiver defined.");
			}
			return receivedMessages;
		}
		private async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesSessionAsync(ReceiveMessageOptions receiveOptions)
		{
			IReadOnlyList<ServiceBusReceivedMessage> receivedMessages;
			int maxMessagesReceive = MAX_MESSAGES_DEFAULT;
			if ((receiveOptions != null) && (receiveOptions.MaxMessages != 0))
				maxMessagesReceive = receiveOptions.MaxMessages;

			TimeSpan maxWait = TimeSpan.Zero;
			if ((receiveOptions != null) && receiveOptions.MaxWaitTime != 0)
				maxWait = TimeSpan.FromSeconds(receiveOptions.MaxWaitTime);

			if (_sessionReceiver != null)
			{
				if ((receiveOptions != null) && (receiveOptions.ReceiveDeferredSequenceNumbers != null) && (receiveOptions.ReceiveDeferredSequenceNumbers.Count > 0))
				{
					receivedMessages = await _sessionReceiver.ReceiveDeferredMessagesAsync(receiveOptions.ReceiveDeferredSequenceNumbers).ConfigureAwait(false);
				}
				else
				{
					if (maxWait == TimeSpan.Zero)
						if ((receiveOptions != null) && (receiveOptions.PeekReceive != null) && (receiveOptions.PeekReceive.Peek))
							if (receiveOptions.PeekReceive.PeekFromSequenceNumber != 0)
								receivedMessages = await _sessionReceiver.PeekMessagesAsync(maxMessages: maxMessagesReceive, receiveOptions.PeekReceive.PeekFromSequenceNumber).ConfigureAwait(false);
							else
								receivedMessages = await _sessionReceiver.PeekMessagesAsync(maxMessages: maxMessagesReceive).ConfigureAwait(false);
						else
							receivedMessages = await _sessionReceiver.ReceiveMessagesAsync(maxMessages: maxMessagesReceive).ConfigureAwait(false);
					else
						receivedMessages = await _sessionReceiver.ReceiveMessagesAsync(maxMessages: maxMessagesReceive, maxWaitTime: maxWait).ConfigureAwait(false);

					//Release session lock
					if (_sessionEnabledQueueReceiveMode != ServiceBusReceiveMode.PeekLock)
						await _sessionReceiver.CloseAsync().ConfigureAwait(false);
				}
			}
			else
			{
				throw new Exception("Invalid Operation. No valid Session receiver defined.");
			}
			return receivedMessages;
		}
		private async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesAsync(string options)
		{
			ReceiveMessageOptions receiveOptions = JSONHelper.Deserialize<ReceiveMessageOptions>(options);

			IReadOnlyList<ServiceBusReceivedMessage> receivedMessages;
			try
			{
				await InitializeReceiversForReceiveMthAsync(receiveOptions.BrokerReceiverOptions).ConfigureAwait(false);
				if (_sessionEnabled)
					receivedMessages = await ReceiveMessagesSessionAsync(receiveOptions).ConfigureAwait(false);
				else
					receivedMessages = await ReceiveMessagesNotSessionAsync(receiveOptions).ConfigureAwait(false);

				return receivedMessages;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private async Task<ServiceBusReceivedMessage> ReceiveMessageSessionAsync(ReceiveMessageOptions receiveOptions)
		{
			TimeSpan maxWait = TimeSpan.Zero;
			ServiceBusReceivedMessage receivedMessage;
			if (_sessionReceiver != null)
			{
				if ((receiveOptions != null) && receiveOptions.MaxWaitTime != 0)
					maxWait = TimeSpan.FromSeconds(receiveOptions.MaxWaitTime);

				if ((receiveOptions != null) && (receiveOptions.ReceiveDeferredSequenceNumbers != null) && (receiveOptions.ReceiveDeferredSequenceNumbers.Count > 0))
					receivedMessage = await _sessionReceiver.ReceiveDeferredMessageAsync(receiveOptions.ReceiveDeferredSequenceNumbers[0]).ConfigureAwait(false);
				
				else
				{
					if (maxWait != TimeSpan.Zero)
						receivedMessage = await _sessionReceiver.ReceiveMessageAsync(maxWaitTime: maxWait).ConfigureAwait(false);
					else
						if ((receiveOptions != null) && (receiveOptions.PeekReceive != null) && (receiveOptions.PeekReceive.Peek))
						if (receiveOptions.PeekReceive.PeekFromSequenceNumber != 0)
							receivedMessage = await _sessionReceiver.PeekMessageAsync(receiveOptions.PeekReceive.PeekFromSequenceNumber).ConfigureAwait(false);
						else
							receivedMessage = await _sessionReceiver.PeekMessageAsync().ConfigureAwait(false);
					else
						receivedMessage = await _sessionReceiver.ReceiveMessageAsync().ConfigureAwait(false);

					//Release session lock
					if (_sessionEnabledQueueReceiveMode != ServiceBusReceiveMode.PeekLock)
						await _sessionReceiver.CloseAsync().ConfigureAwait(false);
				}
				return receivedMessage;
			}
			else
			{
				throw new Exception("Invalid Operation. No valid Session receiver defined.");
			}
		}

		private async Task<ServiceBusReceivedMessage> ReceiveMessageNotSessionAsync(ReceiveMessageOptions receiveOptions)
		{
			TimeSpan maxWait = TimeSpan.Zero;
			ServiceBusReceivedMessage receivedMessage;
			if (_receiver != null)
			{
				if ((receiveOptions != null) && receiveOptions.MaxWaitTime != 0)
					maxWait = TimeSpan.FromSeconds(receiveOptions.MaxWaitTime);

				if ((receiveOptions != null) && (receiveOptions.ReceiveDeferredSequenceNumbers != null) && (receiveOptions.ReceiveDeferredSequenceNumbers.Count > 0))
				{
					receivedMessage = await _receiver.ReceiveDeferredMessageAsync(receiveOptions.ReceiveDeferredSequenceNumbers[0]).ConfigureAwait(false);
				}
				else
				{
					if (maxWait != TimeSpan.Zero)
						receivedMessage = await _receiver.ReceiveMessageAsync(maxWaitTime: maxWait).ConfigureAwait(false);
					else
						if ((receiveOptions != null) && (receiveOptions.PeekReceive != null) && (receiveOptions.PeekReceive.Peek))
						if (receiveOptions.PeekReceive.PeekFromSequenceNumber != 0)
							receivedMessage = await _receiver.PeekMessageAsync(receiveOptions.PeekReceive.PeekFromSequenceNumber).ConfigureAwait(false);
						else
							receivedMessage = await _receiver.PeekMessageAsync().ConfigureAwait(false);
					else
						receivedMessage = await _receiver.ReceiveMessageAsync().ConfigureAwait(false);

				}
				return receivedMessage;
			}
			else
			{
				throw new Exception("Invalid Operation. No valid receiver defined.");
			}
		}
		private async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(string options)
		{
			ReceiveMessageOptions receiveOptions = JSONHelper.Deserialize<ReceiveMessageOptions>(options);
			try
			{
				 
				await InitializeReceiversForReceiveMthAsync(receiveOptions.BrokerReceiverOptions).ConfigureAwait(false);

				if (_sessionEnabled)
					return await ReceiveMessageSessionAsync(receiveOptions).ConfigureAwait(false);
				else
					return await ReceiveMessageNotSessionAsync(receiveOptions).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		private async Task<bool> CompleteMessageAsync(ServiceBusReceiver receiver, ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			try
			{ 
				await receiver.CompleteMessageAsync(serviceBusReceivedMessage).ConfigureAwait(false);
				return true;
			}
			catch (ServiceBusException sbex)
			{
				throw sbex;
			}
		}
		private async Task<bool> AbandonMessageAsync(ServiceBusReceiver receiver, ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			try
			{
				await receiver.AbandonMessageAsync(serviceBusReceivedMessage).ConfigureAwait(false);
				return true;
			}
			catch (ServiceBusException sbex)
			{
				throw sbex;
			}
		}
		private async Task<bool> DeadLetterMessageAsync(ServiceBusReceiver receiver, ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			try
			{
				await receiver.DeadLetterMessageAsync(serviceBusReceivedMessage).ConfigureAwait(false);
				return true;
			}
			catch (ServiceBusException sbex)
			{
				throw sbex;
			}
		}
		private async Task<bool> DeferMessageAsync(ServiceBusReceiver receiver, ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			try
			{
				await receiver.DeferMessageAsync(serviceBusReceivedMessage).ConfigureAwait(false);
				return true;
			}
			catch (ServiceBusException sbex)
			{
				throw sbex;
			}
		}
		private async Task<bool> RenewMessageLockAsync(ServiceBusReceiver receiver, ServiceBusReceivedMessage serviceBusReceivedMessage)
		{
			try
			{
				await receiver.RenewMessageLockAsync(serviceBusReceivedMessage).ConfigureAwait(false);
				return true;
			}
			catch (ServiceBusException sbex)
			{
				throw sbex;
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
					task = Task.Run<bool>(async () => await sendAsync(serviceBusMessage, options).ConfigureAwait(false));
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
				Task<bool> task = Task<bool>.Run(async () => await SendMessagesBatchAsync(brokerMessages, options).ConfigureAwait(false));
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
				Task<IReadOnlyList<ServiceBusReceivedMessage>> receivedMessages = Task<IReadOnlyList<ServiceBusReceivedMessage>>.Run(async () => await ReceiveMessagesAsync(options).ConfigureAwait(false));
				if (receivedMessages != null && receivedMessages.Result != null)
				{
					ClearServiceBusAuxiliaryStorage();
					foreach (ServiceBusReceivedMessage serviceBusReceivedMessage in receivedMessages.Result)
					{
						if (serviceBusReceivedMessage != null)
							brokerMessages.Add(SBReceivedMessageToBrokerMessage(serviceBusReceivedMessage));

							//If receive Mode = Peek Lock, save the messages to be retrieved later
							if (GetReceiveMode() != null && (GetReceiveMode() == ServiceBusReceiveMode.PeekLock))
							{ 
								if (!AddOrUpdateStoredServiceReceivedMessage(serviceBusReceivedMessage))
								{
								throw new Exception("Invalid operation. Try retrieving the message again.");
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

		/// <summary>
		/// Settling a message
		/// </summary>
		public bool ConsumeMessage(BrokerMessage brokerMessage, string options)
		{
			ConsumeMessageOptions consumeOptions = JSONHelper.Deserialize<ConsumeMessageOptions>(options);
			if (consumeOptions != null)
			{			
				ServiceBusReceiver receiver;
				if (_sessionEnabled && _sessionReceiver != null)
				{
					receiver = _sessionReceiver;
				}
				else
					if (!_sessionEnabled)
						receiver = _receiver;
					else
						throw new Exception("Invalid operation. Try retrieving the message again.");
				
				ClearServiceBusAuxiliaryStorage();
				ServiceBusReceivedMessage serviceBusReceivedMessage = GetStoredServiceBusReceivedMessage(brokerMessage);
				if (serviceBusReceivedMessage != null)
				{
					try
					{
						Task<bool> taskB;
						switch (consumeOptions.ConsumeMode)
						{
							case ConsumeMessageOptions.ConsumeModeOpts.Complete:
								{
									taskB = Task.Run(async () => await CompleteMessageAsync(receiver,serviceBusReceivedMessage).ConfigureAwait(false));
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
									return taskB.Result;
								}
							case ConsumeMessageOptions.ConsumeModeOpts.Abandon:
								{
									taskB = Task.Run(async () => await AbandonMessageAsync(receiver,serviceBusReceivedMessage).ConfigureAwait(false));
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
									return taskB.Result;
								}
							case ConsumeMessageOptions.ConsumeModeOpts.DeadLetter:
								{
									taskB = Task.Run(async () => await DeadLetterMessageAsync(receiver,serviceBusReceivedMessage).ConfigureAwait(false));
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
									return taskB.Result; 
								}
							case ConsumeMessageOptions.ConsumeModeOpts.Defer:
								{
									taskB = Task.Run(async () => await DeferMessageAsync(receiver,serviceBusReceivedMessage).ConfigureAwait(false));
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
									return taskB.Result;
								}
							case ConsumeMessageOptions.ConsumeModeOpts.RenewMessageLock:
								{
									taskB = Task.Run(async () => await RenewMessageLockAsync(receiver,serviceBusReceivedMessage).ConfigureAwait(false));
									RemoveStoredServiceBusReceivedMessage(brokerMessage);
									return taskB.Result;
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
					throw new Exception("Invalid operation. Try retrieving the message again.");
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
				Task<ServiceBusReceivedMessage> receivedMessage = Task<ServiceBusReceivedMessage>.Run(async () => await ReceiveMessageAsync(options).ConfigureAwait(false));
				if (receivedMessage != null && receivedMessage.Result != null)
				{
					ServiceBusReceivedMessage serviceBusReceivedMessage = receivedMessage.Result;
					
					//If receive Mode = Peek Lock, save the message to be settled later
					if (GetReceiveMode() != null && (GetReceiveMode() == ServiceBusReceiveMode.PeekLock))
					{
						if (!AddOrUpdateStoredServiceReceivedMessage(serviceBusReceivedMessage))
							throw new Exception("Invalid operation. Try retrieving the message again.");
					}
					success = true;
					return (SBReceivedMessageToBrokerMessage(serviceBusReceivedMessage));
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
					task = Task.Run<long>(async () => await ScheduleMessageAsync(serviceBusMessage, options).ConfigureAwait(false));
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
					task = Task.Run<bool>(async () => await CancelScheduleAsync(sequenceNumber).ConfigureAwait(false));
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
				ServiceBusException az_ex = (ServiceBusException)ex;
				msg.gxTpr_Id = az_ex.Reason.ToString();
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
		private ServiceBusSessionReceiverOptions BrokerRecOptsToServiceBusSessionRecOpts(BrokerReceiverOpts brokerReceiverOptions)
		{
			ServiceBusSessionReceiverOptions serviceBusSessionReceiverOptions = new ServiceBusSessionReceiverOptions();
			if (brokerReceiverOptions != null)
			{
				serviceBusSessionReceiverOptions.PrefetchCount = brokerReceiverOptions.PrefetchCount;
				serviceBusSessionReceiverOptions.Identifier	= brokerReceiverOptions.Identifier;
				serviceBusSessionReceiverOptions.ReceiveMode = brokerReceiverOptions.ReceiveMode;
				return serviceBusSessionReceiverOptions;
			}
			return null;
			
		}
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

			PeekReceiveOpts _peekreceiveopts;
			IList<long> _receivedeferredsequencenumbers;
			BrokerReceiverOpts _brokerreceiveroptions;

			[DataMember()]
			internal int MaxMessages { get => _maxmessages; set => _maxmessages = value; }

			[DataMember()]
			internal int MaxWaitTime { get => _maxwaittime; set => _maxwaittime = value; }

			[DataMember()]
			internal PeekReceiveOpts PeekReceive { get => _peekreceiveopts; set => _peekreceiveopts = value; }

			[DataMember()]
			internal IList<long> ReceiveDeferredSequenceNumbers { get => _receivedeferredsequencenumbers; set => _receivedeferredsequencenumbers = value; }

			[DataMember()]
			internal BrokerReceiverOpts BrokerReceiverOptions { get => _brokerreceiveroptions; set => _brokerreceiveroptions = value; }
		}

		[DataContract()]
		public class BrokerReceiverOpts
		{
			ServiceBusReceiveMode _receiveMode;
			int _prefetchCount;
			string _identifier;
			string _sessionId;

			[DataMember()]
			internal int PrefetchCount { get => _prefetchCount; set => _prefetchCount = value; }

			[DataMember()]
			internal string Identifier { get => _identifier; set => _identifier = value; }

			[DataMember()]
			internal string SessionId { get => _sessionId; set => _sessionId = value; }

			[DataMember()]
			internal ServiceBusReceiveMode ReceiveMode { get => _receiveMode; set => _receiveMode = value; }

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

			BrokerReceiverOpts _brokerreceiveroptions;

			[DataMember]
			internal ConsumeModeOpts ConsumeMode { get; set; }

			[DataMember()]
			internal BrokerReceiverOpts BrokerReceiverOptions { get => _brokerreceiveroptions; set => _brokerreceiveroptions = value; }

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

		private ServiceBusReceiveMode? GetReceiveMode()
		{
			if (_sessionEnabled)
				return _sessionEnabledQueueReceiveMode;
			else
				if (_serviceBusReceiverOptions != null)
					return _serviceBusReceiverOptions.ReceiveMode;
			return null;
		}
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



