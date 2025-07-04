using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using Jose;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
using SecurityAPICommons.Utils;
using log4net;

namespace SecurityAPICommons.Commons
{
	[SecuritySafeCritical]
	public class PublicKey : Key
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PublicKey));

		public SubjectPublicKeyInfo subjectPublicKeyInfo;

		private readonly string className = typeof(PublicKey).Name;	

		[SecuritySafeCritical]
		public PublicKey() : base()
		{
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/
		[SecuritySafeCritical]
		override
		public bool Load(string path)
		{
			string method = "Load";
			logger.Debug(method);
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput(className, method, "path", path, this.error);
			if (!(SecurityUtils.extensionIs(path, ".pem") || SecurityUtils.extensionIs(path, "key")))
			{
				this.error.setError("PU001", "Public key should be loaded from a .pem or .key file");
				logger.Error("Public key should be loaded from a .pem or .key file");
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
				logger.Error(method, e);
				return false;
			}
			return loaded;
		}

		[SecuritySafeCritical]
		override
		public bool FromBase64(string base64Data)
		{
			string method = "FromBase64";
			logger.Debug(method);
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput(className, method, "base64Data", base64Data, this.error);
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
				logger.Error(method, e);
				flag = false;
			}
			setAlgorithm();

			return flag;
		}

		[SecuritySafeCritical]
		override
		public string ToBase64()
		{
			string method = "ToBase64";
			logger.Debug(method);
			if (this.subjectPublicKeyInfo == null)
			{
				this.error.setError("PU004", "Not loaded key");
				logger.Error("Not loaded key");
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
				logger.Error(method, e);
			}

			return base64Encoded;
		}

		[SecuritySafeCritical]
		public bool FromJwks(string jwks, string kid)
		{
			string method = "FromJwks";
			logger.Debug(method);
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateStringInput(className, method, "jwks", jwks, this.error);
			SecurityUtils.validateStringInput(className, method, "kid", kid, this.error);
			if (this.HasError())
			{
				return false;
			}

			/******* INPUT VERIFICATION - END *******/

			bool flag = false;
			string b64 = "";
			try
			{
				b64 = FromJson(jwks, kid);
			}
			catch (Exception e)
			{
				this.error.setError("PU016", e.Message);
				logger.Error(method, e);
				return false;
			}
			flag = this.FromBase64(b64);
			return flag;

		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/


		private string FromJson(string json, string id)
		{
			string method = "FromJson";
			logger.Debug(method);
			JwkSet set;
			try
			{
				set = JwkSet.FromJson(json, JWT.DefaultSettings.JsonMapper);
			}
			catch (Exception e)
			{
				this.error.setError("PU015", e.Message);
				logger.Error(method, e);
				return "";
			}

			foreach (Jwk key in set)
			{
				if (key.KeyId.CompareTo(id) == 0)
				{
					byte[] m = Base64Url.Decode(key.N);
					byte[] e = Base64Url.Decode(key.E);

					RsaKeyParameters parms = new RsaKeyParameters(false, new Org.BouncyCastle.Math.BigInteger(1, m), new Org.BouncyCastle.Math.BigInteger(1, e));
					SubjectPublicKeyInfo subpubkey = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(parms);
					return Convert.ToBase64String(subpubkey.GetEncoded());

				}
			}
			return "";
		}

		private bool loadPublicKeyFromFile(string path, string alias, string password)
		{
			string method = "loadPublicKeyFromFile";
			logger.Debug(method);
			bool loaded = false;
			try
			{
				loaded = loadPublicKeyFromFile(path);
			}
			catch (Exception e)
			{
				this.error.setError("PU006", e.Message);
				logger.Error(method, e);
				return false;
			}
			return loaded;

		}

		private bool loadPublicKeyFromFile(string path)
		{
			string method = "loadPublicKeyFromFile";
			logger.Debug(method);
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
					logger.Error("This file contains a certificate, use the Certificate object instead");
					flag = false;

				}
				if (obj.GetType() == typeof(Org.BouncyCastle.X509.X509Certificate))
				{
					this.error.setError("PU011", "This file contains a certificate, use the Certificate object instead");
					logger.Error("This file contains a certificate, use the Certificate object instead");
					flag = false;
				}
				if (obj.GetType() == typeof(X509CertificateStructure))
				{
					this.error.setError("PU012", "This file contains a certificate, use the Certificate object instead");
					logger.Error("This file contains a certificate, use the Certificate object instead");
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
			string alg = this.subjectPublicKeyInfo.Algorithm.Algorithm.Id;
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
			string method = "getAsymmetricKeyParameter";
			logger.Debug(method);
			AsymmetricKeyParameter akp = null;
			try
			{
				akp = PublicKeyFactory.CreateKey(this.subjectPublicKeyInfo);
			}
			catch (Exception e)
			{
				this.error.setError("PU006", e.Message);
				logger.Error(method, e);
				return null;
			}
			return akp;
		}

		[SecuritySafeCritical]
		override
		public AsymmetricAlgorithm getAsymmetricAlgorithm()
		{
			string method = "getAsymmetricAlgorithm";
			logger.Debug(method);
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
					AlgorithmIdentifier algid = this.subjectPublicKeyInfo.Algorithm;
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
						logger.Error("Not implemented for not Windows platforms, use a x509 certificate instead");
					}		
#endif
					break;
				default:
					this.error.setError("PU014", "Unrecognized algorithm");
					logger.Error("Unrecognized algorithm");
					break;
			}
			return alg;
		}
	}
}
