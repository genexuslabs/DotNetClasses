using GamUtils;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestEncryption
	{
		[Test]
		public void TestAesGcm()
		{
			string key = GamUtilsEO.RandomHexaBits(256);
			string nonce = GamUtilsEO.RandomHexaBits(128);
			string txt = "hello world";
			int macSize = 64;
			string encrypted = GamUtilsEO.AesGcm(txt, key, nonce, macSize, true);
			Assert.IsFalse(string.IsNullOrEmpty(encrypted), "testAesGcm encrypt");
			string decrypted = GamUtilsEO.AesGcm(encrypted, key, nonce, macSize, false);
			Assert.AreEqual(txt, decrypted, "testAesGcm decrypt");
		}
	}
}
