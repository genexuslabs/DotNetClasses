using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Cryptography;
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
			return Verify(CertificateUtil.GetCertificate(path, alias, password), token);
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		[SecuritySafeCritical]
		public static bool Verify(RSAParameters publicKey, string token)
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
		public static string Create(RSAParameters privateKey, string payload, string header)
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
