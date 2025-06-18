using System;
using System.Text;
using log4net;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.Encoders;
using System.Security;

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	internal enum Hash
	{
	NONE, SHA512, SHA256
	}

	[SecuritySafeCritical]
	public class HashUtil
	{

		private static readonly ILog logger = LogManager.GetLogger(typeof(HashUtil));

		[SecuritySafeCritical]
		internal static string Hashing(string plainText, Hash hash)
		{
			switch (hash)
			{
				case Hash.SHA256:
					return InternalHash(new Sha256Digest(), plainText);
				case Hash.SHA512:
					return InternalHash(new Sha512Digest(), plainText);
				default:
					logger.Error("unrecognized hash");
					return "";
			}
		}

		private static string InternalHash(IDigest digest, string plainText)
		{
			logger.Debug("InternalHash");
			if (String.IsNullOrEmpty(plainText))
			{
				logger.Error("hash plainText is empty");
				return "";
			}
			byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			byte[] retValue = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(inputBytes, 0, inputBytes.Length);
			digest.DoFinal(retValue, 0);
			return Base64.ToBase64String(retValue);
		}
	}
}

