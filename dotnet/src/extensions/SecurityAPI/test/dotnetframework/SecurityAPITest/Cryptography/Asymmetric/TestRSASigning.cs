using SecurityAPITest.SecurityAPICommons.commons;
using System;
using NUnit.Framework;
using SecurityAPICommons.Config;
using GeneXusCryptography.Asymmetric;
using SecurityAPICommons.Keys;
using System.IO;

namespace SecurityAPITest.Cryptography.Asymmetric
{
    [TestFixture]
    public class TestRSASigning: SecurityAPITestObject
    {

		private static string path_RSA_sha1_1024;
		private static string path_RSA_sha256_1024;
		private static string path_RSA_sha256_2048;
		private static string path_RSA_sha512_2048;
		private static string[] encodings;
		private static EncodingUtil eu;

		//private static String[] arrayPaddings;

		private static string plainText;
		private static string filePath;

		public static string alias;
		public static string password;

		[SetUp]
		public virtual void SetUp()
		{

			path_RSA_sha1_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha1_1024");
			path_RSA_sha256_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_1024");
			path_RSA_sha256_2048 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_2048");
			path_RSA_sha512_2048 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha512_2048");

			//arrayPaddings = new String[] { "OAEPPADDING", "PCKS1PADDING", "ISO97961PADDING" };

			plainText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam venenatis ex sit amet risus pellentesque, a faucibus quam ultrices. Ut tincidunt quam eu aliquam maximus. Quisque posuere risus at erat blandit eleifend. Curabitur viverra rutrum volutpat. Donec quis quam tellus. Aenean fermentum elementum augue, a semper risus scelerisque sit amet. Nullam vitae sapien vitae dui ullamcorper dapibus quis quis leo. Sed neque felis, pellentesque in risus et, lobortis ultricies nulla. Quisque quis quam risus. Donec vestibulum, lectus vel vestibulum eleifend, velit ante volutpat lacus, ut mattis quam ligula eget est. Sed et pulvinar lectus. In mollis turpis non ipsum vehicula, sit amet rutrum nibh dictum. Duis consectetur convallis ex, eu ultricies enim bibendum vel. Vestibulum vel libero nibh. Morbi nec odio mattis, vestibulum quam blandit, pretium orci.Aenean pellentesque tincidunt nunc a malesuada. Etiam gravida fermentum mi, at dignissim dui aliquam quis. Nullam vel lobortis libero. Phasellus non gravida posuere";
			filePath = Path.Combine(BASE_PATH, "Temp", "flag.jpg");

			alias = "1";
			password = "dummy";

			encodings = new string[] { "UTF_8", "UTF_16", "UTF_16BE", "UTF_16LE", "UTF_32", "UTF_32BE", "UTF_32LE", "SJIS", "GB2312" };

			eu = new EncodingUtil();
		}

		[Test]
		public void Test_sha1_1024_DER()
		{

			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA1");
		}

		[Test]
		public void Test_sha1_1024_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA1");

		}



