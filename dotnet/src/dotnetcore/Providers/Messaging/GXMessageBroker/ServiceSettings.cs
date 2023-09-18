using System;
using GeneXus.Encryption;
using GeneXus.Services;
using log4net;

namespace GeneXus.Messaging.Common
{
	public class ServiceSettings 
	{
		static readonly ILog logger = LogManager.GetLogger(typeof(ServiceSettings));

		internal GXService service;
		public string serviceNameResolver { get; }
		public string name { get; }

		public ServiceSettings(string serviceNameResolver, string name, GXService gXService)
		{
			this.serviceNameResolver = serviceNameResolver;
			this.name = name;
			service = gXService;
		}

		public string GetPropertiesValue(string propertyName)
		{
			return service.Properties.Get(propertyName);
		}
		public string GetEncryptedPropertyValue(string propertyName, string alternativePropertyName = null)
		{
			string value = GetEncryptedPropertyValue(propertyName, alternativePropertyName, null);
			if (value == null)
			{
				string errorMessage = string.Format($"Service configuration error - Property name {ResolvePropertyName(propertyName)} must be defined");
				GXLogging.Error(logger, errorMessage);
				throw new Exception(errorMessage);
			}
			return value;
		}
		public string GetEncryptedPropertyValue(string propertyName, string alternativePropertyName, string defaultValue)
		{
			string value = GetPropertyValue(propertyName, alternativePropertyName, defaultValue);
			if (!string.IsNullOrEmpty(value))
			{
				try
				{
					string ret = string.Empty;
					if (CryptoImpl.Decrypt(ref ret, value))
					{
						value = ret;
					}
				}
				catch (Exception)
				{
					GXLogging.Warn(logger, $"Could not decrypt property name: {ResolvePropertyName(propertyName)}");
				}
			}
			return value;
		}

		internal string GetPropertyValue(string propertyName, string alternativePropertyName = null)
		{
			string value = GetPropertyValue(propertyName, alternativePropertyName, null);
			if (value == null)
			{
				string errorMessage = string.Format($"Service configuration error - Property name {ResolvePropertyName(propertyName)} must be defined");
				GXLogging.Error(logger,errorMessage);
				throw new Exception(errorMessage);
			}
			return value;
		}

		internal string GetPropertyValue(string propertyName, string alternativePropertyName, string defaultValue)
		{
			string value = null;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(ResolvePropertyName(propertyName)) : value;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(propertyName) : value;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(alternativePropertyName) : value;
			value = string.IsNullOrEmpty(value) ? defaultValue : value;
			return value;
		}

		internal string GetPropertyValueImpl(string propertyName)
		{
			string value = null;
			if (!string.IsNullOrEmpty(propertyName))
			{
				value = Environment.GetEnvironmentVariable(propertyName);
				if (service != null && value == null)
				{
					value = service.Properties.Get(propertyName);
				}
			}
			return value;
		}

		internal string ResolvePropertyName(string propertyName)
		{
			return $"{serviceNameResolver}_{name}_{propertyName}";
		}
	}
}
