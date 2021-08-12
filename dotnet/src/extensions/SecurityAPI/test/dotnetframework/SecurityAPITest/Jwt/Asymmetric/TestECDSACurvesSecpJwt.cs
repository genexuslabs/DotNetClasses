using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;
using NUnit.Framework;
using SecurityAPICommons.Commons;
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
	public class TestECDSACurvesSecpJwt: SecurityAPITestObject
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

		public void test_shouldWork()
		{
			
			secp192k1();
			secp256k1();
			secp384r1();
			secp521r1();

		}

		public void test_shouldntWork()
		{
			secp112r1();
			secp112r2();
			secp128r1();
			secp128r2();
			secp160k1();
			secp160r1();
			/***ESTOS DOS FUNCIONAN A VECES SI Y A VECES NO***/
			//secp224k1();
			//secp224r1();
			secp160r2();

	}

		[Test]
		public void secp112r1()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp112r1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp112r1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp112r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void secp112r2()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp112r2", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp112r2", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp112r2";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void secp128r1()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp128r1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp128r1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp128r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void secp128r2()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp128r2", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp128r2", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp128r2";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void secp160k1()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp160k1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp160k1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp160k1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void secp160r1()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp160r1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp160r1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp160r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void secp160r2()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp160r2", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp160r2", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp160r2";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}

		[Test]
		public void secp192k1()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp192k1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp192k1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp192k1";
#if NETCORE
	bulkTest_shouldntWork(key, cert, alg, curve);
#else
			bulkTest_shouldWork(key, cert, alg, curve);
#endif
		}

		/*private void secp224k1() {
			string pathKey = Path.Combine(ECDSA_path, "secp224k1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp224k1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp224k1";
			bulkTest_shouldntWork(key, cert, alg, curve);

		}*/

		/*private void secp224r1() {
			string pathKey = Path.Combine(ECDSA_path, "secp224r1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp224r1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp224r1";
			bulkTest_shouldntWork(key, cert, alg, curve);
		}*/

		[Test]
		public void secp256k1()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp256k1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp256k1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp256k1";
#if NETCORE
		bulkTest_shouldntWork(key, cert, alg, curve);
#else
			bulkTest_shouldWork(key, cert, alg, curve);
#endif

		}

		[Test]
		public void secp384r1()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp384r1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp384r1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp384r1";
			bulkTest_shouldWork(key, cert, alg, curve);
		}

		[Test]
		public void secp521r1()
		{
			string pathKey = Path.Combine(ECDSA_path, "secp521r1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "secp521r1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "secp521r1";
			bulkTest_shouldWork(key, cert, alg, curve);
		}
	}
}
