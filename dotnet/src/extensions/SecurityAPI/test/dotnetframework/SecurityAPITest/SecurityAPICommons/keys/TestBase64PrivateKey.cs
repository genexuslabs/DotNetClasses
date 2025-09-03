using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.SecurityAPICommons.keys
{
	[TestFixture]
	public class TestBase64PrivateKey : SecurityAPITestObject
	{
		protected static string path;
		protected static string base64string;
		protected static string base64Wrong;

		[SetUp]
		public virtual void SetUp()
		{
			path = BASE_PATH + "dummycerts\\RSA_sha256_1024\\sha256d_key.pem";
			base64Wrong = "--BEGINKEY--sdssf--ENDKEY—";
			base64string = "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMDrssm9NdOu+m8+4kUsOGiwpBG4tapy/FUkZ1yUS02S+NTaPCl/i6VySFQOKjnEDWpeZKkIw9ekYk0u/iwSOR1G3SvXg8ETLnZJJZVMisG6TLFwCS3eYAVKemCbt9SeAmQTkIxvwPqcptoVD2kp5eICN3x5fArcFxEXMHn2W+lJAgMBAAECgYEAjs3qCmuE7K0ZtD9YPtv85YHb8UJJN2LmZiAMYvtiwomIqAbjgdRoCpAN+iqCF0CIrbQxzu4uCfIk0f13KChVHYspqw70EYmUSZIhxlbrvg1vZ8RNq/O+lwoGpd/n8kRgLTRMGgEFe265v28sSF7WeZNQejd+wuVgv9mZjv7AwAECQQD8ZTBU38/sw7uV4Y8gp6t9qEO+IavXQhsjM6y3Lt3y6vRtPx311HVP9eNR3QK/UwW9cDW685Pz26oPV88YrKvxAkEAw60OzMblAREhKu5y9ZX5OmBDix76lxp6/peBXguRymc3enLKm4flRwHGlrkzBcRgxsBzSF1DuBYOPLxr63LK2QJBALOej7bXUPH+mhEgZOuoZ7MVfKBi9hhLQ2TZ8aCsCehGrYzRzlCU0qgFJbGsx7fBLeSTZqmVj0WMnoosw4Wb3QECQE9GMmPGBIsdHHnfJtXWD6WV0GdxgoZrJP819CRcvZDppjFGhkzijoHo90Ki/0fL2oVK/KmJl2DiFpyGnZZC6GkCQQDwtQwfNk/UWeeVkTeXDiJyYFLxfRIhEIoi80Nxg4AXuPs1+SGFha1IjimpQ6gfmAc60f4SW8Xi0Ei8arZ/LTQA";
		}

		[Test]
		public void TestImport()
		{
			PrivateKeyManager pkm = new PrivateKeyManager();
			bool loaded = pkm.FromBase64(base64string);
			True(loaded, pkm);
		}

		[Test]
		public void TestExport()
		{
			PrivateKeyManager pkm = new PrivateKeyManager();
			pkm.Load(path);
			string base64res = pkm.ToBase64();
			Assert.IsTrue(SecurityUtils.compareStrings(base64res, base64string));
			Assert.IsFalse(pkm.HasError());
		}

		[Test]
		public void TestWrongBase64()
		{
			PrivateKeyManager pkm = new PrivateKeyManager();
			pkm.FromBase64(base64Wrong);
			Assert.IsTrue(pkm.HasError());
		}
	}
}
