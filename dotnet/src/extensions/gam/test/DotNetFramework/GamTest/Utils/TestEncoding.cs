using System;
using System.Text;
using NUnit.Framework;
using GamUtils;
using Org.BouncyCastle.Utilities.Encoders;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestEncoding
	{
		[SetUp]
		public virtual void SetUp()
		{

		}

		[Test]
		public void TestB64ToB64Url()
		{
			int i = 0;
			do
			{
				string randomString = GamUtilsEO.RandomAlphanumeric(128);
				string testing = GamUtilsEO.Base64ToBase64Url(Base64.ToBase64String(Encoding.UTF8.GetBytes(randomString)));
				Assert.AreEqual(randomString, B64UrlToUtf8(testing), "TestB64ToB64Url");
				i++;
			} while (i < 50);
		}

		private static string B64UrlToUtf8(string base64Url)
		{
			try
			{
				byte[] bytes = UrlBase64.Decode(base64Url);
				return Encoding.UTF8.GetString(bytes);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				return "";
			}
		}

		[Test]
		public void TestHexaToBase64()
		{
			int i = 0;
			do
			{
				string randomHexa = GamUtilsEO.RandomHexaBits(128);
				string testing = B64ToHexa(GamUtilsEO.HexaToBase64(randomHexa));
				Assert.AreEqual(randomHexa, testing, "TestHexaToBase64");
				i++;
			} while (i < 50);
		}

		[Test]
		public void TestToBase64Url()
		{
			int i = 0;
			do
			{
				string randomString = GamUtilsEO.RandomAlphanumeric(128);
				string testing = GamUtilsEO.ToBase64Url(randomString);
				Assert.AreEqual(randomString, GamUtilsEO.FromBase64Url(testing), "testB64ToB64Url");
				i++;
			} while (i < 50);
		}

		private static string B64ToHexa(string base64)
		{
			try
			{
				byte[] bytes = Base64.Decode(base64);
				return Hex.ToHexString(bytes);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return "";
			}
		}

	}
}
