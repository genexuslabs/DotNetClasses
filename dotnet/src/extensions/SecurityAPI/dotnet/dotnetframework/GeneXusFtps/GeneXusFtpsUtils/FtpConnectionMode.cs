using log4net;
using SecurityAPICommons.Commons;
using System;
using System.Security;

namespace GeneXusFtps.GeneXusFtpsUtils
{
	[SecuritySafeCritical]
	public enum FtpConnectionMode
	{
		NONE, ACTIVE, PASSIVE
	}

	[SecuritySafeCritical]
	public static class FtpConnectionModeUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(FtpConnectionModeUtils));

		[SecuritySafeCritical]
		public static FtpConnectionMode getFtpMode(string ftpMode, Error error)
		{
			logger.Debug("getFtpMode");
			if (error == null) return FtpConnectionMode.NONE;
			if (ftpMode == null)
			{
				error.setError("FM001", "Unrecognized FtpMode");
				logger.Error("Unrecognized FtpMode");
				return FtpConnectionMode.NONE;
			}
			switch (ftpMode.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
			{
				case "ACTIVE":
					return FtpConnectionMode.ACTIVE;
				case "PASSIVE":
					return FtpConnectionMode.PASSIVE;
				default:
					error.setError("FM001", "Unrecognized FtpMode");
					logger.Error("Unrecognized FtpMode");
					return FtpConnectionMode.NONE;
			}
		}

		[SecuritySafeCritical]
		public static string valueOf(FtpConnectionMode ftpMode, Error error)
		{
			logger.Debug("valueOf");
			if (error == null) return "";
			switch (ftpMode)
			{
				case FtpConnectionMode.ACTIVE:
					return "ACTIVE";
				case FtpConnectionMode.PASSIVE:
					return "PASSIVE";
				default:
					error.setError("FM002", "Unrecognized FtpMode");
					logger.Error("Unrecognized FtpMode");
					return "";
			}
		}
	}
}
