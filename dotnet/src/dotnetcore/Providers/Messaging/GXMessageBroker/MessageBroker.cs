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
		bool ConsumeMessage(BrokerMessage brokerMessage, string options);
		long ScheduleMessage(BrokerMessage brokerMessage, string options);
		bool CancelSchedule(long handleId);
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
}
