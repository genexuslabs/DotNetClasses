using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using NUnit.Framework;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Jwt.Other
{
	[TestFixture]
	public class TestIssue103626: SecurityAPITestObject
	{
		protected static JWTOptions options;
		protected static PrivateClaims claims;
		protected static JWTCreator jwt;
		protected static string token;

		[SetUp]
		public virtual void SetUp()
		{
			jwt = new JWTCreator();
			options = new JWTOptions();
			claims = new PrivateClaims();

			claims.setClaim("hola1", "hola1");
			claims.setClaim("hola2", "hola2");

		}

		[Test]
		public void Test_SymmetricError()
		{
			string dummytoken = jwt.DoCreate("HS256", claims, options);
			Assert.IsTrue(jwt.HasError());

		}

		[Test]
		public void Test_AsymmetricError()
		{
			string dummytoken = jwt.DoCreate("RS256", claims, options);
			Assert.IsTrue(jwt.HasError());

		}
	}
}
