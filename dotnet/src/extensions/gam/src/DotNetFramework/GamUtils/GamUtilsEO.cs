using System;
using System.Security;
using GamUtils.Utils;

namespace GamUtils
{
	[SecuritySafeCritical]
	public class GamUtilsEO
	{

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

		//**HASH**//
		[SecuritySafeCritical]
		public static string Sha512(String plainText)
		{
			return Hash.Sha512(plainText);
		}

		//**RANDOM**//
		[SecuritySafeCritical]
		public static string RandomAlphanumeric(int length)
		{
			return Utils.Random.RandomAlphanumeric(length);
		}

		[SecuritySafeCritical]
		public static string RandomNumeric(int length)
		{
			return Utils.Random.RandomNumeric(length);
		}


		//**JWKS**//
		[SecuritySafeCritical]
		public static string GenerateKeyPair() { return Jwks.GenerateKeyPair(); }

		[SecuritySafeCritical]
		public static string GetB64PublicKeyFromJwk(string jwkString) { return Jwks.GetB64PublicKeyFromJwk(jwkString); }

		[SecuritySafeCritical]
		public static string GetB64PrivateKeyFromJwk(string jwkString) { return Jwks.GetB64PrivateKeyFromJwk(jwkString); }

		[SecuritySafeCritical]
		public static string GetPublicJwk(string jwkString) { return Jwks.GetPublicJwk(jwkString); }


		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/
	}
}
