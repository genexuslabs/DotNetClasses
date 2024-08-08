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
			return Verify(CertificateUtil.GetCertificate(path, alias, password), token);
		}

		[SecuritySafeCritical]
		public static string Create(string path, string alias, string password, string payload, string header)
		{
			return Create(PrivateKeyUtil.GetPrivateKey(path, alias, password), payload, header);
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		[SecuritySafeCritical]
		public static bool Verify(RSAParameters publicKey, string token)
		{

			Console.WriteLine("token: " + token);

			try
			{
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
				{
					rsa.ImportParameters(publicKey);
					string payload = JWT.Decode(token, rsa, JwsAlgorithm.RS256);
					
					if(payload.IsNullOrEmpty())
					{
						Console.WriteLine("payload null or empty");
							return false;
					}
					else
					{
						return true;
					}
					//return payload.IsNullOrEmpty() ? false : true;
				}
			}
			catch (Exception e)
			{
				logger.Error("verify", e);
				Console.WriteLine("error verify");
				Console.WriteLine(e.Message);
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
