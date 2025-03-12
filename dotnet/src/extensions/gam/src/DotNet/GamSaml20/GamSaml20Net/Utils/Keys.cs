using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using log4net;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities.Encoders;
using GeneXus;

namespace GamSaml20Net.Utils
{
	internal class Keys
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Keys));
		internal static AsymmetricAlgorithm LoadPrivateKey(string path, string password)
		{
			if (IsBase64(path))
			{
				return CastPrivateKey(ReadBase64_privateKey(path));
			}
			else
			{

				//loads a private key from a pkcs12 keystore
				logger.Debug("LoadPrivateKey");
				Pkcs12Store pkcs12 = null;
				//System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "yourFileName.txt")
				try
				{
					pkcs12 = new Pkcs12StoreBuilder().Build();
					using (FileStream fs = new FileStream(GetCertPath(path), FileMode.Open, FileAccess.Read))
					{
						pkcs12.Load(fs, password.ToCharArray());
					}
				}

				catch (Exception e)
				{
					logger.Error("LoadPrivateKey", e);
					return null;
				}
				string pName = null;
				foreach (string n in pkcs12.Aliases)
				{
					if (pkcs12.IsKeyEntry(n) && pkcs12.GetKey(n).Key.IsPrivate)
					{
						pName = n;
						PrivateKeyInfo pk = PrivateKeyInfoFactory.CreatePrivateKeyInfo(pkcs12.GetKey(n).Key);
						return CastPrivateKey(pk);
					}

				}
				logger.Error("LoadPrivateKey", new FileLoadException("private key not found"));
				return null;
			}

		}

		internal static bool IsBase64(string path)
		{
			try
			{
				Base64.Decode(path);
				return true;
			}
			catch
			{
				return false;
			}
		}
		private static PrivateKeyInfo ReadBase64_privateKey(string base64)
		{
			byte[] keybytes = Base64.Decode(base64);
			Asn1InputStream istream = new Asn1InputStream(keybytes);
			Asn1Sequence seq = (Asn1Sequence)istream.ReadObject();
			PrivateKeyInfo privateKeyInfo = PrivateKeyInfo.GetInstance(seq);
			istream.Close();
			return privateKeyInfo;
		}

		internal static string GetHash(string certPath, string certPass)
		{
			logger.Trace("GetHash");
			string algorithm = LoadPublicKeyHash(certPath, certPass);

			string[] aux = algorithm.Split(new string[] { "with" }, StringSplitOptions.None);
			string aux1 = aux[0].Replace("-", String.Empty);
			return aux1.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
		}

		private static string GetCertPath(string path)
		{
			string currentDir = System.IO.Directory.GetParent(System.AppDomain.CurrentDomain.BaseDirectory).Parent.ToString();
			logger.Debug($"GetCertPath - Certificate directory: {currentDir} Path: {path}");
			return Path.IsPathRooted(path) ? path : System.IO.Path.Combine(currentDir, path);
		}

		private static string LoadPublicKeyHash(string path, string password)
		{
			logger.Trace("LoadPublicKeyHash");
			Pkcs12Store pkcs12 = null;

			try
			{
				pkcs12 = new Pkcs12StoreBuilder().Build();
				using (FileStream fs = new FileStream(GetCertPath(path), FileMode.Open, FileAccess.Read))
				{
					pkcs12.Load(fs, password.ToCharArray());
				}

			}

			catch (Exception e)
			{
				logger.Error("LoadPublicKeyHash", e);
				return String.Empty;
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
						return cert.SigAlgName;
					}
				}

			}
			return String.Empty;
		}

		private static AsymmetricAlgorithm CastPrivateKey(PrivateKeyInfo privateKeyInfo)
		{
			logger.Trace("CastPrivateKey");
			byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetDerEncoded();
			string serializedPrivate = Convert.ToBase64String(serializedPrivateBytes);
			RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(serializedPrivate));

			if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
			{
				return DotNetUtilities.ToRSA(privateKey);
			}
			else
			{
				try
				{
					RSA rsa = RSA.Create();
					rsa.ImportPkcs8PrivateKey(serializedPrivateBytes, out int outthing);
					return rsa;
				}
				catch (Exception e)
				{
					logger.Error("CastPrivateKey", e);
					return null;
				}
			}

		}

		internal static RSACryptoServiceProvider GetPrivateRSACryptoServiceProvider(string path, string password)
		{
			logger.Trace("GetPrivateRSACryptoServiceProvider");
			RSA asymmetricAlgorithm = (RSA)LoadPrivateKey(path, password);
			RSAParameters rsaAParameters = asymmetricAlgorithm.ExportParameters(true);
			try
			{
				RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
				provider.ImportParameters(rsaAParameters);
				return provider;

			}
			catch (Exception e)
			{
				logger.Error("GetPrivateRSACryptoServiceProvider", e);
				return null;
			}
		}

		internal static RSACryptoServiceProvider GetPublicRSACryptoServiceProvider(string path)
		{
			logger.Trace("GetPublicRSACryptoServiceProvider");

			Org.BouncyCastle.X509.X509Certificate cert = SortCertType(path);

			RsaKeyParameters key = (RsaKeyParameters)cert.GetPublicKey();

			RSAParameters param = new RSAParameters();
			param.Exponent = key.Exponent.ToByteArrayUnsigned();
			param.Modulus = key.Modulus.ToByteArrayUnsigned();
			try
			{
				RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
				provider.ImportParameters(param);
				return provider;

			}
			catch (Exception e)
			{
				logger.Error("GetPublicRSACryptoServiceProvider", e);
				return null;
			}

		}

		private static Org.BouncyCastle.X509.X509Certificate SortCertType(string path)
		{
			if (IsBase64(path))
			{
				return new X509CertificateParser().ReadCertificate(Base64.Decode(path));
			}
			else
			{
				Org.BouncyCastle.X509.X509Certificate cert = LoadPublicKeyFromDerFile(path);
				return cert == null ? loadPublicKeyFromPEMFile(path) : cert;
			}
		}

		internal static X509Certificate2 GetPublicX509Certificate2(string path)
		{
			logger.Trace("GetPublicX509Certificate2");
			Org.BouncyCastle.X509.X509Certificate cert = SortCertType(path);
			return new X509Certificate2(DotNetUtilities.ToX509Certificate(cert));
		}

		private static Org.BouncyCastle.X509.X509Certificate LoadPublicKeyFromDerFile(string path)
		{
			logger.Trace("LoadPublicKeyFromDerFile");
			//loads a public key from a DER file
			FileStream fs = null;

			try
			{
				using (fs = new FileStream(GetCertPath(path), FileMode.Open))
				{
					X509CertificateParser parser = new X509CertificateParser();
					return parser.ReadCertificate(fs);
				}

			}
			catch (Exception e)
			{
				logger.Error("LoadPublicKeyFromDerFile", e);
				return null;
			}

		}

		private static Org.BouncyCastle.X509.X509Certificate loadPublicKeyFromPEMFile(string path)
		{
			logger.Trace("loadPublicKeyFromPEMFile");


			using (StreamReader streamReader = new StreamReader(GetCertPath(path)))
			{

				PemReader pemReader = new PemReader(streamReader);
				Object obj = pemReader.ReadObject();
				if (obj.GetType() == typeof(AsymmetricKeyParameter))
				{
					logger.Error("loadPublicKeyFromPEMFile - The file contains a private key");
					return null;
				}

				if (obj.GetType() == typeof(ECPublicKeyParameters))
				{
					logger.Error("loadPublicKeyFromPEMFile - It is a public key not a certificate");
					return null;
				}

				if (obj.GetType() == typeof(System.Security.Cryptography.X509Certificates.X509Certificate))
				{
					return (Org.BouncyCastle.X509.X509Certificate)obj;


				}
				if (obj.GetType() == typeof(Org.BouncyCastle.X509.X509Certificate))
				{
					return (Org.BouncyCastle.X509.X509Certificate)obj;
				}
				if (obj.GetType() == typeof(X509CertificateStructure))
				{
					return (Org.BouncyCastle.X509.X509Certificate)obj;
				}

				pemReader.Reader.Close();
				return null;

			}
		}
	}
}
