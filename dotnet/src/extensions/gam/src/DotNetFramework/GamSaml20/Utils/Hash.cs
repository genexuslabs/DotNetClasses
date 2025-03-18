using System;
using GeneXus;
using log4net;

namespace GamSaml20.Utils
{
	internal enum Hash
	{
		NONE, SHA1, SHA256, SHA512

	}

	internal class HashUtils
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(HashUtils));
		internal static Hash GetHash(string hash)
		{
			logger.Trace("GetHash");
			switch (hash.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
			{
				case "SHA1":
					return Hash.SHA1;
				case "SHA256":
					return Hash.SHA256;
				case "SHA512":
					return Hash.SHA512;
				default:
					logger.Error($"GetHash - not implemented signature hash: {hash}");
					return Hash.NONE;
			}
		}

		internal static string ValueOf(Hash hash)
		{
			switch (hash)
			{
				case Hash.SHA1:
					return "SHA1";
				case Hash.SHA256:
					return "SHA256";
				case Hash.SHA512:
					return "SHA512";
				default:
					return String.Empty;
			}

		}

		internal static string GetSigAlg(Hash hash)
		{
			logger.Trace("GetSigAlg");
			switch (hash)
			{
				case Hash.SHA1:
					return @"http://www.w3.org/2001/04/xmldsig-more#rsa-sha1";
				case Hash.SHA256:
					return @"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
				case Hash.SHA512:
					return @"http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";
				default:
					logger.Error("GetSigAlg - not implemented signature hash");
					return String.Empty;
			}
		}

		internal static Hash GetHashFromSigAlg(string sigAlg)
		{
			logger.Trace("GetHashFromSigAlg");
			switch (sigAlg.Trim())
			{
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha1":
					return Hash.SHA1;
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
					return Hash.SHA256;
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512":
					return Hash.SHA512;
				default:
					logger.Error($"GetHashFromSigAlg - not implemented signature algorithm: {sigAlg}");
					return Hash.NONE;

			}
		}
	}
}
