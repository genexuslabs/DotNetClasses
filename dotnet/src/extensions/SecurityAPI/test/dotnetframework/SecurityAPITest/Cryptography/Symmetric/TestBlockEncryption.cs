using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Config;
using GeneXusCryptography.Symmetric;
using SecurityAPICommons.Utils;

namespace SecurityAPITest.Cryptography.Symmetric
{
    [TestFixture]
    public class TestBlockEncryption: SecurityAPITestObject
    {
		private static string plainText;
		private static string plainTextCTS;
		private static string[] arrayPaddings;
		private static string[] arrayModes;
		private static string[] arrayModes64;
		private static string[] arrayNoncesCCM;
		private static string[] arrayNonces;
		private static string[] arrayModes_160_224;
		private static int[] arrayTagsGCM;
		private static int[] arrayMacsEAX;
		private static int[] arrayTagsCCM;
		

		protected static string key1024;
		protected static string key512;
		protected static string key448;
		protected static string key256;
		protected static string key192;
		protected static string key160;
		protected static string key128;
		protected static string key64;

		protected static string IV1024;
		protected static string IV512;
		protected static string IV256;
		protected static string IV224;
		protected static string IV192;
		protected static string IV160;
		protected static string IV128;
		protected static string IV64;

		private static SymmetricKeyGenerator keyGen;

		private static string[] encodings;
		private static EncodingUtil eu;

		[SetUp]
		protected virtual void SetUp()
		{

			// new EncodingUtil().setEncoding("UTF8");
			plainText = "Neque porro quisquam est qui dolorem ipsum quia dolor sit amet";
			plainTextCTS = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam venenatis ex sit amet risus pellentesque, a faucibus quam ultrices. Ut tincidunt quam eu aliquam maximus. Quisque posuere risus at erat blandit eleifend. Curabitur viverra rutrum volutpat. Donec quis quam tellus. Aenean fermentum elementum augue, a semper risus scelerisque sit amet. Nullam vitae sapien vitae dui ullamcorper dapibus quis quis leo. Sed neque felis, pellentesque in risus et, lobortis ultricies nulla. Quisque quis quam risus. Donec vestibulum, lectus vel vestibulum eleifend, velit ante volutpat lacus, ut mattis quam ligula eget est. Sed et pulvinar lectus. In mollis turpis non ipsum vehicula, sit amet rutrum nibh dictum. Duis consectetur convallis ex, eu ultricies enim bibendum vel. Vestibulum vel libero nibh. Morbi nec odio mattis, vestibulum quam blandit, pretium orci.Aenean pellentesque tincidunt nunc a malesuada. Etiam gravida fermentum mi, at dignissim dui aliquam quis. Nullam vel lobortis libero. Phasellus non gravida posuere";

			keyGen = new SymmetricKeyGenerator();

			/**** CREATE KEYS ****/
			key1024 = keyGen.doGenerateKey("GENERICRANDOM", 1024);
			key512 = keyGen.doGenerateKey("GENERICRANDOM", 512);
			key448 = keyGen.doGenerateKey("GENERICRANDOM", 448);
			key256 = keyGen.doGenerateKey("GENERICRANDOM", 256);
			key160 = keyGen.doGenerateKey("GENERICRANDOM", 160);
			key192 = keyGen.doGenerateKey("GENERICRANDOM", 192);
			key128 = keyGen.doGenerateKey("GENERICRANDOM", 128);
			key64 = keyGen.doGenerateKey("GENERICRANDOM", 64);

			/**** CREATE IVs ****/
			IV1024 = keyGen.doGenerateIV("GENERICRANDOM", 1024);
			IV512 = keyGen.doGenerateIV("GENERICRANDOM", 512);
			IV256 = keyGen.doGenerateIV("GENERICRANDOM", 256);
			IV224 = keyGen.doGenerateIV("GENERICRANDOM", 224);
			IV192 = keyGen.doGenerateIV("GENERICRANDOM", 192);
			IV160 = keyGen.doGenerateIV("GENERICRANDOM", 160);
			IV128 = keyGen.doGenerateIV("GENERICRANDOM", 128);
			IV64 = keyGen.doGenerateIV("GENERICRANDOM", 64);

			/**** CREATE nonces ****/
			string nonce104 = keyGen.doGenerateIV("GENERICRANDOM", 104);
			string nonce64 = keyGen.doGenerateIV("GENERICRANDOM", 64);
			string nonce56 = keyGen.doGenerateIV("GENERICRANDOM", 56);
			arrayNoncesCCM = new string[] { nonce56, nonce64, nonce104 };

			/**** CREATE PADDINGS ****/
			arrayPaddings = new string[] { "PKCS7PADDING", "ISO10126D2PADDING", "X923PADDING", "ISO7816D4PADDING",
				"ZEROBYTEPADDING" };

			/**** CREATEMODES ****/
			arrayModes = new string[] { "ECB", "CBC", "CFB", "CTR", "CTS", "OFB", "OPENPGPCFB" };
			arrayModes64 = new string[] { "ECB", "CBC", "CFB", "CTR", "CTS", "OFB", "OPENPGPCFB" };
			arrayTagsGCM = new int[] { 128, 120, 112, 104, 96 };
			arrayTagsCCM = new int[] { 64, 128 };
			arrayMacsEAX = new int[] { 8, 16, 64, 128 };
			arrayNonces = new string[] { IV64, IV128, IV192, IV256 };
			arrayModes_160_224 = new string[] { "ECB", "CBC", "CTR", "CTS", "OPENPGPCFB" }; //CFB mode does not work on 160 and 224 block sizes

			encodings = new string[] { "UTF_8", "UTF_16", "UTF_16BE", "UTF_16LE", "UTF_32", "UTF_32BE", "UTF_32LE", "SJIS",
				"GB2312" };

			eu = new EncodingUtil();
		}

