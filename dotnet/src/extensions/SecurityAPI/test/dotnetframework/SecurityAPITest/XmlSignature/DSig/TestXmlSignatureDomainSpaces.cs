using GeneXusXmlSignature.GeneXusCommons;
using GeneXusXmlSignature.GeneXusDSig;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;
using System.IO;

namespace SecurityAPITest.XmlSignature.DSig
{
	[TestFixture]
	public class TestXmlSignatureDomainSpaces : SecurityAPITestObject
	{
		private static string path_RSA_sha1_1024;
		//private static string xmlUnsigned;
		//private static string dSigType;
		private static DSigOptions options;
		private static string pathKey;
		private static string pathCert;
		private static XmlDSigSigner signer;
		private static PrivateKeyManager key;
		private static CertificateX509 cert;

		[SetUp]
		public virtual void SetUp()
		{
			signer = new XmlDSigSigner();
			path_RSA_sha1_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha1_1024");
			//xmlUnsigned = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + "<Envelope xmlns=\"http://example.org/envelope\">"
			//		+ "  <Body>" + "    Ola mundo" + "  </Body>" + "</Envelope>";
			//dSigType = "ENVELOPED ";
			options = new DSigOptions();

			pathKey = Path.Combine(path_RSA_sha1_1024, "sha1d_key.pem");
			pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.crt");

			key = new PrivateKeyManager();
			cert = new CertificateX509();
		}

		/*[Test]
		public void TestDomains()
		{
			key.Load(pathKey);
			Assert.IsFalse(key.HasError());
			cert.Load(pathCert);
			Assert.IsFalse(cert.HasError());
			options.DSigSignatureType = dSigType;
			options.Canonicalization = "C14n_OMIT_COMMENTS ";
			options.KeyInfoType = " X509Certificate";
			string signed = signer.DoSign(xmlUnsigned, key, cert, options);
			//System.out.println("Error. Code: " + signer.getErrorCode() + " Desc: " + signer.getErrorDescription());
			Assert.IsFalse(signer.HasError());
			bool verified = signer.DoVerify(signed, options);
			True(verified, signer);
		}*/

	}
}
