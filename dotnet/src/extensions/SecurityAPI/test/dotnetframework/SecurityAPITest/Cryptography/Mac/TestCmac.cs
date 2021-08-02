using System;
using GeneXusCryptography.Mac;
using NUnit.Framework;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Cryptography.Mac
{
    [TestFixture]
    public class TestCmac: SecurityAPITestObject
    {
		private static string plainText;
		private static string plainTextCTS;

		protected static string key1024;
		protected static string key512;
		protected static string key448;
		protected static string key256;
		protected static string key192;
		protected static string key160;
		protected static string key128;
		protected static string key64;

		protected static string IV128;
		protected static string IV64;

		private static SymmetricKeyGenerator keyGen;

		private static string[] encodings;
		private static EncodingUtil eu;

		[SetUp]
		public virtual void SetUp()
		{
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
			IV128 = keyGen.doGenerateIV("GENERICRANDOM", 128);
			IV64 = keyGen.doGenerateIV("GENERICRANDOM", 64);


			encodings = new string[] { "UTF_8", "UTF_16", "UTF_16BE", "UTF_16LE", "UTF_32", "UTF_32BE", "UTF_32LE", "SJIS", "GB2312" };
			eu = new EncodingUtil();

		}

		[Test]
		public void TestAES()
		{
			// key legths 128,192 & 256
			// blocksize 128

			testBulkAlgorithm("AES", key128, IV128, 128, false);
			testBulkAlgorithm("AES", key128, IV128, 0, false);
			testBulkAlgorithm("AES", key128, IV128, 64, false);

			testBulkAlgorithm("AES", key128, "", 128, false);
			testBulkAlgorithm("AES", key128, "", 0, false);
			testBulkAlgorithm("AES", key128, "", 64, false);

			testBulkAlgorithm("AES", key192, IV128, 128, false);
			testBulkAlgorithm("AES", key192, IV128, 0, false);
			testBulkAlgorithm("AES", key192, IV128, 64, false);

			testBulkAlgorithm("AES", key192, "", 128, false);
			testBulkAlgorithm("AES", key192, "", 0, false);
			testBulkAlgorithm("AES", key192, "", 64, false);

			testBulkAlgorithm("AES", key256, IV128, 128, false);
			testBulkAlgorithm("AES", key256, IV128, 0, false);
			testBulkAlgorithm("AES", key256, IV128, 64, false);

			testBulkAlgorithm("AES", key256, "", 128, false);
			testBulkAlgorithm("AES", key256, "", 0, false);
			testBulkAlgorithm("AES", key256, "", 64, false);



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

			testBulkAlgorithm("BLOWFISH", key128, IV64, 64, false);
			testBulkAlgorithm("BLOWFISH", key128, IV64, 0, false);

			testBulkAlgorithm("BLOWFISH", key128, "", 64, false);
			testBulkAlgorithm("BLOWFISH", key128, "", 0, false);

			testBulkAlgorithm("BLOWFISH", key192, IV64, 64, false);
			testBulkAlgorithm("BLOWFISH", key192, IV64, 0, false);

			testBulkAlgorithm("BLOWFISH", key192, "", 64, false);
			testBulkAlgorithm("BLOWFISH", key192, "", 0, false);

			testBulkAlgorithm("BLOWFISH", key256, IV64, 64, false);
			testBulkAlgorithm("BLOWFISH", key256, IV64, 0, false);

			testBulkAlgorithm("BLOWFISH", key256, "", 64, false);
			testBulkAlgorithm("BLOWFISH", key256, "", 0, false);

		}

		[Test]
		public void TestCAMELLIA()
		{
			// key lengths 128.192.256
			// blocksize 128

			testBulkAlgorithm("CAMELLIA", key128, IV128, 128, false);
			testBulkAlgorithm("CAMELLIA", key128, IV128, 64, false);
			testBulkAlgorithm("CAMELLIA", key128, IV128, 0, false);

			testBulkAlgorithm("CAMELLIA", key128, "", 128, false);
			testBulkAlgorithm("CAMELLIA", key128, "", 64, false);
			testBulkAlgorithm("CAMELLIA", key128, "", 0, false);

			testBulkAlgorithm("CAMELLIA", key192, IV128, 128, false);
			testBulkAlgorithm("CAMELLIA", key192, IV128, 64, false);
			testBulkAlgorithm("CAMELLIA", key192, IV128, 0, false);

			testBulkAlgorithm("CAMELLIA", key192, "", 128, false);
			testBulkAlgorithm("CAMELLIA", key192, "", 64, false);
			testBulkAlgorithm("CAMELLIA", key192, "", 0, false);

			testBulkAlgorithm("CAMELLIA", key256, IV128, 128, false);
			testBulkAlgorithm("CAMELLIA", key256, IV128, 64, false);
			testBulkAlgorithm("CAMELLIA", key256, IV128, 0, false);

			testBulkAlgorithm("CAMELLIA", key256, "", 128, false);
			testBulkAlgorithm("CAMELLIA", key256, "", 64, false);
			testBulkAlgorithm("CAMELLIA", key256, "", 0, false);


		}

		[Test]
		public void TestCAST5()
		{
			// key length 0...128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("CAST5", key64, IV64, 64, false);
			testBulkAlgorithm("CAST5", key64, IV64, 0, false);

			testBulkAlgorithm("CAST5", key64, "", 64, false);
			testBulkAlgorithm("CAST5", key64, "", 0, false);

			testBulkAlgorithm("CAST5", key128, IV64, 64, false);
			testBulkAlgorithm("CAST5", key128, IV64, 0, false);

			testBulkAlgorithm("CAST5", key128, "", 64, false);
			testBulkAlgorithm("CAST5", key128, "", 0, false);


		}

		[Test]
		public void TestCAST6()
		{
			// key length 0...256
			// blocksize 128
			testBulkAlgorithm("CAST6", key64, IV128, 128, false);
			testBulkAlgorithm("CAST6", key64, IV128, 0, false);
			testBulkAlgorithm("CAST6", key64, IV128, 64, false);

			testBulkAlgorithm("CAST6", key64, "", 128, false);
			testBulkAlgorithm("CAST6", key64, "", 0, false);
			testBulkAlgorithm("CAST6", key64, "", 64, false);

			testBulkAlgorithm("CAST6", key128, IV128, 128, false);
			testBulkAlgorithm("CAST6", key128, IV128, 0, false);
			testBulkAlgorithm("CAST6", key128, IV128, 64, false);

			testBulkAlgorithm("CAST6", key128, "", 128, false);
			testBulkAlgorithm("CAST6", key128, "", 0, false);
			testBulkAlgorithm("CAST6", key128, "", 64, false);

			testBulkAlgorithm("CAST6", key192, IV128, 128, false);
			testBulkAlgorithm("CAST6", key192, IV128, 0, false);
			testBulkAlgorithm("CAST6", key192, IV128, 64, false);

			testBulkAlgorithm("CAST6", key192, "", 128, false);
			testBulkAlgorithm("CAST6", key192, "", 0, false);
			testBulkAlgorithm("CAST6", key192, "", 64, false);

			testBulkAlgorithm("CAST6", key256, IV128, 128, false);
			testBulkAlgorithm("CAST6", key256, IV128, 0, false);
			testBulkAlgorithm("CAST6", key256, IV128, 64, false);

			testBulkAlgorithm("CAST6", key256, "", 128, false);
			testBulkAlgorithm("CAST6", key256, "", 0, false);
			testBulkAlgorithm("CAST6", key256, "", 64, false);


		}

		[Test]
		public void TestDES()
		{
			// key length 64
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX no se puede usar (keylength != 128, 192 o 256

			testBulkAlgorithm("DES", key64, IV64, 64, false);
			testBulkAlgorithm("DES", key64, IV64, 0, false);

			testBulkAlgorithm("DES", key64, "", 64, false);
			testBulkAlgorithm("DES", key64, "", 0, false);

		}

		[Test]
		public void TestTRIPLEDES()
		{
			// key length 128.192
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("TRIPLEDES", key128, IV64, 64, false);
			testBulkAlgorithm("TRIPLEDES", key128, IV64, 0, false);

			testBulkAlgorithm("TRIPLEDES", key128, "", 64, false);
			testBulkAlgorithm("TRIPLEDES", key128, "", 0, false);

			testBulkAlgorithm("TRIPLEDES", key192, IV64, 64, false);
			testBulkAlgorithm("TRIPLEDES", key192, IV64, 0, false);

			testBulkAlgorithm("TRIPLEDES", key192, "", 64, false);
			testBulkAlgorithm("TRIPLEDES", key192, "", 0, false);

		}

		[Test]
		public void TestDSTU7624()
		{
			// key length 128, 256, 512
			// blocksize 128.256.512
			// input should be as lenght as the block
			testBulkAlgorithm("DSTU7624_128", key128, IV128, 128, true);
			testBulkAlgorithm("DSTU7624_128", key128, IV128, 64, true);
			testBulkAlgorithm("DSTU7624_128", key128, IV128, 0, true);

			testBulkAlgorithm("DSTU7624_128", key128, "", 128, true);
			testBulkAlgorithm("DSTU7624_128", key128, "", 64, true);
			testBulkAlgorithm("DSTU7624_128", key128, "", 0, true);


		}

		[Test]
		public void TestGOST28147()
		{
			// key length 256
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("GOST28147", key256, IV64, 64, false);
			testBulkAlgorithm("GOST28147", key256, IV64, 0, false);

			testBulkAlgorithm("GOST28147", key256, "", 64, false);
			testBulkAlgorithm("GOST28147", key256, "", 0, false);

		}

		[Test]
		public void TestNOEKEON()
		{
			// key length 128
			// blocksize 128
			testBulkAlgorithm("NOEKEON", key128, IV128, 128, false);
			testBulkAlgorithm("NOEKEON", key128, IV128, 64, false);
			testBulkAlgorithm("NOEKEON", key128, IV128, 0, false);


			testBulkAlgorithm("NOEKEON", key128, "", 128, false);
			testBulkAlgorithm("NOEKEON", key128, "", 64, false);
			testBulkAlgorithm("NOEKEON", key128, "", 0, false);
		}

		[Test]
		public void TestRC2()
		{
			// key length 0...1024
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("RC2", key64, IV64, 64, false);
			testBulkAlgorithm("RC2", key64, IV64, 0, false);

			testBulkAlgorithm("RC2", key64, "", 64, false);
			testBulkAlgorithm("RC2", key64, "", 0, false);

			testBulkAlgorithm("RC2", key128, IV64, 64, false);
			testBulkAlgorithm("RC2", key128, IV64, 0, false);

			testBulkAlgorithm("RC2", key128, "", 64, false);
			testBulkAlgorithm("RC2", key128, "", 0, false);

			testBulkAlgorithm("RC2", key192, IV64, 64, false);
			testBulkAlgorithm("RC2", key192, IV64, 0, false);

			testBulkAlgorithm("RC2", key192, "", 64, false);
			testBulkAlgorithm("RC2", key192, "", 0, false);

			testBulkAlgorithm("RC2", key256, IV64, 64, false);
			testBulkAlgorithm("RC2", key256, IV64, 0, false);

			testBulkAlgorithm("RC2", key256, "", 64, false);
			testBulkAlgorithm("RC2", key256, "", 0, false);

			testBulkAlgorithm("RC2", key448, IV64, 64, false);
			testBulkAlgorithm("RC2", key448, IV64, 0, false);

			testBulkAlgorithm("RC2", key448, "", 64, false);
			testBulkAlgorithm("RC2", key448, "", 0, false);

			testBulkAlgorithm("RC2", key512, IV64, 64, false);
			testBulkAlgorithm("RC2", key512, IV64, 0, false);

			testBulkAlgorithm("RC2", key512, "", 64, false);
			testBulkAlgorithm("RC2", key512, "", 0, false);

			testBulkAlgorithm("RC2", key1024, IV64, 64, false);
			testBulkAlgorithm("RC2", key1024, IV64, 0, false);

			testBulkAlgorithm("RC2", key1024, "", 64, false);
			testBulkAlgorithm("RC2", key1024, "", 0, false);


		}

		[Test]
		public void TestRC532()
		{
			// key length 0...128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("RC532", key64, IV64, 64, false);
			testBulkAlgorithm("RC532", key64, IV64, 0, false);

			testBulkAlgorithm("RC532", key64, "", 64, false);
			testBulkAlgorithm("RC532", key64, "", 0, false);

			testBulkAlgorithm("RC532", key128, IV64, 64, false);
			testBulkAlgorithm("RC532", key128, IV64, 0, false);

			testBulkAlgorithm("RC532", key128, "", 64, false);
			testBulkAlgorithm("RC532", key128, "", 0, false);
		}

		[Test]
		public void TestRC6()
		{
			// key length 0...256
			// blocksize 128
			testBulkAlgorithm("RC6", key64, IV128, 128, false);
			testBulkAlgorithm("RC6", key64, IV128, 64, false);
			testBulkAlgorithm("RC6", key64, IV128, 0, false);

			testBulkAlgorithm("RC6", key64, "", 128, false);
			testBulkAlgorithm("RC6", key64, "", 64, false);
			testBulkAlgorithm("RC6", key64, "", 0, false);

			testBulkAlgorithm("RC6", key128, IV128, 128, false);
			testBulkAlgorithm("RC6", key128, IV128, 64, false);
			testBulkAlgorithm("RC6", key128, IV128, 0, false);

			testBulkAlgorithm("RC6", key128, "", 128, false);
			testBulkAlgorithm("RC6", key128, "", 64, false);
			testBulkAlgorithm("RC6", key128, "", 0, false);

			testBulkAlgorithm("RC6", key192, IV128, 128, false);
			testBulkAlgorithm("RC6", key192, IV128, 64, false);
			testBulkAlgorithm("RC6", key192, IV128, 0, false);

			testBulkAlgorithm("RC6", key192, "", 128, false);
			testBulkAlgorithm("RC6", key192, "", 64, false);
			testBulkAlgorithm("RC6", key192, "", 0, false);

			testBulkAlgorithm("RC6", key256, IV128, 128, false);
			testBulkAlgorithm("RC6", key256, IV128, 64, false);
			testBulkAlgorithm("RC6", key256, IV128, 0, false);

			testBulkAlgorithm("RC6", key256, "", 128, false);
			testBulkAlgorithm("RC6", key256, "", 64, false);
			testBulkAlgorithm("RC6", key256, "", 0, false);

		}

		[Test]
		public void TestRIJNDAEL()
		{
			// key length 128.160.224.256
			// blocksize 128, 160, 192, 224, 256

			testBulkAlgorithm("RIJNDAEL_128", key128, IV128, 128, false);
			testBulkAlgorithm("RIJNDAEL_128", key128, IV128, 64, false);
			testBulkAlgorithm("RIJNDAEL_128", key128, IV128, 0, false);

			testBulkAlgorithm("RIJNDAEL_128", key128, "", 128, false);
			testBulkAlgorithm("RIJNDAEL_128", key128, "", 64, false);
			testBulkAlgorithm("RIJNDAEL_128", key128, "", 0, false);

			testBulkAlgorithm("RIJNDAEL_128", key160, IV128, 128, false);
			testBulkAlgorithm("RIJNDAEL_128", key160, IV128, 64, false);
			testBulkAlgorithm("RIJNDAEL_128", key160, IV128, 0, false);

			testBulkAlgorithm("RIJNDAEL_128", key160, "", 128, false);
			testBulkAlgorithm("RIJNDAEL_128", key160, "", 64, false);
			testBulkAlgorithm("RIJNDAEL_128", key160, "", 0, false);

			testBulkAlgorithm("RIJNDAEL_128", key192, IV128, 128, false);
			testBulkAlgorithm("RIJNDAEL_128", key192, IV128, 64, false);
			testBulkAlgorithm("RIJNDAEL_128", key192, IV128, 0, false);

			testBulkAlgorithm("RIJNDAEL_128", key192, "", 128, false);
			testBulkAlgorithm("RIJNDAEL_128", key192, "", 64, false);
			testBulkAlgorithm("RIJNDAEL_128", key192, "", 0, false);

			testBulkAlgorithm("RIJNDAEL_128", key256, IV128, 128, false);
			testBulkAlgorithm("RIJNDAEL_128", key256, IV128, 64, false);
			testBulkAlgorithm("RIJNDAEL_128", key256, IV128, 0, false);

			testBulkAlgorithm("RIJNDAEL_128", key256, "", 128, false);
			testBulkAlgorithm("RIJNDAEL_128", key256, "", 64, false);
			testBulkAlgorithm("RIJNDAEL_128", key256, "", 0, false);


		}

		[Test]
		public void TestSEED()
		{
			// key length 128
			// blocksize 128


			testBulkAlgorithm("SEED", key128, IV128, 128, false);
			testBulkAlgorithm("SEED", key128, IV128, 0, false);
			testBulkAlgorithm("SEED", key128, IV128, 64, false);

			testBulkAlgorithm("SEED", key128, "", 0, false);
			testBulkAlgorithm("SEED", key128, "", 128, false);
			testBulkAlgorithm("SEED", key128, "", 64, false);

		}

		[Test]
		public void TestSERPENT()
		{
			// key length 128.192.256
			// blocksize 128

			testBulkAlgorithm("SERPENT", key128, IV128, 128, false);
			testBulkAlgorithm("SERPENT", key128, IV128, 0, false);
			testBulkAlgorithm("SERPENT", key128, IV128, 64, false);

			testBulkAlgorithm("SERPENT", key128, "", 128, false);
			testBulkAlgorithm("SERPENT", key128, "", 0, false);
			testBulkAlgorithm("SERPENT", key128, "", 64, false);

			testBulkAlgorithm("SERPENT", key192, IV128, 128, false);
			testBulkAlgorithm("SERPENT", key192, IV128, 0, false);
			testBulkAlgorithm("SERPENT", key192, IV128, 64, false);

			testBulkAlgorithm("SERPENT", key192, "", 128, false);
			testBulkAlgorithm("SERPENT", key192, "", 0, false);
			testBulkAlgorithm("SERPENT", key192, "", 64, false);

			testBulkAlgorithm("SERPENT", key256, IV128, 128, false);
			testBulkAlgorithm("SERPENT", key256, IV128, 0, false);
			testBulkAlgorithm("SERPENT", key256, IV128, 64, false);

			testBulkAlgorithm("SERPENT", key256, "", 128, false);
			testBulkAlgorithm("SERPENT", key256, "", 0, false);
			testBulkAlgorithm("SERPENT", key256, "", 64, false);

		}

		[Test]
		public void TestSKIPJACK()
		{
			// key length 128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("SKIPJACK", key128, IV64, 64, false);
			testBulkAlgorithm("SKIPJACK", key128, IV64, 0, false);

			testBulkAlgorithm("SKIPJACK", key128, "", 64, false);
			testBulkAlgorithm("SKIPJACK", key128, "", 0, false);

		}

		[Test]
		public void TestSM4()
		{
			// key length 128
			// blocksize 128
			testBulkAlgorithm("SM4", key128, IV128, 128, false);
			testBulkAlgorithm("SM4", key128, IV128, 0, false);
			testBulkAlgorithm("SM4", key128, IV128, 64, false);

			testBulkAlgorithm("SM4", key128, "", 128, false);
			testBulkAlgorithm("SM4", key128, "", 0, false);
			testBulkAlgorithm("SM4", key128, "", 64, false);

		}

		[Test]
		public void TestTEA()
		{
			// key length 128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("TEA", key128, IV64, 64, false);
			testBulkAlgorithm("TEA", key128, IV64, 0, false);

			testBulkAlgorithm("TEA", key128, "", 64, false);
			testBulkAlgorithm("TEA", key128, "", 0, false);

		}

		[Test]
		public void TestTWOFISH()
		{
			// key length 128.192.256
			// blocksize 128
			testBulkAlgorithm("TWOFISH", key128, IV128, 128, false);
			testBulkAlgorithm("TWOFISH", key128, IV128, 0, false);
			testBulkAlgorithm("TWOFISH", key128, IV128, 64, false);

			testBulkAlgorithm("TWOFISH", key128, "", 128, false);
			testBulkAlgorithm("TWOFISH", key128, "", 0, false);
			testBulkAlgorithm("TWOFISH", key128, "", 64, false);


			testBulkAlgorithm("TWOFISH", key192, IV128, 128, false);
			testBulkAlgorithm("TWOFISH", key192, IV128, 0, false);
			testBulkAlgorithm("TWOFISH", key192, IV128, 64, false);

			testBulkAlgorithm("TWOFISH", key192, "", 128, false);
			testBulkAlgorithm("TWOFISH", key192, "", 0, false);
			testBulkAlgorithm("TWOFISH", key192, "", 64, false);

			testBulkAlgorithm("TWOFISH", key256, IV128, 128, false);
			testBulkAlgorithm("TWOFISH", key256, IV128, 0, false);
			testBulkAlgorithm("TWOFISH", key256, IV128, 64, false);

			testBulkAlgorithm("TWOFISH", key256, "", 128, false);
			testBulkAlgorithm("TWOFISH", key256, "", 0, false);
			testBulkAlgorithm("TWOFISH", key256, "", 64, false);


		}

		[Test]
		public void TestXTEA()
		{
			// key length 128
			// blocksize 64
			// no se puede usar CCM (blosize!=128)
			// no se pude usar GCM (blocksize <128)
			// EAX siempre explota, no se puede usar con AEAD

			testBulkAlgorithm("XTEA", key128, IV64, 64, false);
			testBulkAlgorithm("XTEA", key128, IV64, 0, false);

			testBulkAlgorithm("XTEA", key128, "", 64, false);
			testBulkAlgorithm("XTEA", key128, "", 0, false);


		}


		private void testBulkAlgorithm(string algorithm, string key, string IV, int macSize, bool cts)
		{
			for (int i = 0; i < encodings.Length; i++)
			{
				eu.setEncoding(encodings[i]);
				if (cts)
				{
					testBulkAlgorithmCTS(algorithm, key, IV, macSize, plainTextCTS);
				}
				else
				{
					testBulkAlgorithmCTS(algorithm, key, IV, macSize, plainText);
				}
			}
		}

		private void testBulkAlgorithmCTS(string algorithm, string key, string IV, int macSize, string input)
		{

			Cmac mac = new Cmac();
			string res = mac.calculate(input, key, algorithm, macSize);
			bool verified = mac.verify(input, key, res, algorithm, macSize);
			//System.out.println("res: "+res);
			/*if(mac.hasError())
			{
				System.out.println("Eror. Code: "+mac.getErrorCode()+" Desc: "+mac.getErrorDescription());
			}else {
				System.out.println("no error");
			}*/
			Assert.IsTrue(verified);
			True(true, mac);
		}
	}
}
