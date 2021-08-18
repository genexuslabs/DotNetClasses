using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using SecurityAPICommons.Keys;
using GeneXusJWT.GenexusJWTUtils;
using GeneXusJWT.GenexusJWTClaims;
using System;
using System.IO;

namespace SecurityAPITest.Jwt.Asymmetric
{
    [TestFixture]
    public class TestAsymmetricJwt: SecurityAPITestObject
    {
		protected static JWTOptions options;
		protected static PrivateClaims claims;
		protected static GUID guid;
		protected static DateUtil du;
		protected static SymmetricKeyGenerator keyGenerator;
		protected static JWTCreator jwt;

		protected static string path_RSA_sha256_1024;
		protected static string path_RSA_sha256_2048;
		protected static string path_RSA_sha512_2048;
		protected static string path_EC;

		protected static string alias;
		protected static string password;

		[SetUp]
		public virtual void SetUp()
		{

			du = new DateUtil();
			guid = new GUID();
			keyGenerator = new SymmetricKeyGenerator();
			jwt = new JWTCreator();
			options = new JWTOptions();
			claims = new PrivateClaims();

			options.AddRegisteredClaim("iss", "GXSA");
			options.AddRegisteredClaim("sub", "subject1");
			options.AddRegisteredClaim("aud", "audience1");

			options.AddRegisteredClaim("jti", guid.Generate());

			options.AddCustomTimeValidationClaim("exp", du.CurrentPlusSeconds(100), "20");
			options.AddCustomTimeValidationClaim("iat", du.GetCurrentDate(), "20");
			options.AddCustomTimeValidationClaim("nbf", du.GetCurrentDate(), "20");

			claims.setClaim("hola1", "hola1");
			claims.setClaim("hola2", "hola2");

			path_RSA_sha256_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_1024");
			path_RSA_sha256_2048 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_2048");
			path_RSA_sha512_2048 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha512_2048");
			path_EC = Path.Combine(BASE_PATH, "dummycerts", "JWT_ECDSA", "prime_test");

			alias = "1";
			password = "dummy";

		}

		[Test]
		public void Test_sha256_1024_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "RS256";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha256_1024_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "RS256";
			bulkTest(key, cert, alg);
		}



		[Test]
		public void Test_sha256_1024_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.LoadPKCS12(pathKey, alias, password);
			cert.LoadPKCS12(pathCert, alias, password);
			string alg = "RS256";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha256_2048_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "RS256";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha256_2048_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "RS256";
			bulkTest(key, cert, alg);
		}



		[Test]
		public void Test_sha256_2048_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.LoadPKCS12(pathKey, alias, password);
			cert.LoadPKCS12(pathCert, alias, password);
			string alg = "RS256";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha512_2048_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "RS512";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha512_2048_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "RS512";
			bulkTest(key, cert, alg);
		}



		[Test]
		public void Test_sha512_2048_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.p12");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.LoadPKCS12(pathKey, alias, password);
			cert.LoadPKCS12(pathCert, alias, password);
			string alg = "RS512";
			bulkTest(key, cert, alg);
		}

		/*[Test]
		public void Test_sha256_EC()
		{
			string pathKey = Path.Combine(path_EC, "key.pem");
			string pathCert = Path.Combine(path_EC, "cert_sha256.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha384_EC()
		{
			string pathKey = Path.Combine(path_EC, "key.pem");
			string pathCert = Path.Combine(path_EC, "cert_sha384.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES384";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha512_EC()
		{
			string pathKey = Path.Combine(path_EC, "key.pem");
			string pathCert = Path.Combine(path_EC, "cert_sha512.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES512";
			bulkTest(key, cert, alg);
		}*/



		private void bulkTest(PrivateKeyManager key, CertificateX509 cert, string alg)
		{
			options.SetPrivateKey(key);
			options.SetCertificate(cert);
			string token = jwt.DoCreate(alg, claims, options);
			Assert.IsFalse(jwt.HasError());
			//bool verification = jwt.DoVerify(token, alg, claims, options);
			//True(verification, jwt);
		}

		[Test]
		public void Test_sha512_2048_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.LoadEncrypted(pathKey, password);
			cert.Load(pathCert);
			string alg = "RS512";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha256_1024_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.LoadEncrypted(pathKey, password);
			cert.Load(pathCert);
			string alg = "RS256";
			bulkTest(key, cert, alg);
		}

		[Test]
		public void Test_sha256_2048_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.LoadEncrypted(pathKey, password);
			cert.Load(pathCert);
			string alg = "RS256";
			bulkTest(key, cert, alg);
		}

	}
}
