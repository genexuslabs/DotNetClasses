using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.Nist;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;
using Org.BouncyCastle.Utilities.Encoders;
using System.Security.AccessControl;
using System.Globalization;

namespace SecurityAPICommons.Keys
{
    [SecuritySafeCritical]
    public class PrivateKeyManager : PrivateKey
    {

        private PrivateKeyInfo privateKeyInfo;
        private bool hasPrivateKey;
        public bool HasPrivateKey
        {
            get { return hasPrivateKey; }
        }
        private string privateKeyAlgorithm;
        private string encryptionPassword;

        [SecuritySafeCritical]
        public PrivateKeyManager() : base()
        {
            this.hasPrivateKey = false;
            this.encryptionPassword = null;

        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/

        [SecuritySafeCritical]
        public bool Load(string privateKeyPath)
        {
            return LoadPKCS12(privateKeyPath, "", "");
        }

        [SecuritySafeCritical]
        public bool LoadEncrypted(string privateKeyPath, string encryptionPassword)
		{
            this.encryptionPassword = encryptionPassword;
            return LoadPKCS12(privateKeyPath, "", "");

        }


        [SecuritySafeCritical]
        public bool LoadPKCS12(string privateKeyPath, string alias, string password)
        {
            try
            {
                loadKeyFromFile(privateKeyPath, alias, password);
            }
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				this.error.setError("PK018", e.Message);
                return false;
            }
            if (this.HasError())
            {
                return false;
            }
            return true;
        }

		[SecuritySafeCritical]
		public bool FromBase64(string base64)
		{
			bool res;
			try
			{
				res = ReadBase64(base64);
			}
			catch (IOException e)
			{
				this.error.setError("PK0015", e.Message);
				return false;
			}
			this.hasPrivateKey = res;
			return res;
		}

		[SecuritySafeCritical]
		public string ToBase64()
		{
			if (this.hasPrivateKey)
			{
				string encoded = "";
				try
				{
					encoded = Base64.ToBase64String(this.privateKeyInfo.GetEncoded());
				}
#pragma warning disable CA1031 // Do not catch general exception types
				catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
				{
					this.error.setError("PK0017", e.Message);
					return "";
				}
				return encoded;
			}
			this.error.setError("PK0016", "No private key loaded");
			return "";


		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		private bool ReadBase64(string base64)
		{
		byte[] keybytes = Base64.Decode(base64);
		Asn1InputStream istream = new Asn1InputStream(keybytes);
		Asn1Sequence seq = (Asn1Sequence)istream.ReadObject();
	    this.privateKeyInfo = PrivateKeyInfo.GetInstance(seq);
	    istream.Close();
	    if (this.privateKeyInfo == null)

		{
			this.error.setError("PK015", "Could not read private key from base64 string");
			return false;
		}
	    this.privateKeyAlgorithm = this.privateKeyInfo.PrivateKeyAlgorithm.Algorithm.Id;//this.privateKeyInfo.GetPrivateKeyAlgorithm().getAlgorithm().getId(); // 1.2.840.113549.1.1.1
			return true;
	}

	[SecuritySafeCritical]
        public AsymmetricAlgorithm getPrivateKeyForXML()
        {
            if (!this.hasPrivateKey)
            {
                this.error.setError("PK0021", "No private key loaded");
                return null;
            }
            string algorithm = getPrivateKeyAlgorithm();
            if (SecurityUtils.compareStrings("RSA", algorithm))
            {


                byte[] serializedPrivateBytes = this.privateKeyInfo.ToAsn1Object().GetDerEncoded();
                string serializedPrivate = Convert.ToBase64String(serializedPrivateBytes);
                RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(serializedPrivate));
#if NETCORE
 return DotNetUtilities.ToRSA(privateKey);
#else


                /****System.Security.Cryptography.CryptographicException: The system cannot find the file specified.****/
                /****HACK****/
                //https://social.msdn.microsoft.com/Forums/vstudio/en-US/7ea48fd0-8d6b-43ed-b272-1a0249ae490f/systemsecuritycryptographycryptographicexception-the-system-cannot-find-the-file-specified?forum=clr#37d4d83d-0eb3-497a-af31-030f5278781a
                CspParameters cspParameters = new CspParameters();
                cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
                if (SecurityUtils.compareStrings(Config.SecurityApiGlobal.GLOBALKEYCOONTAINERNAME, ""))
                {
                    string uid = Guid.NewGuid().ToString();
                    cspParameters.KeyContainerName = uid;
                    Config.SecurityApiGlobal.GLOBALKEYCOONTAINERNAME = uid;
                    System.Security.Principal.SecurityIdentifier userId = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WindowsIdentity.GetCurrent().User.ToString());
                    CryptoKeyAccessRule rule = new CryptoKeyAccessRule(userId, CryptoKeyRights.FullControl, AccessControlType.Allow);
                    cspParameters.CryptoKeySecurity = new CryptoKeySecurity();
                    cspParameters.CryptoKeySecurity.SetAccessRule(rule);
                }
                else
                {
                    cspParameters.KeyContainerName = Config.SecurityApiGlobal.GLOBALKEYCOONTAINERNAME;

                }
                /****System.Security.Cryptography.CryptographicException: The system cannot find the file specified.****/
                /****HACK****/
                return DotNetUtilities.ToRSA(privateKey, cspParameters);
#endif


            }
            else
            {
                this.error.setError("PK002", "XML signature with ECDSA keys is not implemented on Net Framework");
                return null;
                //https://stackoverflow.com/questions/27420789/sign-xml-with-ecdsa-and-sha256-in-net?rq=1
                // https://www.powershellgallery.com/packages/Posh-ACME/2.6.0/Content/Private%5CConvertFrom-BCKey.ps1

            }

        }

