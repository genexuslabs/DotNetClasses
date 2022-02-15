using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using GeneXus.Cryptography.CryptoException;
using GeneXus.Utils;

namespace GeneXus.Cryptography.Signing.Standards
{
    public class PKCS1Signature : IPkcsSign
    {
        private X509Certificate2 _cert;
        private string _hashAlgorithm;
        private string _signAlgorithm;
        private bool _validateCertificates;

        public PKCS1Signature(string hashAlgorithm, string signAlgorithm)
        {
            _hashAlgorithm = hashAlgorithm;
            _signAlgorithm = signAlgorithm.ToUpper();
        }

        public string Sign(string text)
        {
            string signed = string.Empty;
            if (!AnyError())
            {
                string oid = CryptoConfig.MapNameToOID(_hashAlgorithm);
                if (!String.IsNullOrEmpty(oid) && !AnyError())
                {
                    if (_cert.HasPrivateKey)
                    {
                        byte[] data = Constants.DEFAULT_ENCODING.GetBytes(text);
                        switch (_signAlgorithm)
                        {
                            case "RSA":
                                signed = SignRSA(signed, data);
                                break;
                            case "DSA":
                                signed = SignDSA(signed, oid, data);
                                break;
                            default:
                                break;
                        }

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
            }
            return signed;
        }

        private string SignRSA(string signed, byte[] data)
        {
            RSACryptoServiceProvider rsa;
			if (!GXUtil.IsWindowsPlatform)
			{
				throw new AlgorithmNotSupportedException("This platform does not support RSA Sign Algorithm");
			}
			else
			{
				try
				{
					rsa = (RSACryptoServiceProvider)_cert.PrivateKey;
				}
				catch (Exception)
				{
					throw new AlgorithmNotSupportedException("Private Key does not support RSA Sign Algorithm");
				}

				HashAlgorithm ha = HashAlgorithm.Create(_hashAlgorithm);
				byte[] signature = null;
				try
				{
					signature = rsa.SignData(data, ha);
				}
				catch (CryptographicException)
				{
					try
					{
						RSACryptoServiceProvider rsaClear = new RSACryptoServiceProvider();
						rsaClear.ImportParameters(rsa.ExportParameters(true));
						signature = rsaClear.SignData(data, ha);
					}
					catch (CryptographicException e)
					{
						throw new AlgorithmNotSupportedException(e.Message);
					}
				}
				return Convert.ToBase64String(signature);
			}
        }

        private string SignDSA(string signed, string oid, byte[] data)
        {
            DSACryptoServiceProvider dsa;
			if (!GXUtil.IsWindowsPlatform)
			{
				throw new AlgorithmNotSupportedException("This platform does not support DSA Sign Algorithm");
			}
			else
			{
				try
				{
					dsa = (DSACryptoServiceProvider)_cert.PrivateKey;
				}
				catch (Exception)
				{
					throw new AlgorithmNotSupportedException("Private Key does not support DSA Sign Algorithm");
				}


				HashAlgorithm ha = System.Security.Cryptography.HashAlgorithm.Create(_hashAlgorithm);
				byte[] hash = ha.ComputeHash(data);
				byte[] signature = dsa.SignHash(hash, oid);

				return Convert.ToBase64String(signature);
			}
        }


        public bool Verify(string signatureB64, string text)
        {
            if (!AnyError())
            {
                try
                {
					HashAlgorithm ha = GXUtil.IsWindowsPlatform ? HashAlgorithm.Create(_hashAlgorithm) : null;
                    string oid = CryptoConfig.MapNameToOID(_hashAlgorithm);
                    if (ha != null && !string.IsNullOrEmpty(oid))
                    {
                        byte[] textData = Constants.DEFAULT_ENCODING.GetBytes(text);
                        byte[] signature = Convert.FromBase64String(signatureB64);
                        byte[] hashedData = ha.ComputeHash(textData);
                        bool certValid = !ValidateCertificates || Certificate.Verify();

                        if (_cert.PublicKey.Key is RSACryptoServiceProvider)
                        {
                            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)_cert.PublicKey.Key;
                            return certValid && rsa.VerifyHash(hashedData, oid, signature);
                        }
                        else
                        {
                            if (_cert.PublicKey.Key is DSACryptoServiceProvider)
                            {
                                DSACryptoServiceProvider dsa = (DSACryptoServiceProvider)_cert.PublicKey.Key;
                                return certValid && dsa.VerifyHash(hashedData, oid, signature);
                            }
                        }
                    }
                    else
                    {
                        throw new AlgorithmNotSupportedException();
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidSignatureException(e);
                }
            }
            return false;
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

        public bool ValidateCertificates
        {
            get { return _validateCertificates; }
            set { _validateCertificates = value; }
        }
    }
}
