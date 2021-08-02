
using GeneXusCryptography.AsymmetricUtils;
using GeneXusCryptography.Commons;
using GeneXusCryptography.Hash;
using GeneXusCryptography.HashUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Utilities.Encoders;
using System.Security;


namespace GeneXusCryptography.Asymmetric
{
    /// <summary>
    /// Implements Asymmetric Block Cipher Engines and methods to encrypt and decrypt
    /// </summary>
    [SecuritySafeCritical]
    public class AsymmetricCipher : SecurityAPIObject, IAsymmetricCipherObject
    {

        /// <summary>
        /// AsymmetricCipher class constructor
        /// </summary>
        public AsymmetricCipher() : base()
        {

        }

        /********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

        [SecuritySafeCritical]
        public string DoEncrypt_WithPrivateKey(string hashAlgorithm, string asymmetricEncryptionPadding, PrivateKeyManager key, string plainText)
        {

            if (this.HasError() || key == null)
            {
                return "";
            }
            return DoEncryptInternal(hashAlgorithm, asymmetricEncryptionPadding, key, true, plainText);
        }

        [SecuritySafeCritical]
        public string DoEncrypt_WithPublicKey(string hashAlgorithm, string asymmetricEncryptionPadding, CertificateX509 certificate, string plainText)
        {

            if (this.HasError() || certificate == null)
            {
                return "";
            }
            return DoEncryptInternal(hashAlgorithm, asymmetricEncryptionPadding, certificate, false, plainText);
        }

        [SecuritySafeCritical]
        public string DoDecrypt_WithPrivateKey(string hashAlgorithm, string asymmetricEncryptionPadding, PrivateKeyManager key, string encryptedInput)
        {

            if (this.HasError() || key == null)
            {
                return "";
            }
            return DoDecryptInternal(hashAlgorithm, asymmetricEncryptionPadding, key, true, encryptedInput);
        }

        [SecuritySafeCritical]
        public string DoDecrypt_WithPublicKey(string hashAlgorithm, string asymmetricEncryptionPadding, CertificateX509 certificate, string encryptedInput)
        {

            if (this.HasError() || certificate == null)
            {
                return "";
            }
            return DoDecryptInternal(hashAlgorithm, asymmetricEncryptionPadding, certificate, false, encryptedInput);
        }


        /********EXTERNAL OBJECT PUBLIC METHODS  - END ********/


        /// <summary>
        /// Encrypts the string encoded plain text
        /// </summary>
        /// <param name="asymmetricEncryptionAlgorithm">string AsymmetricEncryptionAlgorithm enum, algorithm name</param>
        /// <param name="hashAlgorithm">string HashAlgorithm enum, algorithm name</param>
        /// <param name="asymmetricEncryptionPadding">string AsymmetricEncryptionPadding enum, padding name</param>
        /// <param name="keyPath">string path to key/certificate</param>
        /// <param name="isPrivate">boolean true if key is private, false if it is public</param>
        /// <param name="alias">string keystore/certificate pkcs12 format alias</param>
        /// <param name="password">Srting keysore/certificate pkcs12 format alias</param>
        /// <param name="plainText">string to encrypt</param>
        /// <returns>string Base64 encrypted plainText text</returns>
        private string DoEncryptInternal(string hashAlgorithm, string asymmetricEncryptionPadding, Key key, bool isPrivate, string plainText)
        {
            this.error.cleanError();

            HashAlgorithm hash = HashAlgorithmUtils.getHashAlgorithm(hashAlgorithm, this.error);
            AsymmetricEncryptionPadding padding = AsymmetricEncryptionPaddingUtils.getAsymmetricEncryptionPadding(asymmetricEncryptionPadding, this.error);
            if (this.error.existsError())
            {
                return "";
            }

            string asymmetricEncryptionAlgorithm = "";
            AsymmetricKeyParameter asymKey = null;
            if (isPrivate)
            {
                PrivateKeyManager keyMan = (PrivateKeyManager)key;
                if (!keyMan.HasPrivateKey || keyMan.HasError())
                {
                    this.error = keyMan.GetError();
                    return "";
                }
                asymmetricEncryptionAlgorithm = keyMan.getPrivateKeyAlgorithm();

                asymKey = keyMan.getPrivateKeyParameterForEncryption();
                if (keyMan.HasError())
                {
                    this.error = keyMan.GetError();
                    return "";
                }
            }
            else
            {
                CertificateX509 cert = (CertificateX509)key;
                if (!cert.Inicialized || cert.HasError())
                {
                    this.error = cert.GetError();
                    return "";
                }
                asymmetricEncryptionAlgorithm = cert.getPublicKeyAlgorithm();
                asymKey = cert.getPublicKeyParameterForEncryption();
                if (cert.HasError())
                {
                    this.error = cert.GetError();
                    return "";
                }
            }

            AsymmetricEncryptionAlgorithm algorithm = AsymmetricEncryptionAlgorithmUtils
                    .getAsymmetricEncryptionAlgorithm(asymmetricEncryptionAlgorithm, this.error);
            try
            {
                return doEncrypt(algorithm, hash, padding, asymKey, plainText);
            }
            catch (InvalidCipherTextException)
            {
                this.error.setError("AE036", "Algoritmo inválido" + algorithm);

                return "";
            }
        }


