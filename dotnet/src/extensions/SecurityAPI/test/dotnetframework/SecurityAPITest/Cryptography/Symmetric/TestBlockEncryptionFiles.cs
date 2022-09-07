using System;
using System.IO;
using GeneXusCryptography.Checksum;
using GeneXusCryptography.Symmetric;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Cryptography.Symmetric
{
	[TestFixture]
	public class TestBlockEncryptionFiles : SecurityAPITestObject
	{

		protected static string key256;
		protected static string key192;
		protected static string key160;
		protected static string key128;
		protected static string key64;

		protected static string IV256;
		protected static string IV224;
		protected static string IV192;
		protected static string IV160;
		protected static string IV128;
		protected static string IV64;

		private static SymmetricKeyGenerator keyGen;

		protected static string pathInput;
		protected static string pathOutputEncrypted;
		protected static string pathOutput;

		[SetUp]
		protected virtual void SetUp()
		{
			pathInput = Path.Combine(BASE_PATH, "Temp", "flag.jpg");
			pathOutputEncrypted = Path.Combine(BASE_PATH, "Temp", "flagEncrypted");
			pathOutput = Path.Combine(BASE_PATH, "Temp", "flagOut.jpg");

			keyGen = new SymmetricKeyGenerator();

			/**** CREATE KEYS ****/
			key256 = keyGen.doGenerateKey("GENERICRANDOM", 256);
			key160 = keyGen.doGenerateKey("GENERICRANDOM", 160);
			key192 = keyGen.doGenerateKey("GENERICRANDOM", 192);
			key128 = keyGen.doGenerateKey("GENERICRANDOM", 128);
			key64 = keyGen.doGenerateKey("GENERICRANDOM", 64);

			/**** CREATE IVs ****/
			IV256 = keyGen.doGenerateIV("GENERICRANDOM", 256);
			IV224 = keyGen.doGenerateIV("GENERICRANDOM", 224);
			IV192 = keyGen.doGenerateIV("GENERICRANDOM", 192);
			IV160 = keyGen.doGenerateIV("GENERICRANDOM", 160);
			IV128 = keyGen.doGenerateIV("GENERICRANDOM", 128);
			IV64 = keyGen.doGenerateIV("GENERICRANDOM", 64);
		}

		[Test]
		public void TestAES()
		{
			TestBulkFiles("AES", "CBC", "ZEROBYTEPADDING", key128, IV128);
			TestBulkFiles("AES", "CBC", "ZEROBYTEPADDING", key192, IV128);
			TestBulkFiles("AES", "CBC", "ZEROBYTEPADDING", key256, IV128);

			TestBulkFiles("AES", "ECB", "ZEROBYTEPADDING", key128, IV128);
			TestBulkFiles("AES", "ECB", "ZEROBYTEPADDING", key192, IV128);
			TestBulkFiles("AES", "ECB", "ZEROBYTEPADDING", key256, IV128);

			TestBulkFiles("AES", "CBC", "PKCS7PADDING", key128, IV128);
			TestBulkFiles("AES", "CBC", "PKCS7PADDING", key192, IV128);
			TestBulkFiles("AES", "CBC", "PKCS7PADDING", key256, IV128);

			TestBulkFiles("AES", "CBC", "X923PADDING", key128, IV128);
			TestBulkFiles("AES", "CBC", "X923PADDING", key192, IV128);
			TestBulkFiles("AES", "CBC", "X923PADDING", key256, IV128);

			TestBulkFiles("AES", "CBC", "ISO7816D4PADDING", key128, IV128);
			TestBulkFiles("AES", "CBC", "ISO7816D4PADDING", key192, IV128);
			TestBulkFiles("AES", "CBC", "ISO7816D4PADDING", key256, IV128);

			TestBulkGCM("AES", key128, IV128);
			TestBulkGCM("AES", key192, IV128);
			TestBulkGCM("AES", key256, IV128);
		}

		[Test]
		public void TestDES()
		{
			TestBulkFiles("DES", "CBC", "ZEROBYTEPADDING", key64, IV64);
		}

		[Test]
		public void TestTRIPLEDES()
		{
			TestBulkFiles("TRIPLEDES", "CBC", "ZEROBYTEPADDING", key128, IV64);
			TestBulkFiles("TRIPLEDES", "CBC", "ZEROBYTEPADDING", key192, IV64);
		}

		[Test]
		public void TestRIJNDAEL()
		{
			TestBulkFiles("RIJNDAEL_128", "CBC", "ZEROBYTEPADDING", key128, IV128);
			TestBulkFiles("RIJNDAEL_128", "CBC", "ZEROBYTEPADDING", key256, IV128);
			TestBulkFiles("RIJNDAEL_256", "CBC", "ZEROBYTEPADDING", key128, IV256);
			TestBulkFiles("RIJNDAEL_256", "CBC", "ZEROBYTEPADDING", key256, IV256);

			TestBulkFiles("RIJNDAEL_128", "ECB", "ZEROBYTEPADDING", key128, IV128);
			TestBulkFiles("RIJNDAEL_128", "ECB", "ZEROBYTEPADDING", key256, IV128);
			TestBulkFiles("RIJNDAEL_256", "ECB", "ZEROBYTEPADDING", key128, IV256);
			TestBulkFiles("RIJNDAEL_256", "ECB", "ZEROBYTEPADDING", key256, IV256);

			TestBulkFiles("RIJNDAEL_128", "CBC", "PKCS7PADDING", key128, IV128);
			TestBulkFiles("RIJNDAEL_128", "CBC", "PKCS7PADDING", key256, IV128);
			TestBulkFiles("RIJNDAEL_256", "CBC", "PKCS7PADDING", key128, IV256);
			TestBulkFiles("RIJNDAEL_256", "CBC", "PKCS7PADDING", key256, IV256);

			TestBulkFiles("RIJNDAEL_128", "CBC", "X923PADDING", key128, IV128);
			TestBulkFiles("RIJNDAEL_128", "CBC", "X923PADDING", key256, IV128);
			TestBulkFiles("RIJNDAEL_256", "CBC", "X923PADDING", key128, IV256);
			TestBulkFiles("RIJNDAEL_256", "CBC", "X923PADDING", key256, IV256);

			TestBulkFiles("RIJNDAEL_128", "CBC", "ISO7816D4PADDING", key128, IV128);
			TestBulkFiles("RIJNDAEL_128", "CBC", "ISO7816D4PADDING", key256, IV128);
			TestBulkFiles("RIJNDAEL_256", "CBC", "ISO7816D4PADDING", key128, IV256);
			TestBulkFiles("RIJNDAEL_256", "CBC", "ISO7816D4PADDING", key256, IV256);

			TestBulkGCM("RIJNDAEL_128", key128, IV128);
			TestBulkGCM("RIJNDAEL_128", key256, IV128);
		}

		[Test]
		public void TestTWOFISH()
		{
			TestBulkFiles("TWOFISH", "CBC", "ZEROBYTEPADDING", key128, IV128);
			TestBulkFiles("TWOFISH", "CBC", "ZEROBYTEPADDING", key192, IV128);
			TestBulkFiles("TWOFISH", "CBC", "ZEROBYTEPADDING", key256, IV128);


			TestBulkFiles("TWOFISH", "ECB", "ZEROBYTEPADDING", key128, IV128);
			TestBulkFiles("TWOFISH", "ECB", "ZEROBYTEPADDING", key192, IV128);
			TestBulkFiles("TWOFISH", "ECB", "ZEROBYTEPADDING", key256, IV128);

			TestBulkFiles("TWOFISH", "CBC", "PKCS7PADDING", key128, IV128);
			TestBulkFiles("TWOFISH", "CBC", "PKCS7PADDING", key192, IV128);
			TestBulkFiles("TWOFISH", "CBC", "PKCS7PADDING", key256, IV128);


			TestBulkFiles("TWOFISH", "CBC", "X923PADDING", key128, IV128);
			TestBulkFiles("TWOFISH", "CBC", "X923PADDING", key192, IV128);
			TestBulkFiles("TWOFISH", "CBC", "X923PADDING", key256, IV128);


			TestBulkFiles("TWOFISH", "CBC", "ISO7816D4PADDING", key128, IV128);
			TestBulkFiles("TWOFISH", "CBC", "ISO7816D4PADDING", key192, IV128);
			TestBulkFiles("TWOFISH", "CBC", "ISO7816D4PADDING", key256, IV128);

			TestBulkGCM("TWOFISH", key128, IV128);
			TestBulkGCM("TWOFISH", key192, IV128);
			TestBulkGCM("TWOFISH", key256, IV128);

		}

		private void TestBulkFiles(String algorithm, String mode, String padding, String key, String IV)
		{
			SymmetricBlockCipher cipher = new SymmetricBlockCipher();
			bool encrypts = cipher.DoEncryptFile(algorithm, mode, padding, key, IV, pathInput, pathOutputEncrypted);
			True(encrypts, cipher);
			bool decrypts = cipher.DoDecryptFile(algorithm, mode, padding, key, IV, pathOutputEncrypted, pathOutput);
			True(decrypts, cipher);
			ChecksumCreator check = new ChecksumCreator();
			String checksum = check.GenerateChecksum(pathInput, "LOCAL_FILE", "CRC8_DARC");
			bool checks = check.VerifyChecksum(pathOutput, "LOCAL_FILE", "CRC8_DARC", checksum);
			True(checks, check);
		}

		private void TestBulkGCM(String algorithm, String key, String nonce)
		{
			SymmetricBlockCipher cipher = new SymmetricBlockCipher();
			bool encrypts = cipher.DoAEADEncryptFile(algorithm, "AEAD_GCM", key, 128, nonce, pathInput, pathOutputEncrypted);
			True(encrypts, cipher);
			bool decrypts = cipher.DoAEADDecryptFile(algorithm, "AEAD_GCM", key, 128, nonce, pathOutputEncrypted, pathOutput);
			True(decrypts, cipher);
			ChecksumCreator check = new ChecksumCreator();
			string checksum = check.GenerateChecksum(pathInput, "LOCAL_FILE", "CRC8_DARC");
			bool checks = check.VerifyChecksum(pathOutput, "LOCAL_FILE", "CRC8_DARC", checksum);
			True(checks, check);
		}


	}
}
