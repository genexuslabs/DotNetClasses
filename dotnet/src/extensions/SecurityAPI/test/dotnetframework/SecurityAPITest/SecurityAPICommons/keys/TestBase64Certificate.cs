using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityAPITest.SecurityAPICommons.keys
{
	[TestFixture]
	public class TestBase64Certificate: SecurityAPITestObject
	{

		protected static string path;
		protected static string base64string;
		protected static string base64Wrong;

		[SetUp]
		public virtual void SetUp()
		{
			path = BASE_PATH + "dummycerts\\RSA_sha256_1024\\sha256_cert.pem";
			base64Wrong = "--BEGINKEY--sdssf--ENDKEY—";
			base64string = "MIIDCjCCAnOgAwIBAgIURHVGg5Ozz1spxIPCKiwHZT0K9wwwDQYJKoZIhvcNAQELBQAwgZYxCzAJBgNVBAYTAlVZMRMwEQYDVQQIDApNb250ZXZpZGVvMRMwEQYDVQQHDApNb250ZXZpZGVvMRAwDgYDVQQKDAdHZW5lWHVzMREwDwYDVQQLDAhTZWN1cml0eTESMBAGA1UEAwwJc2dyYW1wb25lMSQwIgYJKoZIhvcNAQkBFhVzZ3JhbXBvbmVAZ2VuZXh1cy5jb20wHhcNMjUwNzA4MTQxMjU1WhcNMzAwNzA3MTQxMjU1WjCBljELMAkGA1UEBhMCVVkxEzARBgNVBAgMCk1vbnRldmlkZW8xEzARBgNVBAcMCk1vbnRldmlkZW8xEDAOBgNVBAoMB0dlbmVYdXMxETAPBgNVBAsMCFNlY3VyaXR5MRIwEAYDVQQDDAlzZ3JhbXBvbmUxJDAiBgkqhkiG9w0BCQEWFXNncmFtcG9uZUBnZW5leHVzLmNvbTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAwOuyyb010676bz7iRSw4aLCkEbi1qnL8VSRnXJRLTZL41No8KX+LpXJIVA4qOcQNal5kqQjD16RiTS7+LBI5HUbdK9eDwRMudkkllUyKwbpMsXAJLd5gBUp6YJu31J4CZBOQjG/A+pym2hUPaSnl4gI3fHl8CtwXERcwefZb6UkCAwEAAaNTMFEwHQYDVR0OBBYEFHgcEkhRiSnjjEtjZgr3LEcavww/MB8GA1UdIwQYMBaAFHgcEkhRiSnjjEtjZgr3LEcavww/MA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQELBQADgYEALeKQIY1p9YNxkxchoK2IR01FqJM8qx7aLbiZOy9H2cjaFocY/clBYbFPD+uXCp+dlJBoVfLD51w8+vhKJi3RA0EvBREIe+ltAuviT6UwAI6xN4SKKX/Up8shUxpdeF0JhxBCXTCrKu504c16qOSnTrd3x7KmnsRscpwu0uiRDg8=";
		}

		[Test]
		public void TestImport()
		{
			CertificateX509 cert = new CertificateX509();
			bool loaded = cert.FromBase64(base64string);
			True(loaded, cert);
		}


		[Test]
		public void TestExport()
		{
			CertificateX509 cert = new CertificateX509();
			cert.Load(path);
			string base64res = cert.ToBase64();
			Assert.IsTrue(SecurityUtils.compareStrings(base64res, base64string));
			Assert.IsFalse(cert.HasError());
		}

		[Test]
		public void TestWrongBase64()
		{
			CertificateX509 cert = new CertificateX509();
			cert.FromBase64(base64Wrong);
			Assert.IsTrue(cert.HasError());
		}
	}
}
