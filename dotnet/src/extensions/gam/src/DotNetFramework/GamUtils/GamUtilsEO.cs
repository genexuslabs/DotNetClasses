using System;
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
			return HashUtil.Hashing(plainText, Hash.SHA512);
		}

		[SecuritySafeCritical]
		public static string Sha256(string plainText)
		{
			return HashUtil.Hashing(plainText, Hash.SHA256);
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

		[SecuritySafeCritical]
		public static string RandomUrlSafeCharacters(int length)
		{
			return Utils.Random.UrlSafe(length);
		}

		//**JWK**//
		[SecuritySafeCritical]
		public static string GenerateKeyPair() { return Jwk.GenerateKeyPair(); }

		[SecuritySafeCritical]
		public static string GetPublicJwk(string jwkString) { return Jwk.GetPublic(jwkString); }

		[SecuritySafeCritical]
		public static string GetJwkAlgorithm(string jwkString) { return Jwk.GetAlgorithm(jwkString); }

		//**JWT**//
		[SecuritySafeCritical]
		public static bool VerifyJwt(string path, string alias, string password, string token) { return Jwt.Verify(path, alias, password, token); }

		[SecuritySafeCritical]
		public static string CreateJwt(string path, string alias, string password, string payload, string header) { return Jwt.Create(path, alias, password, payload, header); }

		[SecuritySafeCritical]
		public static long CreateUnixTimestamp(DateTime date) { return UnixTimestamp.Create(date); }

		[SecuritySafeCritical]
		public static string GetJwtHeader(string token) { return Jwt.GetHeader(token); }

		[SecuritySafeCritical]
		public static string GetJwtPayload(string token) { return Jwt.GetPayload(token); }

		[SecuritySafeCritical]
		public static bool VerifyAlgorithm(string expectedAlgorithm, string token) { return Jwt.VerifyAlgorithm(expectedAlgorithm, token); }

		//**ENCODING**//
		[SecuritySafeCritical]
		public static string Base64ToBase64Url(string base64) { return Encoding.B64ToB64Url(base64); }

		[SecuritySafeCritical]
		public static string HexaToBase64(string hexa) { return Encoding.HexaToBase64(hexa); }

		[SecuritySafeCritical]
		public static string ToBase64Url(string input) { return Encoding.ToBase64Url(input); }

		[SecuritySafeCritical]
		public static string FromBase64Url(string base64) { return Encoding.FromBase64Url(base64); }

		[SecuritySafeCritical]
		public static string Base64ToHexa(string base64) { return Encoding.Base64ToHexa(base64);  }

		//**PKCE**//

		[SecuritySafeCritical]
		public static string Pkce_Create(int len, string option) { return Pkce.Create(len, option);  }

		[SecuritySafeCritical]
		public static bool Pkce_Verify(string code_verifier, string code_challenge, string option) { return Pkce.Verify(code_verifier, code_challenge, option);  }

	}
}
