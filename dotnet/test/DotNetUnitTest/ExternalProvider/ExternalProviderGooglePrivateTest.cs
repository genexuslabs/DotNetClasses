using GeneXus.Storage.GXAmazonS3;
using GeneXus.Storage.GXAzureStorage;
using GeneXus.Storage.GXGoogleCloud;
using UnitTesting;

namespace DotNetUnitTest
{
	public class ExternalProviderGooglePrivateTest : ExternalProviderTest
	{
		public ExternalProviderGooglePrivateTest(): base(ExternalProviderGoogle.Name, typeof(ExternalProviderGoogle), true)
		{
		}

	}
}
