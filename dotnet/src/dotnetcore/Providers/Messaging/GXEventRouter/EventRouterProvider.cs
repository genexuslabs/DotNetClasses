using System;
using GeneXus.Attributes;
using GeneXus.Encryption;
using GeneXus.Services;
using GeneXus.Utils;
using GxClasses.Helpers;

namespace GeneXus.Messaging.Common
{
	[GXApi]
	public class EventRouterProvider : EventRouterProviderBase
	{
		static readonly IGXLogger logger = GXLoggerFactory.GetLogger<EventRouterProvider>();
		private static GXService providerService;
		public EventRouterProvider() {
		}
		public EventRouterProviderBase Connect(string providerTypeName, GXProperties properties, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			EventRouterProviderBase eventRouter = new EventRouterProviderBase();
			if (string.IsNullOrEmpty(providerTypeName))
			{
				GXUtil.ErrorToMessages("GXEventRouter", "Event Router provider cannot be empty", errorMessages);
				GXLogging.Error(logger, "(GXEventRouter)Failed to Connect to a Event Router : Provider cannot be empty.");
				success = false;
				return eventRouter;
			}
			try
			{
				if (providerService == null || !string.Equals(providerService.Name, providerTypeName, StringComparison.OrdinalIgnoreCase))
				{
					providerService = new GXService();
					providerService.Type = GXServices.EVENTROUTER_SERVICE;
					providerService.Name = providerTypeName;
					providerService.AllowMultiple = false;
					providerService.Properties = new GXProperties();
				}
				Preprocess(providerTypeName, properties);

				GxKeyValuePair prop = properties.GetFirst();
				while (!properties.Eof())
				{
					providerService.Properties.Set(prop.Key, prop.Value);
					prop = properties.GetNext();
				}

				string typeFullName = providerService.ClassName;
				GXLogging.Debug(logger, "Loading Event Router provider: " + typeFullName);
				Type type = AssemblyLoader.GetType(typeFullName);
				eventRouter.eventRouter = (IEventRouter)Activator.CreateInstance(type, new object[] { providerService });

			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, "(GXEventRouter)Couldn't connect to Event Router provider: " + ExceptionExtensions.GetInnermostException(ex));
				GXUtil.ErrorToMessages("GXEventRouter", ex, errorMessages);
				success = false;
				return eventRouter;
			}
			success = true;
			return (eventRouter);
		}
		private static void Preprocess(string name, GXProperties properties)
		{
			string className;

			switch (name)
			{
				case Providers.AzureEventGrid:
					className = PropertyConstants.AZURE_EG_CLASSNAME;
					SetEncryptedProperty(properties, PropertyConstants.EVENTROUTER_AZUREEG_ENDPOINT);
					SetEncryptedProperty(properties, PropertyConstants.EVENTROUTER_AZUREEG_ACCESS_KEY);
					if (string.IsNullOrEmpty(providerService.ClassName) || !providerService.ClassName.Contains(className))
					{
						providerService.ClassName = PropertyConstants.AZURE_EG_PROVIDER_CLASSNAME;
					}
					break;
				default:
					throw new SystemException(string.Format("Provider {0} is not supported.", name));
			}
		}
		private static void SetEncryptedProperty(GXProperties properties, string prop)
		{
			string value = properties.Get(prop);
			if (string.IsNullOrEmpty(value))
				value = string.Empty;
			value = CryptoImpl.Encrypt(value);
			properties.Set(prop, value);
		}

	}
	public static class ExceptionExtensions
	{
		public static string GetInnermostException(Exception e)
		{
			Exception ex = e;
			if (ex != null)
			{ 
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}
				
			}
			return ex.Message;
		}
	}
	static class Providers
	{
		public const string AzureEventGrid = "AZUREEVENTGRID";
	}
}
