using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using SecurityAPICommons.Encoders;

namespace SecurityAPITest.SecurityAPICommons.encoders
{
    [TestFixture]
    public class TestHexa: SecurityAPITestObject
    {
        protected static string expected_plainText;
        protected static string expected_hexaText;
        protected static HexaEncoder hexa;

        [SetUp]
        public virtual void SetUp()
        {
            expected_plainText = "hello world";
            expected_hexaText = "68656C6C6F20776F726C64";
            hexa = new HexaEncoder();
        }

        [Test]
        public void TestFromHexa()
        {
            string plainText = hexa.fromHexa(expected_hexaText);
            Equals(expected_plainText, plainText, hexa);
        }

        [Test]
        public void TestToHexa()
        {
            string hexaText = hexa.toHexa(expected_plainText);
            Equals(expected_hexaText, hexaText, hexa);
        }

		[Test]
		public void TestIsHexa()
		{
			bool isHexaTrue = hexa.isHexa(expected_hexaText);
			Assert.IsTrue(isHexaTrue);
			Assert.IsFalse(hexa.HasError());
			bool isHexaFalse = hexa.isHexa(expected_plainText);
			Assert.IsFalse(isHexaFalse);
			Assert.IsFalse(hexa.HasError());
			bool isHexaTrue_ = hexa.isHexa("68-65-6C-6C-6F-20-77-6F-72-6C-64");
			Assert.IsTrue(isHexaTrue_);
			Assert.IsFalse(hexa.HasError());
		}
	}
}
