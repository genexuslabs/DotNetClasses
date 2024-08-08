using System.Security;
using GamUtils.Utils;
using GamUtils.Utils.Cryprography;
using GamUtils.Utils.Json;

namespace GamUtils
{
	[SecuritySafeCritical]
	public class GamUtilsEO
	{
		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

		//**HASH**//
		[SecuritySafeCritical]
		public static string Sha512(string plainText)
		{
			return Hash.Sha512(plainText);
		}

		//**ENCRYPTION**//

		[SecuritySafeCritical]
		public static string AesGcm(string input, string key, string nonce, int macSize, bool toEncrypt)
		{
			return Encryption.AesGcm(input, key, nonce, macSize, toEncrypt);
		}

		//**RANDOM**//
		[SecuritySafeCritical]
		public static string RandomAlphanumeric(int length)
		{
			return Utils.Random.Alphanumeric(length);
		}

		[SecuritySafeCritical]
		public static string RandomNumeric(int length)
		{
			return Utils.Random.Numeric(length);
		}

		[SecuritySafeCritical]
		public static string RandomHexaBits(int bits)
		{
			return Utils.Random.HexaBits(bits);
		}

		//**JWK**//
		[SecuritySafeCritical]
		public static string GenerateKeyPair() { return Jwk.GenerateKeyPair(); }

		[SecuritySafeCritical]
		public static string GetPublicJwk(string jwkString) { return Jwk.GetPublic(jwkString); }

		[SecuritySafeCritical]
		public static string Jwk_createJwt(string jwkString, string payload, string header) { return Jwk.CreateJwt(jwkString, payload, header); }

		[SecuritySafeCritical]
		public static bool Jwk_verifyJWT(string jwkString, string token) { return Jwk.VerifyJWT(jwkString, token); }

		//**JWKS**//

		[SecuritySafeCritical]
		public static bool Jwks_verifyJWT(string jwksString, string token, string kid) { return Jwks.VerifyJWT(jwksString, token, kid); }

		//**JWT**//
		[SecuritySafeCritical]
		public static bool VerifyJWTWithFile(string path, string alias, string password, string token) { return Jwt.Verify(path, alias, password, token); }

		[SecuritySafeCritical]
		public static string CreateJWTWithFile(string path, string alias, string password, string payload, string header) { return Jwt.Create(path, alias, password, payload, header); }
	}
}
