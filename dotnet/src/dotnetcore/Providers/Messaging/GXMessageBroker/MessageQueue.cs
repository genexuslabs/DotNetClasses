using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using GeneXus.Application;
using GeneXus.Services;
using GeneXus.Utils;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Messaging.Common
{
	public class MessageQueue
	{
		internal IMessageBroker messageBroker = null;
		public static Assembly assembly;
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(MessageQueue));
		private const string SDT_MESSAGE_CLASS_NAME = @"SdtMessage";
		private const string SDT_MESSAGEPROPERTY_CLASS_NAME = @"SdtMessageProperty";
		//private const string SDT_MESSAGERESULT_CLASS_NAME = @"SdtMessageResult";
		private const string NAMESPACE = @"GeneXus.Programs.genexusmessagingmessagebroker";
		private const string GENEXUS_COMMON_DLL = @"GeneXus.Programs.Common.dll";

		public MessageQueue()
		{
		}
		public MessageQueue(MessageQueue other)
		{
			messageBroker = other.messageBroker;
		}
		void ValidQueue()
		{
			if (messageBroker == null)
			{
				GXLogging.Error(logger, "Message Broker was not instantiated.");
				throw new Exception("Message Broker was not instantiated.");
			}
		}
		private static Assembly LoadAssembly(string fileName)
		{
			if (File.Exists(fileName))
			{
				Assembly assemblyLoaded = Assembly.LoadFrom(fileName);
				return assemblyLoaded;
			}
			else
				return null;
		}

		private static void LoadAssemblyIfRequired()
		{
			if (assembly == null)
			{
				assembly = LoadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), GENEXUS_COMMON_DLL));
				if (assembly == null)
					assembly = LoadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), "bin", GENEXUS_COMMON_DLL));
			}
		}

		public void Dispose()
		{
			try
			{
				ValidQueue();
				messageBroker.Dispose();
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex);
				
			}
		}

		public bool ConsumeMessage(GxUserType messageQueue, string options, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{
			bool success = false;
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			GxUserType result = new GxUserType();
			try
			{
				BrokerMessage brokerQueueMessage = TransformGXUserTypeToBrokerMessage(messageQueue);
				LoadAssemblyIfRequired();
				try
				{
					ValidQueue();
					return (messageBroker.ConsumeMessage(brokerQueueMessage, options));
				}
				catch (Exception ex)
				{
					QueueErrorMessagesSetup(ex, out errorMessages);
					success = false;
					GXLogging.Error(logger, ex);
				}
			}
			catch (Exception ex)
			{
				success = false;
				GXLogging.Error(logger, ex);
				throw ex;
			}
			return success;
		}
		public GxUserType GetMessage(string options, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			GxUserType receivedMessage = new GxUserType();
			success = false;
			try
			{
				try
				{
					ValidQueue();
					BrokerMessage brokerMessage = messageBroker.GetMessage(options, out success);
					LoadAssemblyIfRequired();
					
					if (TransformBrokerMessage(brokerMessage) is GxUserType result)
					{
						success = true;
						return result;
					}
				}
				catch (Exception ex)
				{
					QueueErrorMessagesSetup(ex, out errorMessages);
					GXLogging.Error(logger, ex);
					success = false;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex);
				success = false;
				throw ex;
			}
			return receivedMessage;
		}
		public IList<GxUserType> GetMessages(string options, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			IList<GxUserType> resultMessages = new List<GxUserType>();
			success = false;
			try
			{
				try
				{
					ValidQueue();
					IList<BrokerMessage> brokerMessages = messageBroker.GetMessages(options, out success);
					LoadAssemblyIfRequired();
					foreach (BrokerMessage brokerMessage in brokerMessages)
					{
						if (TransformBrokerMessage(brokerMessage) is GxUserType result)
							resultMessages.Add(result);
					}
					success = true;

				}
				catch (Exception ex)
				{
					QueueErrorMessagesSetup(ex, out errorMessages);
					GXLogging.Error(logger, ex);
					success = false;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex);
				success = false;
				throw ex;
			}

			return resultMessages;
		}
		public bool SendMessage(GxUserType messageQueue, string options, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{
			bool success = false;
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			GxUserType result = new GxUserType();
			try
			{
				BrokerMessage brokerQueueMessage = TransformGXUserTypeToBrokerMessage(messageQueue);
				LoadAssemblyIfRequired();
				try
				{
					ValidQueue();
					return(messageBroker.SendMessage(brokerQueueMessage, options));
				}
				catch (Exception ex)
				{
					QueueErrorMessagesSetup(ex, out errorMessages);
					success = false;
					GXLogging.Error(logger, ex);
				}
			}
			catch (Exception ex)
			{
				success = false;
				GXLogging.Error(logger,ex);
				throw ex;
			}
			return success;
		}

		public bool SendMessages(IList queueMessages, string options, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{	
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			bool success = false;
			try
			{
				IList<BrokerMessage> brokerMessagesList = new List<BrokerMessage>();	
				foreach (GxUserType queueMessage in queueMessages)
				{
					if (TransformGXUserTypeToBrokerMessage(queueMessage) is BrokerMessage brokerMessage)
						brokerMessagesList.Add(brokerMessage);
				}
				try
				{
					ValidQueue();
					success = messageBroker.SendMessages(brokerMessagesList, options);
					LoadAssemblyIfRequired();
					
				}
				catch (Exception ex)
				{
					QueueErrorMessagesSetup(ex, out errorMessages);
					success = false;
					GXLogging.Error(logger, ex);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex);
				throw ex;
			}
			return success;
		}

		protected void QueueErrorMessagesSetup(Exception ex, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			if (errorMessages != null && ex != null)
			{
				SdtMessages_Message msg = new SdtMessages_Message();
				if (messageBroker != null && messageBroker.GetMessageFromException(ex, msg))
				{
					msg.gxTpr_Type = 1;
					StringBuilder str = new StringBuilder();
					str.Append(ex.Message);
					while (ex.InnerException != null)
					{
						str.Append(ex.InnerException.Message);
						ex = ex.InnerException;
					}
					msg.gxTpr_Description = str.ToString();
					errorMessages.Add(msg);
				}
				else
				{
					GXLogging.Error(logger, "(GXServiceBus1002)Queue Error", ex);
					GXUtil.ErrorToMessages("GXServiceBus1002", ex, errorMessages);
				}
			}
		}
		#region Transform operations

		private GxUserType TransformBrokerMessage(BrokerMessage brokerMessage)
		{
			Type classType = assembly.GetType(NAMESPACE + "." + SDT_MESSAGE_CLASS_NAME, false, ignoreCase: true);
			Type propertyClassType = assembly.GetType(NAMESPACE + "." + SDT_MESSAGEPROPERTY_CLASS_NAME, false, ignoreCase: true);

			if (classType != null && Activator.CreateInstance(classType) is GxUserType messageSDT)
			{
				messageSDT.SetPropertyValue("Messageid", brokerMessage.MessageId);
				messageSDT.SetPropertyValue("Messagebody", brokerMessage.MessageBody);
				messageSDT.SetPropertyValue("Messagehandleid", brokerMessage.MessageHandleId);

				IList messageResultSDTAttributes = (IList)Activator.CreateInstance(classType.GetProperty("gxTpr_Messageattributes").PropertyType, new object[] { messageSDT.context, "MessageProperty", string.Empty });

				if ((brokerMessage != null) && (brokerMessage.MessageAttributes != null))
				{
					GxKeyValuePair prop = brokerMessage.MessageAttributes.GetFirst();
					while (!brokerMessage.MessageAttributes.Eof())
					{
						if (propertyClassType != null && Activator.CreateInstance(propertyClassType) is GxUserType propertyClassTypeSDT)
						{
							propertyClassTypeSDT.SetPropertyValue("Propertykey", prop.Key);
							propertyClassTypeSDT.SetPropertyValue("Propertyvalue", prop.Value);
							messageResultSDTAttributes.Add(propertyClassTypeSDT);
							prop = brokerMessage.MessageAttributes.GetNext();
						}
					}
					messageSDT.SetPropertyValue("Messageattributes", messageResultSDTAttributes);
				}
				return messageSDT;
			}
			return null;
		}
		private BrokerMessageOptions TransformOptions(GxUserType messageQueueOptions)
		{
			BrokerMessageOptions options = new BrokerMessageOptions();
			
			options.MaxNumberOfMessages = messageQueueOptions.GetPropertyValue<short>("Maxnumberofmessages");
			options.DeleteConsumedMessages = messageQueueOptions.GetPropertyValue<bool>("Deleteconsumedmessages");
			options.WaitTimeout = messageQueueOptions.GetPropertyValue<int>("Waittimeout");
			options.VisibilityTimeout = messageQueueOptions.GetPropertyValue<int>("Visibilitytimeout");
			options.TimetoLive = messageQueueOptions.GetPropertyValue<int>("Timetolive");
			options.DelaySeconds = messageQueueOptions.GetPropertyValue<int>("Delayseconds");
			options.ReceiveMode = messageQueueOptions.GetPropertyValue<short>("Receivemode");
			options.ReceiveRequestAttemptId = messageQueueOptions.GetPropertyValue<string>("Receiverequestattemptid");
			options.ReceiveMessageAttributes = messageQueueOptions.GetPropertyValue<bool>("Receivemessageattributes");
			options.PrefetchCount = messageQueueOptions.GetPropertyValue<short>("Prefetchcount");
			options.SubscriptionName = messageQueueOptions.GetPropertyValue<string>("Subscriptionname");
;			return options;
		}

		private BrokerMessage TransformGXUserTypeToBrokerMessage(GxUserType queueMessage)
		{
			BrokerMessage brokerQueueMessage = new BrokerMessage();
			brokerQueueMessage.MessageId = queueMessage.GetPropertyValue<string>("Messageid");
			brokerQueueMessage.MessageBody = queueMessage.GetPropertyValue<string>("Messagebody");
			brokerQueueMessage.MessageHandleId = queueMessage.GetPropertyValue<string>("Messagehandleid");
			IList messageAttributes = queueMessage.GetPropertyValue<IList>("Messageattributes_GXBaseCollection");
			brokerQueueMessage.MessageAttributes = new GXProperties();
			foreach (GxUserType messageAttribute in messageAttributes)
			{
				string messagePropKey = messageAttribute.GetPropertyValue<string>("Propertykey");
				string messagePropValue = messageAttribute.GetPropertyValue<string>("Propertyvalue");
				brokerQueueMessage.MessageAttributes.Add(messagePropKey, messagePropValue);
			}
			return brokerQueueMessage;
		}
		#endregion

	}
	internal class ServiceFactory
	{
		private static IMessageBroker messageBroker;
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Services.ServiceFactory));

		public static GXServices GetGXServices()
		{
			return GXServices.Instance;
		}

		public static IMessageBroker GetMessageBroker()
		{
			if (messageBroker == null)
			{
				messageBroker = GetMessageBrokerImpl(GXServices.MESSAGEBROKER_SERVICE);
			}
			return messageBroker;
		}

		public static IMessageBroker GetMessageBrokerImpl(string service)
		{
			IMessageBroker messageBrokerImpl = null;
			if (GetGXServices() != null)
			{
				GXService providerService = GetGXServices().Get(service);
				if (providerService != null)
				{
					try
					{
						string typeFullName = providerService.ClassName;
						GXLogging.Debug(log, "Loading Message Broker settings:", typeFullName);
						Type type = AssemblyLoader.GetType(typeFullName);
						messageBrokerImpl = (IMessageBroker)Activator.CreateInstance(type);
					}
					catch (Exception e)
					{
						GXLogging.Error(log, "Couldn't connect to the Message Broker.", e.Message, e);
						throw e;
					}
				}
			}
			return messageBrokerImpl;
		}
	}

}

	

