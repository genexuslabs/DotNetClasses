using GeneXus.Storage.GXAmazonS3;
using GeneXus.Storage.GXAzureStorage;
using GeneXus.Storage.GXGoogleCloud;
using UnitTesting;

namespace DotNetUnitTest
{
	public class ExternalProviderGoogleTest : ExternalProviderTest
	{
		public ExternalProviderGoogleTest(): base(ExternalProviderGoogle.Name, typeof(ExternalProviderGoogle))
		{
		}

	}
}
