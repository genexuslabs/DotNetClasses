using GamTest.Utils.Resources;
using NUnit.Framework;
using GamUtils;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestHash
	{
#pragma warning disable CS0414
		private static string one;
		private static string two;
		private static string three;
		private static string four;
		private static string five;

		private CryptographicHash cryptographicHash;
#pragma warning restore CS0414

		[SetUp]
		public virtual void SetUp()
		{
			one = "one";
			two = "two";
			three = "three";
			four = "four";
			five = "five";
			cryptographicHash = new CryptographicHash("SHA-512");
		}

		[Test]
		public void TestSha512()
		{
			Assert.AreEqual(cryptographicHash.ComputeHash(one), GamUtilsEO.Sha512(one), "one");
			Assert.AreEqual(cryptographicHash.ComputeHash(two), GamUtilsEO.Sha512(two), "two");
			Assert.AreEqual(cryptographicHash.ComputeHash(three), GamUtilsEO.Sha512(three), "three");
			Assert.AreEqual(cryptographicHash.ComputeHash(four), GamUtilsEO.Sha512(four), "four");
			Assert.AreEqual(cryptographicHash.ComputeHash(five), GamUtilsEO.Sha512(five), "five");
		}

		[Test]
		public void TestSha512Random()
		{
			for (int i = 0; i < 100; i++)
			{
				string value = GamUtilsEO.RandomAlphanumeric(15);
				Assert.AreEqual(cryptographicHash.ComputeHash(value), GamUtilsEO.Sha512(value), "random sha512 ");
			}
		}
	}
}