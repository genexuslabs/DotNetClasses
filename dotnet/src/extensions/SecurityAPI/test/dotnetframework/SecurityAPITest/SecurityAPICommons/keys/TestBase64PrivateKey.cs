using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.SecurityAPICommons.keys
{
	[TestFixture]
	public class TestBase64PrivateKey : SecurityAPITestObject
	{
		protected static string path;
		protected static string base64string;
		protected static string base64Wrong;

		[SetUp]
		public virtual void SetUp()
		{
			path = BASE_PATH + "dummycerts\\RSA_sha256_1024\\sha256d_key.pem";
			base64Wrong = "--BEGINKEY--sdssf--ENDKEY—";
			base64string = "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMZ8m4ftIhfrdugi5kEszRZr5IRuqGDLTex+CfVnhnBYXyQgJXeCI0eyRYUAbNzw/9MPdFN//pV26AXeH/ajORVu1JVoOACZdNOIPFnwXXh8oBxNxLAYlqoK2rAL+/tns8rKqqS4p8HSat9tj07TUXnsYJmmbXJM/eB94Ex66D1ZAgMBAAECgYA1xrTs0taV3HnO0wXHSrgWBw1WxBRihTKLjGpuTqoh7g943izIgD3GwwoKyt6zzafCK0G9DcSQAjNCw7etPvPL3FxwhDl+AHSv9JcChk/auICtMWwjurG4npto+s3byj/N00Idpz1xuOgKd8k9sdoPBGKa8l+LL+adSXzoivLG8QJBAPDvbOLSs9petB2iM6w5/DiC8EoxqDaBc7I1JFCvPOfB7i1GFFxkQ7hlgxpvaPX3NHXjAZpgdOW68P/SjU0izKsCQQDS5bjrNo3xn/MbYKojzwprR/Bo8Kvbi4/2M9NE3GwHegVmx5I+df+J0aObrbBNPLs/rhrFtt12OtgxJaac+FYLAkEA8DUUbvO4wj7m/iBnug65irHo1V+6oFThv0tCIHsFkt4DEvoqdI62AZKbafCnSYqjr+CaCYqfIScG/Vay77OBLwJBAI8EYAmKPmn7+SW4wMh1z+/+ogbYJwNEOoVQkdXh0JSlZ+JSNleLN5ajhtq8x5EpPSYrEFbB8p8JurBhgwJx2g8CQQDrp9scoK8eKBJ2p/63xqLGYSN6OZQo/4Lkq3983rmHoDCAp3Bz1zUyxQB3UVyrOj4U44C7RtDNiMSZuCwvjYAI";
		}

		[Test]
		public void TestImport()
		{
			PrivateKeyManager pkm = new PrivateKeyManager();
			bool loaded = pkm.FromBase64(base64string);
			True(loaded, pkm);
		}

		[Test]
		public void TestExport()
		{
			PrivateKeyManager pkm = new PrivateKeyManager();
			pkm.Load(path);
			string base64res = pkm.ToBase64();
			Assert.IsTrue(SecurityUtils.compareStrings(base64res, base64string));
			Assert.IsFalse(pkm.HasError());
		}

		[Test]
		public void TestWrongBase64()
		{
			PrivateKeyManager pkm = new PrivateKeyManager();
			pkm.FromBase64(base64Wrong);
			Assert.IsTrue(pkm.HasError());
		}
	}
}
