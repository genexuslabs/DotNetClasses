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

		//**ENCODING**//
		public static string Base64ToBase64Url(string base64) { return Encoding.B64ToB64Url(base64); }
	}
}
