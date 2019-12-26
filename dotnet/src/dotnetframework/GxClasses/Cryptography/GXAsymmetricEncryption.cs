using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using GeneXus.Cryptography.CryptoException;
using GeneXus.Cryptography.Encryption;

namespace GeneXus.Cryptography
{
    public class GXAsymmetricEncryption
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _lastError;
        private string _lastErrorDescription;
        private GXCertificate _cert;
        private IGXAsymmetricEncryption _asymAlg;
        private string _algorithm;
        private bool isDirty;

        public GXAsymmetricEncryption()
        {
            isDirty = true;
            Initialize();
            
        }

        private void Initialize()
        {
            if (isDirty)
            {
                // Support algorithms = RSA only for now..
                SetError(0);

                if (_cert != null && _cert.Certificate != null)
                {
                    _asymAlg = new RSAEncryption(_cert.Certificate);
                    isDirty = false;
                }
                else
                {
                    SetError(4);
                }
            }
        }

        public string Encrypt(string text)
        {
            Initialize();
            string encrypted = string.Empty;
            if (!AnyError)
            {
                try
                {
                    encrypted = _asymAlg.Encrypt(text);
                }
                catch (CertificateNotLoadedException)
                {
                    SetError(4);
                }
                catch (EncryptionException e)
                {
                    SetError(1, e.Message);
                    _log.Error("Encryption Error", e);
                }
            }
            return encrypted;
        }

        public string Decrypt(string text)
        {
            Initialize();
            string decrypted = string.Empty;
            if (!AnyError)
            {
                try
                {
                    decrypted = _asymAlg.Decrypt(text);
                }
                catch (CertificateNotLoadedException)
                {
                    SetError(4);
                }
                catch (PrivateKeyNotFoundException)
                {
                    SetError(5); //Certificate does not contain private key. 
                }
                catch (EncryptionException e)
                {
                    SetError(1, e.Message);
                    _log.Error("Encryption Error", e);
                }
            }
            return decrypted;
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
                    //Custom Error
                    break;
                case 4:
                    _lastErrorDescription = "Certificate not initialized";
                    break;
                case 5:
                    _lastErrorDescription = "Certificate does not contain private key.";
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(errDsc))
            {
                _lastErrorDescription = errDsc;
            }
        }


        public string Algorithm
        {
            get { return _algorithm; }
            set
            {
                isDirty = isDirty || value != _algorithm;
                _algorithm = value;
            }
        }

        public GXCertificate Certificate
        {
            get { return _cert; }
            set
            {
                isDirty = isDirty || value != _cert;
                _cert = value;
            }
        }

        private bool AnyError
        {
            get
            {
                if (_cert == null || (!_cert.CertLoaded()))
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
