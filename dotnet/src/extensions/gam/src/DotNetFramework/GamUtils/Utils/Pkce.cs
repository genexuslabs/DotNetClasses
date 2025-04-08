using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using GeneXus;
using log4net;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities.Encoders;

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	public class Pkce
    {
		private static readonly ILog logger = LogManager.GetLogger(typeof(Encoding));

		[SecuritySafeCritical]
		internal static string Create(int len, string option)
		{
			logger.Trace("Create");
			byte[] code_verifier_bytes = GetRandomBytes(len);
			string code_verifier = System.Text.Encoding.UTF8.GetString(UrlBase64.Encode(code_verifier_bytes));
			switch (option.ToUpper().Trim())
			{
				case "S256":
					byte[] digest = Hash(new Sha256Digest(), System.Text.Encoding.ASCII.GetBytes(code_verifier));
					return $"{code_verifier},{Jose.Base64Url.Encode(digest)}";
				case "PLAIN":
					return $"{code_verifier},{code_verifier}";
				default:
					logger.Error("Unknown PKCE option");
					return "";
			}
		}

		[SecuritySafeCritical]
		public static bool Verify(string code_verifier, string code_challenge, string option)
		{
			logger.Trace("Verify");
			switch (option.ToUpper().Trim())
			{
				case "S256":
					byte[] digest = Hash(new Sha256Digest(), System.Text.Encoding.ASCII.GetBytes(code_verifier));
					return Jose.Base64Url.Encode(digest).Equals(code_challenge.Trim());
				case "PLAIN":
					return code_challenge.Trim().Equals(code_verifier.Trim());
				default:
					logger.Error("Unknown PKCE option");
					return false;
			}
		}

		private static byte[] Hash(IDigest digest, byte[] inputBytes)
		{
			byte[] retValue = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(inputBytes, 0, inputBytes.Length);
			digest.DoFinal(retValue, 0);
			return retValue;
		}

		private static byte[] GetRandomBytes(int len)
		{
			byte[] data = new byte[len];
#if NETCORE
			var arraySpan = new Span<byte>(data);
			System.Security.Cryptography.RandomNumberGenerator.Fill(arraySpan);
#else
			RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
			crypto.GetBytes(data);
#endif
			return data;
		}
	}
}
