using System;

namespace GeneXus.Encryption
{

	public class Crypto
	{

		public static String Encrypt64(String value, String key)
		{
			return CryptoImpl.Encrypt64(value, key);
		}
		
		public static String Decrypt64(String value, String key)
		{
			return CryptoImpl.Decrypt64(value, key);
		}

		
		public static string GetServerKey()
		{
			return CryptoImpl.GetServerKey();
		}

		public static string GetSiteKey()
		{
			return CryptoImpl.GetSiteKey();
		}


		public static String CheckSum(String value, int length)
		{
			return CryptoImpl.CheckSum(value, length);
		}

	

		public static string GetEncryptionKey()
		{
			return CryptoImpl.GetEncryptionKey();
		}


	}
}
