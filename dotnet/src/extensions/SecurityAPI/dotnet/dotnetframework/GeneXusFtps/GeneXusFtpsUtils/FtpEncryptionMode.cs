using log4net;
using SecurityAPICommons.Commons;
using System;
using System.Security;

namespace GeneXusFtps.GeneXusFtpsUtils
{
	[SecuritySafeCritical]
	public enum FtpEncryptionMode
	{
		NONE, IMPLICIT, EXPLICIT
	}

	[SecuritySafeCritical]
	public static class FtpEncryptionModeUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(FtpEncryptionModeUtils));

		[SecuritySafeCritical]
		public static FtpEncryptionMode getFtpEncryptionMode(string ftpEncryptionMode, Error error)
		{
			logger.Debug("getFtpEncryptionMode");
			if (error == null) return FtpEncryptionMode.NONE;
			if (ftpEncryptionMode == null)
			{
				error.setError("EM001", "Unknown encryption mode");
				logger.Error("Unknown encryption mode");
				return FtpEncryptionMode.NONE;
			}
			switch (ftpEncryptionMode.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
			{
				case "IMPLICIT":
					return FtpEncryptionMode.IMPLICIT;
				case "EXPLICIT":
					return FtpEncryptionMode.EXPLICIT;
				default:
					error.setError("EM001", "Unknown encryption mode");
					logger.Error("Unknown encryption mode");
					return FtpEncryptionMode.NONE;
			}
		}

		[SecuritySafeCritical]
		public static string valueOf(FtpEncryptionMode ftpEncryptionMode, Error error)
		{
			logger.Debug("valueOf");
			if (error == null) return "";
			switch (ftpEncryptionMode)
			{
				case FtpEncryptionMode.IMPLICIT:
					return "IMPLICIT";
				case FtpEncryptionMode.EXPLICIT:
					return "EXPLICIT";
				default:
					error.setError("EM002", "Unknown encryption mode");
					logger.Error("Unknown encryption mode");
					return "";
			}
		}
	}
}
