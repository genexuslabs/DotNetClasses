using GeneXus.Messaging.Common;
using GeneXus.Utils;

namespace GeneXus.Messaging.GXAzureEventGrid
{
	/// <summary>
	/// Implementation of EventGridRouterProvider External Object.
	/// </summary>
	public class EventGridRouterProvider
	{
		public EventRouterProviderBase Connect(string endpoint, string accesskey, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			EventRouterProvider eventRouterProvider = new EventRouterProvider();
			GXProperties properties = new GXProperties
			{
				{ PropertyConstants.EVENTROUTER_AZUREEG_ENDPOINT, endpoint },
				{ PropertyConstants.EVENTROUTER_AZUREEG_ACCESS_KEY, accesskey }
			};

			EventRouterProviderBase evtRouterProvider = eventRouterProvider.Connect(PropertyConstants.AZUREEVENTGRID, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return evtRouterProvider;
		}
		public EventRouterProviderBase Connect(string endpoint, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			EventRouterProvider eventRouterProvider = new EventRouterProvider();
			GXProperties properties = new GXProperties
			{
				{ PropertyConstants.EVENTROUTER_AZUREEG_ENDPOINT, endpoint },
			};

			EventRouterProviderBase evtRouterProvider = eventRouterProvider.Connect(PropertyConstants.AZUREEVENTGRID, properties, out GXBaseCollection<SdtMessages_Message> errorMessagesConnect, out bool successConnect);
			errorMessages = errorMessagesConnect;
			success = successConnect;
			return evtRouterProvider;
		}
	}
}
