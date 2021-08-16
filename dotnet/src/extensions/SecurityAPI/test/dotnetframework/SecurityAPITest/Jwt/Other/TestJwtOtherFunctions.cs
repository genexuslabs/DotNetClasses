using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusJWTClaims;
using SecurityAPICommons.Keys;
using GeneXusJWT.GenexusComons;
using SecurityAPICommons.Utils;

namespace SecurityAPITest.Jwt.Other
{
    [TestFixture]
    public class TestJwtOtherFunctions: SecurityAPITestObject
    {
        protected static string ID;
        protected static JWTCreator jwt;
        protected static JWTOptions options;
        protected static PrivateClaims claims;
        protected static SymmetricKeyGenerator keyGen;
        protected static string token;

		[SetUp]
		public virtual void SetUp()
		{
			options = new JWTOptions();
			jwt = new JWTCreator();
			claims = new PrivateClaims();
			keyGen = new SymmetricKeyGenerator();

			options.AddRegisteredClaim("iss", "GXSA");
			options.AddRegisteredClaim("sub", "subject1");
			options.AddRegisteredClaim("aud", "audience1");
			ID = "0696bb20-6223-4a1c-9ebf-e15c74387b9c, 0696bb20-6223-4a1c-9ebf-e15c74387b9c";// &guid.Generate()
			options.AddRegisteredClaim("jti", ID);
			claims.setClaim("hola1", "hola1");
			claims.setClaim("hola2", "hola2");
		}

		[Test]
		public void GenerateToken()
		{
			string hexaKey = keyGen.doGenerateKey("GENERICRANDOM", 256);
			options.SetSecret(hexaKey);
			token = jwt.DoCreate("HS256", claims, options);
			Assert.IsFalse(jwt.HasError());
			bool verification = jwt.DoVerify(token, "HS256", claims, options);
			True(verification, jwt);
		}

		[Test]
		public void TestGetID()
		{
			string tID = jwt.GetTokenID(token);
			Assert.IsTrue(SecurityUtils.compareStrings(ID, tID));
		}

		[Test]
		public void TestGetPayload()
		{
			string payload = "{\"sub\":\"subject1\",\"aud\":\"audience1\",\"hola1\":\"hola1\",\"iss\":\"GXSA\",\"hola2\":\"hola2\",\"jti\":\""
					+ ID + "\"}";
			string payload1 = "{\"hola1\":\"hola1\",\"hola2\":\"hola2\",\"iss\":\"GXSA\",\"sub\":\"subject1\",\"aud\":\"audience1\",\"jti\":\""
					+ ID + "\"}";
			string payload2 = "{\"sub\":\"subject1\",\"aud\":\"audience1\",\"hola1\":\"hola1\",\"iss\":\"GXSA\",\"hola2\":\"hola2\",\"jti\":\""
					+ ID + "\"}";

			string tPayload = jwt.GetPayload(token);
			Assert.IsTrue(SecurityUtils.compareStrings(payload, tPayload) || SecurityUtils.compareStrings(payload1, tPayload)
					|| SecurityUtils.compareStrings(payload2, tPayload));
		}

		[Test]
		public void TestGetHeader()
		{
			string header = "{\"typ\":\"JWT\",\"alg\":\"HS256\"}";
			string header1 = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
			string tHeader = jwt.GetHeader(token);
			Assert.IsTrue(SecurityUtils.compareStrings(header, tHeader) || SecurityUtils.compareStrings(header1, tHeader));
		}

		[Test]
		public void TestbadAlgorithm()
		{
			string hexaKey = keyGen.doGenerateKey("GENERICRANDOM", 256);
			options.SetSecret(hexaKey);
			string token1 = jwt.DoCreate("HS256", claims, options);
			bool verification = jwt.DoVerify(token1, "RS256", claims, options);
			Assert.IsFalse(verification);
			Assert.IsTrue(jwt.HasError());
		}

	}
}
