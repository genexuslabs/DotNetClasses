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

		[Test]
		public void TestBase64Url()
		{
			string[] utf8 = { "GQTuYnnS9AbcKXndwxiZbxk4Q60nhuEd", "rf7tZx8aWO28YOKLISDWY33HuarNHkIZ", "sF7Ic0iuZxE50nz3W5Jnj7R0nQlRD0b1", "GGKmW2ubkhnA9ASaVlVAKM6FQdPCQ1pj", "LMW0GSCVyeGiGzf84eIwuX6OHAfur9fp", "zq9Kni7W1r0UIzG9hjYeiqJhSYlWVZSa", "WcyhGLQNyQkP2YmOjVtIilpqcHgYCzjq", "DuhO4PBiXRDDj50RBRo8wNUU8R3UXbp0", "pkPfYXOyoLUsEwm4HjjDB6E2c3aUjYNh", "fgbrZoKKMym9HN5zlKj0a8ohgQlJm3PM", "owGXQ7p6BeFeK1KFVOsdbSRd0sMwgFRU" };
			string[] b64 = { "R1FUdVlublM5QWJjS1huZHd4aVpieGs0UTYwbmh1RWQ", "cmY3dFp4OGFXTzI4WU9LTElTRFdZMzNIdWFyTkhrSVo", "c0Y3SWMwaXVaeEU1MG56M1c1Sm5qN1IwblFsUkQwYjE", "R0dLbVcydWJraG5BOUFTYVZsVkFLTTZGUWRQQ1ExcGo", "TE1XMEdTQ1Z5ZUdpR3pmODRlSXd1WDZPSEFmdXI5ZnA", "enE5S25pN1cxcjBVSXpHOWhqWWVpcUpoU1lsV1ZaU2E", "V2N5aEdMUU55UWtQMlltT2pWdElpbHBxY0hnWUN6anE", "RHVoTzRQQmlYUkREajUwUkJSbzh3TlVVOFIzVVhicDA", "cGtQZllYT3lvTFVzRXdtNEhqakRCNkUyYzNhVWpZTmg", "ZmdiclpvS0tNeW05SE41emxLajBhOG9oZ1FsSm0zUE0", "b3dHWFE3cDZCZUZlSzFLRlZPc2RiU1JkMHNNd2dGUlU" };
			for (int i = 0; i < utf8.Length; i++)
			{
				Assert.AreEqual(b64[i], GamUtilsEO.ToBase64Url(utf8[i]), $"testBase64Url ToBase64Url fail index: {i}");
				Assert.AreEqual(utf8[i], GamUtilsEO.FromBase64Url(b64[i]), $"testBase64Url FromBase64Url fail index: {i}");
			}
		}

		private static string B64UrlToUtf8(string base64Url)
		{
			try
			{
				return Encoding.UTF8.GetString(Jose.Base64Url.Decode(base64Url));
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
