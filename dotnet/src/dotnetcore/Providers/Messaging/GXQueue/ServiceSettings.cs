using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Encryption;
using GeneXus.Messaging.Common;
using GeneXus.Services;
using log4net;

namespace GeneXus.Messaging.Queue
{
	public class ServiceSettings 
	{
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(ServiceSettings));

		internal GXService service;
		public string serviceNameResolver { get; }
		public string name { get; }

		public ServiceSettings(string serviceNameResolver, string name, GXService gXService)
		{
			this.serviceNameResolver = serviceNameResolver;
			this.name = name;
			this.service = gXService;
		}

		public string GetEncryptedPropertyValue(string propertyName, string alternativePropertyName = null)
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
		public string GetEncryptedPropertyValue(string propertyName, string alternativePropertyName, string defaultValue)
		{
			String value = GetPropertyValue(propertyName, alternativePropertyName, defaultValue);
			if (!String.IsNullOrEmpty(value))
			{
				try
				{
					string ret = String.Empty;
					if (CryptoImpl.Decrypt(ref ret, value))
					{
						value = ret;
					}
				}
				catch (Exception)
				{
					logger.Warn($"Could not decrypt property name: {ResolvePropertyName(propertyName)}");
				}
			}
			return value;
		}

		internal string GetPropertyValue(string propertyName, string alternativePropertyName = null)
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

		internal string GetPropertyValue(string propertyName, string alternativePropertyName, string defaultValue)
		{
			String value = null;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(ResolvePropertyName(propertyName)) : value;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(propertyName) : value;
			value = string.IsNullOrEmpty(value) ? GetPropertyValueImpl(alternativePropertyName) : value;
			value = string.IsNullOrEmpty(value) ? defaultValue : value;
			return value;
		}

		internal string GetPropertyValueImpl(string propertyName)
		{
			String value = null;
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
