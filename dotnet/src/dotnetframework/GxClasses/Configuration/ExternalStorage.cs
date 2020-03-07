using System;
using GeneXus.Services;
using GeneXus.Attributes;
using GeneXus.Utils;
using GeneXus.Encryption;
using log4net;
#if NETCORE
using GxClasses.Helpers;
#endif

namespace GeneXus.Configuration
{
	[GXApi]
	public class ExternalStorage : GxStorageProvider
	{

		private GXService providerService;

		static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


		public ExternalStorage()
		{
			providerService = ServiceFactory.GetGXServices().Get(GXServices.STORAGE_APISERVICE);
			if (providerService == null)
			{
				providerService = ServiceFactory.GetGXServices().Get(GXServices.STORAGE_SERVICE);
			}
		}

		public bool Create(string name, GXProperties properties, ref GxStorageProvider storageProvider, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			storageProvider = null;

			if (string.IsNullOrEmpty(name))
			{
				GXUtil.ErrorToMessages("Unsopported", "Provider cannot be empty", messages);
				return false;
			}

			try
			{
				if (providerService == null || !string.Equals(providerService.Name, name, StringComparison.OrdinalIgnoreCase))
				{
					providerService = new GXService();
					providerService.Type = GXServices.STORAGE_SERVICE;
					providerService.Name = name;
					providerService.AllowMultiple = false;
					providerService.Properties = new GXProperties();
				}

				preprocess(name, properties);

				GxKeyValuePair prop = properties.GetFirst();
				while (!properties.Eof())
				{
					providerService.Properties.Set(prop.Key, prop.Value);
					prop = properties.GetNext();
				}

				string typeFullName = providerService.ClassName;
				logger.Debug("Loading storage provider: "+ typeFullName);
#if !NETCORE
				Type type = Type.GetType(typeFullName, true, true);
#else
				Type type = new AssemblyLoader(FileUtil.GetStartupDirectory()).GetType(typeFullName);
#endif
				this.provider = (ExternalProvider) Activator.CreateInstance(type, new object[] { providerService });
				
			}
			catch (Exception ex)
			{
				logger.Error("Couldn't connect to external storage provider. ", ex);
				StorageMessages(ex, messages);
				return false;
			}

			storageProvider = this;
			return true;
		}

		public bool Connect(string profileName, GXProperties properties, ref GxStorageProvider storageProvider, ref GXBaseCollection<SdtMessages_Message> messages)
		{
			if (providerService != null)
			{
				if (profileName.Trim().ToLower() == "default")
				{
					profileName = providerService.Name;
				}
				return Create(profileName, properties, ref storageProvider, ref messages);
			}
			StorageMessages(new SystemException("Provider cannot be local"), messages);
			return false;
		}

		private void preprocess(String name, GXProperties properties)
		{
			string className = null;

			switch (name)
			{

				case "AMAZONS3":
					className = "GeneXus.Storage.GXAmazonS3.ExternalProviderS3";
					SetDefaultProperty(properties, "STORAGE_PROVIDER_REGION", "us-east-1");
					SetDefaultProperty(properties, "STORAGE_ENDPOINT", "s3.amazonaws.com");
					SetEncryptProperty(properties, "STORAGE_PROVIDER_ACCESSKEYID");
					SetEncryptProperty(properties, "STORAGE_PROVIDER_SECRETACCESSKEY");
					SetEncryptProperty(properties, "BUCKET_NAME");
					break;

				case "AZURESTORAGE":
					className = "GeneXus.Storage.GXAzureStorage.AzureStorageExternalProvider";
					SetEncryptProperty(properties, "PUBLIC_CONTAINER_NAME");
					SetEncryptProperty(properties, "PRIVATE_CONTAINER_NAME");
					SetEncryptProperty(properties, "ACCOUNT_NAME");
					SetEncryptProperty(properties, "ACCESS_KEY");
					break;

				case "BLUEMIXSTORAGE":
					className = "GeneXus.Storage.GXBluemix.ExternalProviderBluemix";
					SetDefaultProperty(properties, "SERVER_URL", "https://identity.open.softlayer.com");
					SetDefaultProperty(properties, "STORAGE_PROVIDER_REGION", "dallas");
					SetEncryptProperty(properties, "PUBLIC_BUCKET_NAME");
					SetEncryptProperty(properties, "PRIVATE_BUCKET_NAME");
					SetEncryptProperty(properties, "STORAGE_PROVIDER_USER");
					SetEncryptProperty(properties, "STORAGE_PROVIDER_PASSWORD");
					break;

				//case "BOX":
				//	className = "{class}";
				//	break;

				case "GOOGLE":
					className = "GeneXus.Storage.GXGoogleCloud.ExternalProviderGoogle";
					SetEncryptProperty(properties, "KEY");
					SetEncryptProperty(properties, "BUCKET_NAME");
					break;

				//case "IBMCOS":
				//	className = "{class}";
				//	SetEncryptProperty(properties, "STORAGE_PROVIDER_ACCESS_KEY");
				//	SetEncryptProperty(properties, "STORAGE_PROVIDER_SECRET_KEY");
				//	SetEncryptProperty(properties, "BUCKET_NAME");
				//	break;

				case "OPENSTACKSTORAGE":
					className = "GeneXus.Storage.GXOpenStack.ExternalProviderOpenStack";
					SetEncryptProperty(properties, "BUCKET_NAME");
					SetEncryptProperty(properties, "STORAGE_PROVIDER_USER");
					SetEncryptProperty(properties, "STORAGE_PROVIDER_PASSWORD");
					break;

				default:
					throw new SystemException(string.Format("Provider {0} is not supported", name));

			}

			if (string.IsNullOrEmpty(providerService.ClassName) || !providerService.ClassName.Contains(className))
			{
				providerService.ClassName = string.Format(@"{0}, {1}, {2}", // get GeneXus.Storage.{assembly}.{class}, {assmebly}, Version={version}, Culture={culture}, PublicKeyToken={token}
					className,
					className.Split('.').GetValue(2),
					System.Reflection.Assembly.GetExecutingAssembly().FullName.Substring(@"GxClasses, ".Length));
			}
		}

		private void SetDefaultProperty(GXProperties properties, String prop, String value)
		{
			if (!properties.ContainsKey(prop))
				properties.Set(prop, value);
		}

		private void SetEncryptProperty(GXProperties properties, String prop)
		{
			String value = properties.Get(prop);
			if (string.IsNullOrEmpty(value))
				value = "";
			value = CryptoImpl.Encrypt(value);
			properties.Set(prop, value);
		}

	}
}

/*
	TODO:
	+ Provider's libraries (dll) must be manually copied from '{gx}/Services/Storage/{provider}/*.dll' to '{webapp}/web/bin'
*/
