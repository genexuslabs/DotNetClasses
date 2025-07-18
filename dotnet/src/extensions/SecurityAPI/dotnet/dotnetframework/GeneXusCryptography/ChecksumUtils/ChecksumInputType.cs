using Org.BouncyCastle.Utilities.Encoders;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using SecurityAPICommons.Utils;
using System;
using System.Security;
using System.Text;
using log4net;

namespace GeneXusCryptography.ChecksumUtils
{
	[SecuritySafeCritical]
	public enum ChecksumInputType
	{
#pragma warning disable CA1707 // Identifiers should not contain underscores
		NONE, BASE64, HEX, TXT, ASCII, LOCAL_FILE,
#pragma warning restore CA1707 // Identifiers should not contain underscores
	}

	[SecuritySafeCritical]
	public static class ChecksumInputTypeUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ChecksumInputTypeUtils));
		public static ChecksumInputType getChecksumInputType(string checksumInputType, Error error)
		{
			logger.Debug("getChecksumInputType");
			if (error == null) return ChecksumInputType.NONE;
			if (checksumInputType == null)
			{
				error.setError("CHI06", "Unrecognized checksum input type");
				logger.Error("Unrecognized checksum input type");
				return ChecksumInputType.NONE;
			}
			switch (checksumInputType.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
			{
				case "BASE64":
					return ChecksumInputType.BASE64;
				case "HEX":
					return ChecksumInputType.HEX;
				case "TXT":
					return ChecksumInputType.TXT;
				case "ASCII":
					return ChecksumInputType.ASCII;
				case "LOCAL_FILE":
					return ChecksumInputType.LOCAL_FILE;
				default:
					error.setError("CHI01", "Unrecognized checksum input type");
					logger.Error("Unrecognized checksum input type");
					return ChecksumInputType.NONE;
			}
		}

		public static string valueOf(ChecksumInputType checksumInputType, Error error)
		{
			logger.Debug("valueOf");
			if (error == null) return "";
			switch (checksumInputType)
			{
				case ChecksumInputType.BASE64:
					return "BASE64";
				case ChecksumInputType.HEX:
					return "HEX";
				case ChecksumInputType.TXT:
					return "TXT";
				case ChecksumInputType.ASCII:
					return "ASCII";
				case ChecksumInputType.LOCAL_FILE:
					return "LOCAL_FILE";
				default:
					error.setError("CHI02", "Unrecognized checksum input type");
					logger.Error("Unrecognized checksum input type");
					return "";
			}
		}

		public static byte[] getBytes(ChecksumInputType checksumInputType, string input, Error error)
		{
			logger.Debug("getBytes");
			if (error == null) return null;
			EncodingUtil eu = new EncodingUtil();
			byte[] aux = null;
			switch (checksumInputType)
			{
				case ChecksumInputType.BASE64:
					try
					{
						aux = Base64.Decode(input);
					}
					catch (Exception e)
					{
						error.setError("CHI03", e.Message);
					}
					break;
				case ChecksumInputType.HEX:
					aux = SecurityUtils.HexaToByte(input, error);
					break;
				case ChecksumInputType.TXT:
					aux = eu.getBytes(input);
					if (eu.HasError())
					{
						error = eu.GetError();
					}
					break;
				case ChecksumInputType.ASCII:
					try
					{
						aux = new ASCIIEncoding().GetBytes(input);
					}
					catch (Exception e)
					{
						error.setError("CHI04", e.Message);
						logger.Error("getBytes", e);
					}
					break;
				case ChecksumInputType.LOCAL_FILE:
					aux = SecurityUtils.getFileBytes(input, error);
					break;
				default:
					error.setError("CHI05", "Unrecognized checksum input type");
					logger.Error("Unrecognized checksum input type");
					break;
			}
			return aux;
		}
	}
}
