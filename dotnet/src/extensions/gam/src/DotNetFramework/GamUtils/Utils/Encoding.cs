using System;
using Org.BouncyCastle.Utilities.Encoders;
using log4net;
using System.Security;

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
	}
}
