using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Jwt.Other
{
	[TestFixture]
	public class TestJwtDomainSpaces : SecurityAPITestObject
	{
		protected static JWTCreator jwt;
		protected static JWTOptions options;
		protected static SymmetricKeyGenerator keyGen;
		protected static DateUtil du;
		protected PrivateClaims claims;
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

			options.AddRegisteredClaim("aud ", "jitsi");
			options.AddRegisteredClaim(" iss", "my_client");
			options.AddRegisteredClaim(" sub ", "meet.jit.si");
			string expiration = du.CurrentPlusSeconds(200);
			options.AddCustomTimeValidationClaim("exp", expiration, "20");

			claims.setClaim("hola", "hola");

			options.AddHeaderParameter("cty", "twilio-fpa;v=1");
			options.SetSecret(hexaKey);

		}

		[Test]
		public void TestDomains()
		{

			options.SetSecret(hexaKey);
			string token = jwt.DoCreate("HS256 ", claims, options);
			Assert.IsFalse(jwt.HasError());
			bool verification = jwt.DoVerifyJustSignature(token, " HS256", options);
			True(verification, jwt);
		}
	}
}
