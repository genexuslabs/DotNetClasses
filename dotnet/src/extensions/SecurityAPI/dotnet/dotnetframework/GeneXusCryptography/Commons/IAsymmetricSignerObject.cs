
using SecurityAPICommons.Commons;
using SecurityAPICommons.Keys;
using System.Security;

namespace GeneXusCryptography.Commons
{
    /// <summary>
    /// AsymmetricSignerObject interface fr EO
    /// </summary>
    [SecuritySafeCritical]
    public interface IAsymmetricSignerObject
    {

        string DoSign(PrivateKeyManager key, string hashAlgorithm, string plainText);

        bool DoVerify(CertificateX509 cert, string plainText, string signature);
    }
}
