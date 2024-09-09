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

		[Test]
		public void TestLoadBase64()
		{
			string base64 = "MIIEATCCAumgAwIBAgIJAIAqvKHZ+gFhMA0GCSqGSIb3DQEBCwUAMIGWMQswCQYDVQQGEwJVWTETMBEGA1UECAwKTW9udGV2aWRlbzETMBEGA1UEBwwKTW9udGV2aWRlbzEQMA4GA1UECgwHR2VuZVh1czERMA8GA1UECwwIU2VjdXJpdHkxEjAQBgNVBAMMCXNncmFtcG9uZTEkMCIGCSqGSIb3DQEJARYVc2dyYW1wb25lQGdlbmV4dXMuY29tMB4XDTIwMDcwODE4NTcxN1oXDTI1MDcwNzE4NTcxN1owgZYxCzAJBgNVBAYTAlVZMRMwEQYDVQQIDApNb250ZXZpZGVvMRMwEQYDVQQHDApNb250ZXZpZGVvMRAwDgYDVQQKDAdHZW5lWHVzMREwDwYDVQQLDAhTZWN1cml0eTESMBAGA1UEAwwJc2dyYW1wb25lMSQwIgYJKoZIhvcNAQkBFhVzZ3JhbXBvbmVAZ2VuZXh1cy5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC1zgaU+Wh63p9DNWoAy64252EvZjN49AY3x0QCnAa8JO9Pk7znQwrxEFUKgZzv0GHEYW7+X+uyJr7BW4TA6fuJJ8agE/bmZRZyjdJjoue0FML6fbmCZ9Tsxpxe4pzispyWQ8jYT4Kl4I3fdZNUSn4XSidnDKBISeC05mrcchDKhInpiYDJ481lsB4JTEti3S4Xy/ToKwY4t6attw6z5QDhBc+Yro+YUqruliOAKqcfybe9k07jwMCvFVM1hrYYJ7hwHDSFo3MKwZ0y2gw0w6SgVBxLFo+KYP3q63b5wVhD8lzaSh+8UcyiHM2/yjEej7EnRFzdclTSNXRFNaiLnEVdAgMBAAGjUDBOMB0GA1UdDgQWBBQtQAWJRWNr/OswPSAdwCQh0Eei/DAfBgNVHSMEGDAWgBQtQAWJRWNr/OswPSAdwCQh0Eei/DAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQCjHe3JbNKv0Ywc1zlLacUNWcjLbmzvnjs8Wq5oxtf5wG5PUlhLSYZ9MPhuf95PlibnrO/xVY292P5lo4NKhS7VOonpbPQ/PrCMO84Pz1LGfM/wCWQIowh6VHq18PiZka9zbwl6So0tgClKkFSRk4wpKrWX3+M3+Y+D0brd8sEtA6dXeYHDtqV0YgjKdZIIOx0vDT4alCoVQrQ1yAIq5INT3cSLgJezIhEadDv3Tc7bMxMFeL+81qHm9Z/9/KE6Z+JB0ZEOkF/2NSQJd+Z7MBR8CxOdTQis3ltMoXDatNkjZ2Env40sw4NICB8YYhsWMIarew5uNT+RS28YHNlbmogh";
			bool result = GamUtilsEO.VerifyJWTWithFile(base64 , "", "", tokenFile);
			Assert.IsTrue(result, "TestLoadKey");
		}
	}
}
