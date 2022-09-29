using System;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using Org.BouncyCastle.Utilities.Encoders;
using SecurityAPICommons.Commons;
using Org.BouncyCastle.Asn1.X509;
using SecurityAPICommons.Utils;

namespace SecurityAPICommons.Keys
{
    [SecuritySafeCritical]
    public sealed class CertificateX509 : Certificate, IDisposable
	{
        private string publicKeyAlgorithm;
        private X509Certificate2 cert;
        public X509Certificate2 Cert => cert;
        [SecuritySafeCritical]
        private string _issuer;
        public string Issuer
        {
            get { return _issuer; }
            set { _issuer = value; }
        }
        [SecuritySafeCritical]
        private string _subject;
        public string Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }
        [SecuritySafeCritical]
        private string _serialNumber;
        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; }
        }
        [SecuritySafeCritical]
        private string _thumbprint;
        public string Thumbprint
        {
            get { return _thumbprint; }
            set { _thumbprint = value; }
        }
        [SecuritySafeCritical]
        private DateTime _notAfter;
        public DateTime NotAfter
        {
            get { return _notAfter; }
            set { _notAfter = value; }
        }
        [SecuritySafeCritical]
        private DateTime _notBefore;
        public DateTime NotBefore
        {
            get { return _notBefore; }
            set { _notBefore = value; }
        }
        [SecuritySafeCritical]
        private int _version;
        public int Version
        {
            get { return _version; }
            set { _version = value; }
        }
        private bool inicialized;
        public bool Inicialized => inicialized;

        [SecuritySafeCritical]
        public CertificateX509() : base()
        {

            this.inicialized = false;
        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/

        [SecuritySafeCritical]
		override
        public bool Load(string certificatePath)
        {
            return LoadPKCS12(certificatePath, "", "");
        }

        [SecuritySafeCritical]
		override
        public bool LoadPKCS12(string certificatePath, string alias, string password)
        {
            bool result = false;
            try
            {
                result = loadPublicKeyFromFile(certificatePath, alias, password);
            }
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
			{
                this.error.setError("CE001", "Invalid certificate or could not found bouncy castle assembly");
                return false;
            }
            if (result)
            {
                inicializeParameters();
            }
            return result;
        }


        [SecuritySafeCritical]
		override
		public bool FromBase64(string base64Data)
        {
            bool flag;
            try
            {
                //this.cert = new X509Certificate2(Convert.FromBase64String(base64Data));
                Org.BouncyCastle.X509.X509Certificate c = new X509CertificateParser().ReadCertificate(Base64.Decode(base64Data));
                castCertificate(c);
                inicializeParameters();
                flag = true;

            }
            catch (Exception e)
            {
                this.error.setError("CE002", e.Message);
                flag = false;
            }
            return flag;
        }

        [SecuritySafeCritical]
		override
        public string ToBase64()
        {
            if (!this.inicialized)
            {
                this.error.setError("CE003", "Not loaded certificate");
                return "";
            }
            try
            {
                return Convert.ToBase64String(this.cert.Export(X509ContentType.Cert));
            }
            catch (Exception e)
            {
                this.error.setError("CE004", e.Message);
                return "";
            }

        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

        private void inicializeParameters()
        {
            this._serialNumber = this.cert.SerialNumber;
            this._subject = this.cert.Subject;
            this._version = this.cert.Version;
            this._issuer = this.cert.IssuerName.Name;
            this._thumbprint = this.cert.Thumbprint;
            this._notAfter = this.cert.NotAfter;
            this._notBefore = this.cert.NotBefore;

            this.inicialized = true;
        }

        /// <summary>
        /// Returns the certificate's hash for signing
        /// </summary>
        /// <returns>string certificate-s hash algorithm for sign verification</returns>
        [SecuritySafeCritical]
        public string getPublicKeyHash()
        {

            string[] aux = this.publicKeyAlgorithm.Split(new string[] { "with" }, StringSplitOptions.None);
            if (SecurityUtils.compareStrings(aux[0], "1.2.840.10045.2.1") || SecurityUtils.compareStrings(aux[0], "EC"))
            {
                return "ECDSA";
            }
            string aux1 = aux[0].Replace("-", "");
            return aux1.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// stores SubjectPublicKeyInfo Data Type of public key from certificate, algorithm and digest
        /// </summary>
        /// <param name="path">string of the certificate file</param>
        /// <param name="alias">Srting certificate's alias, required if PKCS12</param>
        /// <param name="password">string certificate's password, required if PKCS12</param>
        /// <returns>boolean true if loaded correctly</returns>
        private bool loadPublicKeyFromFile(string path, string alias, string password)
        {

            bool flag = false;
            if (SecurityUtils.extensionIs(path, ".pem"))
            {
                return loadPublicKeyFromPEMFile(path);
            }
            if (SecurityUtils.extensionIs(path, ".crt") || SecurityUtils.extensionIs(path, ".cer"))
            {
                return loadPublicKeyFromDERFile(path);
            }
            if (SecurityUtils.extensionIs(path, ".pfx") || SecurityUtils.extensionIs(path, ".p12") || SecurityUtils.extensionIs(path, ".pkcs12"))
            {
                return loadPublicKeyFromPKCS12File(path, password);
            }
            if (SecurityUtils.extensionIs(path, ".jks"))
            {
                this.error.setError("CE006", "Java Key Stores not allowed on .Net applications");
               // throw new Exception("Java Key Stores not allowed on .Net applications");
            }
            return flag;
        }

        /// <summary>
        /// stores SubjectPublicKeyInfo Data Type from certificate's public key, asymmetric algorithm and digest
        /// </summary>
        /// <param name="path">string .pem certificate path</param>
        /// <returns>boolean true if loaded correctly</returns>
        private bool loadPublicKeyFromPEMFile(string path)
        {
            bool flag = false;
			using (StreamReader streamReader = new StreamReader(path))
			{
				PemReader pemReader = new PemReader(streamReader);
				Object obj = pemReader.ReadObject();
				if (obj.GetType() == typeof(AsymmetricKeyParameter))
				{
					this.error.setError("CE007", "The file contains a private key");
					flag = false;
				}

				if (obj.GetType() == typeof(ECPublicKeyParameters))
				{
					/*ECPublicKeyParameters ecParms = (ECPublicKeyParameters)obj;
					 this.subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(ecParms);
					 this.publicKeyAlgorithm = ecParms.AlgorithmName;
					 this.hasPublicKey = true;
					 return true;*/
					this.error.setError("CE008", "Invalid X509 Certificate format");
					flag = false;
				}

				if (obj.GetType() == typeof(System.Security.Cryptography.X509Certificates.X509Certificate))
				{
					Org.BouncyCastle.X509.X509Certificate cert = (Org.BouncyCastle.X509.X509Certificate)obj;
					castCertificate(cert);
					flag = true;

				}
				if (obj.GetType() == typeof(Org.BouncyCastle.X509.X509Certificate))
				{
					Org.BouncyCastle.X509.X509Certificate cert = (Org.BouncyCastle.X509.X509Certificate)obj;
					castCertificate(cert);
					flag = true;
				}
				if (obj.GetType() == typeof(X509CertificateStructure))
				{
					Org.BouncyCastle.X509.X509Certificate cert = (Org.BouncyCastle.X509.X509Certificate)obj;
					castCertificate(cert);
					flag = true;
				}
				pemReader.Reader.Close();
			}
            return flag;

        }

        /// <summary>
        /// stores PublicKeyInfo Data Type from the certificate's public key, asymmetric algorithm and digest
        /// </summary>
        /// <param name="path">string .crt .cer file certificate</param>
        /// <returns>boolean true if loaded correctly</returns>
        private bool loadPublicKeyFromDERFile(string path)
        {
            bool flag = false;
            FileStream fs = null;
            Org.BouncyCastle.X509.X509Certificate cert = null;

            try
            {
				using (fs = new FileStream(path, FileMode.Open))
				{
					X509CertificateParser parser = new X509CertificateParser();
					cert = parser.ReadCertificate(fs);
				}
					
            }
			catch
			{
                this.error.setError("CE009", "Certificate coud not be loaded");
                return false;
                // throw new FileLoadException(path + " certificate coud not be loaded");
            }


            if (cert != null)
            {
                castCertificate(cert);
                fs.Close();

                return true;
            }
            return flag;

        }

        [SecuritySafeCritical]
		override
        public AsymmetricAlgorithm getAsymmetricAlgorithm()
        {
			
			AsymmetricAlgorithm alg = null;
			try
			{
				switch (this.getAlgorithm())
				{
					case "RSA":
						alg = cert.PublicKey.Key;
						break;
					case "ECDSA":
						alg = cert.GetECDsaPublicKey();
						break;
					default:
						this.error.setError("CE014", "Unrecrognized key type");
						alg = null;
						break;
				}
			}catch(Exception e)
			{
				this.error.setError("XXX", e.Message);
				alg = null;
			}
			return alg;
		}

        /// <summary>
        /// stores SubjectPublicKeyInfo Data Type from certificate's public key, asymmetric algorithm and digest
        /// </summary>
        /// <param name="path">string .ps12, pfx or .jks (PKCS12 fromat) certificate path</param>
        /// <param name="password">string certificate's password, required if PKCS12</param>
        /// <returns>boolean true if loaded correctly</returns>
        private bool loadPublicKeyFromPKCS12File(string path, string password)
        {
            bool flag = false;
            if (password == null)
            {
                this.error.setError("CE012", "Alias and password are required for PKCS12 certificates");
                return false;
            }

            Pkcs12Store pkcs12 = null;

            try
            {
                pkcs12 = new Pkcs12StoreBuilder().Build();
				using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					pkcs12.Load(fs, password.ToCharArray());
				}
                

            }

			catch (Exception e)
			{
                this.error.setError("CE013", e.Message);
                // throw new FileLoadException(path + "not found.");
            }

            if (pkcs12 != null)
            {
                string pName = null;
                foreach (string n in pkcs12.Aliases)
                {
                    if (pkcs12.IsKeyEntry(n))
                    {
                        pName = n;


                        Org.BouncyCastle.X509.X509Certificate cert = pkcs12.GetCertificate(pName).Certificate;
                        castCertificate(cert);
                        return true;
                    }
                }

            }
            this.error.setError("CE014", path + "not found.");
            return flag;
        }

        private void castCertificate(Org.BouncyCastle.X509.X509Certificate cert)
        {
            this.subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(cert.GetPublicKey());
            this.publicKeyAlgorithm = cert.SigAlgName;
            System.Security.Cryptography.X509Certificates.X509Certificate x509certificate = DotNetUtilities.ToX509Certificate(cert);
            this.cert = new X509Certificate2(x509certificate);
			setAlgorithm();
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
		public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			if(this.cert != null)
			{
				this.cert.Dispose();
				this.cert = null;
			}
		}
	}
}
