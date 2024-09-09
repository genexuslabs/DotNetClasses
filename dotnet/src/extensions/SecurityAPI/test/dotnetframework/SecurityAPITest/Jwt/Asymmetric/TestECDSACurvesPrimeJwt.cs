using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;
using System.IO;

namespace SecurityAPITest.Jwt.Asymmetric
{
	[TestFixture]
	public class TestECDSACurvesPrimeJwt: SecurityAPITestObject
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

		private void bulkTest_shouldWork(PrivateKeyManager key, PublicKey cert, string alg, string curve, bool isPublicKey)
		{
			options.SetPrivateKey(key);
			if (isPublicKey)
			{
				options.SetPublicKey(cert);
			}
			else
			{
				options.SetCertificate((CertificateX509)cert);
			}
			string token = jwt.DoCreate(alg, claims, options);
			Assert.IsFalse(jwt.HasError());
			bool verification = jwt.DoVerify(token, alg, claims, options);
			True(verification, jwt);
		}

		private void bulkTest_shouldntWork(PrivateKeyManager key, PublicKey cert, string alg, string curve, bool isPublicKey)
		{
			options.SetPrivateKey(key);
			if (isPublicKey)
			{
				options.SetPublicKey(cert);
			}
			else
			{
				options.SetCertificate((CertificateX509)cert);
			}
			string token = jwt.DoCreate(alg, claims, options);
			Assert.IsTrue(jwt.HasError());
		}

		public void test_shouldWork()
		{

			prime192v1();
			prime192v2();
			prime192v3();
			prime239v1();
			prime239v2();
			prime239v3();
			prime256v1();
		}


		[Test]
		public void prime192v1()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime192v1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime192v1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime192v1";
#if NETCORE
	bulkTest_shouldntWork(key, cert, alg, curve, false);
#else
			bulkTest_shouldWork(key, cert, alg, curve, false);
#endif


		}

		[Test]
		public void prime192v2()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime192v2", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime192v2", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime192v2";
#if NETCORE
	bulkTest_shouldntWork(key, cert, alg, curve, false);
#else
			bulkTest_shouldWork(key, cert, alg, curve, false);
#endif
		}

		[Test]
		public void prime192v2_PublicKey()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime192v2", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime192v2", "pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			PublicKey cert = new PublicKey();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime192v2";
#if NETCORE
			bulkTest_shouldntWork(key, cert, alg, curve, true);
#else
			bulkTest_shouldWork(key, cert, alg, curve, true);
#endif
		}

		[Test]
		public void prime192v3()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime192v3", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime192v3", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime192v3";
#if NETCORE
	bulkTest_shouldntWork(key, cert, alg, curve, false);
#else
			bulkTest_shouldWork(key, cert, alg, curve, false);
#endif
		}

		[Test]
		public void prime192v3_PublicKey()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime192v3", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime192v3", "pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			PublicKey cert = new PublicKey();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime192v3";
#if NETCORE
			bulkTest_shouldntWork(key, cert, alg, curve, true);
#else
			bulkTest_shouldWork(key, cert, alg, curve, true);
#endif
		}

		[Test]
		public void prime239v1()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime239v1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime239v1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime239v1";
#if NETCORE
	bulkTest_shouldntWork(key, cert, alg, curve, false);
#else
			bulkTest_shouldWork(key, cert, alg, curve, false);
#endif
		}

		[Test]
		public void prime239v1_PublicKey()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime239v1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime239v1", "pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			PublicKey cert = new PublicKey();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime239v1";
#if NETCORE
			bulkTest_shouldntWork(key, cert, alg, curve, true);
#else
			bulkTest_shouldWork(key, cert, alg, curve, true);
#endif
		}

		[Test]
		public void prime239v2()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime239v2", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime239v2", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime239v2";
#if NETCORE
	bulkTest_shouldntWork(key, cert, alg, curve, false);
#else
			bulkTest_shouldWork(key, cert, alg, curve, false);
#endif
		}

		[Test]
		public void prime239v2_PublicKey()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime239v2", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime239v2", "pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			PublicKey cert = new PublicKey();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime239v2";
#if NETCORE
			bulkTest_shouldntWork(key, cert, alg, curve, true);
#else
			bulkTest_shouldWork(key, cert, alg, curve, true);
#endif
		}

		[Test]
		public void prime239v3()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime239v3", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime239v3", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime239v3";
#if NETCORE
	bulkTest_shouldntWork(key, cert, alg, curve, false);
#else
			bulkTest_shouldWork(key, cert, alg, curve, false);
#endif
		}

		[Test]
		public void prime239v3_PublicKey()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime239v3", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime239v3", "pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			PublicKey cert = new PublicKey();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime239v3";
#if NETCORE
			bulkTest_shouldntWork(key, cert, alg, curve, true);
#else
			bulkTest_shouldWork(key, cert, alg, curve, true);
#endif
		}

		[Test]
		public void prime256v1()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime256v1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime256v1", "cert.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime256v1";
#if NETCORE
			if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
			{ 
				bulkTest_shouldntWork(key, cert, alg, curve, false);
			}
			else
			{
				bulkTest_shouldWork(key, cert, alg, curve, false);
			}
#else
			bulkTest_shouldWork(key, cert, alg, curve, false);
#endif
		}

		[Test]
		public void prime256v1_PublicKey()
		{
			string pathKey = Path.Combine(ECDSA_path, "prime256v1", "key.pem");
			string pathCert = Path.Combine(ECDSA_path, "prime256v1", "pubkey.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			PublicKey cert = new PublicKey();
			key.Load(pathKey);
			cert.Load(pathCert);
			string alg = "ES256";
			string curve = "prime256v1";
#if NETCORE
			if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
			{
				bulkTest_shouldntWork(key, cert, alg, curve, true);
			}
			else
			{
				bulkTest_shouldWork(key, cert, alg, curve, true);
			}
#else
			bulkTest_shouldWork(key, cert, alg, curve, true);
#endif
		}

	}
}
