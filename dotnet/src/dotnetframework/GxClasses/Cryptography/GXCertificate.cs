using System;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Security.Cryptography;
using log4net;
using GeneXus.Utils;


namespace GeneXus.Cryptography
{
	public class GXCertificate
    {
        static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private X509Certificate2 _cert;
        private int _lastError;
        private string _lastErrorDescription;
        private string _password;

        public GXCertificate()
        {
            _password = string.Empty;
        }

        public int Load(string certPath)
        {
            return Load(certPath, string.Empty);
        }

        public int Load(string certPath, string password)
        {
			if (File.Exists(certPath) && GXUtil.IsWindowsPlatform)
            {
                try
                {
                    if (!string.IsNullOrEmpty(password))
                    {

						_cert = new X509Certificate2(certPath, password, X509KeyStorageFlags.MachineKeySet |
                                     X509KeyStorageFlags.PersistKeySet |
                                     X509KeyStorageFlags.Exportable);
                        _password = password;
                    }
                    else
                    {
                        _cert = new X509Certificate2(certPath);
                    }
                    SetError(0);
                }
                catch (CryptographicException ex)
                {
                    GXLogging.Error(log, String.Format("Error loading certificate from", certPath), ex);
                    SetError(1);
                }
            }
            else
            {
                // Certificate Path is not valid. 
                SetError(3);
            }

            return 0;
        }

        public int FromBase64(string base64Data)
        {
            try
            {
				if (GXUtil.IsWindowsPlatform)
				{
					_cert = new X509Certificate2(Convert.FromBase64String(base64Data));
					SetError(0);
				}
				else
				{
					SetError(1);
					GXLogging.Error(log, String.Format("FromBase64 not supported in this platform"));
				}
            }
            catch (FormatException)
            {
                SetError(1);
                return _lastError;
            }
            catch (Exception)
            {
                SetError(1);
                return _lastError;
            }
            return _lastError;
        }

        public string ToBase64()
        {
            if (CertLoaded())
            {
                try
                {
                    return Convert.ToBase64String(_cert.Export(X509ContentType.Cert, _password));
                }
                catch (CryptographicException e)
                {
                    GXLogging.Error(log, String.Format("Error loading certificate from base64 string"), e);
                }
            }

            SetError(1);
            return string.Empty;

        }
        public string SerialNumber
        {
            get
            {
                string value = string.Empty;
                if (CertLoaded())
                {
                    value = _cert.SerialNumber;
                }
                return value;
            }
        }
        public string Subject
        {
            get
            {
                string value = string.Empty;
                if (CertLoaded())
                {
                    value = _cert.Subject;
                }
                return value;
            }
        }

        public int Version
        {
            get
            {
                int value = 0;
                if (CertLoaded())
                {
                    value = _cert.Version;
                }
                return value;
            }
        }

        public string Issuer
        {
            get
            {
                string value = string.Empty;
                if (CertLoaded())
                {
                    value = _cert.IssuerName.Name;
                }
                return value;
            }
        }

        public string Thumbprint
        {
            get
            {
                string value = string.Empty;
                if (CertLoaded())
                {
                    value = _cert.Thumbprint;
                }
                return value;
            }
        }


        public DateTime NotAfter
        {
            get
            {
                DateTime value = DateTimeUtil.NullDate();
                if (CertLoaded())
                {
                    value = _cert.NotAfter;
                }
                return value;
            }
        }

        public DateTime NotBefore
        {
            get
            {
                DateTime value = DateTimeUtil.NullDate();
                if (CertLoaded())
                {
                    value = _cert.NotBefore;
                }
                return value;
            }
        }

        public bool HasPrivateKey()
        {
            if (CertLoaded())
            {
                return _cert.HasPrivateKey;
            }
            return false;
        }

        public bool Verify()
        {
            try
            {
                if (CertLoaded())
                {
                    if (VerifiyCertificateChain(new X509Certificate2[] { _cert }) != null)
                    {
                        if (!Certificate.Verify())
                        {
                            SetError(2);
                            X509Chain ch = new X509Chain();
                            ch.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                            ch.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                            ch.Build(Certificate);
                            foreach (X509ChainStatus ss in ch.ChainStatus)
                            {
                                _lastErrorDescription += ss.Status + " ";
                            }
                            return false;
                        }
                    }
                    SetError(0);
                    return true;
                }

            }
            catch (Exception ex)
            {
                GXLogging.Error(log, "Error Verfying certificate", ex);
                return false;
            }
            return false;
        }

        public static X509ChainStatus[] VerifiyCertificateChain(X509Certificate2[] partialChain)
        {
            //The policy is configured to build the certificate chain. 
            //If more than one certificate is received it is taken as the complete certificate chain.
			//In that case all certificates are taken into account for validation except the first one
			//which belongs to the user who signed the document.
			//It is important to remove this last certificate because
            //otherwise it will be automatically added to the certificate list
            //trusted by the system where this check is executed.
            X509ChainPolicy chainPolicy = new X509ChainPolicy();
            if (partialChain.Length > 2)
            {
                for (int i = 1; i < partialChain.Length - 1; i++)
                    chainPolicy.ExtraStore.Add(partialChain[i]);
            }
            chainPolicy.RevocationMode = X509RevocationMode.Offline;
            chainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chainPolicy.VerificationFlags = X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown | X509VerificationFlags.IgnoreEndRevocationUnknown | X509VerificationFlags.IgnoreRootRevocationUnknown;

            //The certificate chain is initialized to use the system certificates, not those of the current user.
            X509Chain certchain = new X509Chain(true);
            certchain.ChainPolicy = chainPolicy;

            if (certchain.Build(partialChain[0]))
                return null;
            else
                return certchain.ChainStatus;
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
                    _lastErrorDescription = "Certificate could not be loaded";
                    break;
                case 2:
                    _lastErrorDescription = "Certificate is not trusted. ";
                    break;
                case 3:
                    _lastErrorDescription = "Certificate was not found";
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


        public X509Certificate2 Certificate
        {
            get { return _cert; }
        }

        internal bool CertLoaded()
        {
            return _cert != null;
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
