using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
using SecurityAPICommons.Utils;

namespace SecurityAPICommons.Commons
{
	[SecuritySafeCritical]
	public class PublicKey : Key
	{
		public SubjectPublicKeyInfo subjectPublicKeyInfo;

		[SecuritySafeCritical]
		public PublicKey() : base()
		{
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/
		[SecuritySafeCritical]
		override
		public bool Load(string path)
		{

			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("path", path, this.error);
			if (!(SecurityUtils.extensionIs(path, ".pem") || SecurityUtils.extensionIs(path, "key")))
			{
				this.error.setError("PU001", "Public key should be loaded from a .pem or .key file");
				return false;
			}
			/******* INPUT VERIFICATION - END *******/
			bool loaded = false;
			try
			{
				loaded = loadPublicKeyFromFile(path);
			}
			catch (Exception e)
			{
				this.error.setError("PU002", e.Message);
				return false;
			}
			return loaded;
		}

		[SecuritySafeCritical]
		override
		public bool FromBase64(string base64Data)
		{

			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput("base64Data", base64Data, this.error);
			if (this.HasError())
			{
				return false;
			}

			/******* INPUT VERIFICATION - END *******/

			bool flag;
			try
			{
				byte[] dataBuffer = Base64.Decode(base64Data);
				
				this.subjectPublicKeyInfo = SubjectPublicKeyInfo.GetInstance(Asn1Sequence.GetInstance(dataBuffer));
				flag = true;
				
			}
			catch (Exception e)
			{
				this.error.setError("PU003", e.Message);
				flag = false;
			}
			setAlgorithm();

			return flag;
		}

		[SecuritySafeCritical]
		override
		public string ToBase64()
		{
			if (this.subjectPublicKeyInfo == null)
			{
				this.error.setError("PU004", "Not loaded key");
				return "";
			}
			string base64Encoded = "";

			try
			{
				base64Encoded = Convert.ToBase64String(this.subjectPublicKeyInfo.GetEncoded());

			}
			catch (Exception e)
			{
				this.error.setError("PU005", e.Message);
			}

			return base64Encoded;
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		private bool loadPublicKeyFromFile(string path, string alias, string password)
		{
			bool loaded = false;
			try
			{
				loaded = loadPublicKeyFromFile(path);
			}
			catch (Exception e)
			{
				this.error.setError("PU006", e.Message);
				return false;
			}
			return loaded;

		}

		private bool loadPublicKeyFromFile(string path)
		{
			bool flag = false;
			using (StreamReader streamReader = new StreamReader(path))
			{
				PemReader pemReader = new PemReader(streamReader);
				Object obj = pemReader.ReadObject();
				if (obj.GetType() == typeof(AsymmetricKeyParameter))
				{
					this.error.setError("PU008", "The file contains a private key");
					flag = false;
				}
				if (obj.GetType() == typeof(SubjectPublicKeyInfo))
				{
					this.subjectPublicKeyInfo = (SubjectPublicKeyInfo)obj;
					flag = true;
				}
				if (obj.GetType() == typeof(ECPublicKeyParameters))
				{
					ECPublicKeyParameters ecParms = (ECPublicKeyParameters)obj;
					this.subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(ecParms);
					flag = true;
				}
				if (obj.GetType() == typeof(RsaKeyParameters))
				{
					RsaKeyParameters ecParms = (RsaKeyParameters)obj;
					this.subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(ecParms);
					flag = true;
				}

				if (obj.GetType() == typeof(System.Security.Cryptography.X509Certificates.X509Certificate))
				{
					this.error.setError("PU009", "This file contains a certificate, use the Certificate object instead");
					flag = false;

				}
				if (obj.GetType() == typeof(Org.BouncyCastle.X509.X509Certificate))
				{
					this.error.setError("PU011", "This file contains a certificate, use the Certificate object instead");
					flag = false;
				}
				if (obj.GetType() == typeof(X509CertificateStructure))
				{
					this.error.setError("PU012", "This file contains a certificate, use the Certificate object instead");
					flag = false;
				}

				pemReader.Reader.Close();
			}
			if (flag) { setAlgorithm(); }
			return flag;

		}

		[SecuritySafeCritical]
		override
		public void setAlgorithm()
		{
			if (this.subjectPublicKeyInfo == null)
			{
				return;
			}
			string alg = this.subjectPublicKeyInfo.AlgorithmID.Algorithm.Id;
			switch (alg)
			{
				case "1.2.840.113549.1.1.1":
					this.algorithm = "RSA";
					break;
				case "1.2.840.10045.2.1":
					this.algorithm = "ECDSA";
					break;
			}

		}

		[SecuritySafeCritical]
		override
		public AsymmetricKeyParameter getAsymmetricKeyParameter()
		{
			AsymmetricKeyParameter akp = null;
			try
			{
				akp = PublicKeyFactory.CreateKey(this.subjectPublicKeyInfo);
			}
			catch (Exception e)
			{
				this.error.setError("PU006", e.Message);
				return null;
			}
			return akp;
		}

		[SecuritySafeCritical]
		override
		public AsymmetricAlgorithm getAsymmetricAlgorithm()
		{
			AsymmetricAlgorithm alg = null;
			switch (this.getAlgorithm())
			{
				case "RSA":
					RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters((RsaKeyParameters)this.getAsymmetricKeyParameter());
					RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
					rsa.ImportParameters(rsaParameters);
					alg = rsa;
					break;
				case "ECDSA":

#if !NETCORE
					ECPublicKeyParameters pubkeyparms = (ECPublicKeyParameters)this.getAsymmetricKeyParameter();
					AlgorithmIdentifier algid = this.subjectPublicKeyInfo.AlgorithmID;
					string oid = ((DerObjectIdentifier)algid.Parameters).Id;
					ECParameters ecparams = new ECParameters();
					ecparams.Curve = ECCurve.CreateFromOid(new Oid(oid));
					ecparams.Q.X = pubkeyparms.Q.XCoord.GetEncoded();
					ecparams.Q.Y = pubkeyparms.Q.YCoord.GetEncoded();
					ECDsaCng msEcdsa = new ECDsaCng();
					msEcdsa.ImportParameters(ecparams);
					alg = msEcdsa;
					//this.error.setError("XXX", "Not implemented for .Net Framework, use a x509 certificate instead");
#else
					int bytesRead;
					ECDsa ecdsa;
					try
					{
#pragma warning disable CA1416 // Validate platform compatibility
						ecdsa = new ECDsaCng();
#pragma warning restore CA1416 // Validate platform compatibility
						ecdsa.ImportSubjectPublicKeyInfo(this.subjectPublicKeyInfo.GetDerEncoded(), out bytesRead);
						alg = ecdsa;
					}
					catch(PlatformNotSupportedException)
					{
						this.error.setError("PU013", "Not implemented for not Windows platforms, use a x509 certificate instead");
					}		
#endif
					break;
				default:
					this.error.setError("PU014", "Unrecognized algorithm");
					break;
			}
			return alg;
		}
	}
}
