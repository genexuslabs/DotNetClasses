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
		private string base64 = "MIIC/DCCAmWgAwIBAgIJAIh1DtAn5T0IMA0GCSqGSIb3DQEBBQUAMIGWMQswCQYDVQQGEwJVWTETMBEGA1UECAwKTW9udGV2aWRlbzETMBEGA1UEBwwKTW9udGV2aWRlbzEQMA4GA1UECgwHR2VuZVh1czERMA8GA1UECwwIU2VjdXJpdHkxEjAQBgNVBAMMCXNncmFtcG9uZTEkMCIGCSqGSIb3DQEJARYVc2dyYW1wb25lQGdlbmV4dXMuY29tMB4XDTIwMDcwODE4NDM1N1oXDTI1MDcwNzE4NDM1N1owgZYxCzAJBgNVBAYTAlVZMRMwEQYDVQQIDApNb250ZXZpZGVvMRMwEQYDVQQHDApNb250ZXZpZGVvMRAwDgYDVQQKDAdHZW5lWHVzMREwDwYDVQQLDAhTZWN1cml0eTESMBAGA1UEAwwJc2dyYW1wb25lMSQwIgYJKoZIhvcNAQkBFhVzZ3JhbXBvbmVAZ2VuZXh1cy5jb20wgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAKvo5gGDQ2w0veZSDxd+nJc7w7z/Is+4iGhOEuK9A/U713RfBdXYx2prp+7BAkUrGYm+Z6SkXZ6r78Tl5D/L2pNeA6nn5geCoWH1KSFOlAvEnjXcGvkdo8bIE/Day3PWFdeIGD8Mt0badAoIM+0m6s5jfSu9N8o4I4UX9O4PoEwhAgMBAAGjUDBOMB0GA1UdDgQWBBSLvqEYCzyExQe0fuRFBXpHjVbb6TAfBgNVHSMEGDAWgBSLvqEYCzyExQe0fuRFBXpHjVbb6TAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBBQUAA4GBAArYRju3NQeCspTxvpixMLLPWaYzxRmtUkEz1yr7VhlIH63RTIqbRcbP+40DRxx83LkIOJRdOcCVeLX3ZutknJglfrqFkUF5grWrhrHpd+IRSeN3lePMYa3GeeljTyrPINCwnv0YFLQOwRf8UlZcKAquJO2ouQZkVd9t1tRWTvNo";

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
