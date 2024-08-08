using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using GamUtils;
using GamUtils.Utils;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.Runtime.InteropServices;



namespace GamTest.Utils
{
	[TestFixture]
	public class TestJwt
	{
#pragma warning disable CS0414
		private static string resources;
		private static string header;
		private static string payload;
		private static string kid;
		private static RSAParameters keypair;
		private static string path_RSA_sha256_2048;
		private static string alias;
		private static string password;
		private static string tokenFile;
#pragma warning disable IDE0044
		private static string BASE_PATH;
#pragma warning restore IDE0044
#pragma warning restore CS0414



		[SetUp]
		public virtual void SetUp()
		{
			BASE_PATH = GetStartupDirectory();
			resources = Path.Combine(BASE_PATH, "Resources", "dummycerts");
			kid = Guid.NewGuid().ToString();
			header = "{\n" +
				"  \"alg\": \"RS256\",\n" +
				"  \"kid\": \"" + kid + "\",\n" +
				"  \"typ\": \"JWT\"\n" +
				"}";
			payload = "{\n" +
				"  \"sub\": \"1234567890\",\n" +
				"  \"name\": \"John Doe\",\n" +
				"  \"iat\": 1516239022\n" +
				"}";
			keypair = GenerateKeys();
			path_RSA_sha256_2048 = Path.Combine(resources, "RSA_sha256_2048");
			alias = "1";
			password = "dummy";
			tokenFile = Jwt.Create(LoadPrivateKey(path_RSA_sha256_2048 + "\\sha256d_key.pem"), payload, header);
		}

		internal static string GetStartupDirectory()
		{
#pragma warning disable SYSLIB0044
			string dir = Assembly.GetCallingAssembly().GetName().CodeBase;
#pragma warning restore SYSLIB0044
			Uri uri = new Uri(dir);
			return Path.GetDirectoryName(uri.LocalPath);
		}


		private static RSAParameters GenerateKeys()
		{
			try
			{
				RSA rsa = RSA.Create();
				return rsa.ExportParameters(true);
			}
			catch (Exception)
			{
				return new RSAParameters();
			}
		}

		
		[Test]
		public void TestCreate()
		{
			try
			{
				string token = Jwt.Create(keypair, payload, header);
				Assert.IsFalse(token.IsNullOrEmpty(), "testCreate fail");
			}
			catch (Exception e)
			{
				Assert.Fail("testCreate fail. Exception: " + e.Message);
			}
		}


		[Test]
		public void TestVerify()
		{
			try
			{
				string token = Jwt.Create(keypair, payload, header);
				bool verifies = Jwt.Verify(keypair, token);
				Assert.IsTrue(verifies, "testVerify fail");
			}
			catch (Exception e)
			{
				Assert.Fail("testVerify fail. Exception: " + e.Message);
			}
		}


		[Test]
		public void TestVerify_wrong()
		{
			try
			{
				string token = Jwt.Create(keypair, payload, header);
				bool verifies = Jwt.Verify(GenerateKeys(), token);
				Assert.IsFalse(verifies, "TestVerify_wrong fail");
			}
			catch (Exception e)
			{
				Assert.Fail("testVerify_wrong fail. Exception: " + e.Message);
			}
		}


		[Test]
		public void TestVerifyPkcs8()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.pem", "", "", tokenFile);
			Assert.IsTrue(result, "testVerifyPkcs8");
		}

		
		[Test]
		public void TestVerifyDer()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.cer", "", "", tokenFile);
			Assert.IsTrue(result, "testVerifyDer");
		}


		[Test]
		public void TestVerifyPkcs12()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.p12", alias, password, tokenFile);
			Assert.IsTrue(result, "testVerifyPkcs12");
		}


		[Test]
		public void TestVerifyPkcs12_withoutalias()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.p12", "", password, tokenFile);
			Assert.IsTrue(result, "testVerifyPkcs12_withoutalias");
		}

		internal static RSAParameters LoadPrivateKey(string path)
		{
			using (StreamReader streamReader = new StreamReader(path))
			{
				PemReader pemReader = new PemReader(streamReader);
				try
				{
					Object obj = pemReader.ReadObject();
					if (obj.GetType() == typeof(AsymmetricCipherKeyPair))
					{
						AsymmetricCipherKeyPair asymKeyPair = (AsymmetricCipherKeyPair)obj;
						PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymKeyPair.Private);
						return CastPrivateKey(privateKeyInfo).ExportParameters(true);
					}else
					{
						Assert.Fail("LoadPrivateKey unknown private key type");
						return new RSAParameters();
					}
				}
				catch (Exception e)
				{
					Assert.Fail("LoadPrivateKey fail with exception " + e.Message);
					return new RSAParameters();
				}
				finally
				{
					pemReader.Reader.Close();
				}
			}

		}

		
		private static RSA CastPrivateKey(PrivateKeyInfo privateKeyInfo)
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
						return rsa;
					}catch(Exception)
					{
						return null;
					}
}
#endif
			return DotNetUtilities.ToRSA(privateKey);

		}
	}
}
