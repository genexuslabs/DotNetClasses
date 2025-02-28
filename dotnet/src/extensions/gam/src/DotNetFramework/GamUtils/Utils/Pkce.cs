using System.Security;
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
			string code_verifier = Random.Alphanumeric(len);
			switch (option.ToUpper().Trim())
			{
				case "S256":
					byte[] digest = Hash(new Sha256Digest(), System.Text.Encoding.UTF8.GetBytes(code_verifier.Trim()));
					return $"{code_verifier.Trim()},{System.Text.Encoding.UTF8.GetString(UrlBase64.Encode(digest))}";
				case "PLAIN":
					return $"{code_verifier.Trim()},{Encoding.ToBase64Url(code_verifier.Trim())}";
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
					byte[] digest = Hash(new Sha256Digest(), System.Text.Encoding.UTF8.GetBytes(code_verifier.Trim()));
					return System.Text.Encoding.UTF8.GetString(UrlBase64.Encode(digest)).Equals(code_challenge.Trim());
				case "PLAIN":
					byte[] bytes_plain = UrlBase64.Decode(System.Text.Encoding.UTF8.GetBytes(code_challenge.Trim()));
					return System.Text.Encoding.UTF8.GetString(bytes_plain).Equals(code_verifier.Trim());
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
	}
}
