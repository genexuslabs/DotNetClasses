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
	public class SimpleMessageQueue
	{
		internal IQueue queue = null;
		public static Assembly assembly;
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(SimpleMessageQueue));
		private const string SDT_MESSAGE_CLASS_NAME = @"SdtMessage";
		private const string SDT_MESSAGEPROPERTY_CLASS_NAME = @"SdtMessageProperty";
		private const string SDT_MESSAGERESULT_CLASS_NAME = @"SdtMessageResult";
		private const string NAMESPACE = @"GeneXus.Programs.genexusmessagingqueue.simplequeue";
		private const string MODULE_DLL = @"GeneXusMessagingQueue";
		
		public SimpleMessageQueue()
		{
		}
		public SimpleMessageQueue(SimpleMessageQueue other)
		{
			queue = other.queue;
		}
		void ValidQueue()
		{
			if (queue == null)
			{
				GXLogging.Error(logger, "Queue was not instantiated.");
				throw new Exception("Queue was not instantiated.");
			}
		}

		public void Clear(out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				ValidQueue();
				queue.Clear(out success);
			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}
		}

		public int GetQueueLength(out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			int queueLength = 0;
			try
			{
				ValidQueue();
				queueLength = queue.GetQueueLength(out success);
			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}

			return queueLength;
		}
		public GxUserType DeleteMessage(GxUserType simpleQueueMessage, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			success = false;
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				ValidQueue();	
				messageQueueResult = queue.DeleteMessage(TransformGXUserTypeToSimpleQueueMessage(simpleQueueMessage), out success);
				LoadAssemblyIfRequired();
				try
				{
					if (messageQueueResult != null && TransformMessageQueueResult(messageQueueResult) is GxUserType result)
					{
						success = true; 
						return result;
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(logger, ex);
					throw ex;
				}
			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				GXLogging.Error(logger, ex);
			}
			return TransformMessageQueueResult(messageQueueResult);
		}

		public IList<GxUserType> DeleteMessages(IList simpleQueueMessages, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			IList<MessageQueueResult> messageQueueResults = new List<MessageQueueResult>();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			IList<GxUserType> messageResults = new List<GxUserType>();
			success = false;

			IList<SimpleQueueMessage> simpleQueueMessagesList = new List<SimpleQueueMessage>();
			foreach (GxUserType simpleQueueMessage in simpleQueueMessages)
			{
				if (TransformGXUserTypeToSimpleQueueMessage(simpleQueueMessage) is SimpleQueueMessage queueMessage)
					simpleQueueMessagesList.Add(queueMessage);
			}
			try
			{
				try
				{
					ValidQueue();
					messageQueueResults = queue.DeleteMessages(simpleQueueMessagesList, out success);
					LoadAssemblyIfRequired();
					foreach (MessageQueueResult messageResult in messageQueueResults)
					{
						if (TransformMessageQueueResult(messageResult) is GxUserType result)
							messageResults.Add(result);
					}
					success = true;
				}
				catch (Exception ex)
				{
					GXLogging.Error(logger, ex);
					QueueErrorMessagesSetup(ex, out errorMessages);
					success = false;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex);
				success = false;
				throw ex;
			}
			
			return messageResults;
		}

		public IList<GxUserType> GetMessages(out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			IList<GxUserType> resultMessages = new List<GxUserType>();
			success = false;
			try
			{
				ValidQueue();
				IList<SimpleQueueMessage> simpleQueueMessages = queue.GetMessages(out success);

				try
				{
					LoadAssemblyIfRequired();

					foreach (SimpleQueueMessage simpleQueueMessage in simpleQueueMessages)
					{
						if (TransformSimpleQueueMessage(simpleQueueMessage) is GxUserType result)
							resultMessages.Add(result);
					}
					success = true;
				}
				catch (Exception ex)
				{
					GXLogging.Error(logger, ex);
					success = false;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex);
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}
			return resultMessages;
		}

		public IList<GxUserType> GetMessages(GxUserType messageQueueOptions, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			IList<GxUserType> resultMessages = new List<GxUserType>();
			success = false;
			try
			{
				MessageQueueOptions options = TransformOptions(messageQueueOptions);

				try
				{
					ValidQueue();
					IList<SimpleQueueMessage>  simpleQueueMessages = queue.GetMessages(options, out success);
					LoadAssemblyIfRequired();
					foreach (SimpleQueueMessage simpleQueueMessage in simpleQueueMessages)
					{
						if (TransformSimpleQueueMessage(simpleQueueMessage) is GxUserType result)
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

		private static void LoadAssemblyIfRequired()
		{
			if (assembly == null)
			{
				assembly = AssemblyLoader.LoadAssembly(new AssemblyName(MODULE_DLL));
			}
		}

		private MessageQueueOptions TransformOptions(GxUserType messageQueueOptions)
		{
			MessageQueueOptions options = new MessageQueueOptions();
			options.MaxNumberOfMessages = messageQueueOptions.GetPropertyValue<short>("Maxnumberofmessages");
			options.DeleteConsumedMessages = messageQueueOptions.GetPropertyValue<bool>("Deleteconsumedmessages");
			options.WaitTimeout = messageQueueOptions.GetPropertyValue<int>("Waittimeout");
			options.VisibilityTimeout = messageQueueOptions.GetPropertyValue<int>("Visibilitytimeout");
			options.TimetoLive = messageQueueOptions.GetPropertyValue<int>("Timetolive");
			return options;
		}

		private SimpleQueueMessage TransformGXUserTypeToSimpleQueueMessage(GxUserType simpleQueueMessage)
		{
			SimpleQueueMessage queueMessage = new SimpleQueueMessage();
			queueMessage.MessageId = simpleQueueMessage.GetPropertyValue<string>("Messageid");
			queueMessage.MessageBody = simpleQueueMessage.GetPropertyValue<string>("Messagebody");
			queueMessage.MessageHandleId = simpleQueueMessage.GetPropertyValue<string>("Messagehandleid");
			IList messageAttributes = simpleQueueMessage.GetPropertyValue<IList>("Messageattributes_GXBaseCollection");
			queueMessage.MessageAttributes = new GXProperties();
			foreach (GxUserType messageAttribute in messageAttributes)
			{
				string messagePropKey = messageAttribute.GetPropertyValue<string>("Propertykey");
				string messagePropValue = messageAttribute.GetPropertyValue<string>("Propertyvalue");
				queueMessage.MessageAttributes.Add(messagePropKey, messagePropValue);
			}
			return queueMessage;
		}
		private GxUserType TransformSimpleQueueMessage(SimpleQueueMessage simpleQueueMessage)
		{
			Type classType = assembly.GetType(NAMESPACE + "." + SDT_MESSAGE_CLASS_NAME, false, ignoreCase: true);
			Type propertyClassType = assembly.GetType(NAMESPACE + "." + SDT_MESSAGEPROPERTY_CLASS_NAME, false, ignoreCase: true);

			if (classType != null && Activator.CreateInstance(classType) is GxUserType simpleMessageSDT)
			{
				simpleMessageSDT.SetPropertyValue("Messageid", simpleQueueMessage.MessageId);
				simpleMessageSDT.SetPropertyValue("Messagebody", simpleQueueMessage.MessageBody);
				simpleMessageSDT.SetPropertyValue("Messagehandleid", simpleQueueMessage.MessageHandleId);
				
				IList messageResultSDTAttributes = (IList)Activator.CreateInstance(classType.GetProperty("gxTpr_Messageattributes").PropertyType, new object[] { simpleMessageSDT.context, "MessageProperty", string.Empty });

				if ((simpleQueueMessage != null) && (simpleQueueMessage.MessageAttributes != null))
				{ 
					GxKeyValuePair prop = simpleQueueMessage.MessageAttributes.GetFirst();
					while (!simpleQueueMessage.MessageAttributes.Eof())
					{
						if (propertyClassType != null && Activator.CreateInstance(propertyClassType) is GxUserType propertyClassTypeSDT)
						{
							propertyClassTypeSDT.SetPropertyValue("Propertykey", prop.Key);
							propertyClassTypeSDT.SetPropertyValue("Propertyvalue", prop.Value);
							messageResultSDTAttributes.Add(propertyClassTypeSDT);
							prop = simpleQueueMessage.MessageAttributes.GetNext();
						}
					}
					simpleMessageSDT.SetPropertyValue("Messageattributes", messageResultSDTAttributes);
				}
				return simpleMessageSDT;
			}
			return null;
		}
		private GxUserType TransformMessageQueueResult(MessageQueueResult messageQueueResult)
		{
			Type classType = assembly.GetType(NAMESPACE + "." + SDT_MESSAGERESULT_CLASS_NAME, false, ignoreCase: true);
			Type propertyClassType = assembly.GetType(NAMESPACE + "." + SDT_MESSAGEPROPERTY_CLASS_NAME, false, ignoreCase: true);

			if (classType != null && Activator.CreateInstance(classType) is GxUserType messageResultSDT)
			{
				messageResultSDT.SetPropertyValue("Messageid", messageQueueResult.MessageId);
				messageResultSDT.SetPropertyValue("Servermessageid", messageQueueResult.ServerMessageId);
				messageResultSDT.SetPropertyValue("Messagehandleid", messageQueueResult.MessageHandleId);
				messageResultSDT.SetPropertyValue("Messagestatus", messageQueueResult.MessageStatus);

				IList messageResultSDTAttributes = (IList)Activator.CreateInstance(classType.GetProperty("gxTpr_Messageattributes").PropertyType, new object[] { messageResultSDT.context, "MessageProperty", string.Empty });
				GxKeyValuePair prop;
				if ((messageQueueResult != null) && (messageQueueResult.MessageAttributes != null))
				{ 
					prop = messageQueueResult.MessageAttributes.GetFirst();
					while (!messageQueueResult.MessageAttributes.Eof())
					{
						if (propertyClassType != null && Activator.CreateInstance(propertyClassType) is GxUserType propertyClassTypeSDT)
						{
							propertyClassTypeSDT.SetPropertyValue("Propertykey", prop.Key);
							propertyClassTypeSDT.SetPropertyValue("Propertyvalue", prop.Value);

							messageResultSDTAttributes.Add(propertyClassTypeSDT);
							prop = messageQueueResult.MessageAttributes.GetNext();
						}
					}
					messageResultSDT.SetPropertyValue("Messageattributes", messageResultSDTAttributes);
				}
				return messageResultSDT;
			}
			return null;
		}
		public GxUserType SendMessage(GxUserType simpleQueueMessage, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			success = false;
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			GxUserType result = new GxUserType();
			try
			{
				SimpleQueueMessage queueMessage = TransformGXUserTypeToSimpleQueueMessage(simpleQueueMessage);
				LoadAssemblyIfRequired();
				try
				{
					ValidQueue();
					messageQueueResult = queue.SendMessage(queueMessage, out success);

					if (TransformMessageQueueResult(messageQueueResult) is GxUserType messageResult)
						return messageResult;
					success = true;
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

			return TransformMessageQueueResult(messageQueueResult);
		}

		public IList<GxUserType> SendMessages(IList simpleQueueMessages, GxUserType messageQueueOptions, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{	
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			List<GxUserType> messageResults = new List<GxUserType>();
			try
			{
				// Load Message Queue Options//
				MessageQueueOptions options = TransformOptions(messageQueueOptions);
				
				IList<SimpleQueueMessage> simpleQueueMessagesList = new List<SimpleQueueMessage>();	
				foreach (GxUserType simpleQueueMessage in simpleQueueMessages)
				{
					if (TransformGXUserTypeToSimpleQueueMessage(simpleQueueMessage) is SimpleQueueMessage queueMessage)		
						simpleQueueMessagesList.Add(queueMessage);
				}
				try
				{
					ValidQueue();
					IList<MessageQueueResult> messageQueueResults = queue.SendMessages(simpleQueueMessagesList, options, out success);
					LoadAssemblyIfRequired();
					foreach (MessageQueueResult messageResult in messageQueueResults)
					{
						if (TransformMessageQueueResult(messageResult) is GxUserType result)
							messageResults.Add(result);
					}
					success = true;
					
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
			return messageResults;
		}

		protected void QueueErrorMessagesSetup(Exception ex, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			if (errorMessages != null && ex != null)
			{
				SdtMessages_Message msg = new SdtMessages_Message();
				if (queue != null && queue.GetMessageFromException(ex, msg))
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
					GXLogging.Error(logger, "(GXQueue1002)Queue Error", ex);
					GXUtil.ErrorToMessages("GXQueue1002", ex, errorMessages);
				}
			}
		}
	}
	internal class ServiceFactory
	{
		private static IQueue queue;
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Services.ServiceFactory));

		public static GXServices GetGXServices()
		{
			return GXServices.Instance;
		}

		public static IQueue GetQueue()
		{
			if (queue == null)
			{
				queue = GetQueueImpl(GXServices.QUEUE_SERVICE);
			}
			return queue;
		}

		public static IQueue GetQueueImpl(string service)
		{
			IQueue queueImpl = null;
			if (GetGXServices() != null)
			{
				GXService providerService = GetGXServices().Get(service);
				if (providerService != null)
				{
					try
					{
						string typeFullName = providerService.ClassName;
						GXLogging.Debug(log, "Loading Queue settings:", typeFullName);
#if !NETCORE
						if (!string.IsNullOrEmpty(typeFullName))
							Type type = Type.GetType(typeFullName, true, true);
#else
						Type type = AssemblyLoader.GetType(typeFullName);
#endif
						queueImpl = (IQueue)Activator.CreateInstance(type);
					}
					catch (Exception e)
					{
						GXLogging.Error(log, "Couldn't connect to the Queue.", e.Message, e);
						throw e;
					}
				}
			}
			return queueImpl;
		}
	}
}

	

