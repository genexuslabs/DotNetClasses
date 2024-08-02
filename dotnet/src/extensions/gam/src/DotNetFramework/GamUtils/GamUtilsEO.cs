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


		//**JWK**//
		[SecuritySafeCritical]
		public static string GenerateKeyPair() { return Jwk.GenerateKeyPair(); }

		public static string GetPublicJwk(string jwkString) { return Jwk.GetPublic(jwkString); }

		public static string Jwk_createJwt(string jwkString, string payload, string header) { return Jwk.CreateJwt(jwkString, payload, header); }

		public static bool Jwk_verifyJWT(string jwkString, string token) { return Jwk.VerifyJWT(jwkString, token); }

		//**JWKS**//

		public static bool Jwks_verifyJWT(string jwksString, string token, string kid) { return Jwks.VerifyJWT(jwksString, token, kid); }

		//**JWT**//
		public static bool VerifyJWTWithFile(string path, string alias, string password, string token) { return Jwt.Verify(path, alias, password, token); }
	}
}
