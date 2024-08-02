using System;
using System.IO;
using System.Security;
using log4net;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	internal enum CertificateExt
	{
		NONE, crt, cer, pfx, jks, pkcs12, p12, pem, key
	}


	[SecuritySafeCritical]
	internal static class CertificateUtil
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(CertificateUtil));
		public static CertificateExt Value(string ext)
		{
			switch (ext.ToLower().Trim())
			{
				case "crt":
					return CertificateExt.crt;
				case "cer":
					return CertificateExt.cer;
				case "pfx":
					return CertificateExt.pfx;
				case "jks":
					return CertificateExt.jks;
				case "pkcs12":
					return CertificateExt.pkcs12;
				case "p12":
					return CertificateExt.p12;
				case "pem":
					return CertificateExt.pem;
				case "key":
					return CertificateExt.key;
				default:
					logger.Error("Invalid certificate file extension");
					return CertificateExt.NONE;
			}
		}

		public static RSAParameters GetCertificate(string path, string alias, string password)
		{
			CertificateExt ext = CertificateUtil.Value(Path.GetExtension(path).Replace(".", String.Empty).Trim());
			if (ext == CertificateExt.NONE)
			{
				logger.Error("Error reading certificate path");
				return new RSAParameters();
			}
			switch (ext)
			{
				case CertificateExt.crt:
				case CertificateExt.cer:
					return GetPublicRSAParameters(LoadFromDer(path));
				case CertificateExt.pfx:
				case CertificateExt.jks:
				case CertificateExt.pkcs12:
				case CertificateExt.p12:
					return GetPublicRSAParameters(LoadFromPkcs12(path, alias, password));
				case CertificateExt.pem:
				case CertificateExt.key:
					return GetPublicRSAParameters(LoadFromPkcs8(path));
				default:
					logger.Error("Invalid certificate file extension");
					return new RSAParameters();
			}
		}

		private static RSAParameters GetPublicRSAParameters(Org.BouncyCastle.X509.X509Certificate cert)
		{
				X509Certificate2 cert2 = new X509Certificate2(DotNetUtilities.ToX509Certificate(cert));
				return cert2.GetRSAPublicKey().ExportParameters(false);
		}

		private static Org.BouncyCastle.X509.X509Certificate LoadFromPkcs12(string path, string alias, string password)
		{
			logger.Debug("Loading pkcs12 certificate");
			if (password.IsNullOrEmpty())
			{
				logger.Error("LoadFromPkcs12: password is null or empty");
				return null;
			}

			try
			{
				Pkcs12Store pkcs12 = new Pkcs12StoreBuilder().Build();
				using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					pkcs12.Load(fs, password.ToCharArray());
					foreach (string n in pkcs12.Aliases)
					{
						if (pkcs12.IsKeyEntry(n))
						{
							return pkcs12.GetCertificate(n).Certificate;
						}
					}
				}


			}

			catch (Exception e)
			{
				logger.Error("LoadFromPkcs12", e);
				return null;
			}
			logger.Error("LoadFromPkcs12: path not found");
			return null;

		}
		private static Org.BouncyCastle.X509.X509Certificate LoadFromPkcs8(string path)
		{
			logger.Debug("Loading pkcs8 certificate");
			using (StreamReader streamReader = new StreamReader(path))
			{
				PemReader pemReader = null;
				try
				{
					pemReader = new PemReader(streamReader);
					Object obj = pemReader.ReadObject();

					if (obj.GetType() == typeof(System.Security.Cryptography.X509Certificates.X509Certificate) || obj.GetType() == typeof(Org.BouncyCastle.X509.X509Certificate) || obj.GetType() == typeof(X509CertificateStructure))
					{
						return (Org.BouncyCastle.X509.X509Certificate)obj;
					}
					else
					{
						logger.Error("Unknown certificate type: " + obj.GetType().FullName);
						return null;
					}
				}
				catch (Exception e)
				{
					logger.Error("LoadFromPkcs8", e);
					return null;
				}
				finally
				{
					pemReader.Reader.Close();
				}
			}
		}

		private static Org.BouncyCastle.X509.X509Certificate LoadFromDer(string path)
		{
			logger.Debug("Loading der certificate");
			try
			{
				using (FileStream fs = new FileStream(path, FileMode.Open))
				{
					X509CertificateParser parser = new X509CertificateParser();
					return parser.ReadCertificate(fs);
				}

			}
			catch (Exception e)
			{
				logger.Error("LoadFromDer", e);
				return null;
			}
		}
	}
}