		[Test]
		public void TestAES()
		{
			// key legths 128,192 & 256
			// blocksize 128

			testBulkAlgorithm("AES", arrayModes, arrayPaddings, key128, IV128, false);
			testBulkAlgorithm("AES", arrayModes, arrayPaddings, key192, IV128, false);
			testBulkAlgorithm("AES", arrayModes, arrayPaddings, key256, IV128, false);
			// testCCM(string algorithm, string key, int macSize, boolean cts) {
			// testGCM(string algorithm, string key, string nonce, boolean cts) {
			// testEAX(string algorithm, string key, string nonce, boolean cts) {
			testCCM("AES", key128, false);
			testCCM("AES", key192, false);
			testCCM("AES", key256, false);

			testGCM("AES", key128, false);
			testGCM("AES", key192, false);
			testGCM("AES", key256, false);

			testEAX("AES", key128, false);
			testEAX("AES", key192, false);
			testEAX("AES", key256, false);

		}

		[Test]
		public void TestBLOWFISH()
		{
			// key lengths 0...448
			// blocksize 64
			// no gcm
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("BLOWFISH", arrayModes64, arrayPaddings, key128, IV64, false);
			testBulkAlgorithm("BLOWFISH", arrayModes64, arrayPaddings, key192, IV64, false);
			testBulkAlgorithm("BLOWFISH", arrayModes64, arrayPaddings, key256, IV64, false);
			testBulkAlgorithm("BLOWFISH", arrayModes64, arrayPaddings, key448, IV64, false);

		}

		[Test]
		public void TestCAMELLIA()
		{
			// key lengths 128.192.256
			// blocksize 128

			testBulkAlgorithm("CAMELLIA", arrayModes, arrayPaddings, key128, IV128, false);
			testBulkAlgorithm("CAMELLIA", arrayModes, arrayPaddings, key192, IV128, false);
			testBulkAlgorithm("CAMELLIA", arrayModes, arrayPaddings, key256, IV128, false);

			// testCCM(string algorithm, string key, int macSize, boolean cts) {
			// testGCM(string algorithm, string key, string nonce, boolean cts) {
			// testEAX(string algorithm, string key, string nonce, boolean cts) {

			testCCM("CAMELLIA", key128, false);
			testCCM("CAMELLIA", key192, false);
			testCCM("CAMELLIA", key256, false);

			testGCM("CAMELLIA", key128, false);
			testGCM("CAMELLIA", key192, false);
			testGCM("CAMELLIA", key256, false);

			testEAX("CAMELLIA", key128, false);
			testEAX("CAMELLIA", key192, false);
			testEAX("CAMELLIA", key256, false);

		}

		[Test]
		public void TestCAST5()
		{
			// key length 0...128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("CAST5", arrayModes64, arrayPaddings, key64, IV64, false);
			testBulkAlgorithm("CAST5", arrayModes64, arrayPaddings, key128, IV64, false);

		}

