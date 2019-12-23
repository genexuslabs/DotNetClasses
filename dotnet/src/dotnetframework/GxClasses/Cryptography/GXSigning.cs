using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneXus.Cryptography.Signing.Standards;
using GeneXus.Cryptography.CryptoException;
using GeneXus.Cryptography.Signing;
using GeneXus.Cryptography;

namespace GeneXus.Cryptography
{
    public class GXSigning
    {        
        private GXCertificate _cert;
        private string _alg;
        private string _hashAlgorithm;        
        private IPkcsSign _sign;
        private int _lastError;
        private string _lastErrorDescription;
        private bool isDirty;
        private bool _validateCertificates;
        private Constants.PKCSStandard _standard;
        private Constants.PKCSSignAlgorithm _signAlgorithm;

       
        public GXSigning()
        {
            isDirty = true;
            _validateCertificates = true;
            _standard = Constants.DEFAULT_SIGN_FORMAT;
            _signAlgorithm = Constants.DEFAULT_SIGN_ALGORITHM;
            _hashAlgorithm = Constants.DEFAULT_HASH_ALGORITHM;
        }

        public string Sign(string text, bool detached)
        {
            Initialize();
            string signed = string.Empty;
            if (!AnyError)
            {
                try
                {
                    _sign.Certificate = _cert.Certificate;
                    _sign.ValidateCertificates = _validateCertificates;
                    if (_sign is PKCS7Signature)
                    {
                        ((PKCS7Signature)_sign).Detached = detached;
                    }
                    signed = _sign.Sign(text);
                }
                catch (AlgorithmNotSupportedException e)
                {
                    SetError(2, e.Message);
                }
                catch (CertificateNotLoadedException)
                {
                    SetError(4);
                }
                catch (PrivateKeyNotFoundException)
                {
                    SetError(5);
                }
            }
            return signed;

        }

        public bool Verify(string signature, string text, bool detached)
        {

            Initialize();
            bool ok = false;
            if (!AnyError)
            {
                try
                {
                    _sign.Certificate = _cert.Certificate;
                    _sign.ValidateCertificates = _validateCertificates;
                    if (_sign is PKCS7Signature)
                    {
                        ((PKCS7Signature)_sign).Detached = detached;
                    }
                    ok = _sign.Verify(signature, text);
                }
                catch (AlgorithmNotSupportedException e)
                {
                    SetError(2, e.Message);
                }
                catch (CertificateNotLoadedException)
                {
                    SetError(4);
                }
                catch (InvalidSignatureException e)
                {
                    SetError(6, e.Message);
                }
                catch (PrivateKeyNotFoundException)
                {
                    SetError(5);
                }
            }
            return ok;

        }

        private void Initialize()
        {
            if (isDirty)
            {
                switch (_standard)
                {
                    case Constants.PKCSStandard.PKCS1:
                        _sign = new PKCS1Signature(_hashAlgorithm, _signAlgorithm.ToString());
                        break;
                    case Constants.PKCSStandard.PKCS7:
                        _sign = new PKCS7Signature(_hashAlgorithm);
                        break;
                    default:
                        break;
                }

                isDirty = false;
            }
        }

        private void SetError(int errorCode)
        {
            SetError(errorCode, string.Empty);
        }
        private void SetError(int errorCode, string errDsc)
        {
            _lastError = errorCode;
            switch (errorCode)
            {
                case 0:
                    _lastErrorDescription = string.Empty;
                    break;
                case 1:
                    break;
                case 2:
                    _lastErrorDescription = "Algorithm not supported";
                    break;
                case 3:
                    _lastErrorDescription = "Invalid Algorithm format";
                    break;
                case 4:
                    _lastErrorDescription = "Certificate not initialized";
                    break;
                case 5:
                    _lastErrorDescription = "Certificate does not contain private key.";
                    break;
                case 6:
                    _lastErrorDescription = "Signature Exception";
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(errDsc))
            {
                if (!string.IsNullOrEmpty(_lastErrorDescription))
                {
                    _lastErrorDescription = String.Format("{0} - {1}", _lastErrorDescription, errDsc);
                }
                else
                {
                    _lastErrorDescription = errDsc;
                }
            }
        }


        public bool ValidateCertificate
        {
            get { return _validateCertificates; }
            set { _validateCertificates = value; }
        }

        public string Standard
        {
            get { return _standard.ToString(); }
            set
            {
                GeneXus.Cryptography.Constants.PKCSStandard oldV = _standard;
                switch(value)
                {                        
                    case "PKCS7":
                        _standard = GeneXus.Cryptography.Constants.PKCSStandard.PKCS7;
                        break;
                    case "PKCS1":
                        _standard = GeneXus.Cryptography.Constants.PKCSStandard.PKCS1;
                        break;
                    default:
                        SetError(2); //Algorithm not supported
                        break;
                }                
                isDirty = isDirty || oldV != _standard;
            }
        }
        public string Algorithm
        {
            get { return _alg; }
            set
            {
                isDirty = isDirty || value != _alg;
                _alg = value;
                string[] parts = _alg.Split(' ');
                if (parts.Length == 2) //Format Example: MD5 RSA.
                {                    
                    string hash = parts[0];
                    string sign = parts[1];
                    
                    _hashAlgorithm = hash;                 
                    switch (sign)
                    {
                        case "RSA":
                            _signAlgorithm = GeneXus.Cryptography.Constants.PKCSSignAlgorithm.RSA;
                            break;
                        case "DSA":
                            _signAlgorithm = GeneXus.Cryptography.Constants.PKCSSignAlgorithm.DSA;
                            break;
                        default:
                            SetError(2); //Algorithm not supported
                            break;
                    }
                }
                else
                {
                    SetError(3);
                    //invalid format algorithm.
                }
            }
        }

        public GXCertificate Certificate
        {
            get { return _cert; }
            set { _cert = value; }
        }

        private bool AnyError
        {
            get
            {
                if (_cert == null || (_cert != null && !_cert.CertLoaded()))
                {
                    SetError(4); //Certificate not initialized
                }
                return _lastError != 0;
            }
        }

        public int ErrCode
        {
            get
            {
                return _lastError;
            }
        }

        public string ErrDescription
        {
            get
            {
                return _lastErrorDescription;
            }
        }
    }
}
