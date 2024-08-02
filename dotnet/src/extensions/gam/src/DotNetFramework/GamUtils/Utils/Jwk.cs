using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security;
using Jose;
using log4net;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	public class Jwk
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Jwk));

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/

		[SecuritySafeCritical]
		internal static string GenerateKeyPair()
		{
			Jose.Jwk jwk = CreateJwk();
			if (jwk == null) { return ""; }
			return jwk.ToJson();
		}


		[SecuritySafeCritical]
		internal static string GetPublic(string jwkString)
		{
			if (String.IsNullOrEmpty(jwkString))
			{
				logger.Error("GetPublicJwk jwkString parameter is empty");
				return "";
			}
			else
			{
				logger.Debug("GetPublicJwk jwkString parameter: " + jwkString);
			}
			try
			{
				Jose.Jwk jwk = Jose.Jwk.FromJson(jwkString);
				var dict = new Dictionary<string, string>
				{
					["kty"] = jwk.Kty,
					["e"] = jwk.E,
					["use"] = jwk.Use,
					["kid"] = jwk.KeyId,
					["alg"] = jwk.Alg,
					["n"] = jwk.N,
				};

				return JsonConvert.SerializeObject(dict);
			}
			catch (Exception e)
			{
				logger.Error("GetPublicJwk", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		public static string CreateJwt(string jwkString, string payload, string header)
		{
			if (jwkString.IsNullOrEmpty())
			{
				logger.Error("createJwt jwkString parameter is empty");
				return "";
			}
			if (payload.IsNullOrEmpty())
			{
				logger.Error("createJwt payload parameter is empty");
				return "";
			}
			if (header.IsNullOrEmpty())
			{
				logger.Error("createJwt header parameter is empty");
				return "";
			}
			try
			{
				return Jwt.Create(GetPrivateKey(jwkString), payload, header);
			}
			catch (Exception e)
			{
				logger.Error("createJwt", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		public static bool VerifyJWT(string jwkString, string token)
		{
			if (jwkString.IsNullOrEmpty())
			{
				logger.Error("verifyJWT jwkString parameter is empty");
				return false;
			}
			if (token.IsNullOrEmpty())
			{
				logger.Error("verifyJWT token parameter is empty");
				return false;
			}
			try
			{
				return Jwt.Verify(GetPublicKey(jwkString), token);
			}
			catch (Exception e)
			{
				logger.Error("verifyJWT", e);
				return false;
			}

		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		[SecuritySafeCritical]
		private static Jose.Jwk CreateJwk()
		{
			try
			{
				RSA key = Create(2048);

				RSAParameters parameters = key.ExportParameters(true);
				Jose.Jwk jwk = new Jose.Jwk
				{
					Kty = "RSA",
					Alg = "RS256",
					Use = "sig",
					KeyId = Guid.NewGuid().ToString(),
					N = Convert.ToBase64String(parameters.Modulus),
					E = Convert.ToBase64String(parameters.Exponent),
					P = Convert.ToBase64String(parameters.P),
					Q = Convert.ToBase64String(parameters.Q),
					DP = Convert.ToBase64String(parameters.DP),
					DQ = Convert.ToBase64String(parameters.DQ),
					QI = Convert.ToBase64String(parameters.InverseQ),
					D = Convert.ToBase64String(parameters.D)
				};

				return jwk;

			}
			catch (Exception e)
			{
				logger.Error("CreateJwk", e);
				return null;
			}
		}

		[SecuritySafeCritical]
		private static RSA Create(int keySizeInBits)
		{
#if NETCORE
			RSA rSA = RSA.Create();
#else
			RSA rSA = (RSA)CryptoConfig.CreateFromName("RSAPSS");
#endif
			rSA.KeySize = keySizeInBits;
			if (rSA.KeySize != keySizeInBits)
			{
				throw new CryptographicException();
			}

			return rSA;
		}

		private static RSAParameters GetPrivateKey(string jwkString)
		{
			Jose.Jwk jwk = Jose.Jwk.FromJson(jwkString);

			RSAParameters privateKey = new RSAParameters();

			privateKey.Exponent = Base64Url.Decode(jwk.E);
			privateKey.Modulus = Base64Url.Decode(jwk.N);
			privateKey.D = Base64Url.Decode(jwk.D);
			privateKey.DP = Base64Url.Decode(jwk.DP);
			privateKey.DQ = Base64Url.Decode(jwk.DQ);
			privateKey.P = Base64Url.Decode(jwk.P);
			privateKey.Q = Base64Url.Decode(jwk.Q);
			privateKey.InverseQ = Base64Url.Decode(jwk.QI);

			return privateKey;
		}

		internal static RSAParameters GetPublicKey(string jwkString)
		{
			Jose.Jwk jwk = Jose.Jwk.FromJson(jwkString);

			RSAParameters publicKey = new RSAParameters();

			publicKey.Exponent = Base64Url.Decode(jwk.E);
			publicKey.Modulus = Base64Url.Decode(jwk.N);

			return publicKey;
		}
	}
}
