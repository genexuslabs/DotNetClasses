using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Cryptography;
using GamUtils.Utils.Json;
using GamUtils.Utils.Keys;
using Jose;
using log4net;
using Microsoft.IdentityModel.Tokens;

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	public class Jwt
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Jwt));

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/

		[SecuritySafeCritical]
		internal static bool Verify(string path, string alias, string password, string token)
		{
			logger.Debug("Verify");
			try
			{
				return Verify_internal(path, alias, password, token);
			} catch (Exception e)
			{
				logger.Error("Verify", e);
				Console.WriteLine(e.Message);
				return false;
			}
		}

		[SecuritySafeCritical]
		internal static string Create(string path, string alias, string password, string payload, string header)
		{
			logger.Debug("Create");
			try
			{
				return Create_internal(path, alias, password, payload, header);
			} catch (Exception e)
			{
				logger.Error("Create", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		internal static string GetHeader(string token)
		{
			logger.Debug("GetHeader");
			return GetParts(token, 0);
		}

		[SecuritySafeCritical]
		internal static string GetPayload(string token)
		{
			logger.Debug("GetPayload");
			return GetParts(token, 1);
		}

		internal static bool VerifyAlgorithm(string algorithm, string token)
		{
			logger.Debug("VerifyAlgorithm");
			try
			{
				return JwtHeader.Deserialize(GetHeader(token)).Alg.Equals(algorithm);

			}
			catch (Exception e)
			{
				logger.Error("VerifyAlgorithm", e);
				return false;
			}
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		[SecuritySafeCritical]
		private static string GetParts(string token, int part)
		{
			logger.Debug("GetParts");
			try
			{
				string[] parts = token.Split('.');
				return System.Text.Encoding.UTF8.GetString(Base64Url.Decode(parts[part]));
			}
			catch (Exception e)
			{
				logger.Error("GetParts", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		private static string Create_internal(string path, string alias, string password, string payload, string header)
		{
			logger.Debug("Create_internal");
			JwtHeader parsedHeader = JwtHeader.Deserialize(header);
			JWTAlgorithm algorithm = JWTAlgorithmUtils.GetJWTAlgoritm(parsedHeader.Alg);
			bool isSymmetric = JWTAlgorithmUtils.IsSymmetric(algorithm);
			if (isSymmetric)
			{
				string token = JWT.Encode(
				payload: payload,
						key: System.Text.Encoding.UTF8.GetBytes(password),
#if NETCORE
						algorithm: JWTAlgorithmUtils.GetJWSAlgorithm(algorithm),
#else
						algorithm: FindAlgorithm(parsedHeader.Alg),
#endif
						extraHeaders: parsedHeader,
						options: new JwtOptions { DetachPayload = false, EncodePayload = true }
						);
				return token;
			}
			else
			{
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
				{
					rsa.ImportParameters(PrivateKeyUtil.GetPrivateKey(path, alias, password));

					return JWT.Encode(
						payload: payload,
						key: rsa,
#if NETCORE
						algorithm: JWTAlgorithmUtils.GetJWSAlgorithm(algorithm),
#else
						algorithm: FindAlgorithm(parsedHeader.Alg),
#endif
						extraHeaders: parsedHeader,
						options: new JwtOptions { DetachPayload = false, EncodePayload = true }
						);
				}
			}
		}

		[SecuritySafeCritical]
		private static bool Verify_internal(string path, string alias, string password, string token)
		{
			logger.Debug("Verify_internal");
			JwtHeader parsedHeader = JwtHeader.Deserialize(GetHeader(token));
			JWTAlgorithm algorithm = JWTAlgorithmUtils.GetJWTAlgoritm(parsedHeader.Alg);
			bool isSymmetric = JWTAlgorithmUtils.IsSymmetric(algorithm);
			if(isSymmetric)
			{
				string payload = JWT.Decode(token, System.Text.Encoding.UTF8.GetBytes(password));
				return string.IsNullOrEmpty(payload) ? false : true;
			}
			else
			{
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
				{
					rsa.ImportParameters(PublicKeyUtil.GetPublicKey(path, alias, password, token));
#if NETCORE
					string payload = JWT.Decode(token, rsa, JWTAlgorithmUtils.GetJWSAlgorithm(algorithm));
#else
					string payload = JWT.Decode(token, rsa, FindAlgorithm(parsedHeader.Alg));
#endif
					return string.IsNullOrEmpty(payload) ? false : true;
				}
			}
		}

		private static JwsAlgorithm FindAlgorithm(string algorithm)
		{
			switch(algorithm.Trim())
			{
				case "HS256":
					return JwsAlgorithm.HS256;
				case "HS384":
					return JwsAlgorithm.HS384;
				case "HS512":
					return JwsAlgorithm.HS512;
				case "RS256":
					return JwsAlgorithm.RS256;
				case "RS512":
					return JwsAlgorithm.RS512;
				default:
					logger.Error("HMAC algorithm not implemented");
					return JwsAlgorithm.none;
			}
		}
		

	}
}
