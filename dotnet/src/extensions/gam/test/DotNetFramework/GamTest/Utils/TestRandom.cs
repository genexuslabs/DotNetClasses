using NUnit.Framework;
using GamUtils;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Utilities.Encoders;
using System;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestRandom
	{
#pragma warning disable CS0414
		private static int l128;
		private static int l256;

		private static int l5;

		private static int l10;
#pragma warning restore CS0414

		[SetUp]
		public virtual void SetUp()
		{
			l128 = 128;
			l256 = 256;
			l5 = 5;
			l10 = 10;
		}

		[Test]
		public void TestRandomNumeric()
		{
			string l5_string = GamUtilsEO.RandomNumeric(l5);
			Assert.AreEqual(l5, l5_string.Length, "l5 numeric");

			string l10_string = GamUtilsEO.RandomNumeric(l10);
			Assert.AreEqual(l10, l10_string.Length, "l10 numeric");

			string l128_string = GamUtilsEO.RandomNumeric(l128);
			Assert.AreEqual(l128, l128_string.Length, "l128 numeric");

			string l256_string = GamUtilsEO.RandomNumeric(l256);
			Assert.AreEqual(l256, l256_string.Length, "l256 numeric");

		}

		[Test]
		public void TestRandomAlphanumeric()
		{
			string l5_string = GamUtilsEO.RandomAlphanumeric(l5);
			Assert.AreEqual(l5, l5_string.Length, "l5 alphanumeric");

			string l10_string = GamUtilsEO.RandomAlphanumeric(l10);
			Assert.AreEqual(l10, l10_string.Length, "l10 alphanumeric");

			string l128_string = GamUtilsEO.RandomAlphanumeric(l128);
			Assert.AreEqual(l128, l128_string.Length, "l128 alphanumeric");

			string l256_string = GamUtilsEO.RandomAlphanumeric(l256);
			Assert.AreEqual(l256, l256_string.Length, "l256 alphanumeric");
		}

		[Test]
		public void TestRandomUrlSafeCharacters()
		{
			string l5_string = GamUtilsEO.RandomUrlSafeCharacters(l5);
			Assert.AreEqual(l5, l5_string.Length, "l5 urlSafeCharacters");

			string l10_string = GamUtilsEO.RandomUrlSafeCharacters(l10);
			Assert.AreEqual(l10, l10_string.Length, "l10 urlSafeCharacters");

			string l128_string = GamUtilsEO.RandomUrlSafeCharacters(l128);
			Assert.AreEqual(l128, l128_string.Length, "l128 urlSafeCharacters");

			string l256_string = GamUtilsEO.RandomUrlSafeCharacters(l256);
			Assert.AreEqual(l256, l256_string.Length, "l256 urlSafeCharacters");
		}

		[Test]
		public void TestHexaBits()
		{
			int[] lengths = new int[] { 32, 64, 128, 256, 512, 1024 };
			foreach(int n in lengths)
			{
				string hexa = GamUtilsEO.RandomHexaBits(n);
				Assert.IsFalse(hexa.IsNullOrEmpty(), "TestHexaBits");
				try
				{
					byte[] decoded = Hex.Decode(hexa);
					if(decoded.Length*8 != n)
					{
						Assert.Fail("TestHexaBits wrong hexa length");
					}
				}catch(Exception e)
				{
					Assert.Fail("TestHexaBits nt hexa characters " + e.Message);
				}
			}
		}
	}
}