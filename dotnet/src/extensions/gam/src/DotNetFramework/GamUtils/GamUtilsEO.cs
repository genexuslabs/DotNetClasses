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

		//**JWK**//
		[SecuritySafeCritical]
		public static string GenerateKeyPair() { return Jwk.GenerateKeyPair(); }

		[SecuritySafeCritical]
		public static string GetPublicJwk(string jwkString) { return Jwk.GetPublic(jwkString); }

		[SecuritySafeCritical]
		public static string GetJwkAlgorithm(string jwkString) { return Jwk.GetAlgorithm(jwkString); }

		//**JWT**//
		[SecuritySafeCritical]
		public static bool VerifyJwtRsa(string path, string alias, string password, string token) { return Jwt.Verify(path, alias, password, token, "", false); }

		[SecuritySafeCritical]
		public static string CreateJwtRsa(string path, string alias, string password, string payload, string header) { return Jwt.Create(path, alias, password, payload, header, "", false); }

		[SecuritySafeCritical]
		public static string CreateJwtSha(string secret, string payload, string header) { return Jwt.Create("", "", "", payload, header, secret, true); }

		[SecuritySafeCritical]
		public static bool VerifyJwtSha(string secret, string token) { return Jwt.Verify("", "", "", token, secret, true); }

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
	}
}
