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

	}
}
