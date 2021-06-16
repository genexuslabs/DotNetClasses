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
		protected TimeSpan defaultExpiration = new TimeSpan(24, 0, 0);

		
		protected GxFileType defaultAcl = GxFileType.Private;

		public ExternalProviderBase()
		{
			Initialize();
		}

		public ExternalProviderBase(GXService s)
		{
			if (s == null) {
				try
				{
					s = ServiceFactory.GetGXServices().Get(GXServices.STORAGE_SERVICE);
				}
				catch (Exception) {
					logger.Warn("STORAGE_SERVICE is not activated in CloudServices.config");
				}
			}
			
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
			if (this.defaultAcl == GxFileType.Default)
			{
				this.defaultAcl = GxFileType.PublicRead;
			}

			String expirationS = GetPropertyValue(DEFAULT_EXPIRATION, DEFAULT_EXPIRATION, defaultExpiration.ToString());
			if (!String.IsNullOrEmpty(expirationS))
			{
				int minutes;
				if (Int32.TryParse(expirationS, out minutes) && minutes > 0)
				{
					defaultExpiration = new TimeSpan(0, minutes, 0);
				}
			}
		}

		protected TimeSpan ResolveExpiration(int expirationMinutes)
		{
			return expirationMinutes > 0 ? new TimeSpan(0, expirationMinutes, 0) : defaultExpiration;
		}

		protected String GetEncryptedPropertyValue(String propertyName, String alternativePropertyName = null)
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

		protected String GetPropertyValue(String propertyName, String alternativePropertyName = null)
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
			String value = null;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(ResolvePropertyName(propertyName)) : value;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(propertyName) : value;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(alternativePropertyName) : value;
			value = string.IsNullOrEmpty(value) ? defaultValue : value;
			return value;
		}

		private string GetPropertyValueImpl(string propertyName)
		{
			String value = null;
			if (!string.IsNullOrEmpty(propertyName))
			{
				value = Environment.GetEnvironmentVariable(propertyName);
				if (this.service != null)
				{
					value = this.service.Properties.Get(propertyName);
				}
			}
			return value;
		}

		protected String ResolvePropertyName(String propertyName)
		{
			return $"STORAGE_{GetName()}_{propertyName}";
		}

	}
}
