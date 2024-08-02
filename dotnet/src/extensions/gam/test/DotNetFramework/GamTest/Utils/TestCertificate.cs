using System;
using System.IO;
using NUnit.Framework;
using GamUtils.Utils;
using GamUtils;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestCertificate
	{
#pragma warning disable CS0414
		private static string resources;
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
			BASE_PATH = TestJwt.GetStartupDirectory();
			resources = Path.Combine(BASE_PATH, "Resources", "dummycerts");
			path_RSA_sha256_2048 = Path.Combine(resources, "RSA_sha256_2048");
			string kid = Guid.NewGuid().ToString();
			alias = "1";
			password = "dummy";
			string header = "{\n" +
				"  \"alg\": \"RS256\",\n" +
				"  \"kid\": \"" + kid + "\",\n" +
				"  \"typ\": \"JWT\"\n" +
				"}";
			string payload = "{\n" +
				"  \"sub\": \"1234567890\",\n" +
				"  \"name\": \"John Doe\",\n" +
				"  \"iat\": 1516239022\n" +
				"}";
			tokenFile = Jwt.Create(TestJwt.LoadPrivateKey(path_RSA_sha256_2048 + "\\sha256d_key.pem"), payload, header);
		}

		
		[Test]
		public void TestLoadCrt()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.crt", "", password, tokenFile);
			Assert.IsTrue(result, "TestLoadCrt");
		}

		[Test]
		public void TestLoadCer()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.cer", "", password, tokenFile);
			Assert.IsTrue(result, "TestLoadCer");
		}

		[Test]
		[Ignore("issues with pfx extension in .Net")]
		public void TestLoadPfx()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.pfx", "", password, tokenFile);
			Assert.IsTrue(result, "TestLoadPfx");
		}

		[Test]
		public void TestLoadPkcs12()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.pkcs12", "", password, tokenFile);
			Assert.IsTrue(result, "TestLoadPkcs12");
		}

		[Test]
		public void TestLoadP12()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.p12", "", password, tokenFile);
			Assert.IsTrue(result, "TestLoadP12");
		}

		[Test]
		public void TestLoadPem()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.pem", "", password, tokenFile);
			Assert.IsTrue(result, "TestLoadPem");
		}

		[Test]
		public void TestLoadKey()
		{
			bool result = GamUtilsEO.VerifyJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.key", "", password, tokenFile);
			Assert.IsTrue(result, "TestLoadKey");
		}
	}
}
