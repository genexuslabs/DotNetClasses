using System;
using GeneXus.Attributes;
using GeneXus.Encryption;
using GeneXus.Services;
using GeneXus.Utils;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Messaging.Common
{
	[GXApi]
	public class MessageBrokerProvider : MessageQueue
	{
		static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static GXService providerService;
		public MessageBrokerProvider()
		{

		}

		public MessageQueue Connect(string providerTypeName, GXProperties properties, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			MessageQueue messageQueue = new MessageQueue();
			if (string.IsNullOrEmpty(providerTypeName))
			{
				GXUtil.ErrorToMessages("GXMessageBroker1000", "Message Broker provider cannot be empty", errorMessages);
				GXLogging.Error(logger, "(GXMessageBroker1000)Failed to Connect to a Message Broker : Provider cannot be empty.");
				success = false;
				return messageQueue;
			}
			try
			{
				if (providerService == null || !string.Equals(providerService.Name, providerTypeName, StringComparison.OrdinalIgnoreCase))
				{
					providerService = new GXService();
					providerService.Type = GXServices.MESSAGEBROKER_SERVICE;
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
				GXLogging.Debug(logger, "Loading Message Broker provider: " + typeFullName);
				Type type = AssemblyLoader.GetType(typeFullName);
				messageQueue.messageBroker = (IMessageBroker)Activator.CreateInstance(type, new object[] { providerService });

			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, "(GXMessageBroker1001)Couldn't connect to Message Broker provider: " + ExceptionExtensions.GetInnermostException(ex));
				GXUtil.ErrorToMessages("GXMessageBroker1001", ex, errorMessages);
				success = false;
				return messageQueue;
			}
			success = true;
			return (messageQueue);
		}
		private static void Preprocess(String name, GXProperties properties)
		{
			string className;

			switch (name)
			{
				case Providers.AzureServiceBus:
					className = PropertyConstants.AZURE_SB_CLASSNAME;
					SetEncryptedProperty(properties, PropertyConstants.MESSAGEBROKER_AZURESB_QUEUENAME);
					SetEncryptedProperty(properties, PropertyConstants.MESSAGEBROKER_AZURESB_TOPICNAME);
					SetEncryptedProperty(properties, PropertyConstants.MESSAGEBROKER_AZURESB_SUBSCRIPTION_NAME);
					SetEncryptedProperty(properties, PropertyConstants.MESSAGEBROKER_AZURESB_CONNECTIONSTRING);
					if (string.IsNullOrEmpty(providerService.ClassName) || !providerService.ClassName.Contains(className))
					{
						providerService.ClassName = PropertyConstants.AZURE_SB_PROVIDER_CLASSNAME;
					}
					break;
				default:
					throw new SystemException(string.Format("Provider {0} is not supported.", name));
			}
		}
		private static void SetEncryptedProperty(GXProperties properties, String prop)
		{
			String value = properties.Get(prop);
			if (string.IsNullOrEmpty(value))
				value = String.Empty;
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
		public const string AzureServiceBus = "AZURESERVICEBUS";
	}
}
