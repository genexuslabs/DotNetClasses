using NUnit.Framework;
using GamUtils;
using System;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Keys;
using Jose;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestJwks
	{
		private static string jwk;

		[SetUp]
		public virtual void SetUp()
		{
			
			jwk = GamUtilsEO.GenerateKeyPair();
		}

		[Test]
		public void TestGenerateKeyPair()
		{
			string jwk = GamUtilsEO.GenerateKeyPair();
			Assert.IsTrue(jwk.Length > 0, "Generate key pair jwk");
		}

		[Test]
		public void TestLoadPublicKey()
		{
			string b64 = GamUtilsEO.GetB64PublicKeyFromJwk(jwk);
			PublicKey key = new PublicKey();
			bool loaded = key.FromBase64(b64);
			Assert.IsTrue(loaded, "Load public key from base64 jwk");
			Assert.IsFalse(key.HasError(), "Public key has error loading from jwk");
		}

		[Test]
		public void TestLoadPrivateKey()
		{
			string b64 = GamUtilsEO.GetB64PrivateKeyFromJwk(jwk);
			PrivateKeyManager key = new PrivateKeyManager();
			bool loaded = key.FromBase64(b64);
			Assert.IsTrue(loaded, "Load private key from base64 jwk");
			Assert.IsFalse(key.HasError(), "Private key has error loading from jwk");
		}

		[Test]
		public void TestPublicJwk()
		{
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
