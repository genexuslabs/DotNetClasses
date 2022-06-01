using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static GXService providerService;
		public MessageQueueProvider()
		{
			/*if (providerService == null)
			{
				providerService = ServiceFactory.GetGXServices().Get(GXServices.QUEUE_SERVICE);
			}*/

		}

		public SimpleMessageQueue Connect(string providerTypeName, GXProperties properties, out GXBaseCollection<SdtMessages_Message> errorMessages, out bool success)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			SimpleMessageQueue simpleMessageQueue = new SimpleMessageQueue();
			if (string.IsNullOrEmpty(providerTypeName))
			{
				GXUtil.ErrorToMessages("Unsupported", "Queue provider cannot be empty", errorMessages);
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
				preprocess(providerTypeName, properties);

				GxKeyValuePair prop = properties.GetFirst();
				while (!properties.Eof())
				{
					providerService.Properties.Set(prop.Key, prop.Value);
					prop = properties.GetNext();
				}

				string typeFullName = providerService.ClassName;
				logger.Debug("Loading Queue provider: " + typeFullName);
#if !NETCORE
				Type type = Type.GetType(typeFullName, true, true);
#else
				Type type = AssemblyLoader.GetType(typeFullName);
#endif
				simpleMessageQueue.queue = (IQueue)Activator.CreateInstance(type, new object[] { providerService });

			}
			catch (Exception ex)
			{
				logger.Error("Couldn't connect to Queue provider. ", ex);
				GXUtil.ErrorToMessages("Queue Error", ex, errorMessages);
				success = false;
				return simpleMessageQueue;
			}
			success = true;
			return (simpleMessageQueue);
		}

		private static void preprocess(String name, GXProperties properties)
		{
			string className;

			switch (name)
			{
				case "AZUREQUEUE":
					className = "GeneXus.Messaging.Queue.AzureQueue";
					SetEncryptedProperty(properties, "QUEUE_AZUREQUEUE_QUEUENAME");
					SetEncryptedProperty(properties, "QUEUE_AZUREQUEUE_CONNECTIONSTRING");
					if (string.IsNullOrEmpty(providerService.ClassName) || !providerService.ClassName.Contains(className))
					{
						providerService.ClassName = "GeneXus.Messaging.Queue.AzureQueue, GXAzureQueue, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
					}
					break;

				default:
					throw new SystemException(string.Format("Provider {0} is not supported", name));

			}
		}
		private static void SetEncryptedProperty(GXProperties properties, String prop)
		{
			String value = properties.Get(prop);
			if (string.IsNullOrEmpty(value))
				value = "";
			value = CryptoImpl.Encrypt(value);
			properties.Set(prop, value);
		}

	}
}
