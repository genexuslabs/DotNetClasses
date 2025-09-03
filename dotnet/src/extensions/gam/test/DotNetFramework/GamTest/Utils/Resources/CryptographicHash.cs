using System;
using System.Text;
using System.Security.Cryptography;

namespace GamTest.Utils.Resources
{
	internal class CryptographicHash
	{
		HashAlgorithm alg;
		public CryptographicHash(string algorithm)
		{
			// Supports algorithm = {MD5, RIPEMD160, SHA1, SHA256, SHA384, SHA512}
			if (String.IsNullOrEmpty(algorithm))
				algorithm = "SHA256";

#if NETCORE
			alg = CryptoUtils.CreateHashAlgorithm(algorithm);
#else
			alg = HashAlgorithm.Create(algorithm);
#endif
		}
		static public CryptographicHash Create(string algorithm)
		{
			return new CryptographicHash(algorithm);
		}
		public string ComputeHash(string data)
		{
			byte[] bin = Encoding.UTF8.GetBytes(data);
			return Convert.ToBase64String(alg.ComputeHash(bin));
		}
	}

	internal class CryptoUtils
	{
		internal static HashAlgorithm CreateHashAlgorithm(string hashAlgorithmName)
		{
			switch (hashAlgorithmName)
			{
				case "SHA256": case "SHA-256": case "System.Security.Cryptography.SHA256": return SHA256.Create();
				case "SHA384": case "SHA-384": case "System.Security.Cryptography.SHA384": return SHA384.Create();
				case "SHA512": case "SHA-512": case "System.Security.Cryptography.SHA512": return SHA512.Create();
				default: return null;
			}
		}
	}
}