using SecurityAPICommons.Commons;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security;



namespace GeneXusJWT.GenexusJWTUtils
{
    [SecuritySafeCritical]
    public enum JWTAlgorithm
    {
        NONE, HS256, HS512, RS256, RS512, ES256, ES384, ES512
    }

    [SecuritySafeCritical]
    public class JWTAlgorithmUtils
    {
        public static string valueOf(JWTAlgorithm jWTAlgorithm, Error error)
        {
            switch (jWTAlgorithm)
            {
                case JWTAlgorithm.HS256:
                    return "HS256";
                case JWTAlgorithm.HS512:
                    return "HS512";
                case JWTAlgorithm.RS256:
                    return "RS256";
                case JWTAlgorithm.RS512:
                    return "RS512";
                case JWTAlgorithm.ES256:
                    return "ES256";
                case JWTAlgorithm.ES384:
                    return "ES384";
                case JWTAlgorithm.ES512:
                    return "ES512";

                default:
                    error.setError("JA001", "Unrecognized algorithm");
                    return "Unrecognized algorithm";
            }
        }

        public static JWTAlgorithm getJWTAlgorithm(string jWTAlgorithm, Error error)
        {
            switch (jWTAlgorithm.ToUpper().Trim())
            {
                case "HS256":
                    return JWTAlgorithm.HS256;
                case "HS512":
                    return JWTAlgorithm.HS512;
                case "RS256":
                    return JWTAlgorithm.RS256;
                case "RS512":
                    return JWTAlgorithm.RS512;
                case "ES256":
                    return JWTAlgorithm.ES256;
                case "ES384":
                    return JWTAlgorithm.ES384;
                case "ES512":
                    return JWTAlgorithm.ES512;

                default:
                    error.setError("JA002", "Unrecognized algorithm");
                    return JWTAlgorithm.NONE;
            }
        }

        public static JWTAlgorithm getJWTAlgorithm_forVerification(string jWTAlgorithm, Error error)
        {
            switch (jWTAlgorithm)
            {
                case SecurityAlgorithms.RsaSha256:
                    return JWTAlgorithm.RS256;
                case SecurityAlgorithms.RsaSha512:
                    return JWTAlgorithm.RS512;
                case SecurityAlgorithms.HmacSha256:
                    return JWTAlgorithm.HS256;
                case SecurityAlgorithms.HmacSha512:
                    return JWTAlgorithm.HS512;
                case SecurityAlgorithms.EcdsaSha256:
                    return JWTAlgorithm.ES256;
                case SecurityAlgorithms.EcdsaSha384:
                    return JWTAlgorithm.ES384;
                case SecurityAlgorithms.EcdsaSha512:
                    return JWTAlgorithm.ES512;
                default:
                    error.setError("JA004", "Unrecognized algorithm");
                    return JWTAlgorithm.NONE;
            }
        }


        public static bool isPrivate(JWTAlgorithm jWTAlgorithm)
        {
            switch (jWTAlgorithm)
            {
                case JWTAlgorithm.RS256:
                case JWTAlgorithm.RS512:
                case JWTAlgorithm.ES256:
                case JWTAlgorithm.ES384:
                case JWTAlgorithm.ES512:

                    return true;
                default:
                    return false;
            }
        }

        internal static SigningCredentials getSigningCredentials(JWTAlgorithm jWTAlgorithm, SecurityKey key, Error error)
        {
            switch (jWTAlgorithm)
            {
                case JWTAlgorithm.HS256:
                    return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                case JWTAlgorithm.HS512:
                    return new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
                case JWTAlgorithm.RS256:
                    return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
                case JWTAlgorithm.RS512:
                    return new SigningCredentials(key, SecurityAlgorithms.RsaSha512);
                case JWTAlgorithm.ES256:
                    return new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);
                case JWTAlgorithm.ES384:
                    return new SigningCredentials(key, SecurityAlgorithms.EcdsaSha384);
                case JWTAlgorithm.ES512:
                    return new SigningCredentials(key, SecurityAlgorithms.EcdsaSha512);

                default:
                    error.setError("JA003", "Unknown algorithm");
                    return null;
            }
        }



    }
}
