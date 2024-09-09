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
using Org.BouncyCastle.Utilities.Encoders;
using Jose;
using System.Collections.Generic;

namespace GamUtils.Utils.Keys
{
	[SecuritySafeCritical]
	internal enum PublicKeyExt
	{
		NONE, crt, cer, pfx, pkcs12, p12, pem, key, b64, json
	}


	[SecuritySafeCritical]
	internal static class PublicKeyUtil
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PublicKeyUtil));



		[SecuritySafeCritical]
		internal static RSAParameters GetPublicKey(string path, string alias, string password, string token)
		{
			logger.Debug("GetPublicKey");
			PublicKeyExt ext = PublicKeyUtil.Value(FixType(path));
			switch (ext)
			{
				case PublicKeyExt.crt:
				case PublicKeyExt.cer:
					return GetPublicRSAParameters(LoadFromDer(path));
				case PublicKeyExt.pfx:
				case PublicKeyExt.pkcs12:
				case PublicKeyExt.p12:
					return GetPublicRSAParameters(LoadFromPkcs12(path, alias, password));
				case PublicKeyExt.pem:
				case PublicKeyExt.key:
					return GetPublicRSAParameters(LoadFromPkcs8(path));
				case PublicKeyExt.b64:
					return GetPublicRSAParameters(LoadFromBase64(path));
				case PublicKeyExt.json:
					return LoadFromJson(path, token);
				default:
					logger.Error("Invalid certificate file extension");
					return new RSAParameters();
			}
		}

		[SecuritySafeCritical]
		private static PublicKeyExt Value(string ext)
		{
			switch (ext.ToLower().Trim())
			{
				case "crt":
					return PublicKeyExt.crt;
				case "cer":
					return PublicKeyExt.cer;
				case "pfx":
					return PublicKeyExt.pfx;
				case "pkcs12":
					return PublicKeyExt.pkcs12;
				case "p12":
					return PublicKeyExt.p12;
				case "pem":
					return PublicKeyExt.pem;
				case "key":
					return PublicKeyExt.key;
				case "b64":
					return PublicKeyExt.b64;
				case "json":
					return PublicKeyExt.json;
				default:
					logger.Error("Invalid certificate file extension");
					return PublicKeyExt.NONE;
			}
		}

		private static RSAParameters LoadFromJson(string json, string token)
		{
			logger.Debug("LoadFromJson");
			RSAParameters publicKey = new RSAParameters();
			try
			{
				Jwk jwk = Jwk.FromJson(json);
#if !NETCORE
				publicKey.Exponent = Base64Url.Decode(jwk.E);
				publicKey.Modulus = Base64Url.Decode(jwk.N);
				return publicKey;
#else
				if(jwk.E == null)
				{
					return LoadFromJwks(json, token);
				}
				else
				{
					publicKey.Exponent = Base64Url.Decode(jwk.E);
					publicKey.Modulus = Base64Url.Decode(jwk.N);
					return publicKey;
				}
#endif
			}
			catch(Exception)
			{
				
				return LoadFromJwks(json, token);
			}
			
		}

		private static RSAParameters LoadFromJwks(string json, string token)
		{
			logger.Debug("LoadFromJwks");
			RSAParameters publicKey = new RSAParameters();
			try
			{
				JwkSet jwks = JwkSet.FromJson(json);
				IDictionary<string, dynamic> headers = JWT.Headers(token);
				string kid = headers["kid"];
				foreach (Jwk jwk in jwks)
				{
					if (jwk.KeyId.Equals(kid))
					{
						publicKey.Exponent = Base64Url.Decode(jwk.E);
						publicKey.Modulus = Base64Url.Decode(jwk.N);
					}
				}
				logger.Error("No matching token kid to jwks");
			}
			catch(Exception e)
			{
				logger.Error("LoadFromJwks", e);
			}
			return publicKey;
		}


		private static string FixType(string input)
		{
			try
			{
				string extension = Path.GetExtension(input).Replace(".", string.Empty).Trim();
#if !NETCORE
				return extension.IsNullOrEmpty() ? "b64" : extension;
#else
				if (extension.IsNullOrEmpty())
				{
					try
					{
						Base64.Decode(input);
						return "b64";
					}
					catch (Exception)
					{
						return "json";
					}
				}
				else
				{
					return extension;
				}
#endif
			}
			catch (ArgumentException)
			{
				return "json";
			}
		}

		private static Org.BouncyCastle.X509.X509Certificate LoadFromBase64(string base64)
		{
			logger.Debug("LoadFromBase64");
			try
			{
				return new X509CertificateParser().ReadCertificate(Base64.Decode(base64));

			}
			catch (Exception e)
			{
				logger.Error("LoadFromBase64", e);
				Console.WriteLine("error LoadFromBase64");
				return null;
			}
		}

		private static RSAParameters GetPublicRSAParameters(Org.BouncyCastle.X509.X509Certificate cert)
		{
			logger.Debug("GetPublicRSAParameters");
			try
			{
				X509Certificate2 cert2 = new X509Certificate2(DotNetUtilities.ToX509Certificate(cert));
				Console.WriteLine("GetPublicRSAParameters" + cert2.GetSerialNumberString());
				return cert2.GetRSAPublicKey().ExportParameters(false);
			}catch(Exception e)
			{
				logger.Error("GetPublicRSAParameters", e);
				return new RSAParameters();
			}
		}

		private static Org.BouncyCastle.X509.X509Certificate LoadFromPkcs12(string path, string alias, string password)
		{
			logger.Debug("LoadFromPkcs12");
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
			logger.Debug("LoadFromPkcs8");
			using (StreamReader streamReader = new StreamReader(path))
			{
				PemReader pemReader = null;
				try
				{
					pemReader = new PemReader(streamReader);
					object obj = pemReader.ReadObject();

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
			logger.Debug("LoadFromDer");
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