		[Test]
		public void TestCAST6()
		{
			// key length 0...256
			// blocksize 128
			testBulkAlgorithm("CAST6", arrayModes, arrayPaddings, key64, IV128, false);
			testBulkAlgorithm("CAST6", arrayModes, arrayPaddings, key128, IV128, false);
			testBulkAlgorithm("CAST6", arrayModes, arrayPaddings, key192, IV128, false);
			testBulkAlgorithm("CAST6", arrayModes, arrayPaddings, key256, IV128, false);

			// testCCM(string algorithm, string key, int macSize, boolean cts) {
			// testGCM(string algorithm, string key, string nonce, boolean cts) {
			// testEAX(string algorithm, string key, string nonce, boolean cts) {

			testCCM("CAST6", key64, false);
			testCCM("CAST6", key128, false);
			testCCM("CAST6", key192, false);
			testCCM("CAST6", key256, false);

			testGCM("CAST6", key64, false);
			testGCM("CAST6", key128, false);
			testGCM("CAST6", key192, false);
			testGCM("CAST6", key256, false);

			testEAX("CAST6", key128, false);
			testEAX("CAST6", key192, false);
			testEAX("CAST6", key256, false);

		}

		[Test]
		public void TestDES()
		{
			// key length 64
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX no se puede usar (keylength != 128, 192 o 256

			testBulkAlgorithm("DES", arrayModes64, arrayPaddings, key64, IV64, false);

		}

		[Test]
		public void TestTRIPLEDES()
		{
			// key length 128.192
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("TRIPLEDES", arrayModes64, arrayPaddings, key128, IV64, false);
			testBulkAlgorithm("TRIPLEDES", arrayModes64, arrayPaddings, key192, IV64, false);

		}

		[Test]
		public void TestDSTU7624()
		{
			// key length 128, 256, 512
			// blocksize 128.256.512
			// input should be as lenght as the block
			testBulkAlgorithm("DSTU7624_128", arrayModes, arrayPaddings, key128, IV128, true);
			testBulkAlgorithm("DSTU7624_256", arrayModes, arrayPaddings, key256, IV256, true);
			testBulkAlgorithm("DSTU7624_512", arrayModes, arrayPaddings, key512, IV512, true);

			// testCCM(string algorithm, string key, int macSize, boolean cts) {
			// testGCM(string algorithm, string key, string nonce, boolean cts) {
			// testEAX(string algorithm, string key, string nonce, boolean cts) {

			testCCM("DSTU7624_128", key128, true);

			testGCM("DSTU7624_128", key128, true);

			testEAX("DSTU7624_128", key128, true);

		}

		[Test]
		public void TestGOST28147()
		{
			// key length 256
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("GOST28147", arrayModes64, arrayPaddings, key256, IV64, false);

		}

		[Test]
		public void TestNOEKEON()
		{
			// key length 128
			// blocksize 128
			testBulkAlgorithm("NOEKEON", arrayModes, arrayPaddings, key128, IV128, false);

			testCCM("NOEKEON", key128, false);
			testGCM("NOEKEON", key128, false);
			testEAX("NOEKEON", key128, false);

		}

		[Test]
		public void TestRC2()
		{
			// key length 0...1024
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("RC2", arrayModes64, arrayPaddings, key64, IV64, false);
			testBulkAlgorithm("RC2", arrayModes64, arrayPaddings, key128, IV64, false);
			testBulkAlgorithm("RC2", arrayModes64, arrayPaddings, key192, IV64, false);
			testBulkAlgorithm("RC2", arrayModes64, arrayPaddings, key256, IV64, false);
			testBulkAlgorithm("RC2", arrayModes64, arrayPaddings, key512, IV64, false);
			testBulkAlgorithm("RC2", arrayModes64, arrayPaddings, key1024, IV64, false);

		}

		[Test]
		public void TestRC532()
		{
			// key length 0...128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("RC532", arrayModes64, arrayPaddings, key64, IV64, false);
			testBulkAlgorithm("RC532", arrayModes64, arrayPaddings, key128, IV64, false);
		}

		[Test]
		public void TestRC6()
		{
			// key length 0...256
			// blocksize 128
			testBulkAlgorithm("RC6", arrayModes, arrayPaddings, key64, IV128, false);
			testBulkAlgorithm("RC6", arrayModes, arrayPaddings, key128, IV128, false);
			testBulkAlgorithm("RC6", arrayModes, arrayPaddings, key192, IV128, false);
			testBulkAlgorithm("RC6", arrayModes, arrayPaddings, key256, IV128, false);

			testCCM("RC6", key64, false);
			testCCM("RC6", key128, false);
			testCCM("RC6", key192, false);
			testCCM("RC6", key256, false);

			testGCM("RC6", key64, false);
			testGCM("RC6", key128, false);
			testGCM("RC6", key192, false);
			testGCM("RC6", key256, false);

			testEAX("RC6", key128, false);
			testEAX("RC6", key192, false);
			testEAX("RC6", key256, false);
		}

		[Test]
		public void TestRIJNDAEL()
		{
			// key length 128.160.224.256
			// blocksize 128, 160, 192, 224, 256

			//Don't support CFB or OFB
			testBulkAlgorithm("RIJNDAEL_160", arrayModes_160_224, arrayPaddings, key128, IV160, false);
			testBulkAlgorithm("RIJNDAEL_160", arrayModes_160_224, arrayPaddings, key160, IV160, false);
			testBulkAlgorithm("RIJNDAEL_160", arrayModes_160_224, arrayPaddings, key192, IV160, false);
			testBulkAlgorithm("RIJNDAEL_160", arrayModes_160_224, arrayPaddings, key256, IV160, false);
		
			testBulkAlgorithm("RIJNDAEL_192", arrayModes, arrayPaddings, key128, IV192, false);
			testBulkAlgorithm("RIJNDAEL_192", arrayModes, arrayPaddings, key160, IV192, false);
			testBulkAlgorithm("RIJNDAEL_192", arrayModes, arrayPaddings, key192, IV192, false);
			testBulkAlgorithm("RIJNDAEL_192", arrayModes, arrayPaddings, key256, IV192, false);

			//Don't support CFB or OFB
			testBulkAlgorithm("RIJNDAEL_224", arrayModes_160_224, arrayPaddings, key128, IV224, false);
			testBulkAlgorithm("RIJNDAEL_224", arrayModes_160_224, arrayPaddings, key160, IV224, false);
			testBulkAlgorithm("RIJNDAEL_224", arrayModes_160_224, arrayPaddings, key192, IV224, false);
			testBulkAlgorithm("RIJNDAEL_224", arrayModes_160_224, arrayPaddings, key256, IV224, false);
		
			testBulkAlgorithm("RIJNDAEL_256", arrayModes, arrayPaddings, key128, IV256, false);
			testBulkAlgorithm("RIJNDAEL_256", arrayModes, arrayPaddings, key160, IV256, false);
			testBulkAlgorithm("RIJNDAEL_256", arrayModes, arrayPaddings, key192, IV256, false);
			testBulkAlgorithm("RIJNDAEL_256", arrayModes, arrayPaddings, key256, IV256, false);

			testCCM("RIJNDAEL_128", key128, false);
			testCCM("RIJNDAEL_128", key160, false);
			testCCM("RIJNDAEL_128", key192, false);
			testCCM("RIJNDAEL_128", key256, false);

			testGCM("RIJNDAEL_128", key128, false);
			testGCM("RIJNDAEL_128", key160, false);
			testGCM("RIJNDAEL_128", key192, false);
			testGCM("RIJNDAEL_128", key256, false);

			testEAX("RIJNDAEL_128", key128, false);
			testEAX("RIJNDAEL_128", key192, false);
			testEAX("RIJNDAEL_128", key256, false);
		
		}

		[Test]
		public void TestSEED()
		{
			// key length 128
			// blocksize 128

			testBulkAlgorithm("SEED", arrayModes, arrayPaddings, key128, IV128, false);

			testCCM("SEED", key128, false);
			testGCM("SEED", key128, false);
			testEAX("SEED", key128, false);
		}

		[Test]
		public void TestSERPENT()
		{
			// key length 128.192.256
			// blocksize 128
			testBulkAlgorithm("SERPENT", arrayModes, arrayPaddings, key128, IV128, false);
			testBulkAlgorithm("SERPENT", arrayModes, arrayPaddings, key192, IV128, false);
			testBulkAlgorithm("SERPENT", arrayModes, arrayPaddings, key256, IV128, false);

			testCCM("SERPENT", key128, false);
			testCCM("SERPENT", key192, false);
			testCCM("SERPENT", key256, false);

			testGCM("SERPENT", key128, false);
			testGCM("SERPENT", key192, false);
			testGCM("SERPENT", key256, false);

			testEAX("SERPENT", key128, false);
			testEAX("SERPENT", key192, false);
			testEAX("SERPENT", key256, false);

		}

		[Test]
		public void TestSKIPJACK()
		{
			// key length 128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("SKIPJACK", arrayModes64, arrayPaddings, key128, IV64, false);

		}

		[Test]
		public void TestSM4()
		{
			// key length 128
			// blocksize 128
			testBulkAlgorithm("SM4", arrayModes, arrayPaddings, key128, IV128, false);

			testCCM("SM4", key128, false);
			testGCM("SM4", key128, false);
			testEAX("SM4", key128, false);
		}

		[Test]
		public void TestTEA()
		{
			// key length 128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("TEA", arrayModes64, arrayPaddings, key128, IV64, false);

		}

		[Test]
		public void TestTHREEFISH()
		{
			// key length 256.512.1024
			// blocksize 256.512.1024
			// key must be same size as the block
			// the input must be the same length or longer than the block
			// no se puede usar CCM (blosize!=128)
			// GCM siempre explota

			testBulkAlgorithm("THREEFISH_256", arrayModes, arrayPaddings, key256, IV256, true);
			testBulkAlgorithm("THREEFISH_512", arrayModes, arrayPaddings, key512, IV512, true);
			testBulkAlgorithm("THREEFISH_1024", arrayModes, arrayPaddings, key1024, IV1024, true);
		}

		[Test]
		public void TestTWOFISH()
		{
			// key length 128.192.256
			// blocksize 128
			testBulkAlgorithm("TWOFISH", arrayModes, arrayPaddings, key128, IV128, false);
			testBulkAlgorithm("TWOFISH", arrayModes, arrayPaddings, key192, IV128, false);
			testBulkAlgorithm("TWOFISH", arrayModes, arrayPaddings, key256, IV128, false);

			testCCM("TWOFISH", key128, false);
			testCCM("TWOFISH", key192, false);
			testCCM("TWOFISH", key256, false);

			testGCM("TWOFISH", key128, false);
			testGCM("TWOFISH", key192, false);
			testGCM("TWOFISH", key256, false);

			testEAX("TWOFISH", key128, false);
			testEAX("TWOFISH", key192, false);
			testEAX("TWOFISH", key256, false);

		}

		[Test]
		public void TestXTEA()
		{
			// key length 128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("XTEA", arrayModes64, arrayPaddings, key128, IV64, false);

		}



		private void testBulkAlgorithm(string algorithm, string[] modes, string[] paddings, string key, string IV,
			bool cts)
		{
			for (int i = 0; i < encodings.Length; i++)
			{
				eu.setEncoding(encodings[i]);
				if (cts)
				{
					testBulkAlgorithmCTS(algorithm, modes, paddings, key, IV, plainTextCTS);
				}
				else
				{
					testBulkAlgorithmCTS(algorithm, modes, paddings, key, IV, plainText);
				}
			}
		}

		/*private void testBulkAlgorithmCTS(string algorithm, string[] modes, string[] paddings, string key, string IV,
				string text)
		{
			for (int m = 0; m < arrayModes.Length; m++)
			{
				for (int p = 0; p < arrayPaddings.Length; p++)
				{
					SymmetricBlockCipher symBlockCipher = new SymmetricBlockCipher();
					string encrypted = symBlockCipher.DoEncrypt(algorithm, arrayModes[m], arrayPaddings[p], key, IV, text);
					string decrypted = symBlockCipher.DoDecrypt(algorithm, arrayModes[m], arrayPaddings[p], key, IV,
							encrypted);

					/*eu.setEncoding("UTF_16");
					string encrypted = symBlockCipher.DoEncrypt(algorithm, arrayModes[2], arrayPaddings[0], key, IV, text);
					string decrypted = symBlockCipher.DoDecrypt(algorithm, arrayModes[2], arrayPaddings[0], key, IV,
							encrypted);
					string encoding = eu.getEncoding();*/
		/*string resText = eu.getString(eu.getBytes(text));
		Assert.IsTrue(SecurityUtils.compareStrings(resText, decrypted));

		True(true, symBlockCipher);
	}

}
}*/

		private void testBulkAlgorithmCTS(string algorithm, string[] modes, string[] paddings, string key, string IV,
		string text)
		{
			for (int m = 0; m < modes.Length; m++)
			{
				for (int p = 0; p < arrayPaddings.Length; p++)
				{
					SymmetricBlockCipher symBlockCipher = new SymmetricBlockCipher();
					string encrypted = symBlockCipher.DoEncrypt(algorithm, modes[m], arrayPaddings[p], key, IV, text);
					string decrypted = symBlockCipher.DoDecrypt(algorithm, modes[m], arrayPaddings[p], key, IV,
							encrypted);
					string resText = eu.getString(eu.getBytes(text));
					Assert.IsTrue(SecurityUtils.compareStrings(resText, decrypted));

					True(true, symBlockCipher);
				}

			}
		}


		private void testCCM(string algorithm, string key, bool cts)
		{
			if (cts)
			{
				testCCM_CTS(algorithm, key, plainTextCTS);
			}
			else
			{
				testCCM_CTS(algorithm, key, plainText);
			}
		}

		private void testCCM_CTS(string algorithm, string key, string text)
		{
			for (int n = 0; n < arrayNoncesCCM.Length; n++)
			{
				for (int t = 0; t < arrayTagsCCM.Length; t++)
				{
					for (int p = 0; p < arrayPaddings.Length; p++)
					{
						SymmetricBlockCipher symBlockCipher = new SymmetricBlockCipher();
						string encrypted = symBlockCipher.DoAEADEncrypt(algorithm, "AEAD_CCM", key, arrayTagsCCM[t],
								arrayNoncesCCM[n], text);
						string decrypted = symBlockCipher.DoAEADDecrypt(algorithm, "AEAD_CCM", key, arrayTagsCCM[t],
								arrayNoncesCCM[n], encrypted);
						Assert.IsTrue(SecurityUtils.compareStrings(text, decrypted));
						True(SecurityUtils.compareStrings(text, decrypted), symBlockCipher);
					}
				}
			}
		}

		private void testEAX(string algorithm, string key, bool cts)
		{
			if (cts)
			{
				testEAX_CTS(algorithm, key, plainTextCTS);
			}
			else
			{
				testEAX_CTS(algorithm, key, plainText);
			}
		}

		private void testEAX_CTS(string algorithm, string key, string text)
		{
			for (int m = 0; m < arrayMacsEAX.Length; m++)
			{
				for (int n = 0; n < arrayNonces.Length; n++)
				{
					for (int p = 0; p < arrayPaddings.Length; p++)
					{
						SymmetricBlockCipher symBlockCipher = new SymmetricBlockCipher();
						string encrypted = symBlockCipher.DoAEADEncrypt(algorithm, "AEAD_EAX", key, arrayMacsEAX[m],
								arrayNonces[n], text);
						string decrypted = symBlockCipher.DoAEADDecrypt(algorithm, "AEAD_EAX", key, arrayMacsEAX[m],
								arrayNonces[n], encrypted);
						Assert.IsTrue(SecurityUtils.compareStrings(text, decrypted));
						True(SecurityUtils.compareStrings(text, decrypted), symBlockCipher);
					}
				}
			}
		}

		private void testGCM(string algorithm, string key, bool cts)
		{
			if (cts)
			{
				testGCM_CTS(algorithm, key, plainTextCTS);
			}
			else
			{
				testGCM_CTS(algorithm, key, plainText);
			}
		}

		private void testGCM_CTS(string algorithm, string key, string text)
		{
			for (int m = 0; m < arrayTagsGCM.Length; m++)
			{
				for (int n = 0; n < arrayNonces.Length; n++)
				{
					for (int p = 0; p < arrayPaddings.Length; p++)
					{
						SymmetricBlockCipher symBlockCipher = new SymmetricBlockCipher();
						string encrypted = symBlockCipher.DoAEADEncrypt(algorithm, "AEAD_GCM", key, arrayTagsGCM[m],
								arrayNonces[n], text);
						string decrypted = symBlockCipher.DoAEADDecrypt(algorithm, "AEAD_GCM", key, arrayTagsGCM[m],
								arrayNonces[n], encrypted);
						Assert.IsTrue(SecurityUtils.compareStrings(text, decrypted));
						True(SecurityUtils.compareStrings(text, decrypted), symBlockCipher);
					}
				}
			}
		}
	}
}
