using System;
using System.Collections;
using System.Collections.Generic;
using GeneXus.Utils;

namespace GeneXus.Messaging.Common
{
	public interface IQueue
	{
		public const int MAX_NUMBER_MESSAGES = 10;
		public const bool DELETE_CONSUMED_MESSAGES = false;
		public const int WAIT_TIMEOUT = 10;
		public const int VISIBILITY_TIMEOUT = 60;

		int GetQueueLength(out bool success);
		MessageQueueResult SendMessage(SimpleQueueMessage simpleQueueMessage, out bool success);
		MessageQueueResult SendMessage(SimpleQueueMessage simpleQueueMessage, MessageQueueOptions messageQueueOptions, out bool success);
		IList<MessageQueueResult> SendMessages(IList<SimpleQueueMessage> simpleQueueMessages, MessageQueueOptions messageQueueOptions, out bool success);
		IList<SimpleQueueMessage> GetMessages(out bool success);
		IList<SimpleQueueMessage> GetMessages(MessageQueueOptions messageQueueOptions, out bool success);
		MessageQueueResult DeleteMessage(out bool success);

		IList<MessageQueueResult> DeleteMessages(List<string> messageHandleId, MessageQueueOptions messageQueueOptions, out bool success);
		void Clear(out bool success);
		bool GetMessageFromException(Exception ex, SdtMessages_Message msg);
	}
	public class SimpleQueueMessage : GxUserType
	{
		public string MessageId { get; set; }
		public string MessageBody { get; set; }
		public GXProperties MessageAttributes { get; set; }
		public string MessageHandleId { get; set; }

		#region Json
		private static Hashtable mapper;
		public override String JsonMap(String value)
		{
			if (mapper == null)
			{
				mapper = new Hashtable();
			}
			return (String)mapper[value]; ;
		}

		public override void ToJSON()
		{
			ToJSON(true);
			return;
		}

		public override void ToJSON(bool includeState)
		{
			AddObjectProperty("MessageId", MessageId, false);
			AddObjectProperty("MessageBody", MessageBody, false);
			AddObjectProperty("MessageHandleId", MessageHandleId, false);
			return;
		}

		#endregion

	}

	public class MessageQueueResult : GxUserType
	{
		public string MessageId { get; set; }
		public string ServerMessageId { get; set; }
		public GXProperties MessageAttributes { get; set; }
		public string MessageHandleId { get; set; }
		public string MessageStatus { get; set; }

		#region Json
		private static Hashtable mapper;
		public override String JsonMap(String value)
		{
			if (mapper == null)
			{
				mapper = new Hashtable();
			}
			return (String)mapper[value]; ;
		}

		public override void ToJSON()
		{
			ToJSON(true);
			return;
		}

		public override void ToJSON(bool includeState)
		{
			AddObjectProperty("MessageId", MessageId, false);
			AddObjectProperty("ServerMessageId", ServerMessageId, false);
			AddObjectProperty("MessageHandleId", MessageHandleId, false);
			AddObjectProperty("MessageStatus", MessageStatus, false);
			
			return;
		}

		#endregion
	}

	public class MessageQueueOptions : GxUserType
	{
		public short MaxNumberOfMessages { get; set; }
		public bool DeleteConsumedMessages { get; set; }
		public int WaitTimeout { get; set; }
		public int VisibilityTimeout { get; set; }
		public int TimetoLive { get; set; }
		public int DelaySeconds { get; set; }
		public string ReceiveRequestAttemptId { get; set; }
		public bool ReceiveMessageAttributes { get; set; }

	}

	public static class MessageQueueResultStatus
	{
		public const string Unknown = "Unknown";
		public const string Sent = "Sent";
		public const string Deleted = "Deleted";
		public const string Failed = "Failed";
	}
}
