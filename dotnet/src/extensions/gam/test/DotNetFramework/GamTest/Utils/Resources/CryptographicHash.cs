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
}
