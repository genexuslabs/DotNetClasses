using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWT;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Jwt.Features
{
	[TestFixture]
	public class TestCreateFromJSON : SecurityAPITestObject
	{
		protected static string payload;
		protected static string key;
		protected static SymmetricKeyGenerator keyGen;
		protected static JWTCreator jwt;
		protected static JWTOptions options;

		[SetUp]
		public virtual void SetUp()
		{
			payload = "{\"sub\":\"subject1\",\"aud\":\"audience1\",\"nbf\":1594116920,\"hola1\":\"hola1\",\"iss\":\"GXSA\",\"hola2\":\"hola2\",\"exp\":1909649720,\"iat\":1596449720,\"jti\":\"0696bb20-6223-4a1c-9ebf-e15c74387b9c, 0696bb20-6223-4a1c-9ebf-e15c74387b9c\"}";
			SymmetricKeyGenerator keyGen = new SymmetricKeyGenerator();
			key = keyGen.doGenerateKey("GENERICRANDOM", 256);
			jwt = new JWTCreator();
			options = new JWTOptions();

		}

		[Test]
		public void TestCreateFromJSONMetod()
		{
			options.SetSecret(key);
			string token = jwt.DoCreateFromJSON("HS256", payload, options);
			bool verifies = jwt.DoVerifyJustSignature(token, "HS256", options);
			True(verifies, jwt);
		}
	}
}
