
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;
using log4net;

namespace GeneXusJWT.GenexusJWTClaims
{
    [SecuritySafeCritical]
    public enum RegisteredClaim
    {
        NONE, iss, exp, sub, aud, nbf, iat, jti,
    }

    [SecuritySafeCritical]
    public static class RegisteredClaimUtils
    {

		private static readonly ILog logger = LogManager.GetLogger(typeof(RegisteredClaimUtils));
		public static string valueOf(RegisteredClaim registeredClaim, Error error)
        {
			logger.Debug("valueOf");
			if(error == null) return "Unknown registered claim";
			switch (registeredClaim)
            {
                case RegisteredClaim.iss:
                    return "iss";
                case RegisteredClaim.exp:
                    return "exp";
                case RegisteredClaim.sub:
                    return "sub";
                case RegisteredClaim.aud:
                    return "aud";
                case RegisteredClaim.nbf:
                    return "nbf";
                case RegisteredClaim.iat:
                    return "iat";
                case RegisteredClaim.jti:
                    return "jti";
                default:
                    error.setError("RC001", "Unknown registered Claim");
					logger.Error("Unknown registered claim");
                    return "Unknown registered claim";

            }
        }

        public static RegisteredClaim getRegisteredClaim(string registeredClaim, Error error)
        {
			logger.Debug("getRegisteredClaim");
			if(error == null) return RegisteredClaim.NONE;
			if (registeredClaim == null)
			{
				error.setError("RCL01", "Unknown registered Claim");
				logger.Error("Unknown registered claim");
				return RegisteredClaim.NONE;
			}
            switch (registeredClaim.Trim())
            {
                case "iss":
                    return RegisteredClaim.iss;
                case "exp":
                    return RegisteredClaim.exp;
                case "sub":
                    return RegisteredClaim.sub;
                case "aud":
                    return RegisteredClaim.aud;
                case "nbf":
                    return RegisteredClaim.nbf;
                case "iat":
                    return RegisteredClaim.iat;
                case "jti":
                    return RegisteredClaim.jti;
                default:
                    error.setError("RCL02", "Unknown registered Claim");
					logger.Error("Unknown registered claim");
					return RegisteredClaim.NONE;
            }
        }

        public static bool exists(string value)
        {
			if (value == null) return false;
            switch (value.Trim())
            {
                case "iss":
                case "exp":
                case "sub":
                case "aud":
                case "nbf":
                case "iat":
                case "jti":
                    return true;
                default:
                    return false;
            }
        }

        public static bool isTimeValidatingClaim(string claimKey)
        {
			if (claimKey == null) return false;
            switch (claimKey.Trim())
            {
                case "iat":
                case "exp":
                case "nbf":
                    return true;
                default:
                    return false;
            }
        }

        public static bool validateClaim(string registeredClaimKey, string registeredClaimValue, long registeredClaimCustomTime, JwtSecurityToken token, Error error)
        {
			logger.Debug("validateClaim");
			if (error == null) return false;
            RegisteredClaim claim = RegisteredClaimUtils.getRegisteredClaim(registeredClaimKey, error);
            if (error.existsError())
            {
                return false;
            }

			if(token == null)
			{
				error.setError("RCL13", "Token parameter is null");
				logger.Error("Token parameter is null");
				return false;
			}
            Int32 newTime = 0;
            switch (claim)
            {
                case RegisteredClaim.iss:
                    return SecurityUtils.compareStrings(token.Payload.Iss, registeredClaimValue);
                case RegisteredClaim.exp:
                    int? exp = token.Payload.Exp;
                    if (exp == null)
                    {
                        return false;
                    }

                    if (registeredClaimCustomTime != 0)
                    {
                        newTime = (Int32)(DateTime.UtcNow.AddSeconds(-1*registeredClaimCustomTime).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    }
                    else
                    {
                        newTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    }
                    return (Int32)exp >= newTime;
                case RegisteredClaim.sub:
                    return SecurityUtils.compareStrings(token.Payload.Sub, registeredClaimValue);
                case RegisteredClaim.aud:
                    IList<string> audience = token.Payload.Aud;
                    return SecurityUtils.compareStrings(audience[0], registeredClaimValue);
                case RegisteredClaim.nbf:
                    int? nbf = token.Payload.Nbf;
                    if (nbf == null)
                    {
                        return false;
                    }

                    if (registeredClaimCustomTime != 0)
                    {
                        newTime = (Int32)(DateTime.UtcNow.AddSeconds(-(double)registeredClaimCustomTime).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    }
                    else
                    {
                        newTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    }
                    return (Int32)nbf >= newTime;
                case RegisteredClaim.iat:
                    int? iat = token.Payload.Iat;
                    if (iat == null)
                    {
                        return false;
                    }
                    return SecurityUtils.compareStrings(iat.ToString(), registeredClaimValue);
                case RegisteredClaim.jti:
                    return SecurityUtils.compareStrings(token.Payload.Jti, registeredClaimValue);
                default:
                    error.setError("RCL03", "Unknown registered Claim");
					logger.Error("Unknown registered claim");
					return false;
            }
        }
    }
}
