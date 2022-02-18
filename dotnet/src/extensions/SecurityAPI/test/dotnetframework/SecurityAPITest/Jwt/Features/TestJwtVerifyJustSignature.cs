using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Jwt.Features
{
	[TestFixture]
	public class TestJwtVerifyJustSignature: SecurityAPITestObject
	{
		protected static JWTCreator jwt;
		protected static JWTOptions options;
		protected static SymmetricKeyGenerator keyGen;
		protected static DateUtil du;
		protected PrivateClaims claims;
		protected static string token;
		protected static string currentDate;
		protected static string hexaKey;

		[SetUp]
		public virtual void SetUp()
		{
			jwt = new JWTCreator();
			options = new JWTOptions();
			du = new DateUtil();
			keyGen = new SymmetricKeyGenerator();
			claims = new PrivateClaims();

			currentDate = du.GetCurrentDate();
			hexaKey = keyGen.doGenerateKey("GENERICRANDOM", 256);

			options.AddRegisteredClaim("aud", "jitsi");
			options.AddRegisteredClaim("iss", "my_client");
			options.AddRegisteredClaim("sub", "meet.jit.si");
			string expiration = du.CurrentPlusSeconds(200);
			options.AddCustomTimeValidationClaim("exp", expiration, "20");

			claims.setClaim("hola", "hola");

			options.AddHeaderParameter("cty", "twilio-fpa;v=1");
			options.SetSecret(hexaKey);

			token = jwt.DoCreate("HS256", claims, options);

		}

		[Test]
		public void testPositive_JustSign()
		{
			JWTOptions options1 = new JWTOptions();
			options1.SetSecret(hexaKey);
			bool verification = jwt.DoVerifyJustSignature(token, "HS256", options1);
			True(verification, jwt);
		}

		[Test]
		public void testComplete_JustSign()
		{
			bool verification = jwt.DoVerifyJustSignature(token, "HS256", options);
			True(verification, jwt);
		}

		[Test]
		public void TestNegative_JustSign()
		{
			JWTOptions options1 = new JWTOptions();
			string hexaKey1 = keyGen.doGenerateKey("GENERICRANDOM", 256);
			options1.SetSecret(hexaKey1);
			bool verification = jwt.DoVerifyJustSignature(token, "HS256", options1);
			Assert.IsFalse(verification);
			Assert.IsTrue(jwt.HasError());
		}

		[Test]
		public void TestPositive_Sign()
		{
			options.SetSecret(hexaKey);
			bool verification = jwt.DoVerifySignature(token, "HS256", options);
			True(verification, jwt);
		}

		[Test]
		public void TestNegative_Sign()
		{
			string hexaKey1 = keyGen.doGenerateKey("GENERICRANDOM", 256);
			options.SetSecret(hexaKey1);
			bool verification = jwt.DoVerifySignature(token, "HS256", options);
			Assert.IsFalse(verification);
			Assert.IsTrue(jwt.HasError());
		}

	}
}
