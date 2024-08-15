using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security;
using log4net;
using Newtonsoft.Json;

namespace GamUtils.Utils.Json
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
			if (string.IsNullOrEmpty(jwkString))
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
	}
}
