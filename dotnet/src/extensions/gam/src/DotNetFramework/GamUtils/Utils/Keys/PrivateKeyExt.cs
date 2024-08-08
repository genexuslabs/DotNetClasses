using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using log4net;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;

namespace GamUtils.Utils.Keys
{
	[SecuritySafeCritical]
	internal enum PrivateKeyExt
	{
		NONE, pfx, pkcs12, p12, pem, key, b64
	}

	[SecuritySafeCritical]
	internal static class PrivateKeyUtil
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PrivateKeyUtil));

		[SecuritySafeCritical]
		internal static PrivateKeyExt Value(string ext)
		{
			switch (ext.ToLower().Trim())
			{
				case "pfx":
					return PrivateKeyExt.pfx;
				case "pkcs12":
					return PrivateKeyExt.pkcs12;
				case "p12":
					return PrivateKeyExt.p12;
				case "pem":
					return PrivateKeyExt.pem;
				case "key":
					return PrivateKeyExt.key;
				case "b64":
					return PrivateKeyExt.b64;
				default:
					logger.Error("Invalid certificate file extension");
					return PrivateKeyExt.NONE;
			}
		}

		[SecuritySafeCritical]
		internal static RSAParameters GetPrivateKey(string path, string alias, string password)
		{
			string extension = Path.GetExtension(path).Replace(".", string.Empty).Trim();
			PrivateKeyExt ext = extension.IsNullOrEmpty() ? Value("b64") : Value(extension);
			if (ext == PrivateKeyExt.NONE)
			{
				logger.Error("Error reading certificate path");
				return new RSAParameters();
			}
			switch (ext)
			{
				case PrivateKeyExt.pfx:
				case PrivateKeyExt.pkcs12:
				case PrivateKeyExt.p12:
					return GetPrivateRSAParameters(LoadFromPkcs12(path, alias, password));
				case PrivateKeyExt.pem:
				case PrivateKeyExt.key:
					return GetPrivateRSAParameters(LoadFromPkcs8(path));
				case PrivateKeyExt.b64:
					return GetPrivateRSAParameters(LoadFromBase64(path));
				default:
					logger.Error("Invalid certificate file extension");
					return new RSAParameters();
			}
		}

		private static PrivateKeyInfo LoadFromBase64(string base64)
		{
			logger.Debug("LoadFromBase64");
			byte[] keybytes = Base64.Decode(base64);
			using (Asn1InputStream istream = new Asn1InputStream(keybytes))
			{
				Asn1Sequence seq = (Asn1Sequence)istream.ReadObject();
				return PrivateKeyInfo.GetInstance(seq);
			}
		}

		private static RSAParameters GetPrivateRSAParameters(PrivateKeyInfo privateKeyInfo)
		{
			byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetDerEncoded();
			string serializedPrivate = Convert.ToBase64String(serializedPrivateBytes);
			RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(serializedPrivate));

#if NETCORE

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
try
					{
						RSA rsa = RSA.Create();
						rsa.ImportPkcs8PrivateKey(serializedPrivateBytes, out int outthing);
						return rsa.ExportParameters(true);
					}catch(Exception e )
					{
						logger.Error("CastPrivateKeyInfo", e);
						return new RSAParameters();
					}
}
#endif
			return DotNetUtilities.ToRSA(privateKey).ExportParameters(true);

		}

		private static PrivateKeyInfo LoadFromPkcs8(string path)
		{
			using (StreamReader streamReader = new StreamReader(path))
			{
				Org.BouncyCastle.OpenSsl.PemReader pemReader = null;
				try
				{
					pemReader = new Org.BouncyCastle.OpenSsl.PemReader(streamReader);
					Object obj = pemReader.ReadObject();
					if (obj.GetType() == typeof(RsaPrivateCrtKeyParameters))
					{

						AsymmetricKeyParameter asymKeyParm = (AsymmetricKeyParameter)obj;
						return CreatePrivateKeyInfo(asymKeyParm);

					}
					else if (obj.GetType() == typeof(AsymmetricCipherKeyPair))
					{
						AsymmetricCipherKeyPair asymKeyPair = (AsymmetricCipherKeyPair)obj;
						return PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymKeyPair.Private);
					}
					else
					{
						logger.Error("loadFromPkcs8: Could not load private key");
						return null;
					}
				}catch(Exception e)
				{
					logger.Error("LoadFromPkcs8", e);
					return null;
				}finally
				{
					pemReader.Reader.Close();
				}
				
			}
		}
		private static PrivateKeyInfo LoadFromPkcs12(string path, string alias, string password)
		{
			logger.Debug("LoadFromPkcs12");
			try
			{
				Pkcs12Store pkcs12 = new Pkcs12StoreBuilder().Build();
				using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					pkcs12.Load(fs, password.ToCharArray());
					foreach (string n in pkcs12.Aliases)
					{
						if (pkcs12.IsKeyEntry(n) && pkcs12.GetKey(n).Key.IsPrivate)
						{
							return PrivateKeyInfoFactory.CreatePrivateKeyInfo(pkcs12.GetKey(n).Key);
						}
					}
				}
			}catch (Exception e)
			{
				logger.Error("LoadFromPkcs12", e);
				return null;
			}
			logger.Error("LoadFromPkcs12: path not found");
			return null;

		}

		private static PrivateKeyInfo CreatePrivateKeyInfo(AsymmetricKeyParameter key)
		{
			if (key is RsaKeyParameters)
			{
				AlgorithmIdentifier algID = new AlgorithmIdentifier(
					PkcsObjectIdentifiers.RsaEncryption, DerNull.Instance);

				RsaPrivateKeyStructure keyStruct;
				if (key is RsaPrivateCrtKeyParameters)
				{
					RsaPrivateCrtKeyParameters _key = (RsaPrivateCrtKeyParameters)key;

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
			logger.Error("CreatePrivateKeyInfo");
			throw new ArgumentNullException("Class provided is not convertible: " + key.GetType().FullName);
		}
	}
}