        /// <summary>
        /// Decrypts the base64 encoded encrypted text
        /// </summary>
        /// <param name="asymmetricEncryptionAlgorithm">string AsymmetricEncryptionAlgorithm enum, algorithm name</param>
        /// <param name="hashAlgorithm">string HashAlgorithm enum, algorithm name</param>
        /// <param name="asymmetricEncryptionPadding">string AsymmetricEncryptionPadding enum, padding name</param>
        /// <param name="keyPath">string path to key/certificate</param>
        /// <param name="isPrivate">boolean true if key is private, false if it is public</param>
        /// <param name="alias">string keystore/certificate pkcs12 format alias</param>
        /// <param name="password">Srting keysore/certificate pkcs12 format alias</param>
        /// <param name="encryptedInput"></param>
        /// <returns>string decypted encryptedInput text</returns>
        private string DoDecryptInternal(string hashAlgorithm, string asymmetricEncryptionPadding, Key key, bool isPrivate, string encryptedInput)
        {
            this.error.cleanError();

            HashAlgorithm hash = HashAlgorithmUtils.getHashAlgorithm(hashAlgorithm, this.error);
            AsymmetricEncryptionPadding padding = AsymmetricEncryptionPaddingUtils.getAsymmetricEncryptionPadding(asymmetricEncryptionPadding, this.error);
            if (this.error.existsError())
            {
                return "";
            }
            string asymmetricEncryptionAlgorithm = "";
            AsymmetricKeyParameter asymKey = null;

            if (isPrivate)
            {
                PrivateKeyManager keyMan = (PrivateKeyManager)key;
                if (!keyMan.HasPrivateKey || keyMan.HasError())
                {
                    this.error = keyMan.GetError();
                    return "";
                }
                asymmetricEncryptionAlgorithm = keyMan.getPrivateKeyAlgorithm();

                asymKey = keyMan.getPrivateKeyParameterForEncryption();
                if (keyMan.HasError())
                {
                    this.error = keyMan.GetError();
                    return "";
                }
            }
            else
            {
                CertificateX509 cert = (CertificateX509)key;
                if (!cert.Inicialized || cert.HasError())
                {
                    this.error = cert.GetError();
                    return "";
                }
                asymmetricEncryptionAlgorithm = cert.getPublicKeyAlgorithm();
                asymKey = cert.getPublicKeyParameterForEncryption();
                if (cert.HasError())
                {
                    this.error = cert.GetError();
                    return "";
                }
            }

            AsymmetricEncryptionAlgorithm algorithm = AsymmetricEncryptionAlgorithmUtils
                    .getAsymmetricEncryptionAlgorithm(asymmetricEncryptionAlgorithm, this.error);


            try
            {
                this.error.cleanError();
                return doDecrypt(algorithm, hash, padding, asymKey, encryptedInput);
            }
            catch (InvalidCipherTextException e)
            {
                this.error.setError("AE039", "Algoritmo inválido" + algorithm);
                throw new InvalidCipherTextException("Algoritmo inválido" + algorithm, e);
            }
        }

