using System;
using Org.BouncyCastle.Utilities.Encoders;
using log4net;
using System.Security;
using log4net.Repository.Hierarchy;

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	internal class Encoding
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Encoding));

		[SecuritySafeCritical]
		internal static string B64ToB64Url(string input)
		{
			logger.Debug("B64ToB64Url");
			try
			{
				return System.Text.Encoding.UTF8.GetString(UrlBase64.Encode(Base64.Decode(input)));
			}
			catch (Exception e)
			{
				logger.Error("B64ToB64Url", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		internal static string HexaToBase64(string hexa)
		{
			logger.Debug("HexaToBase64");
			try
			{
				return Base64.ToBase64String(Hex.Decode(hexa));
			}
			catch (Exception e)
			{
				logger.Error("HexaToBase64", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		internal static string ToBase64Url(string input)
		{
			logger.Debug("ToBase64Url");
			try
			{
				return System.Text.Encoding.UTF8.GetString(UrlBase64.Encode(System.Text.Encoding.UTF8.GetBytes(input)));
			}
			catch (Exception e)
			{
				logger.Error("ToBase64Url", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		internal static string FromBase64Url(string base64)
		{
			logger.Debug("FromBase64Url");
			try
			{
				return System.Text.Encoding.UTF8.GetString(UrlBase64.Decode(System.Text.Encoding.UTF8.GetBytes(base64))); 
			}
			catch (Exception e)
			{
				logger.Error("FromBase64Url", e);
				return "";
			}
		}

	}
}
