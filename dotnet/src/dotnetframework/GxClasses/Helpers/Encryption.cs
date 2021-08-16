using System;

namespace GeneXus.Encryption
{

	public class Crypto
	{

		public static string Encrypt64(string value, string key)
		{
			return CryptoImpl.Encrypt64(value, key);
		}
		public static string Encrypt64(string value, string key, bool safeEncoding)
		{
			return CryptoImpl.Encrypt64(value, key, safeEncoding);
		}

		public static string Decrypt64(string value, string key)
		{
			return CryptoImpl.Decrypt64(value, key);
		}

		public static string Decrypt64(string value, string key, bool safeEncoding)
		{
			return CryptoImpl.Decrypt64(value, key, safeEncoding);
		}
		public static string GetServerKey()
		{
			return CryptoImpl.GetServerKey();
		}

		public static string GetSiteKey()
		{
			return CryptoImpl.GetSiteKey();
		}


		public static string CheckSum(string value, int length)
		{
			return CryptoImpl.CheckSum(value, length);
		}

	

		public static string GetEncryptionKey()
		{
			return CryptoImpl.GetEncryptionKey();
		}


	}
}
