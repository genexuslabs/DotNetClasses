using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus;
using GeneXus.Messaging.Common;
using GeneXus.Services;
using GeneXus.Utils;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Messaging.Common
{
	public class SimpleMessageQueue
	{
		internal IQueue queue = null;


		public SimpleMessageQueue()
		{
			//queue = ServiceFactory.GetQueue();
		}
		public SimpleMessageQueue(SimpleMessageQueue other)
		{
			queue = other.queue;
		}
		void ValidQueue()
		{
			if (queue == null)
				throw new Exception("Queue not found");
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
		public MessageQueueResult DeleteMessage(out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				ValidQueue();
				messageQueueResult = queue.DeleteMessage(out success);

			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}
			return messageQueueResult;
		}

		public IList<MessageQueueResult> DeleteMessages(List<string> messageHandleId, MessageQueueOptions messageQueueOptions, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			IList<MessageQueueResult> messageQueueResults = new List<MessageQueueResult>();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				ValidQueue();
				messageQueueResults = queue.DeleteMessages(messageHandleId, messageQueueOptions, out success);

			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}
			return messageQueueResults;
		}

		public IList<SimpleQueueMessage> GetMessages(out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			IList<SimpleQueueMessage> simpleQueueMessages = new List<SimpleQueueMessage>();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				ValidQueue();
				simpleQueueMessages = queue.GetMessages(out success);

			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}
			return simpleQueueMessages = queue.GetMessages(out success);
		}

		public IList<SimpleQueueMessage> GetMessages(MessageQueueOptions messageQueueOptions, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			IList<SimpleQueueMessage> simpleQueueMessages = new List<SimpleQueueMessage>();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				ValidQueue();
				simpleQueueMessages = queue.GetMessages(messageQueueOptions, out success);

			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}
			return simpleQueueMessages = queue.GetMessages(out success);
		}

		public MessageQueueResult SendMessage(SimpleQueueMessage simpleQueueMessage, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			MessageQueueResult messageQueueResult = new MessageQueueResult();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				ValidQueue();
				messageQueueResult = queue.SendMessage(simpleQueueMessage, out success);

			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}
			return messageQueueResult;

		}

		public IList<MessageQueueResult> SendMessages(IList<SimpleQueueMessage> simpleQueueMessages, MessageQueueOptions messageQueueOptions, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			IList<MessageQueueResult> messageQueueResults = new List<MessageQueueResult>();
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				ValidQueue();
				messageQueueResults = queue.SendMessages(simpleQueueMessages, messageQueueOptions, out success);

			}
			catch (Exception ex)
			{
				QueueErrorMessagesSetup(ex, out errorMessages);
				success = false;
			}
			return messageQueueResults;
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
					GXUtil.ErrorToMessages("Queue Error", ex, errorMessages);
				}
			}
		}
	}
	public class ServiceFactory
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

	

