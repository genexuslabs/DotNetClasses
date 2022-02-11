using GeneXus.Storage.GXAmazonS3;
using UnitTesting;

namespace DotNetUnitTest
{
	public class ExternalProviderMinioTest : ExternalProviderTest
	{
		public ExternalProviderMinioTest(): base(ExternalProviderS3.Name, typeof(ExternalProviderS3))
		{
		}

	}
}
