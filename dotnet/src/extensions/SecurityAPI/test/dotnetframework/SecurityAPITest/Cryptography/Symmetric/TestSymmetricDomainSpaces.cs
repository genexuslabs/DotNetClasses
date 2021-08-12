using GeneXusCryptography.Symmetric;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Cryptography.Symmetric
{
	[TestFixture]
	public class TestSymmetricDomainSpaces : SecurityAPITestObject
	{
		private static string plainText;
		private static SymmetricKeyGenerator keyGen;
		private static string key128;
		private static string IV128;
		private static SymmetricBlockCipher symBlockCipher;

		private static string plainTextStream;
		private static string key1024;
		private static SymmetricStreamCipher symStreamCipher;

		[SetUp]
		public virtual void SetUp()
		{
			plainText = "Neque porro quisquam est qui dolorem ipsum quia dolor sit amet";
			keyGen = new SymmetricKeyGenerator();

			key128 = keyGen.doGenerateKey(" GENERICRANDOM", 128);
			IV128 = keyGen.doGenerateIV("GENERICRANDOM ", 128);
			symBlockCipher = new SymmetricBlockCipher();

			plainTextStream = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam venenatis ex sit amet risus pellentesque, a faucibus quam ultrices. Ut tincidunt quam eu aliquam maximus. Quisque posuere risus at erat blandit eleifend. Curabitur viverra rutrum volutpat. Donec quis quam tellus. Aenean fermentum elementum augue, a semper risus scelerisque sit amet. Nullam vitae sapien vitae dui ullamcorper dapibus quis quis leo. Sed neque felis, pellentesque in risus et, lobortis ultricies nulla. Quisque quis quam risus. Donec vestibulum, lectus vel vestibulum eleifend, velit ante volutpat lacus, ut mattis quam ligula eget est. Sed et pulvinar lectus. In mollis turpis non ipsum vehicula, sit amet rutrum nibh dictum. Duis consectetur convallis ex, eu ultricies enim bibendum vel. Vestibulum vel libero nibh. Morbi nec odio mattis, vestibulum quam blandit, pretium orci.Aenean pellentesque tincidunt nunc a malesuada. Etiam gravida fermentum mi, at dignissim dui aliquam quis. Nullam vel lobortis libero. Phasellus non gravida posuere";
			key1024 = keyGen.doGenerateKey("GENERICRANDOM ", 1024);
			symStreamCipher = new SymmetricStreamCipher();

		}

		[Test]
		public void TestBlockDomains()
		{
			string encrypted = symBlockCipher.DoEncrypt("AES ", "CBC ", "ZEROBYTEPADDING ", key128, IV128, plainText);
			Assert.IsFalse(symBlockCipher.HasError());
			string decrypted = symBlockCipher.DoDecrypt(" AES", " CBC", " ZEROBYTEPADDING", key128, IV128, encrypted);
			Assert.IsFalse(symBlockCipher.HasError());
			Assert.IsTrue(SecurityUtils.compareStrings(decrypted, plainText));
		}

		[Test]
		public void TestStreamDomains()
		{
			string encrypted = symStreamCipher.DoEncrypt("RC4 ", key1024, "", plainTextStream);
			Assert.IsFalse(symStreamCipher.HasError());
			string decrypted = symStreamCipher.DoDecrypt(" RC4", key1024, "", encrypted);
			Assert.IsFalse(symStreamCipher.HasError());
			Assert.IsTrue(SecurityUtils.compareStrings(decrypted, plainTextStream));
		}
	}
}
