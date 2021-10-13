using GeneXus.Storage.GXAmazonS3;
using UnitTesting;

namespace DotNetUnitTest
{
	public class ExternalProviderS3Test : ExternalProviderTest
	{
		public ExternalProviderS3Test(): base(ExternalProviderS3.Name, typeof(ExternalProviderS3))
		{
		}

	}
}
