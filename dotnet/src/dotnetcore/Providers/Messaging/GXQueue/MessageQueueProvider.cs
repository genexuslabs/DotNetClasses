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
	public class MessageQueueProvider : SimpleMessageQueue
	{
		static readonly IGXLogger logger = GXLoggerFactory.GetLogger<MessageQueueProvider>();
		private static GXService providerService;
		public MessageQueueProvider(){
		}

		public SimpleMessageQueue Connect(string providerTypeName, GXProperties properties, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			SimpleMessageQueue simpleMessageQueue = new SimpleMessageQueue();
			if (string.IsNullOrEmpty(providerTypeName))
			{
				GXUtil.ErrorToMessages("GXQueue1000", "Queue provider cannot be empty", errorMessages);
				GXLogging.Error(logger, "(GXQueue1000)Failed to Connect to a queue : Queue provider cannot be empty.");
				success = false;
				return simpleMessageQueue;
			}
			try
			{
				if (providerService == null || !string.Equals(providerService.Name, providerTypeName, StringComparison.OrdinalIgnoreCase))
				{
					providerService = new GXService();
					providerService.Type = GXServices.QUEUE_SERVICE;
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
				GXLogging.Debug(logger, "Loading Queue provider: " + typeFullName);
#if !NETCORE
				Type type = Type.GetType(typeFullName, true, true);
#else
				Type type = AssemblyLoader.GetType(typeFullName);
#endif
				simpleMessageQueue.queue = (IQueue)Activator.CreateInstance(type, new object[] { providerService });

			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, "(GXQueue1001)Couldn't connect to Queue provider: " + ExceptionExtensions.GetInnermostException(ex));
				GXUtil.ErrorToMessages("GXQueue1001", ex, errorMessages);
				success = false;
				return simpleMessageQueue;
			}
			success = true;
			return (simpleMessageQueue);
		}

		private static void Preprocess(string name, GXProperties properties)
		{
			string className;
			switch (name)
			{
				case "AZUREQUEUE":
					className = PropertyConstants.AZURE_QUEUE_CLASSNAME;
					SetEncryptedProperty(properties, PropertyConstants.QUEUENAME);
					SetEncryptedProperty(properties, PropertyConstants.CONNECTIONSTRING);
					SetEncryptedProperty(properties, PropertyConstants.QUEUEURI);
					if (string.IsNullOrEmpty(providerService.ClassName) || !providerService.ClassName.Contains(className))
					{
						providerService.ClassName = PropertyConstants.AZURE_QUEUE_PROVIDER_CLASSNAME;
					}
					break;
				case "AWS_SQS":
					className = "GeneXus.Messaging.Queue.AWSQueue";
					SetEncryptedProperty(properties, PropertyConstants.QUEUE_AWSSQS_QUEUE_URL);
					SetEncryptedProperty(properties, PropertyConstants.QUEUE_AWSSQS_ACCESS_KEY);
					SetEncryptedProperty(properties, PropertyConstants.QUEUE_AWSSQS_SECRET_KEY);
					SetEncryptedProperty(properties, PropertyConstants.QUEUE_AWSSQS_REGION);
					if (string.IsNullOrEmpty(providerService.ClassName) || !providerService.ClassName.Contains(className))
					{
						providerService.ClassName = PropertyConstants.AWSSQS_QUEUE_PROVIDERTYPENAME;
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
}
