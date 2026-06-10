using Jose;
using NUnit.Framework;
using GamUtils;
using Microsoft.IdentityModel.Tokens;
using System;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestJwk
	{

		[SetUp]
		public virtual void SetUp()
		{
		}

		[Test]
		public void TestGenerateKeyPair()
		{
			string jwk = GamUtilsEO.GenerateKeyPair();
			Assert.IsFalse(string.IsNullOrEmpty(jwk), "Generate key pair jwk");
		}


		[Test]
		public void TestPublicJwk()
		{
			string jwk = GamUtilsEO.GenerateKeyPair();
			string public_jwk = GamUtilsEO.GetPublicJwk(jwk);
			string public_jwks = "{\"keys\": [" + public_jwk + "]}";
			try
			{
				JwkSet jwks = JwkSet.FromJson(public_jwks);
				Assert.NotNull(jwks, "To public JWK fail");
			}
			catch (Exception e)
			{
				Assert.Fail("Exception on testPublicJwk" + e.Message);
			}
		}

		[Test]
		public void TestGetAlgorithm()
		{
			string jwk = GamUtilsEO.GenerateKeyPair();
			string algorithm = GamUtilsEO.GetJwkAlgorithm(jwk);
			Assert.AreEqual(algorithm, "RS256", "testGetAlgorithm");
		}
	}
}
