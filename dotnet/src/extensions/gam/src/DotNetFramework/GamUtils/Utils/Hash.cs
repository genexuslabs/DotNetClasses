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
	public class Hash
	{

		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(Hash));

		[SecuritySafeCritical]
		internal static string Sha512(string plainText)
		{

			if (String.IsNullOrEmpty(plainText))
			{
				logger.Error("sha512 plainText is empty");
				return "";
			}
			byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
			IDigest alg = new Sha512Digest();
			byte[] retValue = new byte[alg.GetDigestSize()];
			alg.BlockUpdate(inputBytes, 0, inputBytes.Length);
			alg.DoFinal(retValue, 0);
			return Base64.ToBase64String(retValue);
		}
	}
}
