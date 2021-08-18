using SecurityAPITest.SecurityAPICommons.commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using GeneXusJWT.GenexusJWT;
using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWTClaims;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;

namespace SecurityAPITest.Jwt.Other
{
    [TestFixture]
    public class TestIssue83649: SecurityAPITestObject
    {
        protected static JWTCreator jwt;
        protected static JWTOptions options;
        protected static PrivateClaims claims;
        protected static SymmetricKeyGenerator keyGen;
        protected static string payload;
        protected static string expected;


        [SetUp]
        public virtual void SetUp()
        {
            jwt = new JWTCreator();
            options = new JWTOptions();
            keyGen = new SymmetricKeyGenerator();
            claims = new PrivateClaims();

            options.AddCustomTimeValidationClaim("exp", "2020/07/20 17:56:51", "20");
            options.AddCustomTimeValidationClaim("iat", "2020/07/20 17:56:51", "20");
            options.AddCustomTimeValidationClaim("nbf", "2020/07/20 17:56:51", "20");
            claims.setClaim("hola1", "hola1");
            string hexaKey = keyGen.doGenerateKey("GENERICRANDOM", 256);
            options.SetSecret(hexaKey);
            string token = jwt.DoCreate("HS256", claims, options);
            payload = jwt.GetPayload(token);
            

            expected = "{\"hola1\":\"hola1\",\"exp\":1595267811,\"iat\":1595267811,\"nbf\":1595267811}";
        }

        [Test]
        public void Test_timeClaims()
        {
            Assert.IsTrue(SecurityUtils.compareStrings(expected, payload));
        }


    }
}
