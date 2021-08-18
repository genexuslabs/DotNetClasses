using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWTUtils;
using SecurityAPICommons.Keys;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWT;

namespace SecurityAPITest.Jwt.Symmetric
{
    [TestFixture]
    public class TestSymmetricJwt: SecurityAPITestObject
    {
        protected static JWTOptions options;
        protected static PrivateClaims claims;
        protected static GUID guid;
        protected static DateUtil du;
        protected static SymmetricKeyGenerator keyGenerator;
        protected static JWTCreator jwt;

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

			
			options.AddCustomTimeValidationClaim("iat", du.GetCurrentDate(), "20");
			options.AddCustomTimeValidationClaim("nbf", du.GetCurrentDate(), "20");

			claims.setClaim("hola1", "hola1");
			claims.setClaim("hola2", "hola2");

		}

		[Test]
		public void Test_HS256()
		{
			string hexaKey = keyGenerator.doGenerateKey("GENERICRANDOM", 256);
			options.SetSecret(hexaKey);
			string token = jwt.DoCreate("HS256", claims, options);
			Assert.IsFalse(jwt.HasError());
			bool verification = jwt.DoVerify(token, "HS256", claims, options);
			True(verification, jwt);
		}

		[Test]
		public void Test_HS512()
		{
			string hexaKey = keyGenerator.doGenerateKey("GENERICRANDOM", 512);
			options.SetSecret(hexaKey);
			string token = jwt.DoCreate("HS512", claims, options);
			Assert.IsFalse(jwt.HasError());
			bool verification = jwt.DoVerify(token, "HS512", claims, options);
			True(verification, jwt);
		}
	}
}
