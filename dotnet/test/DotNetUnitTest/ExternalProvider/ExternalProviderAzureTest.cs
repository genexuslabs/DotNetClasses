using GeneXus.Storage.GXAmazonS3;
using GeneXus.Storage.GXAzureStorage;
using UnitTesting;

namespace DotNetUnitTest
{
	public class ExternalProviderAzureTest : ExternalProviderTest
	{
		public ExternalProviderAzureTest(): base(AzureStorageExternalProvider.Name, typeof(AzureStorageExternalProvider))
		{
		}

	}
}
