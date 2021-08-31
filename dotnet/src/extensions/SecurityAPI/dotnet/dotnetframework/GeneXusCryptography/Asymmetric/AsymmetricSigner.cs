
using GeneXusCryptography.AsymmetricUtils;
using GeneXusCryptography.Hash;
using GeneXusCryptography.HashUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Utilities.Encoders;
using GeneXusCryptography.Commons;
using System;
using System.Security;
using SecurityAPICommons.Utils;
using System.IO;

namespace GeneXusCryptography.Asymmetric
{
    /// <summary>
    /// Implements Asymmetric Signer engines and methods to sign and verify signatures
    /// </summary>
    [SecuritySafeCritical]
    public class AsymmetricSigner : SecurityAPIObject, IAsymmetricSignerObject
    {


        /// <summary>
        /// AsymmetricSigner class constructor
        /// </summary>
        public AsymmetricSigner() : base()
        {

        }

        /********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

        [SecuritySafeCritical]
        public string DoSign(PrivateKeyManager key, string hashAlgorithm, string plainText)
        {
			if(key == null)
			{
				this.error.setError("AE000", "Key parameter is null");
				return "";
			}
			EncodingUtil eu = new EncodingUtil();
			byte[] input = eu.getBytes(plainText);
			if(eu.HasError())
			{
				this.error = eu.error;
				return "";
			}
			string aux = "";
			using(Stream istream = new MemoryStream(input))
			{
				aux = DoSignPKCS12(key, hashAlgorithm, istream);
			}
			return aux;
        }

		[SecuritySafeCritical]
		public string DoSignFile(PrivateKeyManager key, string hashAlgorithm, string path)
		{
			if (key == null)
			{
				this.error.setError("AE000", "Key parameter is null");
				return "";
			}
			string aux = "";
			using (Stream input = SecurityUtils.getFileStream(path, this.error))
			{
				if (this.HasError())
				{
					return "";
				}
				aux = DoSignPKCS12(key, hashAlgorithm, input);
			}
			return aux;
		}

		[SecuritySafeCritical]
        public bool DoVerify(CertificateX509 cert, string plainText, string signature)
        {
			if (cert == null)
			{
				this.error.setError("AE000", "Cert parameter is null");
				return false;
			}
			EncodingUtil eu = new EncodingUtil();
			byte[] input = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.error;
				return false;
			}
			bool aux = false;
			using(Stream istream = new MemoryStream(input))
			{
				aux = DoVerifyPKCS12(cert, istream, signature);
			}
			return aux;
        }

		[SecuritySafeCritical]
		public bool DoVerifyFile(CertificateX509 cert, string path, string signature)
		{
			if (cert == null)
			{
				this.error.setError("AE000", "Cert parameter is null");
				return false;
			}
			bool aux = false;
			using (Stream input = SecurityUtils.getFileStream(path, this.error))
			{

				if (this.HasError())
				{
					return false;
				}
				aux = DoVerifyPKCS12(cert, input, signature);
			}
			return aux;
		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

		/// <summary>
		/// Signs UTF-8 plain text
		/// </summary>
		/// <param name="path">string path of the key/certificate file</param>
		/// <param name="hashAlgorithm">string HashAlgorithm enum, algorithm name</param>
		/// <param name="alias">string alias of the certificate/keystore in pkcs12 format</param>
		/// <param name="password">string password of the certificate/keystore in pkcs12 format</param>
		/// <param name="plainText">string UTF-8 text to sign</param>
		/// <returns>string Base64 signature of plainText text</returns>
		private string DoSignPKCS12(PrivateKey key, string hashAlgorithm, Stream input)
        {
            this.error.cleanError();
            HashAlgorithm hash = HashAlgorithmUtils.getHashAlgorithm(hashAlgorithm, this.error);
            if (this.error.existsError())
            {
                return "";
            }
            PrivateKeyManager keyMan = (PrivateKeyManager)key;
            string algorithm = keyMan.getPrivateKeyAlgorithm();
            if (keyMan.GetError().existsError())
            {
                this.error = keyMan.GetError();
                return "";
            }

            if (SecurityUtils.compareStrings(algorithm, "RSA"))
            {
                return signRSA(hash, input, keyMan);
            }
            if (SecurityUtils.compareStrings(algorithm, "ECDSA"))
            {
                return signECDSA(hash, input, keyMan);
            }
            this.error.setError("AE047", "Unrecognized signing algorithm " + algorithm);
            return "";
        }

        /// <summary>
        /// Implements signature verification with RSA or ECDSA keys
        /// </summary>
        /// <param name="path">string path of the key/certificate file</param>
        /// <param name ="alias">string alias of the certificate/keystore in pkcs12 format</param>
        /// <param name="password">string password of the certificate/keystore in pkcs12 format</param>
        /// <param name="plainText">string UTF-8 text to sign</param>
        /// <param name="signature">string Base64 signature of plainText</param>
        /// <returns>boolean true if signature is valid for the specified parameters, false if it is invalid</returns>
        private bool DoVerifyPKCS12(Certificate certificate, Stream input, string signature)
        {
            this.error.cleanError();
            CertificateX509 cert = (CertificateX509)certificate;
            if (!cert.Inicialized || cert.HasError())
            {
                this.error = cert.GetError();
                return false;
            }
            AsymmetricSigningAlgorithm asymmetricSigningAlgorithm = AsymmetricSigningAlgorithmUtils.getAsymmetricSigningAlgorithm(cert.getPublicKeyAlgorithm(), this.error);
            if (this.error.existsError())
            {
                return false;
            }
            switch (asymmetricSigningAlgorithm)
            {
                case AsymmetricSigningAlgorithm.RSA:
                    return verifyRSA(input, signature, cert);
                case AsymmetricSigningAlgorithm.ECDSA:
                    return verifyECDSA(input, signature, cert);
                default:
                    this.error.setError("AE048", "Cannot verify signature");
                    return false;
            }

        }


        private bool verifyRSA(Stream input, string signature, CertificateX509 cert)
        {

            HashAlgorithm hashAlgorithm = (HashAlgorithm)Enum.Parse(typeof(HashAlgorithm), cert.getPublicKeyHash());
            if (HashAlgorithm.NONE != hashAlgorithm)
            {
                EncodingUtil eu = new EncodingUtil();
                Hashing digest = new Hashing();
                IDigest hash = digest.createHash(hashAlgorithm);
                if (digest.GetError().existsError())
                {
                    this.error = digest.GetError();
                    return false;
                }
                RsaDigestSigner signerRSA = new RsaDigestSigner(hash);
                AsymmetricKeyParameter asymmetricKeyParameter = cert.getPublicKeyParameterForSigning();
                signerRSA.Init(false, asymmetricKeyParameter);

				byte[] buffer = new byte[8192];
				int n;
				byte[] signatureBytes = null;
				try
				{
					while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
					{
						signerRSA.BlockUpdate(buffer, 0, n);
					}
					signatureBytes = Base64.Decode(signature);
				}
				catch (Exception e)
				{
					error.setError("AE056", e.Message);
					return false;
				}
                if (signatureBytes == null || signatureBytes.Length == 0)
                {
                    this.error.setError("AE049", "Error on signature verification");
                    return false;
                }
                this.error.cleanError();
                return signerRSA.VerifySignature(signatureBytes);
            }
            this.error.setError("AE050", "Hashalgorithm cannot be NONE");
            return false;
        }
        /// <summary>
        /// Implements signature verification with ECDSA keys, if no hash is defined uses default SHA1
        /// </summary>
        /// <param name="plainText">string UTF-8 signed text</param>
        /// <param name="signature">string Base64 signature of plainText</param>
        /// <param name="km">KeyManager Data Type loaded with keys and key information</param>
        /// <returns>boolean true if signature is valid for the specified parameters, false if it is invalid</returns>
        private bool verifyECDSA(Stream input, string signature, CertificateX509 cert)
        {
            HashAlgorithm hashAlgorithm;
            if (SecurityUtils.compareStrings(cert.getPublicKeyHash(), "ECDSA"))
            {
                hashAlgorithm = HashAlgorithm.SHA1;
            }
            else
            {
                hashAlgorithm = (HashAlgorithm)Enum.Parse(typeof(HashAlgorithm), cert.getPublicKeyHash());
            }
            Hashing hash = new Hashing();
            IDigest digest = hash.createHash(hashAlgorithm);
            if (hash.GetError().existsError())
            {
                this.error = hash.GetError();
                return false;
            }
            ECDsaSigner dsaSigner = new ECDsaSigner();
            DsaDigestSigner digestSigner = new DsaDigestSigner(dsaSigner, digest);
            AsymmetricKeyParameter asymmetricKeyParameter = cert.getPublicKeyParameterForSigning();
            if (this.error.existsError())
            {
                return false;
            }
            digestSigner.Init(false, asymmetricKeyParameter);
			byte[] buffer = new byte[8192];
			int n;
			byte[] signatureBytes = null;
			try
			{
				while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					digestSigner.BlockUpdate(buffer, 0, n);
				}
				signatureBytes = Base64.Decode(signature);
			}
			catch (Exception e)
			{
				error.setError("AE056", e.Message);
				return false;
			}
            if (signatureBytes == null || signatureBytes.Length == 0)
            {
                this.error.setError("AE051", "Error on signature verification");
                return false;
            }
            this.error.cleanError();
            return digestSigner.VerifySignature(signatureBytes);

        }
        /// <summary>
        /// Implements ECDSA signature. Uses specified hash value or SHA1 for default
        /// </summary>
        /// <param name="hashAlgorithm">HashAlgorithm enum, algorithm name</param>
        /// <param name="plainText">string UTF-8 to sign</param>
        /// <param name="km">KeyManager Data Type loaded with keys and key information</param>
        /// <returns>string Base64 ECDSA signature of plainText</returns>
        private string signECDSA(HashAlgorithm hashAlgorithm, Stream input, PrivateKeyManager km)
        {
            Hashing hash = new Hashing();
            IDigest digest = hash.createHash(hashAlgorithm);
            if (hash.GetError().existsError())
            {
                this.error = hash.GetError();
                return "";
            }
            ECDsaSigner dsaSigner = new ECDsaSigner();
            DsaDigestSigner digestSigner = new DsaDigestSigner(dsaSigner, digest);
            AsymmetricKeyParameter asymmetricKeyParameter = km.getPrivateKeyParameterForSigning();
            if (this.error.existsError())
            {
                return "";
            }
            digestSigner.Init(true, asymmetricKeyParameter);
			byte[] buffer = new byte[8192];
			int n;
			byte[] output = null;
			try
			{
				while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					digestSigner.BlockUpdate(buffer, 0, n);
				}
				output = digestSigner.GenerateSignature();
			}
			catch (Exception e)
			{
				error.setError("AE055", e.Message);
				return "";
			}
            if (output == null || output.Length == 0)
            {
                this.error.setError("AE052", "Error on signing");
            }
            this.error.cleanError();
            return Base64.ToBase64String(output);

        }
        /// <summary>
        /// Implements RSSA signature. Hash NONE is not a valid value
        /// </summary>
        /// <param name="hashAlgorithm">HashAlgorithm enum, algorithm name</param>
        /// <param name="plainText">string UTF-8 to sign</param>
        /// <param name="km">KeyManager Data Type loaded with keys and key information</param>
        /// <returns>string Base64 RSA signature of plainText</returns>
        private string signRSA(HashAlgorithm hashAlgorithm, Stream input, PrivateKeyManager km)
        {
            if (HashAlgorithm.NONE != hashAlgorithm)
            {
                Hashing digest = new Hashing();
                IDigest hash = digest.createHash(hashAlgorithm);
                if (digest.GetError().existsError())
                {
                    this.error = digest.GetError();
                    return "";
                }
                RsaDigestSigner signerRSA = new RsaDigestSigner(hash);
                AsymmetricKeyParameter asymmetricKeyParameter = km.getPrivateKeyParameterForSigning();
                if (this.error.existsError())
                {

                    return "";
                }
                signerRSA.Init(true, asymmetricKeyParameter);
				byte[] buffer = new byte[8192];
				int n;

				byte[] outputBytes = null;
				try
				{

					while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
					{
						signerRSA.BlockUpdate(buffer, 0, n);
					}
					outputBytes = signerRSA.GenerateSignature();
				}
				catch (Exception e)
				{
					this.error.setError("AE053", e.Message);
					return "";

				}
                this.error.cleanError();
                return Base64.ToBase64String(outputBytes);
            }
            this.error.setError("AE054", "HashAlgorithm cannot be NONE");
            return "";

        }
    }
}
