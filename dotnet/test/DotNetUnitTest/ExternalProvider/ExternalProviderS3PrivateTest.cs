using GeneXus.Storage.GXAmazonS3;
using UnitTesting;

namespace DotNetUnitTest
{
	public class ExternalProviderS3PrivateTest : ExternalProviderTest
	{
		public ExternalProviderS3PrivateTest(): base(ExternalProviderS3.Name, typeof(ExternalProviderS3), true)
		{
		}

	}
}
