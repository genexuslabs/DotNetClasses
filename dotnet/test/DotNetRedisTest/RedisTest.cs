using GeneXus.Services;
using Xunit;

namespace xUnitTesting
{
	public class RedisTest 
	{
		[Fact]
		public void TestRedisFromCloudServicesDevConfig()
		{
			ISessionService session = GXSessionServiceFactory.GetProvider();
			Assert.NotNull(session);
			Assert.True(session is GxRedisSession);
		}
	}
}