        /// <summary>
        /// Return AsymmetricKeyParameter with private key for the indicated algorithm
        /// </summary>
        /// <returns>AsymmetricKeyParameter type for signing, algorithm dependant</returns>
        [SecuritySafeCritical]
        public AsymmetricKeyParameter getPrivateKeyParameterForSigning()
        {


            if (SecurityUtils.compareStrings(this.getPrivateKeyAlgorithm(), "RSA"))
            {
                return getRSAKeyParameter();
            }
            if (SecurityUtils.compareStrings(this.getPrivateKeyAlgorithm(), "ECDSA"))
            {
                AsymmetricKeyParameter parmsECDSA;
                try
                {
                    parmsECDSA = PrivateKeyFactory.CreateKey(this.privateKeyInfo);
                }
                catch (IOException )
                {
                    this.error.setError("AE007", "Not ECDSA key");
                    return null;
                    throw;
                }
                return parmsECDSA;
            }
            this.error.setError("AE008", "Unrecognized algorithm");
            return null;

        }
        /// <summary>
        /// Return AsymmetricKeyParameter with private key for the indicated algorithm
        /// </summary>
        /// <returns>AsymmetricKeyParameter type for encryption, algorithm dependant</returns>
        [SecuritySafeCritical]
        public AsymmetricKeyParameter getPrivateKeyParameterForEncryption()
        {


            if (SecurityUtils.compareStrings(this.getPrivateKeyAlgorithm(), "RSA"))
            {
                return getRSAKeyParameter();
            }

            this.error.setError("AE009", "Unrecognized encryption algorithm");
            return null;

        }

        /// <summary>
        /// Returns AsymmetricKeyParameter for RSA key types
        /// </summary>
        /// <param name="isPrivate"> boolean true if its a private key, false if its a public key</param>
        /// <returns>AsymmetricKeyParameter for RSA with loaded key</returns>
        private AsymmetricKeyParameter getRSAKeyParameter()
        {
            RsaKeyParameters parms;
            try
            {
                parms = (RsaKeyParameters)PrivateKeyFactory.CreateKey(this.privateKeyInfo);
            }
            catch (IOException)
            {
                this.error.setError("AE013", "Not RSA key");
                return null;
                throw;
            }
            return parms;
        }



