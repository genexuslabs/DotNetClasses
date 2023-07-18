using GeneXus.Utils;
using System.Collections.Generic;
using System;

namespace GeneXus.Messaging.Common
{
	public interface IEventRouter
	{
		bool SendEvent(GXCloudEvent gxCloudEvent, bool binaryData);
		bool SendEvents(IList<GXCloudEvent> gxCloudEvents, bool binaryData);
		bool SendCustomEvents(string jsonString, bool isBinary);
		bool GetMessageFromException(Exception ex, SdtMessages_Message msg);
	}
	public class GXCloudEvent : GxUserType
	{
		public string type { get; set; }
		public string source { get; set; }
		public string data { get; set; }
		public string datacontenttype { get; set; }
		public string id { get; set; }
		public string dataschema { get; set; }
		public string subject { get; set; }
	}

}