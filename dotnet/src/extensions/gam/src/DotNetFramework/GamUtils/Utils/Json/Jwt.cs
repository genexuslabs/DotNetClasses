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
		public static bool Verify(string path, string alias, string password, string token)
		{
			logger.Debug("Verify");
			try
			{
				return Verify(PublicKeyUtil.GetPublicKey(path, alias, password, token), token);
			}catch(Exception e)
			{
				logger.Error("Verify", e);
				return false;
			}
		}

		[SecuritySafeCritical]
		public static string Create(string path, string alias, string password, string payload, string header)
		{
			logger.Debug("Create");
			try
			{
				return Create(PrivateKeyUtil.GetPrivateKey(path, alias, password), payload, header);
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
		private static bool Verify(RSAParameters publicKey, string token)
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
				logger.Error("verify", e);
				return false;
			}
		}


		[SecuritySafeCritical]
		private static string Create(RSAParameters privateKey, string payload, string header)
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
				logger.Error("create", e);
				return "";
			}
		}


	}
}
