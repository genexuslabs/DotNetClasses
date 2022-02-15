using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
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
	public class TestCreateWithoutCert: SecurityAPITestObject
	{
		protected static JWTOptions options;
		protected static PrivateClaims claims;
		protected static SymmetricKeyGenerator keyGenerator;
		protected static JWTCreator jwt;

		protected static string path_RSA_sha256_1024;

		[SetUp]
		public virtual void SetUp()
		{


			keyGenerator = new SymmetricKeyGenerator();
			jwt = new JWTCreator();
			options = new JWTOptions();
			claims = new PrivateClaims();

			options.AddRegisteredClaim("iss", "GXSA");
			options.AddRegisteredClaim("sub", "subject1");
			options.AddRegisteredClaim("aud", "audience1");

			claims.setClaim("hola1", "hola1");
			claims.setClaim("hola2", "hola2");

			path_RSA_sha256_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_1024");

		}

		[Test]
		public void Test_sha256_1024_PEM()
		{
			String pathKey = Path.Combine(path_RSA_sha256_1024, "sha256d_key.pem");
			PrivateKeyManager key = new PrivateKeyManager();
			key.Load(pathKey);
			options.SetPrivateKey(key);
			String alg = "RS256";
			String token = jwt.DoCreate(alg, claims, options);
			Assert.IsFalse(SecurityUtils.compareStrings("", token));
			Assert.IsFalse(jwt.HasError());
		}
	}
}
