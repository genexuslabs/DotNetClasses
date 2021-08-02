using SecurityAPICommons.Commons;
using System;
using System.Security;

namespace GeneXusFtps.GeneXusFtpsUtils
{
	[SecuritySafeCritical]
	public enum FtpEncoding
	{
		NONE, BINARY, ASCII
	}

	[SecuritySafeCritical]
	public class FtpEncodingUtils
	{
		[SecuritySafeCritical]
		public static FtpEncoding getFtpEncoding(String ftpEncoding, Error error)
		{
			switch (ftpEncoding.ToUpper().Trim())
			{
				case "BINARY":
					return FtpEncoding.BINARY;
				case "ASCII":
					return FtpEncoding.ASCII;
				default:
					error.setError("FE001", "Unknown encoding");
					return FtpEncoding.NONE;
			}
		}

		[SecuritySafeCritical]
		public static String valueOf(FtpEncoding ftpEncoding, Error error)
		{
			switch (ftpEncoding)
			{
				case FtpEncoding.BINARY:
					return "BINARY";
				case FtpEncoding.ASCII:
					return "ASCII";
				default:
					error.setError("FE002", "Unknown encoding");
					return "";
			}
		}
	}
}
