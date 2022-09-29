using GeneXusCryptography.Asymmetric;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;
using System.IO;

namespace SecurityAPITest.Cryptography.Asymmetric
{
	[TestFixture]
	public class TestAsymmetricDomainSpaces : SecurityAPITestObject
	{
		private static string path_RSA_sha1_1024;
		private static string plainText;
		private static PrivateKeyManager key;
		private static CertificateX509 cert;
		private static string pathKey;
		private static string pathCert;
		private static AsymmetricCipher asymCipher;

		[SetUp]
		public virtual void SetUp()
		{
			path_RSA_sha1_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha1_1024");
			plainText = "Lorem ipsum";
			pathKey = Path.Combine(path_RSA_sha1_1024, "sha1d_key.pem");
			pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.crt");
			key = new PrivateKeyManager();
			cert = new CertificateX509();
			asymCipher = new AsymmetricCipher();
		}

		[Test]
		public void TestSpaces()
		{

			key.Load(pathKey);
			cert.Load(pathCert);
			string encrypted1 = asymCipher.DoEncrypt_WithPrivateKey("SHA1 ", "PCKS1PADDING ", key, plainText);
			//System.out.println("Error. Code: " + asymCipher.getErrorCode() + " Desc: " + asymCipher.getErrorDescription());
			Assert.IsFalse(asymCipher.HasError());

			string decrypted = asymCipher.DoDecrypt_WithCertificate(" SHA1", " PCKS1PADDING", cert, encrypted1);
			Assert.IsFalse(asymCipher.HasError());
			Assert.IsTrue(SecurityUtils.compareStrings(plainText, decrypted));
		}
	}
}