		[Test]
		public void Test_sha1_1024_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadPKCS12(pathKey, alias, password);
			CertificateX509 cert = new CertificateX509();
			cert.LoadPKCS12(pathCert, alias, password);
			runTestWithEncoding(key, cert, "SHA1");

		}

		[Test]
		public void Test_sha256_1024_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256");

		}

		[Test]
		public void Test_sha256_1024_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256");

		}



		[Test]
		public void Test_sha256_1024_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadPKCS12(pathKey, alias, password);
			CertificateX509 cert = new CertificateX509();
			cert.LoadPKCS12(pathCert, alias, password);
			runTestWithEncoding(key, cert, "SHA256");

		}

		[Test]
		public void Test_sha256_2048_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256");

		}

		[Test]
		public void Test_sha256_2048_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256");

		}



		[Test]
		public void Test_sha256_2048_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadPKCS12(pathKey, alias, password);
			CertificateX509 cert = new CertificateX509();
			cert.LoadPKCS12(pathCert, alias, password);
			runTestWithEncoding(key, cert, "SHA256");

		}

		[Test]
		public void Test_sha512_2048_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA512");

		}

		[Test]
		public void Test_sha512_2048_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA512");

		}



		[Test]
		public void Test_sha512_2048_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadPKCS12(pathKey, alias, password);
			CertificateX509 cert = new CertificateX509();
			cert.LoadPKCS12(pathCert, alias, password);
			runTestWithEncoding(key, cert, "SHA512");
		}

		[Test]
		public void Test_base64()
		{
			string base64stringCert = "MIIC/DCCAmWgAwIBAgIJAPmCVmfcc0IXMA0GCSqGSIb3DQEBCwUAMIGWMQswCQYDVQQGEwJVWTETMBEGA1UECAwKTW9udGV2aWRlbzETMBEGA1UEBwwKTW9udGV2aWRlbzEQMA4GA1UECgwHR2VuZVh1czERMA8GA1UECwwIU2VjdXJpdHkxEjAQBgNVBAMMCXNncmFtcG9uZTEkMCIGCSqGSIb3DQEJARYVc2dyYW1wb25lQGdlbmV4dXMuY29tMB4XDTIwMDcwODE4NDkxNVoXDTI1MDcwNzE4NDkxNVowgZYxCzAJBgNVBAYTAlVZMRMwEQYDVQQIDApNb250ZXZpZGVvMRMwEQYDVQQHDApNb250ZXZpZGVvMRAwDgYDVQQKDAdHZW5lWHVzMREwDwYDVQQLDAhTZWN1cml0eTESMBAGA1UEAwwJc2dyYW1wb25lMSQwIgYJKoZIhvcNAQkBFhVzZ3JhbXBvbmVAZ2VuZXh1cy5jb20wgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAMZ8m4ftIhfrdugi5kEszRZr5IRuqGDLTex+CfVnhnBYXyQgJXeCI0eyRYUAbNzw/9MPdFN//pV26AXeH/ajORVu1JVoOACZdNOIPFnwXXh8oBxNxLAYlqoK2rAL+/tns8rKqqS4p8HSat9tj07TUXnsYJmmbXJM/eB94Ex66D1ZAgMBAAGjUDBOMB0GA1UdDgQWBBTfXY8eOfDONCZpFE0V34mJJeCYtTAfBgNVHSMEGDAWgBTfXY8eOfDONCZpFE0V34mJJeCYtTAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBCwUAA4GBAAPv7AFlCSpJ32c/VYowlbk6UBhOKmVWBQlrAtvVQYtCKO/y9CEB8ikG19c8lHM9axnsbZR+G7g04Rfuiea3T7VPkSmUXPpz5fl6Zyk4LZg5Oji7MMMXGmr+7cpYWRhifCVwoxSgZEXt3d962IZ1Wei0LMO+4w4gnzPxqr8wVHnT";
			string base64stringKey = "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMZ8m4ftIhfrdugi5kEszRZr5IRuqGDLTex+CfVnhnBYXyQgJXeCI0eyRYUAbNzw/9MPdFN//pV26AXeH/ajORVu1JVoOACZdNOIPFnwXXh8oBxNxLAYlqoK2rAL+/tns8rKqqS4p8HSat9tj07TUXnsYJmmbXJM/eB94Ex66D1ZAgMBAAECgYA1xrTs0taV3HnO0wXHSrgWBw1WxBRihTKLjGpuTqoh7g943izIgD3GwwoKyt6zzafCK0G9DcSQAjNCw7etPvPL3FxwhDl+AHSv9JcChk/auICtMWwjurG4npto+s3byj/N00Idpz1xuOgKd8k9sdoPBGKa8l+LL+adSXzoivLG8QJBAPDvbOLSs9petB2iM6w5/DiC8EoxqDaBc7I1JFCvPOfB7i1GFFxkQ7hlgxpvaPX3NHXjAZpgdOW68P/SjU0izKsCQQDS5bjrNo3xn/MbYKojzwprR/Bo8Kvbi4/2M9NE3GwHegVmx5I+df+J0aObrbBNPLs/rhrFtt12OtgxJaac+FYLAkEA8DUUbvO4wj7m/iBnug65irHo1V+6oFThv0tCIHsFkt4DEvoqdI62AZKbafCnSYqjr+CaCYqfIScG/Vay77OBLwJBAI8EYAmKPmn7+SW4wMh1z+/+ogbYJwNEOoVQkdXh0JSlZ+JSNleLN5ajhtq8x5EpPSYrEFbB8p8JurBhgwJx2g8CQQDrp9scoK8eKBJ2p/63xqLGYSN6OZQo/4Lkq3983rmHoDCAp3Bz1zUyxQB3UVyrOj4U44C7RtDNiMSZuCwvjYAI";
			PrivateKeyManager key = new PrivateKeyManager();
			key.FromBase64(base64stringKey);
			CertificateX509 cert = new CertificateX509();
			cert.FromBase64(base64stringCert);
			runTestWithEncoding(key, cert, "SHA256");
		}

		private void bulkTest(PrivateKeyManager key, CertificateX509 cert, string hashAlgorithm)
		{
			bulkTestText(key, cert, hashAlgorithm);
			bulkTestFile(key, cert, hashAlgorithm);
		}

		private void bulkTestText(PrivateKeyManager key, CertificateX509 cert, string hashAlgorithm)
		{
			AsymmetricSigner asymSig = new AsymmetricSigner();
			string signature = asymSig.DoSign(key, hashAlgorithm, plainText);
			bool result = asymSig.DoVerify(cert, plainText, signature);
			Assert.IsTrue(result);
			True(result, asymSig);
		}

		private void bulkTestFile(PrivateKeyManager key, CertificateX509 cert, string hashAlgorithm)
		{
			AsymmetricSigner asymSig = new AsymmetricSigner();
			string signature = asymSig.DoSignFile(key, hashAlgorithm, filePath);
			bool result = asymSig.DoVerifyFile(cert, filePath, signature);
			Assert.IsTrue(result);
			True(result, asymSig);
		}

		private void runTestWithEncoding(PrivateKeyManager key, CertificateX509 cert, string hash)
		{
			for (int i = 0; i < encodings.Length; i++)
			{
				eu.setEncoding(encodings[i]);
				bulkTest(key, cert, hash);
			}
		}

		[Test]
		public void Test_sha256_2048_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadEncrypted(pathKey, password);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256");

		}

		[Test]
		public void Test_sha512_2048_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadEncrypted(pathKey, password);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA512");

		}

		[Test]
		public void Test_sha256_1024_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadEncrypted(pathKey, password);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256");

		}

		[Test]
		public void Test_sha1_1024_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1_key.pem");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadEncrypted(pathKey, password);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA1");

		}
	}
}
