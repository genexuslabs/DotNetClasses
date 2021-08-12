
using GeneXusJWT.GenexusJWTClaims;
using SecurityAPICommons.Commons;
using System.Security;

namespace GeneXusJWT.GenexusComons
{
    [SecuritySafeCritical]
    public interface IJWTObject
    {
        string DoCreate(string algorithm, PrivateClaims privateClaims, JWTOptions options);
        bool DoVerify(string token, string expectedAlgorithm, PrivateClaims privateClaims, JWTOptions options);
        string GetPayload(string token);
        string GetHeader(string token);
        string GetTokenID(string token);

    }
}
