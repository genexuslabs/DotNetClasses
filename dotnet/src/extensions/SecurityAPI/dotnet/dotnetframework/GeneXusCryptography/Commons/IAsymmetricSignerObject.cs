
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
		string DoSignFile(PrivateKeyManager key, string hashAlgorithm, string path);

		bool DoVerify(CertificateX509 cert, string plainText, string signature);
		bool DoVerifyFile(CertificateX509 cert, string path, string signature);
	}
}
