using System;
using GeneXus.Encryption;
using log4net;

namespace GeneXus.Services
{
	public abstract class ExternalProviderBase
	{
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(ExternalProviderBase));

		private GXService service;

		protected static String DEFAULT_ACL = "DEFAULT_ACL";
		protected static String DEFAULT_EXPIRATION = "DEFAULT_EXPIRATION";
		protected static String FOLDER = "FOLDER_NAME";
		protected static String DEFAULT_ACL_DEPRECATED = "STORAGE_PROVIDER_DEFAULT_ACL";
		protected static String DEFAULT_EXPIRATION_DEPRECATED = "STORAGE_PROVIDER_DEFAULT_EXPIRATION";
		protected const int DefaultExpirationMinutes = 24 * 60;


		protected GxFileType defaultAcl = GxFileType.Private;

		public ExternalProviderBase()
		{
			Initialize();
		}

		public ExternalProviderBase(GXService s)
		{
			this.service = s;
			Initialize();
		}

		public abstract String GetName();

		private void Initialize()
		{
			String aclS = GetPropertyValue(DEFAULT_ACL, DEFAULT_ACL_DEPRECATED, "");
			if (!String.IsNullOrEmpty(aclS))
			{
				GxFileType.TryParse(aclS, out this.defaultAcl);
			}
		}

		protected String GetEncryptedPropertyValue(String propertyName, String alternativePropertyName)
		{
			String value = GetEncryptedPropertyValue(propertyName, alternativePropertyName, null);
			if (value == null)
			{
				String errorMessage = String.Format($"Service configuration error - Property name {ResolvePropertyName(propertyName)} must be defined");
				logger.Fatal(errorMessage);
				throw new Exception(errorMessage);
			}
			return value;
		}

		protected String GetEncryptedPropertyValue(String propertyName, String alternativePropertyName, String defaultValue)
		{
			String value = GetPropertyValue(propertyName, alternativePropertyName, defaultValue);
			if (!String.IsNullOrEmpty(value))
			{
				try
				{
					value = CryptoImpl.Decrypt(value);
				}
				catch (Exception)
				{
					logger.Warn($"Could not decrypt property name: {ResolvePropertyName(propertyName)}");
				}
			}
			return value;
		}

		protected String GetPropertyValue(String propertyName, String alternativePropertyName)
		{
			String value = GetPropertyValue(propertyName, alternativePropertyName, null);
			if (value == null)
			{
				String errorMessage = String.Format($"Service configuration error - Property name {ResolvePropertyName(propertyName)} must be defined");
				logger.Fatal(errorMessage);
				throw new Exception(errorMessage);
			}
			return value;
		}

		protected String GetPropertyValue(String propertyName, String alternativePropertyName, String defaultValue)
		{
			propertyName = ResolvePropertyName(propertyName);
			String value = Environment.GetEnvironmentVariable(propertyName);
			if (String.IsNullOrEmpty(value))
			{
				value = Environment.GetEnvironmentVariable(alternativePropertyName);
			}
			if (this.service != null)
			{
				value = this.service.Properties.Get(propertyName);
				if (String.IsNullOrEmpty(value))
				{
					value = this.service.Properties.Get(alternativePropertyName);
				}
			}
			return !String.IsNullOrEmpty(value) ? value : defaultValue;
		}

		protected String ResolvePropertyName(String propertyName)
		{
			return String.Format("STORAGE_%s_%s", GetName(), propertyName);
		}

	}
}
