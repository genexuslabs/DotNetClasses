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
			base64string = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDGfJuH7SIX63boIuZBLM0Wa+SEbqhgy03sfgn1Z4ZwWF8kICV3giNHskWFAGzc8P/TD3RTf/6VdugF3h/2ozkVbtSVaDgAmXTTiDxZ8F14fKAcTcSwGJaqCtqwC/v7Z7PKyqqkuKfB0mrfbY9O01F57GCZpm1yTP3gfeBMeug9WQIDAQAB";

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
