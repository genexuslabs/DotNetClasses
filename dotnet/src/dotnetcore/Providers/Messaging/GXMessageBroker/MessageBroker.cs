using System;
using System.Collections;
using System.Collections.Generic;
using GeneXus.Utils;

namespace GeneXus.Messaging.Common
{
	public interface IMessageBroker
	{
		bool SendMessage(BrokerMessage brokerMessage, string options);
		bool SendMessages(IList<BrokerMessage> brokerMessages, string options);
		IList<BrokerMessage> GetMessages(string options, out bool success);
		BrokerMessage GetMessage(string options, out bool success);
		void Dispose();
		bool GetMessageFromException(Exception ex, SdtMessages_Message msg);
		bool ConsumeMessage(BrokerMessage brokerMessage, string options);
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
		public short ReceiveMode { get; set; }
		public short PrefetchCount { get; set; }
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
