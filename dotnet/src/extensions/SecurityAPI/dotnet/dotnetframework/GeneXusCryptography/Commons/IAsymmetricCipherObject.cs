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

#pragma warning disable CA1707 // Identifiers should not contain underscores
		string DoEncrypt_WithPrivateKey(string hashAlgorithm, string asymmetricEncryptionPadding, PrivateKeyManager key, string plainText);

        string DoEncrypt_WithCertificate(string hashAlgorithm, string asymmetricEncryptionPadding, CertificateX509 certificate, string plainText);

        string DoDecrypt_WithPrivateKey(string hashAlgorithm, string asymmetricEncryptionPadding, PrivateKeyManager key, string encryptedInput);

		string DoDecrypt_WithCertificate(string hashAlgorithm, string asymmetricEncryptionPadding, CertificateX509 certificate, string encryptedInput);
#pragma warning restore CA1707 // Identifiers should not contain underscores
	}
}
