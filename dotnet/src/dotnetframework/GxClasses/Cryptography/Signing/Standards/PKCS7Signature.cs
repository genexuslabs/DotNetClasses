using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using GeneXus.Cryptography.Signing.Standards;
using GeneXus.Cryptography.CryptoException;

namespace GeneXus.Cryptography.Signing
{
    public class PKCS7Signature : IPkcsSign
    {
        private X509Certificate2 _cert;
        private string _hashAlgorithm;
        private bool _validateCertificates;
        private bool _detached;

        internal PKCS7Signature(string hashAlgorithm)
        {
            _hashAlgorithm = hashAlgorithm;
        }

        public string Sign(string text)
        {
#if !NETCORE
            string signed = string.Empty;
            string oid = CryptoConfig.MapNameToOID(_hashAlgorithm);
            if (!String.IsNullOrEmpty(oid) && !AnyError())
            {
				if (_cert.HasPrivateKey)
                {
                    byte[] data = Constants.DEFAULT_ENCODING.GetBytes(text);
                    ContentInfo content = new ContentInfo(data);
                    SignedCms signedMessage = new SignedCms(content, _detached);
                    CmsSigner signer = new CmsSigner(_cert);

                    signer.DigestAlgorithm = new Oid(oid);
                    signedMessage.ComputeSignature(signer);
                    byte[] signedBytes = signedMessage.Encode();
                    
                    signed = Convert.ToBase64String(signedBytes);
                }
                else
                {
                    throw new PrivateKeyNotFoundException();
                }
            }
            else
            {
                throw new AlgorithmNotSupportedException();
            }
            return signed;
#else
			return text;
#endif
		}

        public bool Verify(string signature, string text)
        {
            if (!AnyError())
            {
                byte[] textData = Constants.DEFAULT_ENCODING.GetBytes(text);
                byte[] outData;
                byte[] signatureBytes;
                try
                {
                    signatureBytes = Convert.FromBase64String(signature);
                }
                catch (FormatException e)
                {
                    throw new InvalidSignatureException(e);
                }
                if (_detached)
                {
                    return VerifyDetached(signatureBytes, textData);
                }
                else
                {
                    return VerifyAndRemoveSignature(signatureBytes, out outData);
                }
            }
            return false;
        }

        private bool VerifyDetached(byte[] signature, byte[] data)
        {
#if !NETCORE
			ContentInfo content = new ContentInfo(data);
            SignedCms signedMessage = new SignedCms(content, true);
            try
            {
                signedMessage.Decode(signature);
                signedMessage.CheckSignature(new X509Certificate2Collection(_cert), !_validateCertificates);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
#else
			return false;
#endif
		}

        bool VerifyAndRemoveSignature(byte[] dataWithSignature, out byte[] data)
        {
#if !NETCORE
            try
            {
                SignedCms signedMessage = new SignedCms();
                signedMessage.Decode(dataWithSignature);
                signedMessage.CheckSignature(new X509Certificate2Collection(_cert), !_validateCertificates);
                data = signedMessage.ContentInfo.Content;
                return true;
            }
            catch (CryptographicException)
            {
                data = Array.Empty<byte>();
                return false;
            }
            catch (Exception)
            {
                data = Array.Empty<byte>();
                return false;
            }
#else
			data = Array.Empty<byte>();
			return false;
#endif
		}

        public string ExtractEnvelopedData(string dataWithSignature)
        {
            byte[] outData;
            try
            {
                VerifyAndRemoveSignature(Convert.FromBase64String(dataWithSignature), out outData);
                return Constants.DEFAULT_ENCODING.GetString(outData);
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        private bool AnyError()
        {
            if (_cert == null)
            {
                throw new CertificateNotLoadedException();
            }
            return false;
        }

        public X509Certificate2 Certificate
        {
            get { return _cert; }
            set { _cert = value; }
        }

        public bool Detached
        {
            get { return _detached; }
            set { _detached = value; }
        }

        public bool ValidateCertificates
        {
            get { return _validateCertificates; }
            set { _validateCertificates = value; }
        }

    }
}
