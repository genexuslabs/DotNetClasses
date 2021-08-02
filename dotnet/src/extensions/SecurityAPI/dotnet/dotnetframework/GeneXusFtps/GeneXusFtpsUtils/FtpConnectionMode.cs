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
	public class FtpConnectionModeUtils
	{
		[SecuritySafeCritical]
		public static FtpConnectionMode getFtpMode(String ftpMode, Error error)
		{
			switch (ftpMode.ToUpper().Trim())
			{
				case "ACTIVE":
					return FtpConnectionMode.ACTIVE;
				case "PASSIVE":
					return FtpConnectionMode.PASSIVE;
				default:
					error.setError("FM001", "Unrecognized FtpMode");
					return FtpConnectionMode.NONE;
			}
		}

		[SecuritySafeCritical]
		public static String valueOf(FtpConnectionMode ftpMode, Error error)
		{
			switch (ftpMode)
			{
				case FtpConnectionMode.ACTIVE:
					return "ACTIVE";
				case FtpConnectionMode.PASSIVE:
					return "PASSIVE";
				default:
					error.setError("FM002", "Unrecognized FtpMode");
					return "";
			}
		}
	}
}