        /// <summary>
        /// Returns private key for signing https://www.alvestrand.no/objectid/1.2.840.113549.1.1.1.html
        /// </summary>
        /// <returns>string certificate's algorithm for signing, 1.2.840.113549.1.1.1 if RSA from key pem file</returns>
        [SecuritySafeCritical]
        public string getPrivateKeyAlgorithm()
        {

            if (SecurityUtils.compareStrings(this.privateKeyAlgorithm, "1.2.840.113549.1.1.1") || SecurityUtils.compareStrings(this.privateKeyAlgorithm, "RSA"))
            {
                return "RSA";
            }
            if (SecurityUtils.compareStrings(this.privateKeyAlgorithm, "1.2.840.10045.2.1") || SecurityUtils.compareStrings(this.privateKeyAlgorithm, "EC"))
            {
                return "ECDSA";
            }
            return this.privateKeyAlgorithm.ToUpper(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Stores structure of public or private key from any type of certificate
        /// </summary>
        /// <param name="path">string of the certificate file</param>
        /// <param name="alias">Srting certificate's alias, required if PKCS12</param>
        /// <param name="password">string certificate's password, required if PKCS12</param>
        /// <param name="isPrivate"></param>
        /// <returns>boolean true if private key, boolean false if public key</returns>
        internal bool loadKeyFromFile(string path, string alias, string password)
        {
            return loadPrivateKeyFromFile(path, alias, password);
        }
        private bool loadPrivateKeyFromFile(string path, string alias, string password)
        {

            bool flag = false;
            if (SecurityUtils.extensionIs(path, ".pem") || SecurityUtils.extensionIs(path, ".key"))
            {
                return loadPrivateKeyFromPEMFile(path);
            }
            if (SecurityUtils.extensionIs(path, ".pfx") || SecurityUtils.extensionIs(path, ".p12") || SecurityUtils.extensionIs(path, ".pkcs12"))
            {
                return loadPrivateKeyFromPKCS12File(path, password);
            }
            if (SecurityUtils.extensionIs(path, ".jks"))
            {
                this.error.setError("PK003", "Java Key Stores not allowed on .Net applications");
                //throw new Exception("Java Key Stores not allowed on .Net applications");
            }

            return flag;
        }

        /// <summary>
        /// Stores PrivateKeyInfo Data Type from certificate's private key, algorithm and digest
        /// </summary>
        /// <param name="path">string .ps12, pfx or .jks (PKCS12 fromat) certificate path</param>
        /// <param name="password">string certificate's password, required if PKCS12</param>
        /// <returns></returns>
        private bool loadPrivateKeyFromPKCS12File(string path, string password)
        {
            bool flag = false;
            if (password == null)
            {
                this.error.setError("PK004", "Alias and Password are required for PKCS12 keys");
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

            catch
            {
                this.error.setError("PK005", path + "not found or wrong password.");
                //throw new FileLoadException(path + "not found or wrong password.");
            }

            if (pkcs12 != null)
            {
                string pName = null;
                foreach (string n in pkcs12.Aliases)
                {
                    if (pkcs12.IsKeyEntry(n) && pkcs12.GetKey(n).Key.IsPrivate)
                    {
                        pName = n;

                        this.privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(pkcs12.GetKey(n).Key);
                        this.privateKeyAlgorithm = this.privateKeyInfo.PrivateKeyAlgorithm.Algorithm.Id;
                        this.hasPrivateKey = true;
                        return true;
                    }
                }

            }
            this.error.setError("PK006", path + " not found");
            return flag;

        }

        /// <summary>
        /// stores PrivateKeyInfo Data Type from certificate's private key
        /// </summary>
        /// <param name="path">string .pem certificate path</param>
        /// <returns>boolean true if loaded correctly</returns>
        private bool loadPrivateKeyFromPEMFile(string path)
        {
            bool flag = false;
            StreamReader streamReader = new StreamReader(path);
            PemReader pemReader = new PemReader(streamReader);
            Object obj = null;
            try
            {
                obj = pemReader.ReadObject();
            }catch(Exception)
			{
                if(this.encryptionPassword == null)
				{
                    this.error.setError("PK024", "Password for key decryption is empty");
                    return false;
				}
                try
                {
                    StreamReader sReader = new StreamReader(path);
                    PemReader pReader = new PemReader(sReader, new PasswordFinder(this.encryptionPassword));
                    obj = pReader.ReadObject();
                    closeReaders(sReader, pReader);
                }catch(Exception ex)
				{
                    this.error.setError("PK023", ex.Message);
                    return false;
				}
			}
            if (obj.GetType() == typeof(RsaPrivateCrtKeyParameters))
            {
                AsymmetricKeyParameter asymKeyParm = (AsymmetricKeyParameter)obj;
                this.privateKeyInfo = createPrivateKeyInfo(asymKeyParm);
                this.privateKeyAlgorithm = this.privateKeyInfo.PrivateKeyAlgorithm.Algorithm.Id;
                this.hasPrivateKey = true;
                closeReaders(streamReader, pemReader);
                return true;
            }
            if (obj.GetType() == typeof(Pkcs8EncryptedPrivateKeyInfo))
            {
                this.error.setError("PK007", "Encrypted key, remove the key password");
                flag = false;
            }
            if (obj.GetType() == typeof(AsymmetricCipherKeyPair))
            {
                AsymmetricCipherKeyPair asymKeyPair = (AsymmetricCipherKeyPair)obj;
                this.privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymKeyPair.Private);
                this.privateKeyAlgorithm = this.privateKeyInfo.PrivateKeyAlgorithm.Algorithm.Id;
                this.hasPrivateKey = true;
                return true;
            }
            if (obj.GetType() == typeof(X509Certificate))
            {
                this.error.setError("PK008", "The file contains a public key");
                flag = false;

            }
            closeReaders(streamReader, pemReader);
            return flag;

        }

        /// <summary>
        /// Excecute close methods of PemReader and StreamReader data types
        /// </summary>
        /// <param name="streamReader">StreamReader type</param>
        /// <param name="pemReader">PemReader type</param>
        private void closeReaders(StreamReader streamReader, PemReader pemReader)
        {
            try
            {
                streamReader.Close();
                pemReader.Reader.Close();
            }
#pragma warning disable CA1031 // Do not catch general exception types
			catch
#pragma warning restore CA1031 // Do not catch general exception types
			{
                this.error.setError("PK012", "Error closing StreamReader/ PemReader for certificates");
            }
        }

        /// <summary>
        /// Build private PrivateKeyInfo
        /// https://csharp.hotexamples.com/examples/Org.BouncyCastle.Asn1.Pkcs/RsaPrivateKeyStructure/-/php-rsaprivatekeystructure-class-examples.html
        /// </summary>
        /// <param name="key">AsymmetricKeyParameter key</param>
        /// <returns>PrivateKeyInfo from AsymmetricKeyParameter </returns>
        private PrivateKeyInfo createPrivateKeyInfo(AsymmetricKeyParameter key)
        {

            if (key is DsaPrivateKeyParameters)
            {

                DsaPrivateKeyParameters _key = (DsaPrivateKeyParameters)key;
                this.hasPrivateKey = true;
                this.privateKeyAlgorithm = "ECDSA";
                return new PrivateKeyInfo(
                    new AlgorithmIdentifier(
                    X9ObjectIdentifiers.IdDsa,
                    new DsaParameter(
                    _key.Parameters.P,
                    _key.Parameters.Q,
                    _key.Parameters.G).ToAsn1Object()),
                    new DerInteger(_key.X));
            }


            if (key is RsaKeyParameters)
            {
                AlgorithmIdentifier algID = new AlgorithmIdentifier(
                    PkcsObjectIdentifiers.RsaEncryption, DerNull.Instance);

                RsaPrivateKeyStructure keyStruct;
                if (key is RsaPrivateCrtKeyParameters)
                {
                    RsaPrivateCrtKeyParameters _key = (RsaPrivateCrtKeyParameters)key;
                    this.hasPrivateKey = true;
                    this.privateKeyAlgorithm = "RSA";
                    keyStruct = new RsaPrivateKeyStructure(
                        _key.Modulus,
                        _key.PublicExponent,
                        _key.Exponent,
                        _key.P,
                        _key.Q,
                        _key.DP,
                        _key.DQ,
                        _key.QInv);
                }
                else
                {
                    RsaKeyParameters _key = (RsaKeyParameters)key;
                    this.hasPrivateKey = true;
                    this.privateKeyAlgorithm = "RSA";
                    keyStruct = new RsaPrivateKeyStructure(
                        _key.Modulus,
                        BigInteger.Zero,
                        _key.Exponent,
                        BigInteger.Zero,
                        BigInteger.Zero,
                        BigInteger.Zero,
                        BigInteger.Zero,
                        BigInteger.Zero);
                }

                return new PrivateKeyInfo(algID, keyStruct.ToAsn1Object());
            }
            this.error.setError("PK013", "Class provided is not convertible: " + key.GetType().FullName);
            this.hasPrivateKey = false;
            throw new ArgumentNullException("Class provided is not convertible: " + key.GetType().FullName);

        }

        [SecuritySafeCritical]
        public AsymmetricAlgorithm getPrivateKeyForJWT()
        {
            if (!this.hasPrivateKey)
			{
                this.error.setError("PK0020", "No private key loaded");
                return null;
            }
            AsymmetricAlgorithm alg;
                string algorithm = getPrivateKeyAlgorithm();
                if (SecurityUtils.compareStrings("RSA", algorithm))
                {

                    byte[] serializedPrivateBytes = this.privateKeyInfo.ToAsn1Object().GetDerEncoded();
                    string serializedPrivate = Convert.ToBase64String(serializedPrivateBytes);
                    RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(serializedPrivate));
#if NETCORE
 alg = DotNetUtilities.ToRSA(privateKey);
#else


                /****System.Security.Cryptography.CryptographicException: The system cannot find the file specified.****/
                /****HACK****/
                //https://social.msdn.microsoft.com/Forums/vstudio/en-US/7ea48fd0-8d6b-43ed-b272-1a0249ae490f/systemsecuritycryptographycryptographicexception-the-system-cannot-find-the-file-specified?forum=clr#37d4d83d-0eb3-497a-af31-030f5278781a
                CspParameters cspParameters = new CspParameters();
                cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
                if (SecurityUtils.compareStrings(Config.SecurityApiGlobal.GLOBALKEYCOONTAINERNAME, ""))
                {
                    string uid = Guid.NewGuid().ToString();
                    cspParameters.KeyContainerName = uid;
                    Config.SecurityApiGlobal.GLOBALKEYCOONTAINERNAME = uid;
                    System.Security.Principal.SecurityIdentifier userId = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WindowsIdentity.GetCurrent().User.ToString());
                    CryptoKeyAccessRule rule = new CryptoKeyAccessRule(userId, CryptoKeyRights.FullControl, AccessControlType.Allow);
                    cspParameters.CryptoKeySecurity = new CryptoKeySecurity();
                    cspParameters.CryptoKeySecurity.SetAccessRule(rule);
                }
                else
                {
                    cspParameters.KeyContainerName = Config.SecurityApiGlobal.GLOBALKEYCOONTAINERNAME;

                }
                /****System.Security.Cryptography.CryptographicException: The system cannot find the file specified.****/
                /****HACK****/
                alg = DotNetUtilities.ToRSA(privateKey, cspParameters);
#endif
            }
            else if (SecurityUtils.compareStrings("ECDSA", algorithm))
                {
                    string b64Encoded = this.ToBase64();
                    byte[] privKeyBytes8 = Convert.FromBase64String(b64Encoded);//Encoding.UTF8.GetBytes(privKeyEcc);
                try
                {
					
					using (ECDsaCng pubCNG = new ECDsaCng(CngKey.Import(privKeyBytes8, CngKeyBlobFormat.Pkcs8PrivateBlob)))
					{
						alg = pubCNG;
					}
					
#pragma warning disable CA1031 // Do not catch general exception types
				}
				catch(Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
				{
                    this.error.setError("PK022", e.Message);
                    return null;
				}
                }
                else
                {
                    this.error.setError("PK019", "Unrecognized key type");
                    return null;
                }
            if(alg != null)
			{
                this.error.cleanError();
			}
            return alg;

        }


        private class PasswordFinder : IPasswordFinder
        {
            private string password;

            public PasswordFinder(string password)
            {
                this.password = password;
            }


            public char[] GetPassword()
            {
                return this.password.ToCharArray();
            }
        }


    }
}
