using SecurityAPITest.SecurityAPICommons.commons;
using System;
using NUnit.Framework;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWTClaims;
using SecurityAPICommons.Keys;
using GeneXusJWT.GenexusJWTUtils;

namespace SecurityAPITest.Jwt.Features
{
    [TestFixture]
    public class TestJwtRevocationList: SecurityAPITestObject
    {
		protected static String ID;
		protected static JWTCreator jwt;
		protected static JWTOptions options;
		protected static PrivateClaims claims;
		protected static SymmetricKeyGenerator keyGen;
		protected static String token;
		protected static RevocationList rList;

		[SetUp]
		public virtual void SetUp()
		{
			jwt = new JWTCreator();
			options = new JWTOptions();
			keyGen = new SymmetricKeyGenerator();
			claims = new PrivateClaims();
			rList = new RevocationList();



			options.AddRegisteredClaim("iss", "GXSA");
			options.AddRegisteredClaim("sub", "subject1");
			options.AddRegisteredClaim("aud", "audience1");
			ID = "0696bb20-6223-4a1c-9ebf-e15c74387b9c, 0696bb20-6223-4a1c-9ebf-e15c74387b9c";//&guid.Generate()
			options.AddRegisteredClaim("jti", ID);
			claims.setClaim("hola1", "hola1");
			claims.setClaim("hola2", "hola2");

			String hexaKey = keyGen.doGenerateKey("GENERICRANDOM", 256);
			options.SetSecret(hexaKey);
			options.AddRevocationList(rList);

			token = jwt.DoCreate("HS256", claims, options);
		}

		[Test]
		public void TestPositive()
		{
			bool verification = jwt.DoVerify(token, "HS256", claims, options);
			Assert.IsTrue(verification);
			True(verification, jwt);
		}

		[Test]
		public void TestNegative()
		{
			rList.addIDToRevocationList(ID);
			bool verification = jwt.DoVerify(token, "HS256", claims, options);
			Assert.IsFalse(verification);
			Assert.IsFalse(jwt.HasError());

		}
	}
}
