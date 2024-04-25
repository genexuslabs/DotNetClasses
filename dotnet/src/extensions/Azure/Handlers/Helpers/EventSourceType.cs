using System;
using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Utils;

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
}
