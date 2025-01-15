using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Cryptography;
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
		public static bool Verify(string path, string alias, string password, string token, string secret, bool isSymmetric)
		{
			logger.Debug("Verify");
			try
			{
				return !isSymmetric ? VerifyRsa(PublicKeyUtil.GetPublicKey(path, alias, password, token), token) : VerifySha(secret, token);
			}catch(Exception e)
			{
				logger.Error("Verify", e);
				return false;
			}
		}

		[SecuritySafeCritical]
		public static string Create(string path, string alias, string password, string payload, string header, string secret, bool isSymmetric)
		{
			Console.WriteLine("Create");
			logger.Debug("Create");
			try
			{
				return !isSymmetric ? CreateRsa(PrivateKeyUtil.GetPrivateKey(path, alias, password), payload, header) : CreateSha(secret, payload, header);
			}catch(Exception e)
			{
				logger.Error("Create", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		public static string GetHeader(string token)
		{
			logger.Debug("GetHeader");
			return GetParts(token, 0);
		}

		[SecuritySafeCritical]
		public static string GetPayload(string token)
		{
			logger.Debug("GetPayload");
			return GetParts(token, 1);
		}

		public static bool VerifyAlgorithm(string algorithm, string token)
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
		private static bool VerifyRsa(RSAParameters publicKey, string token)
		{
			try
			{
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
				{
					rsa.ImportParameters(publicKey);
					string payload = JWT.Decode(token, rsa, JwsAlgorithm.RS256);
					return payload.IsNullOrEmpty() ? false : true;
				}
			}
			catch (Exception e)
			{
				logger.Error("VerifyRsa", e);
				return false;
			}
		}


		[SecuritySafeCritical]
		private static string CreateRsa(RSAParameters privateKey, string payload, string header)
		{
			try
			{
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
				{
					rsa.ImportParameters(privateKey);

					return JWT.Encode(
						payload: payload,
						key: rsa,
						algorithm: JwsAlgorithm.RS256,
						extraHeaders: JwtHeader.Deserialize(header),
						options: new JwtOptions { DetachPayload = false, EncodePayload = true }
						);
				}

			}
			catch (Exception e)
			{
				logger.Error("CreateRsa", e);
				return "";
			}
		}


		[SecuritySafeCritical]
		public static string CreateSha(string secret, string payload, string header)
		{
			JwsAlgorithm alg = FindShaAlgorithm(JwtHeader.Deserialize(header).Alg);

			try
			{	
					string token = JWT.Encode(
						payload: payload,
						key: System.Text.Encoding.UTF8.GetBytes(secret),
						algorithm: alg,
						extraHeaders: JwtHeader.Deserialize(header),
						options: new JwtOptions { DetachPayload = false, EncodePayload = true }
						);
				return token;
			}
			catch (Exception e)
			{
				Console.WriteLine("exception: " + e.Message);
				logger.Error("CreateSha", e);
				return "";
			}
		}

		[SecuritySafeCritical]
		private static bool VerifySha(string secret, string token)
		{
			try
			{
					string payload = JWT.Decode(token, System.Text.Encoding.UTF8.GetBytes(secret));
					return payload.IsNullOrEmpty() ? false : true;
			}
			catch (Exception e)
			{
				logger.Error("VerifySha", e);
				return false;
			}
		}


		private static JwsAlgorithm FindShaAlgorithm(string algorithm)
		{
			switch(algorithm.Trim())
			{
				case "HS256":
					return JwsAlgorithm.HS256;
				case "HS384":
					return JwsAlgorithm.HS384;
				case "HS512":
					return JwsAlgorithm.HS512;
				default:
					logger.Error("HMAC algorithm not implemented");
					return JwsAlgorithm.none;
			}
		}

	}
}
