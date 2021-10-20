using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Jwt.Features
{
	[TestFixture]
	public class TestJwtDiverseDataTypes : SecurityAPITestObject
	{
		protected static JWTCreator jwt;
		protected static JWTOptions options;
		protected static SymmetricKeyGenerator keyGen;
		protected PrivateClaims claimslevel1;
		protected PrivateClaims claimslevel2;
		protected PrivateClaims claimslevel3;
		protected static string token;
		protected static string currentDate;
		protected static string hexaKey;

		[SetUp]
		public virtual void SetUp()
		{
			jwt = new JWTCreator();
			options = new JWTOptions();

			keyGen = new SymmetricKeyGenerator();
			claimslevel1 = new PrivateClaims();
			claimslevel2 = new PrivateClaims();
			claimslevel3 = new PrivateClaims();


			hexaKey = keyGen.doGenerateKey("GENERICRANDOM", 256);

			options.AddRegisteredClaim("aud", "jitsi");
			options.AddRegisteredClaim("iss", "my_client");
			options.AddRegisteredClaim("sub", "meet.jit.si");


			claimslevel1.setClaim("room", "*");
			claimslevel1.setNumericClaim("uno", 1);
			claimslevel1.setBooleanClaim("boolean", true);
			//1607626804
			claimslevel1.setDateClaim("date", 1607626804);

			claimslevel1.setClaim("context", claimslevel2);

			claimslevel2.setClaim("user", claimslevel3);
			claimslevel3.setClaim("avatar", "https:/gravatar.com/avatar/abc123");
			claimslevel3.setClaim("name", "John Doe");
			claimslevel3.setClaim("email", "jdoe@example.com");
			claimslevel3.setClaim("id", "abcd:a1b2c3-d4e5f6-0abc1-23de-abcdef01fedcba");
			claimslevel2.setClaim("group", "a123-123-456-789");

			options.SetSecret(hexaKey);
			token = jwt.DoCreate("HS256", claimslevel1, options);
		}

		[Test]
		public void TestPositive()
		{
			string payload = jwt.GetPayload(token);
			bool verification = jwt.DoVerify(token, "HS256", claimslevel1, options);
			
			True(verification, jwt);
		}
	}
}
