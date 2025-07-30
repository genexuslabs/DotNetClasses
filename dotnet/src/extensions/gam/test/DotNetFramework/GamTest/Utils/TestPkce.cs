using GamUtils;
using NUnit.Framework;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestPkce
    {

		[Test]
		public void TestPkceS256()
		{
			int i = 0;
			while (i < 50)
			{
				string[] s256_true = GamUtilsEO.Pkce_Create(20, "S256").Split(',');
				Assert.IsTrue(GamUtilsEO.Pkce_Verify(s256_true[0], s256_true[1], "S256"), "testPkceS256 true");

				string[] s256_false = GamUtilsEO.Pkce_Create(20, "S256").Split(',');
				Assert.IsFalse(GamUtilsEO.Pkce_Verify($"{s256_false[0]}tralala", s256_false[1], "S256"), "testPkceS256 false");
				i++;
			}
		}

		[Test]
		public void TestPkcePlain()
		{
			int i = 0;
			while (i < 50)
			{
				string[] plain_true = GamUtilsEO.Pkce_Create(20, "PLAIN").Split(',');
				Assert.IsTrue(GamUtilsEO.Pkce_Verify(plain_true[0], plain_true[1], "PLAIN"), "testPkceS256");

				string[] plain_false = GamUtilsEO.Pkce_Create(20, "PLAIN").Split(',');
				Assert.IsFalse(GamUtilsEO.Pkce_Verify($"{plain_false[0]}tralala", plain_false[1], "PLAIN"), "testPkceS256 false");
				i++;
			}
		}
	}
}
