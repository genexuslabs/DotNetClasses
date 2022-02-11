using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;
using System;
using System.IO;

namespace SecurityAPITest.Jwt.Other
{

	
	[TestFixture]
	public class TestIssue86867 : SecurityAPITestObject
	{
		protected static JWTCreator jwt;
		protected static JWTOptions options1;
		protected static JWTOptions options2;
		protected static PrivateClaims claims;
		protected static string token;
		protected static string path_RSA_sha256_1024;

		[SetUp]
		public virtual void SetUp()
		{
			options1 = new JWTOptions();
			options2 = new JWTOptions();
			jwt = new JWTCreator();
			claims = new PrivateClaims();
			claims.setClaim("hola1", "hola1");
			path_RSA_sha256_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_1024");

			String pathKey = Path.Combine(path_RSA_sha256_1024, "sha256d_key.pem");
			String pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.crt");
			PrivateKeyManager key = new PrivateKeyManager();
			CertificateX509 cert = new CertificateX509();
			key.Load(pathKey);
			cert.Load(pathCert);

			options1.SetCertificate(cert);
			options1.SetPrivateKey(key);
			options1.AddRegisteredClaim("iss", "GXSA");
			options1.AddRegisteredClaim("sub", "subject1");
			options1.AddRegisteredClaim("aud", "audience1");

			options2.AddRegisteredClaim("iss", "GXSA");
			options2.AddRegisteredClaim("sub", "subject1");
			options2.AddRegisteredClaim("aud", "audience1");
			options2.SetCertificate(cert);

			token = jwt.DoCreate("RS256", claims, options1);
		}

		[Test]
		public void TestVerificationWithoutPrivateKey()
		{
			bool validation = jwt.DoVerify(token, "RS256", claims, options2);
			//System.out.println("Error. Code: " + jwt.getErrorCode() + " Desc: " + jwt.getErrorDescription());
			Assert.IsTrue(validation);
		}
	}
}


