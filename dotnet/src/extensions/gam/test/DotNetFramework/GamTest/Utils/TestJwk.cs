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
			Assert.IsFalse(jwk.IsNullOrEmpty(), "Generate key pair jwk");
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
	}
}
