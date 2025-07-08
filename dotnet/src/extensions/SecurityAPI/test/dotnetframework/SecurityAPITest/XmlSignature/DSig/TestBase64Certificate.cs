using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using GeneXusXmlSignature.GeneXusCommons;
using SecurityAPICommons.Keys;
using GeneXusXmlSignature.GeneXusDSig;
using SecurityAPICommons.Utils;
using System.IO;

namespace SecurityAPITest.XmlSignature.DSig
{
    [TestFixture]
    public class TestBase64Certificate: SecurityAPITestObject
    {
		private string base64 = "MIIDCjCCAnOgAwIBAgIUW1tCN/KqaNQX0DvdDb3WFDCxEuIwDQYJKoZIhvcNAQEFBQAwgZYxCzAJBgNVBAYTAlVZMRMwEQYDVQQIDApNb250ZXZpZGVvMRMwEQYDVQQHDApNb250ZXZpZGVvMRAwDgYDVQQKDAdHZW5lWHVzMREwDwYDVQQLDAhTZWN1cml0eTESMBAGA1UEAwwJc2dyYW1wb25lMSQwIgYJKoZIhvcNAQkBFhVzZ3JhbXBvbmVAZ2VuZXh1cy5jb20wHhcNMjUwNzA4MTQzNzQ5WhcNMzAwNzA3MTQzNzQ5WjCBljELMAkGA1UEBhMCVVkxEzARBgNVBAgMCk1vbnRldmlkZW8xEzARBgNVBAcMCk1vbnRldmlkZW8xEDAOBgNVBAoMB0dlbmVYdXMxETAPBgNVBAsMCFNlY3VyaXR5MRIwEAYDVQQDDAlzZ3JhbXBvbmUxJDAiBgkqhkiG9w0BCQEWFXNncmFtcG9uZUBnZW5leHVzLmNvbTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEA6quMq4knDBYCUhyuD7DobnLm0q6IrHiSr8rOlJ73kTurS0QZRILZFn7O3DoBGFOA7fIYpeSoWLkbkU4AZgRU3t+BtmlJqfWi21FuZa7P86Lova2JqyhzJgk5GLjoPie49WBTmecZXUytwaTlR1d5/Euht/3r4xb3/lpRtMqTEHUCAwEAAaNTMFEwHQYDVR0OBBYEFEZdLshvqnzXpVD17GoS8yVosNVSMB8GA1UdIwQYMBaAFEZdLshvqnzXpVD17GoS8yVosNVSMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQEFBQADgYEAPptVObAvKso9m+QLxddNOrqGZTonRe0SaQo8BO/v3GzX8BL6zptEqNAe5Rxme5TwY8ZlyYsb0f3YKO0czR5YQQDz3EdYXdOV2YuS/o02n9kGv677ITMS4T0ka+QzHrMdUql/IuzFwFr7lYeNEh44afyJ2HQjyea0JbULLuWRUSY=";

		private string path_RSA_sha1_1024;
		private string xmlUnsignedPath;
		private string pathSigned;
		private DSigOptions options;
		private string pathKey;
		private string xmlSignedPathRoot;

		[SetUp]
		public virtual void SetUp()
		{
			path_RSA_sha1_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha1_1024");

			options = new DSigOptions();

			xmlUnsignedPath = Path.Combine(BASE_PATH, "Temp", "toSign.xml");
			pathSigned = "base64.xml";
			pathKey = Path.Combine(path_RSA_sha1_1024, "sha1d_key.pem");
			xmlSignedPathRoot = Path.Combine(BASE_PATH, "Temp", "outputTestFilesJ");

		}

		[Test]
		public void TestSignBase64()
		{
			CertificateX509 newCert = new CertificateX509();
			bool loaded = newCert.FromBase64(base64);
			Assert.IsTrue(loaded);
			True(loaded, newCert);
			PrivateKeyManager key = new PrivateKeyManager();
			bool privateLoaded = key.Load(pathKey);
			Assert.IsTrue(privateLoaded);
			True(privateLoaded, key);
			XmlDSigSigner signer = new XmlDSigSigner();
			bool result = signer.DoSignFile(xmlUnsignedPath, key, newCert, xmlSignedPathRoot + pathSigned, options);
			Assert.IsTrue(result);
			True(result, signer);
			bool verify = signer.DoVerifyFile(xmlSignedPathRoot + pathSigned, options);
			Assert.IsTrue(verify);
			True(verify, signer);
		}

		[Test]
		public void TestToBase64()
		{
			CertificateX509 newCert = new CertificateX509();
			bool loaded = newCert.FromBase64(base64);
			Assert.IsTrue(loaded);
			True(loaded, newCert);
			string newBase64 = newCert.ToBase64();
			bool result = SecurityUtils.compareStrings(newBase64, base64);
			Assert.IsTrue(result);
			True(result, newCert);
		}
	}
}
