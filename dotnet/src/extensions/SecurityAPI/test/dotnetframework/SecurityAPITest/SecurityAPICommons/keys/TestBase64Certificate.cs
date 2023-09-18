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
			base64string = "MIIC/DCCAmWgAwIBAgIJAPmCVmfcc0IXMA0GCSqGSIb3DQEBCwUAMIGWMQswCQYDVQQGEwJVWTETMBEGA1UECAwKTW9udGV2aWRlbzETMBEGA1UEBwwKTW9udGV2aWRlbzEQMA4GA1UECgwHR2VuZVh1czERMA8GA1UECwwIU2VjdXJpdHkxEjAQBgNVBAMMCXNncmFtcG9uZTEkMCIGCSqGSIb3DQEJARYVc2dyYW1wb25lQGdlbmV4dXMuY29tMB4XDTIwMDcwODE4NDkxNVoXDTI1MDcwNzE4NDkxNVowgZYxCzAJBgNVBAYTAlVZMRMwEQYDVQQIDApNb250ZXZpZGVvMRMwEQYDVQQHDApNb250ZXZpZGVvMRAwDgYDVQQKDAdHZW5lWHVzMREwDwYDVQQLDAhTZWN1cml0eTESMBAGA1UEAwwJc2dyYW1wb25lMSQwIgYJKoZIhvcNAQkBFhVzZ3JhbXBvbmVAZ2VuZXh1cy5jb20wgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAMZ8m4ftIhfrdugi5kEszRZr5IRuqGDLTex+CfVnhnBYXyQgJXeCI0eyRYUAbNzw/9MPdFN//pV26AXeH/ajORVu1JVoOACZdNOIPFnwXXh8oBxNxLAYlqoK2rAL+/tns8rKqqS4p8HSat9tj07TUXnsYJmmbXJM/eB94Ex66D1ZAgMBAAGjUDBOMB0GA1UdDgQWBBTfXY8eOfDONCZpFE0V34mJJeCYtTAfBgNVHSMEGDAWgBTfXY8eOfDONCZpFE0V34mJJeCYtTAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBCwUAA4GBAAPv7AFlCSpJ32c/VYowlbk6UBhOKmVWBQlrAtvVQYtCKO/y9CEB8ikG19c8lHM9axnsbZR+G7g04Rfuiea3T7VPkSmUXPpz5fl6Zyk4LZg5Oji7MMMXGmr+7cpYWRhifCVwoxSgZEXt3d962IZ1Wei0LMO+4w4gnzPxqr8wVHnT";
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
