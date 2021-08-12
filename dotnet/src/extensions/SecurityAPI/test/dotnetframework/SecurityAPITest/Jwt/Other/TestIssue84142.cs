using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using GeneXusJWT.GenexusComons;
using SecurityAPICommons.Keys;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;

namespace SecurityAPITest.Jwt.Other
{
    [TestFixture]
    public class TestIssue84142: SecurityAPITestObject
    {
        protected static JWTOptions options;
        protected static PrivateClaims claims;
        protected static SymmetricKeyGenerator keyGenerator;
        protected static JWTCreator jwt;


		[SetUp]
		public virtual void SetUp()
		{

		
			keyGenerator = new SymmetricKeyGenerator();
			jwt = new JWTCreator();
			options = new JWTOptions();
			claims = new PrivateClaims();

			string hexaKey = keyGenerator.doGenerateKey("GENERICRANDOM", 256);
			options.SetSecret(hexaKey);
			
			claims.setClaim("hola1", "hola1");
			

		}

		[Test]
		public void Test_expValidationPositive()
		{
			options.AddCustomTimeValidationClaim("exp", "2030/07/07 10:15:20", "20");
			string token = jwt.DoCreate("HS256", claims, options);
			bool validation = jwt.DoVerify(token, "HS256", claims, options);
			Assert.IsTrue(validation);
		}

		[Test]
		public void Test_expValidationNegative()
		{
			options.AddCustomTimeValidationClaim("exp", "2019/07/07 10:15:20", "20");
			string token = jwt.DoCreate("HS256", claims, options);
			bool validation = jwt.DoVerify(token, "HS256", claims, options);
			Assert.IsFalse(validation);
		}
	}
}
