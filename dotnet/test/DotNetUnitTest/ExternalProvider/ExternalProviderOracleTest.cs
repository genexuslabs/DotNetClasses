using GeneXus.Storage.GXAmazonS3;
using UnitTesting;

namespace DotNetUnitTest
{
	public class ExternalProviderOracleTest : ExternalProviderTest
	{
		public ExternalProviderOracleTest(): base("ORACLE", typeof(ExternalProviderS3))
		{
		}

	}
}
