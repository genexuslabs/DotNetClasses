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

	}
}
