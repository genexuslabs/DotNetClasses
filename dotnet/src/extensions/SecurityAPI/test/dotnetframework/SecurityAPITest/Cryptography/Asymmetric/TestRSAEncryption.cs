using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using GeneXusCryptography.Asymmetric;
using SecurityAPICommons.Utils;
using System.IO;
using SecurityAPICommons.Commons;

namespace SecurityAPITest.Cryptography.Asymmetric
{
    [TestFixture]
    public class TestRSAEncryption: SecurityAPITestObject
    {
		private static string path_RSA_sha1_1024;
		private static string path_RSA_sha256_1024;
		private static string path_RSA_sha256_2048;
		private static string path_RSA_sha512_2048;

		private static string[] arrayPaddings;

		private static string plainText;

		private static string plainText16;
		private static string plainText32;

		private static string alias;
		private static string password;

		private static string[] encodings;
		private static EncodingUtil eu;

		[SetUp]
		public virtual void SetUp()
		{

			path_RSA_sha1_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha1_1024");
			path_RSA_sha256_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_1024");
			path_RSA_sha256_2048 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_2048");
			path_RSA_sha512_2048 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha512_2048");

			arrayPaddings = new string[] { "OAEPPADDING", "PCKS1PADDING", "ISO97961PADDING" };

			plainText = "";
			plainText16 = "Lorem ipsum dolor sit amet";
			plainText32 = "Lorem ipsum";

			alias = "1";
			password = "dummy";

			encodings = new string[] { "UTF_8", "UTF_16", "UTF_16BE", "UTF_16LE", "UTF_32", "UTF_32BE", "UTF_32LE", "SJIS",
				"GB2312" };

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
			runTestWithEncoding(key, cert, "SHA1", false);

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
			runTestWithEncoding(key, cert, "SHA1", false);

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
			runTestWithEncoding(key, cert, "SHA1", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

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
			runTestWithEncoding(key, cert, "SHA512", false);

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
			runTestWithEncoding(key, cert, "SHA512", false);

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
			runTestWithEncoding(key, cert, "SHA512", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

		}

		[Test]
		public void Test_base64_PublicKey()
		{
			string base64stringCert =  "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDGfJuH7SIX63boIuZBLM0Wa+SEbqhgy03sfgn1Z4ZwWF8kICV3giNHskWFAGzc8P/TD3RTf/6VdugF3h/2ozkVbtSVaDgAmXTTiDxZ8F14fKAcTcSwGJaqCtqwC/v7Z7PKyqqkuKfB0mrfbY9O01F57GCZpm1yTP3gfeBMeug9WQIDAQAB";
			string base64stringKey = "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMZ8m4ftIhfrdugi5kEszRZr5IRuqGDLTex+CfVnhnBYXyQgJXeCI0eyRYUAbNzw/9MPdFN//pV26AXeH/ajORVu1JVoOACZdNOIPFnwXXh8oBxNxLAYlqoK2rAL+/tns8rKqqS4p8HSat9tj07TUXnsYJmmbXJM/eB94Ex66D1ZAgMBAAECgYA1xrTs0taV3HnO0wXHSrgWBw1WxBRihTKLjGpuTqoh7g943izIgD3GwwoKyt6zzafCK0G9DcSQAjNCw7etPvPL3FxwhDl+AHSv9JcChk/auICtMWwjurG4npto+s3byj/N00Idpz1xuOgKd8k9sdoPBGKa8l+LL+adSXzoivLG8QJBAPDvbOLSs9petB2iM6w5/DiC8EoxqDaBc7I1JFCvPOfB7i1GFFxkQ7hlgxpvaPX3NHXjAZpgdOW68P/SjU0izKsCQQDS5bjrNo3xn/MbYKojzwprR/Bo8Kvbi4/2M9NE3GwHegVmx5I+df+J0aObrbBNPLs/rhrFtt12OtgxJaac+FYLAkEA8DUUbvO4wj7m/iBnug65irHo1V+6oFThv0tCIHsFkt4DEvoqdI62AZKbafCnSYqjr+CaCYqfIScG/Vay77OBLwJBAI8EYAmKPmn7+SW4wMh1z+/+ogbYJwNEOoVQkdXh0JSlZ+JSNleLN5ajhtq8x5EpPSYrEFbB8p8JurBhgwJx2g8CQQDrp9scoK8eKBJ2p/63xqLGYSN6OZQo/4Lkq3983rmHoDCAp3Bz1zUyxQB3UVyrOj4U44C7RtDNiMSZuCwvjYAI";
			PrivateKeyManager key = new PrivateKeyManager();
			key.FromBase64(base64stringKey);
			PublicKey cert = new PublicKey();
			cert.FromBase64(base64stringCert);
			runTestWithEncoding(key, cert, "SHA256", true);

		}


		private void bulkTest(PrivateKeyManager privateKey, PublicKey cert, string hashAlgorithm, bool isPublicKey)
		{
			string enc = SecurityApiGlobal.GLOBALENCODING;
			if (SecurityUtils.compareStrings(enc, "UTF_32") || SecurityUtils.compareStrings(enc, "UTF_32BE")
					|| SecurityUtils.compareStrings(enc, "UTF_32LE"))
			{
				plainText = eu.getString(eu.getBytes(plainText32));
			}
			else
			{

				plainText = eu.getString(eu.getBytes(plainText16));
			}

			for (int p = 0; p < arrayPaddings.Length; p++)
			{

				AsymmetricCipher asymCipher = new AsymmetricCipher();
				// AsymmetricCipher asymCipherD = new AsymmetricCipher();
				string encrypted1 = asymCipher.DoEncrypt_WithPrivateKey(hashAlgorithm, arrayPaddings[p], privateKey,
						plainText);
				string decrypted1 = isPublicKey ? asymCipher.DoDecrypt_WithPublicKey(hashAlgorithm, arrayPaddings[p], cert, encrypted1): asymCipher.DoDecrypt_WithCertificate(hashAlgorithm, arrayPaddings[p], (CertificateX509)cert, encrypted1);

				Assert.AreEqual(decrypted1, plainText);
				True(SecurityUtils.compareStrings(decrypted1, plainText), asymCipher);
				string encrypted2 = isPublicKey ? asymCipher.DoEncrypt_WithPublicKey(hashAlgorithm, arrayPaddings[p], cert, plainText): asymCipher.DoEncrypt_WithCertificate(hashAlgorithm, arrayPaddings[p], (CertificateX509)cert, plainText);
				string decrypted2 = asymCipher.DoDecrypt_WithPrivateKey(hashAlgorithm, arrayPaddings[p], privateKey,
						encrypted2);
				Assert.IsTrue(SecurityUtils.compareStrings(decrypted2, plainText));
				True(SecurityUtils.compareStrings(decrypted2, plainText), asymCipher);
			}
		}

		private void runTestWithEncoding(PrivateKeyManager key, PublicKey cert, string hash, bool isPublicKey)
		{
			for (int i = 0; i < encodings.Length; i++)
			{
				eu.setEncoding(encodings[i]);
				bulkTest(key, cert, hash, isPublicKey);
			}
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
			runTestWithEncoding(key, cert, "SHA1", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

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
			runTestWithEncoding(key, cert, "SHA256", false);

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
			runTestWithEncoding(key, cert, "SHA512", false);

		}

		[Test]
		public void Test_sha1_1024_PublicKey()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1_key.pem");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			bool loadedkey = key.LoadEncrypted(pathKey, password);
			PublicKey cert = new PublicKey();
			bool loadedcert = cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA1", true);

		}

		[Test]
		public void Test_sha256_1024_PEM_PublicKey()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadEncrypted(pathKey, password);
			PublicKey cert = new PublicKey();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256", true);

		}

		[Test]
		public void Test_sha256_2048_PEM_PublicKey()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadEncrypted(pathKey, password);
			PublicKey cert = new PublicKey();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256", true);

		}

		[Test]
		public void Test_sha512_2048_PEM_PublicKey()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadEncrypted(pathKey, password);
			PublicKey cert = new PublicKey();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA512", true);

		}
	}
}