        /// <summary>
        /// Decrypts the base64 encoded encrypted text 
        /// </summary>
        /// <param name="asymmetricEncryptionAlgorithm">string AsymmetricEncryptionAlgorithm enum, algorithm name</param>
        /// <param name="hashAlgorithm">string HashAlgorithm enum, algorithm name</param>
        /// <param name="asymmetricEncryptionPadding">string AsymmetricEncryptionPadding enum, padding name</param>
        /// <param name="asymmetricKeyParameter">AsymmetricKeyParameter with loaded key for specified algorithm</param>
        /// <param name="encryptedInput">string Base64 to decrypt</param>
        /// <returns>string decypted encryptedInput text</returns>
        private string doDecrypt(AsymmetricEncryptionAlgorithm asymmetricEncryptionAlgorithm, HashAlgorithm hashAlgorithm, AsymmetricEncryptionPadding asymmetricEncryptionPadding, AsymmetricKeyParameter asymmetricKeyParameter, string encryptedInput)
        {

            IAsymmetricBlockCipher asymEngine = getEngine(asymmetricEncryptionAlgorithm);
            IDigest hash = getDigest(hashAlgorithm);
            IAsymmetricBlockCipher cipher = getPadding(asymEngine, hash, asymmetricEncryptionPadding);
            BufferedAsymmetricBlockCipher bufferedCipher = new BufferedAsymmetricBlockCipher(cipher);
            if (this.error.existsError())
            {
                return "";
            }
            bufferedCipher.Init(false, asymmetricKeyParameter);
            byte[] inputBytes = Base64.Decode(encryptedInput);
            bufferedCipher.ProcessBytes(inputBytes, 0, inputBytes.Length);
            byte[] outputBytes = bufferedCipher.DoFinal();
            if (outputBytes == null || outputBytes.Length == 0)
            {
                this.error.setError("AE040", "Asymmetric decryption error");
                return "";
            }
            EncodingUtil eu = new EncodingUtil();
            this.error = eu.GetError();
            return eu.getString(outputBytes);


        }
        /// <summary>
        /// Encrypts the string encoded plain text
        /// </summary>
        /// <param name="asymmetricEncryptionAlgorithm"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="asymmetricEncryptionPadding"></param>
        /// <param name="asymmetricKeyParameter"></param>
        /// <param name="plainText"></param>
        /// <returns>Base64 encrypted encryptedInput text</returns>
        private string doEncrypt(AsymmetricEncryptionAlgorithm asymmetricEncryptionAlgorithm, HashAlgorithm hashAlgorithm, AsymmetricEncryptionPadding asymmetricEncryptionPadding, AsymmetricKeyParameter asymmetricKeyParameter, string plainText)
        {
            IAsymmetricBlockCipher asymEngine = getEngine(asymmetricEncryptionAlgorithm);
            IDigest hash = getDigest(hashAlgorithm);
            IAsymmetricBlockCipher cipher = getPadding(asymEngine, hash, asymmetricEncryptionPadding);
            BufferedAsymmetricBlockCipher bufferedCipher = new BufferedAsymmetricBlockCipher(cipher);
            if (this.error.existsError())
            {
                return "";
            }
            bufferedCipher.Init(true, asymmetricKeyParameter);
            EncodingUtil eu = new EncodingUtil();
            byte[] inputBytes = eu.getBytes(plainText);
            if (eu.GetError().existsError())
            {
                this.error = eu.GetError();
                return "";
            }
            bufferedCipher.ProcessBytes(inputBytes, 0, inputBytes.Length);
            byte[] outputBytes = bufferedCipher.DoFinal();
            if (outputBytes == null || outputBytes.Length == 0)
            {
                this.error.setError("AE041", "Asymmetric encryption error");
                return "";
            }
            this.error.cleanError();
            return Base64.ToBase64String(outputBytes);

        }
        /// <summary>
        /// Build asymmetric block cipher engine
        /// </summary>
        /// <param name="asymmetricEncryptionAlgorithm">AsymmetricEncryptionAlgorithm enum, algorithm name</param>
        /// <returns>IAsymmetricBlockCipher Engine for the specified algorithm</returns>
        private IAsymmetricBlockCipher getEngine(AsymmetricEncryptionAlgorithm asymmetricEncryptionAlgorithm)
        {

            switch (asymmetricEncryptionAlgorithm)
            {
                case AsymmetricEncryptionAlgorithm.RSA:
                    return new RsaEngine();
                default:
                    this.error.setError("AE042", "Unrecognized algorithm");
                    return null;
            }

        }
        /// <summary>
        /// Build Digest engine for asymmetric block cipher and signing
        /// </summary>
        /// <param name="hashAlgorithm">HashAlgorithm enum, algorithm name</param>
        /// <returns>IDigest Engine for the specified algorithm</returns>
        private IDigest getDigest(HashAlgorithm hashAlgorithm)
        {
            Hashing hash = new Hashing();
            IDigest digest = hash.createHash(hashAlgorithm);
            if (digest == null)
            {
                this.error.setError("AE043", "Unrecognized HashAlgorithm");
                return null;
            }
            return digest;
        }
        /// <summary>
        /// Buils Asymmetric Block Cipher engine
        /// </summary>
        /// <param name="asymBlockCipher">AsymmetricBlockCipher enum, algorithm name</param>
        /// <param name="hash">Digest Engine for hashing</param>
        /// <param name="asymmetricEncryptionPadding">AsymmetricEncryptionPadding enum, padding name</param>
        /// <returns>AsymmetricBlockCipher Engine specific for the algoritm, hash and padding</returns>
        private IAsymmetricBlockCipher getPadding(IAsymmetricBlockCipher asymBlockCipher, IDigest hash, AsymmetricEncryptionPadding asymmetricEncryptionPadding)
        {
            switch (asymmetricEncryptionPadding)
            {
                case AsymmetricEncryptionPadding.NOPADDING:
                    return null;
                case AsymmetricEncryptionPadding.OAEPPADDING:
                    if (hash != null)
                    {
                        return new OaepEncoding(asymBlockCipher, hash);
                    }
                    else
                    {
                        return new OaepEncoding(asymBlockCipher);
                    }
                case AsymmetricEncryptionPadding.PCKS1PADDING:
                    return new Pkcs1Encoding(asymBlockCipher);
                case AsymmetricEncryptionPadding.ISO97961PADDING:
                    return new ISO9796d1Encoding(asymBlockCipher);
                default:
                    error.setError("AE044", "Unrecognized AsymmetricEncryptionPadding");
                    return null;
            }
        }
    }
}
