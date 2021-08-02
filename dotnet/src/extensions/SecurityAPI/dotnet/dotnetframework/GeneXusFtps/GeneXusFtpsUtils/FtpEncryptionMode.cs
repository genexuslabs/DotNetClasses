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
		[SecuritySafeCritical]
		public static FtpEncryptionMode getFtpEncryptionMode(String ftpEncryptionMode, Error error)
		{
			if(error == null) return FtpEncryptionMode.NONE;
			if (ftpEncryptionMode == null)
			{
				error.setError("EM001", "Unknown encryption mode");
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
					return FtpEncryptionMode.NONE;
			}
		}

		[SecuritySafeCritical]
		public static String valueOf(FtpEncryptionMode ftpEncryptionMode, Error error)
		{
			if (error == null) return "";
			switch (ftpEncryptionMode)
			{
				case FtpEncryptionMode.IMPLICIT:
					return "IMPLICIT";
				case FtpEncryptionMode.EXPLICIT:
					return "EXPLICIT";
				default:
					error.setError("EM002", "Unknown encryption mode");
					return "";
			}
		}
	}
}
