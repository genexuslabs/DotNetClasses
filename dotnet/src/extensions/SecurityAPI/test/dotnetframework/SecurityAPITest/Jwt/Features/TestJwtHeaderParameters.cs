using SecurityAPITest.SecurityAPICommons.commons;
using System;
using NUnit.Framework;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusComons;
using SecurityAPICommons.Keys;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;

namespace SecurityAPITest.Jwt.Features
{
    [TestFixture]
    public class TestJwtHeaderParameters: SecurityAPITestObject
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


			
			hexaKey = keyGen.doGenerateKey("GENERICRANDOM", 256);

			options.AddRegisteredClaim("aud", "jitsi");
			options.AddRegisteredClaim("iss", "my_client");
			options.AddRegisteredClaim("sub", "meet.jit.si");
			

			claims.setClaim("hola", "hola");

			options.AddHeaderParameter("cty", "twilio-fpa;v=1");
			options.SetSecret(hexaKey);

			token = jwt.DoCreate("HS256", claims, options);
		}

		[Test]
		public void TestPositive()
		{
			bool verification = jwt.DoVerify(token, "HS256", claims, options);
			True(verification, jwt);
		}

		[Test]
		public void TestNegative1()
		{
			options.AddHeaderParameter("pepe", "whatever");
			bool verification = jwt.DoVerify(token, "HS256", claims, options);
			Assert.IsFalse(verification);
			Assert.IsFalse(jwt.HasError());
		}

		[Test]
		public void TestNegative2()
		{
			JWTOptions op = new JWTOptions();
			op.AddRegisteredClaim("aud", "jitsi");
			op.AddRegisteredClaim("iss", "my_client");
			op.AddRegisteredClaim("sub", "meet.jit.si");
			op.SetSecret(hexaKey);
			op.AddHeaderParameter("pepe", "whatever");

			bool verification = jwt.DoVerify(token, "HS256", claims, op);
			Assert.IsFalse(verification);
			Assert.IsFalse(jwt.HasError());

		}

        [Test]
        public void TestNegative3()
        {
            JWTOptions op = new JWTOptions();
            op.AddRegisteredClaim("aud", "jitsi");
            op.AddRegisteredClaim("iss", "my_client");
            op.AddRegisteredClaim("sub", "meet.jit.si");
            op.SetSecret(hexaKey);
            //op.AddHeaderParameter("pepe", "whatever");

            bool verification = jwt.DoVerify(token, "HS256", claims, op);
           // Assert.IsFalse(verification);
            Assert.IsFalse(jwt.HasError());

        }
    }
}
