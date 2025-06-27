using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using log4net;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusUtils
{
	[SecuritySafeCritical]
	public enum KeyInfoType
	{
		NONE, KeyValue, X509Certificate
	}

	[SecuritySafeCritical]
	public static class KeyInfoTypeUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(KeyInfoTypeUtils));
		public static KeyInfoType getKeyInfoType(string keyInfoType, Error error)
		{
			logger.Debug("getKeyInfoType");
			if (error == null) return KeyInfoType.NONE;
			if (keyInfoType == null)
			{
				error.setError("KIT03", "Unrecognized KeyInfoType");
				logger.Error("Unrecognized KeyInfoType");
				return KeyInfoType.NONE;
			}
			switch (keyInfoType.Trim())
			{
				case "NONE":
					return KeyInfoType.NONE;
				case "KeyValue":
					return KeyInfoType.KeyValue;
				case "X509Certificate":
					return KeyInfoType.X509Certificate;
				default:
					error.setError("KIT01", "Unrecognized KeyInfoType");
					logger.Error("Unrecognized KeyInfoType");
					return KeyInfoType.NONE;
			}

		}

		public static string valueOf(KeyInfoType keyInfoType, Error error)
		{
			logger.Debug("valueOf");
			if (error == null) return "";
			switch (keyInfoType)
			{
				case KeyInfoType.NONE:
					return "NONE";
				case KeyInfoType.KeyValue:
					return "KeyValue";
				case KeyInfoType.X509Certificate:
					return "X509Certificate";
				default:
					error.setError("KIT02", "Unrecognized KeyInfoType");
					logger.Error("Unrecognized KeyInfoType");
					return "";
			}
		}

		/// <summary>
		/// Manage Enumerable enum 
		/// </summary>
		/// <typeparam name="KeyInfoType">KeyInfoType enum</typeparam>
		/// <returns>Enumerated values</returns>
		internal static IEnumerable<KeyInfoType> GetValues<KeyInfoType>()
		{
			return Enum.GetValues(typeof(KeyInfoType)).Cast<KeyInfoType>();
		}

	}
}
