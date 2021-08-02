using SecurityAPICommons.Commons;
using SecurityAPICommons.Keys;
using System.Security;


namespace GeneXusCryptography.Commons
{
    /// <summary>
    /// IAsymmetricCipherObject interface for EO
    /// </summary>
    [SecuritySafeCritical]
    public interface IAsymmetricCipherObject
    {


        string DoEncrypt_WithPrivateKey(string hashAlgorithm, string asymmetricEncryptionPadding, PrivateKeyManager key, string plainText);

        string DoEncrypt_WithPublicKey(string hashAlgorithm, string asymmetricEncryptionPadding, CertificateX509 certificate, string plainText);

        string DoDecrypt_WithPrivateKey(string hashAlgorithm, string asymmetricEncryptionPadding, PrivateKeyManager key, string encryptedInput);

        string DoDecrypt_WithPublicKey(string hashAlgorithm, string asymmetricEncryptionPadding, CertificateX509 certificate, string encryptedInput);
    }
}
