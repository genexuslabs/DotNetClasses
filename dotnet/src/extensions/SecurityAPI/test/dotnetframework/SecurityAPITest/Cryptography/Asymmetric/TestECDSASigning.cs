﻿using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using GeneXusCryptography.Asymmetric;
using System.IO;

namespace SecurityAPITest.Cryptography.Asymmetric
{
    [TestFixture]
    public class TestECDSASigning: SecurityAPITestObject
    {
		private static string path_ecdsa_sha1;
		private static string path_ecdsa_sha256;

		private static string plainText;

		private static string alias;
		private static string password;

		private static string[] encodings;
		private static EncodingUtil eu;

		[SetUp]
		public virtual void SetUp()
		{

			path_ecdsa_sha1 = Path.Combine(BASE_PATH, "dummycerts", "ECDSA_sha1");
			path_ecdsa_sha256 = Path.Combine(BASE_PATH, "dummycerts", "ECDSA_sha256");

			plainText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam venenatis ex sit amet risus pellentesque, a faucibus quam ultrices. Ut tincidunt quam eu aliquam maximus. Quisque posuere risus at erat blandit eleifend. Curabitur viverra rutrum volutpat. Donec quis quam tellus. Aenean fermentum elementum augue, a semper risus scelerisque sit amet. Nullam vitae sapien vitae dui ullamcorper dapibus quis quis leo. Sed neque felis, pellentesque in risus et, lobortis ultricies nulla. Quisque quis quam risus. Donec vestibulum, lectus vel vestibulum eleifend, velit ante volutpat lacus, ut mattis quam ligula eget est. Sed et pulvinar lectus. In mollis turpis non ipsum vehicula, sit amet rutrum nibh dictum. Duis consectetur convallis ex, eu ultricies enim bibendum vel. Vestibulum vel libero nibh. Morbi nec odio mattis, vestibulum quam blandit, pretium orci.Aenean pellentesque tincidunt nunc a malesuada. Etiam gravida fermentum mi, at dignissim dui aliquam quis. Nullam vel lobortis libero. Phasellus non gravida posuere";

			alias = "1";
			password = "dummy";

			encodings = new string[] { "UTF_8", "UTF_16", "UTF_16BE", "UTF_16LE", "UTF_32", "UTF_32BE", "UTF_32LE", "SJIS",
				"GB2312" };

			eu = new EncodingUtil();
		}

		[Test]
		public void Test_ecdsa_sha1_PEM()
		{
			string pathKey = Path.Combine(path_ecdsa_sha1, "sha1_key.pem");
			string pathCert = Path.Combine(path_ecdsa_sha1, "sha1_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA1");

		}

		[Test]
		public void Test_ecdsa_sha1_DER()
		{
			string pathKey = Path.Combine(path_ecdsa_sha1, "sha1_key.pem");
			string pathCert = Path.Combine(path_ecdsa_sha1, "sha1_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA1");

		}

		[Test]
		public void Test_ecdsa_sha1_PKCS12()
		{
			string pathKey = Path.Combine(path_ecdsa_sha1, "sha1_cert.p12");
			string pathCert = Path.Combine(path_ecdsa_sha1, "sha1_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadPKCS12(pathKey, alias, password);
			CertificateX509 cert = new CertificateX509();
			cert.LoadPKCS12(pathCert, alias, password);
			runTestWithEncoding(key, cert, "SHA1");
		}

		[Test]
		public void Test_ecdsa_sha256_PEM()
		{
			string pathKey = Path.Combine(path_ecdsa_sha256, "sha256_key.pem");
			string pathCert = Path.Combine(path_ecdsa_sha256, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256");

		}

		[Test]
		public void Test_ecdsa_sha256_DER()
		{
			string pathKey = Path.Combine(path_ecdsa_sha256, "sha256_key.pem");
			string pathCert = Path.Combine(path_ecdsa_sha256, "sha256_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			CertificateX509 cert = new CertificateX509();
			cert.Load(pathCert);
			runTestWithEncoding(key, cert, "SHA256");

		}

		[Test]
		public void Test_ecdsa_sha256_PKCS12()
		{
			string pathKey = Path.Combine(path_ecdsa_sha256, "sha256_cert.p12");
			string pathCert = Path.Combine(path_ecdsa_sha256, "sha256_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			key.LoadPKCS12(pathKey, alias, password);
			CertificateX509 cert = new CertificateX509();
			cert.LoadPKCS12(pathCert, alias, password);
			runTestWithEncoding(key, cert, "SHA256");

		}

		
		private void bulkTest(PrivateKeyManager key, CertificateX509 cert, string hashAlgorithm)
		{
			AsymmetricSigner asymSig = new AsymmetricSigner();
			string signature = asymSig.DoSign(key, hashAlgorithm, plainText);
			bool result = asymSig.DoVerify(cert, plainText, signature);
			Assert.IsTrue(result);
			True(result, asymSig);
		}

		private void runTestWithEncoding(PrivateKeyManager key, CertificateX509 cert, string hash)
		{
			//{ "UTF_8", "UTF_16", "UTF_16BE", "UTF_16LE", "UTF_32", "UTF_32BE", "UTF_32LE", "SJIS",
			//"GB2312" };
		for (int i = 0; i < encodings.Length; i++)
		{
		eu.setEncoding(encodings[i]);
				bulkTest(key, cert, hash);
			}
		}
	}
}
