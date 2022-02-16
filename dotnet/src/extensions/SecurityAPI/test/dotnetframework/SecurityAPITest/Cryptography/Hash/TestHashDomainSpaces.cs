using GeneXusCryptography.Hash;
using NUnit.Framework;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Cryptography.Hash
{
	[TestFixture]
	public class TestHashDomainSpaces : SecurityAPITestObject
	{
		protected static string plainText;
		protected static string SHA1Digest = "38F00F8738E241DAEA6F37F6F55AE8414D7B0219";
		protected static Hashing hash;

		[SetUp]
		public virtual void SetUp()
		{
			SHA1Digest = "38F00F8738E241DAEA6F37F6F55AE8414D7B0219";
			hash = new Hashing();
			plainText = "Lorem ipsum dolor sit amet";
		}

		[Test]
		public void TestSpaces()
		{
			string digest = hash.DoHash(" SHA1", plainText);
			Assert.IsFalse(hash.HasError());
			Assert.IsTrue(SecurityUtils.compareStrings(digest, SHA1Digest));
		}
	}
}
