using System;
using System.Collections;
using System.Collections.Generic;
using GeneXus.Utils;

namespace GeneXus.Messaging.Common
{
	public interface IMessageBroker
	{
		bool SendMessage(BrokerMessage brokerMessage);
		bool SendMessages(IList<BrokerMessage> brokerMessages, BrokerMessageOptions messageQueueOptions);
		IList<BrokerMessage> GetMessages(BrokerMessageOptions messageQueueOptions, out bool success);
		void Dispose();
		bool GetMessageFromException(Exception ex, SdtMessages_Message msg);
	}
	public class BrokerMessage : GxUserType
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

	public class BrokerMessageResult : GxUserType
	{
		public string MessageId { get; set; }
		public string ServerMessageId { get; set; }
		public GXProperties MessageAttributes { get; set; }
		public string MessageHandleId { get; set; }
		public string MessageStatus { get; set; } = "Unknown";

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

	public class BrokerMessageOptions : GxUserType
	{
		public short MaxNumberOfMessages { get; set; }
		public bool DeleteConsumedMessages { get; set; }
		public int WaitTimeout { get; set; }
		public int VisibilityTimeout { get; set; }
		public int TimetoLive { get; set; }
		public int DelaySeconds { get; set; }
		public string ReceiveRequestAttemptId { get; set; }
		public bool ReceiveMessageAttributes { get; set; }
		public int ReceiveMode { get; set; }
		public int PrefetchCount { get; set; }
		public string SubscriptionName { get; set; }

	}

	public static class BrokerMessageResultStatus
	{
		public const string Unknown = "Unknown";
		public const string Sent = "Sent";
		public const string Deleted = "Deleted";
		public const string Failed = "Failed";
	}
}
