using SecurityAPICommons.Encoders;
using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;

namespace SecurityAPITest.SecurityAPICommons.encoders
{
	[TestFixture]
	public class TestBase64Url: SecurityAPITestObject
	{
		protected static string expected_plainText;
		protected static string expected_encoded;
		protected static string expected_hexaText;
		protected static Base64UrlEncoder base64;
		protected static string expected_base64;

		[SetUp]
		public virtual void SetUp()
		{
			expected_plainText = "hello world";
			expected_encoded = "aGVsbG8gd29ybGQ.";
			expected_hexaText = "68656C6C6F20776F726C64";
			base64 = new Base64UrlEncoder();
			expected_base64 = "aGVsbG8gd29ybGQ=";
		}

		[Test]
		public void TestToBase64Url()
		{
			string encoded = base64.toBase64(expected_plainText);
			Equals(expected_encoded, encoded, base64);
		}

		[Test]
		public void TestToPlainTextUrl()
		{
			string plainText = base64.toPlainText(expected_encoded);
			Equals(expected_plainText, plainText, base64);
		}

		[Test]
		public void TestToStringHexaUrl()
		{
			string hexaText = base64.toStringHexa(expected_encoded);
			Equals(expected_hexaText, hexaText, base64);
		}

		[Test]
		public void TestFromStringHexaToBase64Url()
		{
			string encoded = base64.fromStringHexaToBase64(expected_hexaText);
			Equals(expected_encoded, encoded, base64);
		}

		[Test]
		public void TestBase64UrlToBase64()
		{
			string encoded = base64.base64UrlToBase64(expected_encoded);
			Equals(expected_base64, encoded, base64);
		}

		[Test]
		public void TestBase64ToBase64Url()
		{
			string encoded = base64.base64ToBase64Url(expected_base64);
			Equals(expected_encoded, encoded, base64);
		}
	}
}
