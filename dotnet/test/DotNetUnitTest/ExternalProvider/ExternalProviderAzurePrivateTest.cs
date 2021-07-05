using GeneXus.Storage.GXAmazonS3;
using GeneXus.Storage.GXAzureStorage;
using UnitTesting;

namespace DotNetUnitTest
{
	public class ExternalProviderAzurePrivateTest : ExternalProviderTest
	{
		public ExternalProviderAzurePrivateTest(): base(AzureStorageExternalProvider.Name, typeof(AzureStorageExternalProvider), true)
		{
		}

	}
}
