using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityAPITest.Jwt.Asymmetric
{
	[TestFixture]
	public class TestECDSACurvesSectJwt: SecurityAPITestObject
	{
		protected static JWTOptions options;
		protected static PrivateClaims claims;
		protected static GUID guid;
		protected static JWTCreator jwt;
		protected static string ECDSA_path;

		[SetUp]
		public virtual void SetUp()
		{
			guid = new GUID();
			jwt = new JWTCreator();
			options = new JWTOptions();
			claims = new PrivateClaims();

			options.AddRegisteredClaim("iss", "GXSA");
			options.AddRegisteredClaim("sub", "subject1");
			options.AddRegisteredClaim("aud", "audience1");

			options.AddRegisteredClaim("jti", guid.Generate());

			claims.setClaim("hola1", "hola1");
			claims.setClaim("hola2", "hola2");

			ECDSA_path = Path.Combine(BASE_PATH, "dummycerts", "JWT_ECDSA");
		}


		private void bulkTest_shouldWork(PrivateKeyManager key, CertificateX509 cert, string alg, string curve)
		{
			options.SetPrivateKey(key);
			options.SetCertificate(cert);
			string token = jwt.DoCreate(alg, claims, options);
			Assert.IsFalse(jwt.HasError());
			bool verification = jwt.DoVerify(token, alg, claims, options);
			True(verification, jwt);
		}

		private void bulkTest_shouldntWork(PrivateKeyManager key, CertificateX509 cert, string alg, string curve)
		{
			options.SetPrivateKey(key);
			options.SetCertificate(cert);
			string token = jwt.DoCreate(alg, claims, options);
			Assert.IsTrue(jwt.HasError());
		}

		/*public void test_shouldWork()
		{

			


		}*/

		public void test_shouldntWork()
		{
			sect113r1();
			sect113r2();
			sect131r1();
			sect131r2();
			sect163k1();
			sect163r1();
			sect163r2();
			sect193r1();
			sect193r2();
			sect233k1();
			/***ANDA CUANDO QUIERE***/
			// sect233r1();
			sect283k1();
			sect283r1();
			sect409k1();
			sect409r1();
			sect571k1();
			sect571r1();
			sect239k1();
}


		[Test]
		public void sect113r1()
		{
			string pathKey = ECDSA_path + "sect113r1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect113r1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect113r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect113r2()
		{
			string pathKey = ECDSA_path + "sect113r2" + "\\key.pem";
			string pathCert = ECDSA_path + "sect113r2" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect113r2";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect131r1()
		{
			string pathKey = ECDSA_path + "sect131r1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect131r1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect131r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect131r2()
		{
			string pathKey = ECDSA_path + "sect131r2" + "\\key.pem";
			string pathCert = ECDSA_path + "sect131r2" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect131r2";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect163k1()
		{
			string pathKey = ECDSA_path + "sect163k1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect163k1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect163k1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect163r1()
		{
			string pathKey = ECDSA_path + "sect163r1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect163r1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect163r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect163r2()
		{
			string pathKey = ECDSA_path + "sect163r2" + "\\key.pem";
			string pathCert = ECDSA_path + "sect163r2" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect163r2";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect193r1()
		{
			string pathKey = ECDSA_path + "sect193r1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect193r1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect193r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect193r2()
		{
			string pathKey = ECDSA_path + "sect193r2" + "\\key.pem";
			string pathCert = ECDSA_path + "sect193r2" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect193r2";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect233k1()
		{
			string pathKey = ECDSA_path + "sect233k1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect233k1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect233k1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		/*private void sect233r1() {
			string pathKey = ECDSA_path + "sect233r1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect233r1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect233r1";
			bulkTest_shouldWork(key, cert, alg, curve);
		}*/

		[Test]
		public void sect239k1()
		{
			string pathKey = ECDSA_path + "sect239k1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect239k1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect239k1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect283k1()
		{
			string pathKey = ECDSA_path + "sect283k1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect283k1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect283k1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect283r1()
		{
			string pathKey = ECDSA_path + "sect283r1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect283r1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect283r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect409k1()
		{
			string pathKey = ECDSA_path + "sect409k1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect409k1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect409k1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect409r1()
		{
			string pathKey = ECDSA_path + "sect409r1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect409r1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect409r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect571k1()
		{
			string pathKey = ECDSA_path + "sect571k1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect571k1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect571k1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void sect571r1()
		{
			string pathKey = ECDSA_path + "sect571r1" + "\\key.pem";
			string pathCert = ECDSA_path + "sect571r1" + "\\cert.pem";
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "sect571r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}
	}
}
