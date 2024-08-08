using System;
using System.Security;
using Jose;
using Microsoft.IdentityModel.Tokens;
using log4net;
namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	public class Jwks
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Jwks));

		[SecuritySafeCritical]
		internal static bool VerifyJWT(string jwksString, string token, string kid)
		{
			if (jwksString.IsNullOrEmpty())
			{
				logger.Error("verifyJWT jwksString parameter is empty");
				return false;
			}
			if (token.IsNullOrEmpty())
			{
				logger.Error("verifyJWT token parameter is empty");
				return false;
			}
			if (kid.IsNullOrEmpty())
			{
				logger.Error("verifyJWT kid parameter is empty");
				return false;
			}
			try
			{
				JwkSet set = JwkSet.FromJson(jwksString);
				foreach (Jose.Jwk jwk in set)
				{
					if (jwk.KeyId.Equals(kid))
					{
						return Jwt.Verify(Json.Jwk.GetPublicKey(jwk.ToJson()), token);
					}
				}
				logger.Error("Could not find indicated kid");
				return false;
			}
			catch (Exception e)
			{
				logger.Error("verifyJWT", e);
				return false;
			}
		}
	}
}
