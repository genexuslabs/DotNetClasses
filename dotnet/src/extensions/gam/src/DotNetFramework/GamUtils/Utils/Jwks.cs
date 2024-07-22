using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security;
using Jose;
using log4net;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1;
using Newtonsoft.Json;

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	public class Jwks
	{
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(Hash));

		[SecuritySafeCritical]
		internal static string GenerateKeyPair()
		{
			Jwk jwk = CreateJwk();
			if (jwk == null) { return ""; }
			return jwk.ToJson();
		}

		private static RSA Create(int keySizeInBits)
		{
			RSA rSA = (RSA)CryptoConfig.CreateFromName("RSAPSS");
			rSA.KeySize = keySizeInBits;
			if (rSA.KeySize != keySizeInBits)
			{
				throw new CryptographicException();
			}

			return rSA;
		}

		[SecuritySafeCritical]
		private static Jwk CreateJwk()
		{
			try
			{
				RSA key = Create(2048);

				RSAParameters parameters = key.ExportParameters(true);
				Jwk jwk = new Jwk
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
		internal static string GetB64PublicKeyFromJwk(string jwkString)
		{
			if (String.IsNullOrEmpty(jwkString))
			{
				logger.Error("GetB64PublicKeyFromJwk jwkString parameter is empty");
				return "";
			}
			else {
				logger.Debug("GetB64PublicKeyFromJwk jwkString parameter: " + jwkString);
			}
			Jwk jwk = Jwk.FromJson(jwkString);
			try
			{
				byte[] m = Base64Url.Decode(jwk.N);
				byte[] e = Base64Url.Decode(jwk.E);

				RsaKeyParameters parms = new RsaKeyParameters(false, new Org.BouncyCastle.Math.BigInteger(1, m), new Org.BouncyCastle.Math.BigInteger(1, e));
				SubjectPublicKeyInfo subpubkey = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(parms);
				return Convert.ToBase64String(subpubkey.GetEncoded());
			}
			catch (Exception e)
			{
				logger.Error("GetB64PublicKeyFromJwk", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		internal static string GetB64PrivateKeyFromJwk(string jwkString)
		{
			if (String.IsNullOrEmpty(jwkString))
			{
				logger.Error("GetB64PrivateKeyFromJwk jwkString parameter is empty");
				return "";
			}
			else
			{
				logger.Debug("GetB64PrivateKeyFromJwk jwkString parameter: " + jwkString);
			}
			Jwk jwk = Jwk.FromJson(jwkString);
			try
			{
				RsaPrivateKeyStructure keyStruct = new RsaPrivateKeyStructure(
					new Org.BouncyCastle.Math.BigInteger(1, Base64Url.Decode(jwk.N)),
					new Org.BouncyCastle.Math.BigInteger(1, Base64Url.Decode(jwk.E)),
					new Org.BouncyCastle.Math.BigInteger(1, Base64Url.Decode(jwk.D)),
					new Org.BouncyCastle.Math.BigInteger(1, Base64Url.Decode(jwk.P)),
					new Org.BouncyCastle.Math.BigInteger(1, Base64Url.Decode(jwk.Q)),
					new Org.BouncyCastle.Math.BigInteger(1, Base64Url.Decode(jwk.DP)),
					new Org.BouncyCastle.Math.BigInteger(1, Base64Url.Decode(jwk.DQ)),
					new Org.BouncyCastle.Math.BigInteger(1, Base64Url.Decode(jwk.QI))
					);
				AlgorithmIdentifier algID = new AlgorithmIdentifier(PkcsObjectIdentifiers.RsaEncryption, DerNull.Instance);
				PrivateKeyInfo privateKeyInfo = new PrivateKeyInfo(algID, keyStruct.ToAsn1Object());

				return Convert.ToBase64String(privateKeyInfo.GetEncoded());
			}
			catch (Exception e)
			{
				logger.Error("GetB64PrivateKeyFromJwk", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		internal static string GetPublicJwk(string jwkString)
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
				Jwk jwk = Jwk.FromJson(jwkString);
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
	}
}
