using NUnit.Framework;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.SecurityAPICommons.keys
{
	[TestFixture]
	public class TestBase64PublicKey: SecurityAPITestObject
	{
		protected static string path;
		protected static string base64string;
		protected static string base64Wrong;

		[SetUp]
		public virtual void SetUp()
		{
			path = BASE_PATH + "dummycerts\\RSA_sha256_1024\\sha256_pubkey.pem";
			base64Wrong = "--BEGINKEY--sdssf--ENDKEY—";
			base64string = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDA67LJvTXTrvpvPuJFLDhosKQRuLWqcvxVJGdclEtNkvjU2jwpf4ulckhUDio5xA1qXmSpCMPXpGJNLv4sEjkdRt0r14PBEy52SSWVTIrBukyxcAkt3mAFSnpgm7fUngJkE5CMb8D6nKbaFQ9pKeXiAjd8eXwK3BcRFzB59lvpSQIDAQAB";

		}

		[Test]
		public void TestImport()
		{
			PublicKey cert = new PublicKey();
			bool loaded = cert.FromBase64(base64string);
			True(loaded, cert);
		}


		[Test]
		public void TestExport()
		{
			PublicKey cert = new PublicKey();
			cert.Load(path);
			string base64res = cert.ToBase64();
			Assert.IsTrue(SecurityUtils.compareStrings(base64res, base64string));
			Assert.IsFalse(cert.HasError());
		}

		[Test]
		public void TestWrongBase64()
		{
			PublicKey cert = new PublicKey();
			cert.FromBase64(base64Wrong);
			Assert.IsTrue(cert.HasError());
		}
	}
}
