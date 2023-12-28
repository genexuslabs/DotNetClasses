using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GeneXus.Deploy.AzureFunctions.Handlers.Helpers
{
	public class EventSourceType
	{
		public const string QueueMessage = "QueueMessage";
		public const string ServiceBusMessage = "ServiceBusMessage";
		public const string Timer = "Timer";
		public const string CosmosDB = "CosmosDB";
		public const string Blob = "Blob";
	}
	public class EventMessagePropertyMapping
	{
		public static GxUserType CreateEventMessageProperty(Type eventMessPropsItemType, string propertyId, object propertyValue, GxContext gxContext)
		{
			GxUserType eventMessageProperty = (GxUserType)Activator.CreateInstance(eventMessPropsItemType, new object[] { gxContext }); // instance of SdtEventMessageProperty
			ClassLoader.SetPropValue(eventMessageProperty, "gxTpr_Propertyid", propertyId);
			ClassLoader.SetPropValue(eventMessageProperty, "gxTpr_Propertyvalue", propertyValue);
			return eventMessageProperty;
		}
	}
	public class CustomEventType
	{
		public string Id { get; set; }
		public string Topic { get; set; }
		public string Subject { get; set; }
		public string EventType { get; set; }
		public string DataVersion { get; set; }
		public DateTime EventTime { get; set; }
		public IDictionary<string, JsonElement> Data { get; set; }
	}
}
