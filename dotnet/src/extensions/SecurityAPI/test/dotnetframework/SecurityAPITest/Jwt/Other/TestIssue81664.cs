using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;
using GeneXusJWT.GenexusJWT;
using SecurityAPICommons.Utils;

namespace SecurityAPITest.Jwt.Other
{
    [TestFixture]
    public class TestIssue81664: SecurityAPITestObject
	{
		private static CertificateX509 cert;
		private static PrivateKeyManager key;
		private static JWTOptions options;
		private static PrivateClaims claims;
		private static string token;
		private static string expected;
		private static DateUtil du;
		private static JWTCreator jwt;

		[SetUp]
		public virtual void SetUp()
		{
			cert = new CertificateX509();
			key = new PrivateKeyManager();
			options = new JWTOptions();
			claims = new PrivateClaims();
			du = new DateUtil();
			jwt = new JWTCreator();


			cert.Load(BASE_PATH + "dummycerts\\RSA_sha256_1024\\sha256_cert.crt");
			options.SetCertificate(cert);


			key.Load(BASE_PATH + "dummycerts\\RSA_sha256_1024\\sha256d_key.pem");
			options.SetPrivateKey(key);
			//	
			// carga de privateClaim (es parte del Payload)
			claims.setClaim("GeneXus", "Viglia");


			// Carga de Registered Claims
			options.AddRegisteredClaim("iss", "Martin");
			options.AddRegisteredClaim("sub", "Martin1");
			options.AddRegisteredClaim("aud", "martivigliadoocebbooyo.docebosaas.com");
			options.AddCustomTimeValidationClaim("iat", du.GetCurrentDate(), "20");
			options.AddCustomTimeValidationClaim("exp", du.CurrentPlusSeconds(3600), "20");
			options.AddPublicClaim("client_id", "Martin");

			token = jwt.DoCreate("RS256", claims, options);
			expected = "{\"alg\":\"RS256\",\"typ\":\"JWT\"}";
		}

		[Test]
		public void Test_algorithm()
		{
			string header = jwt.GetHeader(token);
			Assert.IsTrue(SecurityUtils.compareStrings(header, expected));
		}



    }
}
