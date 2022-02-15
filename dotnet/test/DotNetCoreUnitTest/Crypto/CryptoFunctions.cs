using GeneXus.Encryption;
using Xunit;

namespace xUnitTesting
{
	public class CryptoFunctions
	{
		const string TEST_KEY_128 = "7E2E22D26FF2989E2444852A85E57867";
		const string TEST_KEY_256 = "7E2E22D26FF2989E2444852A85E578677E2E22D26FF2989E2444852A85E57867";
		const string TEST_VALUE_ENCRYPTED_128 = "L3KpX01Y+7yriRShRiH2vQ==";
		const string TEST_VALUE_ENCRYPTED_256 = "Re6h3arFxc9kUeUF0rBd6A==";
		const string TEST_VALUE = "test";
		[Fact]
		public void TestEncrypt128()
		{
			string encryptedValue = CryptoImpl.Encrypt(TEST_VALUE, TEST_KEY_128);
			Assert.Equal(TEST_VALUE_ENCRYPTED_128, encryptedValue);

			string decryptedValue = CryptoImpl.Decrypt(encryptedValue, TEST_KEY_128, false);
			Assert.Equal(TEST_VALUE, decryptedValue);
		}
		[Fact]
		public void TestEncrypt256()
		{
			string encryptedValue = CryptoImpl.Encrypt(TEST_VALUE, TEST_KEY_256);
			Assert.Equal(TEST_VALUE_ENCRYPTED_256, encryptedValue);

			string decryptedValue = CryptoImpl.Decrypt(encryptedValue, TEST_KEY_256, false);
			Assert.Equal(TEST_VALUE, decryptedValue);
		}
	}
}
