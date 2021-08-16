using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using GeneXusCryptography.Symmetric;
using SecurityAPICommons.Utils;

namespace SecurityAPITest.Cryptography.Symmetric
{
    [TestFixture]
    public class TestStreamEncryption: SecurityAPITestObject
    {
		protected static string key8;
		protected static string key32;
		protected static string key128;
		protected static string key256;
		protected static string key1024;
		protected static string key6144;
		protected static string key8192;

		protected static string IV64;
		protected static string IV128;
		protected static string IV192;
		protected static string IV256;
		protected static string IV512;
		protected static string IV1024;
		protected static string IV6144;

		private static string plainText;

		private static string[] encodings;
		private static EncodingUtil eu;

		[SetUp]
		public virtual void SetUp()
		{

			plainText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam venenatis ex sit amet risus pellentesque, a faucibus quam ultrices. Ut tincidunt quam eu aliquam maximus. Quisque posuere risus at erat blandit eleifend. Curabitur viverra rutrum volutpat. Donec quis quam tellus. Aenean fermentum elementum augue, a semper risus scelerisque sit amet. Nullam vitae sapien vitae dui ullamcorper dapibus quis quis leo. Sed neque felis, pellentesque in risus et, lobortis ultricies nulla. Quisque quis quam risus. Donec vestibulum, lectus vel vestibulum eleifend, velit ante volutpat lacus, ut mattis quam ligula eget est. Sed et pulvinar lectus. In mollis turpis non ipsum vehicula, sit amet rutrum nibh dictum. Duis consectetur convallis ex, eu ultricies enim bibendum vel. Vestibulum vel libero nibh. Morbi nec odio mattis, vestibulum quam blandit, pretium orci.Aenean pellentesque tincidunt nunc a malesuada. Etiam gravida fermentum mi, at dignissim dui aliquam quis. Nullam vel lobortis libero. Phasellus non gravida posuere";
			SymmetricKeyGenerator keyGen = new SymmetricKeyGenerator();

			/**** GENERATE KEYS ****/
			key8 = keyGen.doGenerateKey("GENERICRANDOM", 8);
			key32 = keyGen.doGenerateKey("GENERICRANDOM", 32);
			key128 = keyGen.doGenerateKey("GENERICRANDOM", 128);
			key256 = keyGen.doGenerateKey("GENERICRANDOM", 256);
			key1024 = keyGen.doGenerateKey("GENERICRANDOM", 1024);
			key8192 = keyGen.doGenerateKey("GENERICRANDOM", 8192);
			key6144 = keyGen.doGenerateKey("GENERICRANDOM", 6144);

			/**** GENERATE IVs ****/
			IV64 = keyGen.doGenerateIV("GENERICRANDOM", 64);
			IV128 = keyGen.doGenerateIV("GENERICRANDOM", 128);
			IV192 = keyGen.doGenerateIV("GENERICRANDOM", 192);
			IV256 = keyGen.doGenerateIV("GENERICRANDOM", 256);
			IV512 = keyGen.doGenerateIV("GENERICRANDOM", 512);
			IV1024 = keyGen.doGenerateIV("GENERICRANDOM", 1024);
			IV6144 = keyGen.doGenerateIV("GENERICRANDOM", 6144);

			encodings = new string[] { "UTF_8", "UTF_16", "UTF_16BE", "UTF_16LE", "UTF_32", "UTF_32BE", "UTF_32LE", "SJIS",
				"GB2312" };

			eu = new EncodingUtil();

		}

		[Test]
		public void TestRC4()
		{
			// RC4 key 1024, no nonce
			testBulkAlgorithms("RC4", key1024, "");
		}

		[Test]
		public void TestHC128()
		{
			// HC128 key 128 bits, no nonce
			testBulkAlgorithms("HC128", key128, IV128);
		}

		[Test]
		public void TestHC256()
		{
			// HC256 key 256 bits, IV 128 o 256 bits
			testBulkAlgorithms("HC256", key256, IV128);
			testBulkAlgorithms("HC256", key256, IV256);

		}

		[Test]
		public void TestSALSA20()
		{
			// SALSA20 key 256 o 128 bits, 64 bit nonce
			testBulkAlgorithms("SALSA20", key128, IV64);
			testBulkAlgorithms("SALSA20", key256, IV64);

		}

		[Test]
		public void TestCHACHA20()
		{
			// CHACHA key 128 o 256, IV 64 bits
			testBulkAlgorithms("CHACHA20", key128, IV64);
			testBulkAlgorithms("CHACHA20", key256, IV64);
		}

		[Test]
		public void TestXSALSA20()
		{
			// SALSA20 key 256 bits, 192 bit nonce
			testBulkAlgorithms("XSALSA20", key256, IV192);
		}

		[Test]
		public void TestISAAC()
		{
			// ISAAC 32, 8192 key, no nonce
			testBulkAlgorithms("ISAAC", key32, "");
			testBulkAlgorithms("ISAAC", key8192, "");
		}

		[Test]
		public void TestVMPC()
		{
			// key 8 o 6144, nonce 1...6144 bits
			testBulkAlgorithms("VMPC", key8, IV64);
			testBulkAlgorithms("VMPC", key8, IV128);
			testBulkAlgorithms("VMPC", key8, IV192);
			testBulkAlgorithms("VMPC", key8, IV256);
			testBulkAlgorithms("VMPC", key8, IV512);
			testBulkAlgorithms("VMPC", key8, IV1024);
			testBulkAlgorithms("VMPC", key8, IV6144);

			testBulkAlgorithms("VMPC", key6144, IV64);
			testBulkAlgorithms("VMPC", key6144, IV128);
			testBulkAlgorithms("VMPC", key6144, IV192);
			testBulkAlgorithms("VMPC", key6144, IV256);
			testBulkAlgorithms("VMPC", key6144, IV512);
			testBulkAlgorithms("VMPC", key6144, IV1024);
			testBulkAlgorithms("VMPC", key6144, IV6144);

		}

		private void testBulkAlgorithms(string algorithm, string key, string IV)
		{
			for (int i = 0; i < encodings.Length; i++)
			{
				eu.setEncoding(encodings[i]);
				SymmetricStreamCipher symCipher = new SymmetricStreamCipher();
				string encrypted = symCipher.DoEncrypt(algorithm, key, IV, plainText);
				string decrypted = symCipher.DoDecrypt(algorithm, key, IV, encrypted);
				Assert.IsTrue(SecurityUtils.compareStrings(plainText, decrypted));
				True(SecurityUtils.compareStrings(plainText, decrypted), symCipher);
			}
		}
	}
}
