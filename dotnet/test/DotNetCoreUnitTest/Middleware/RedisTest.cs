using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Services;
using Xunit;

namespace xUnitTesting
{
	public class RedisTest 
	{
		[Fact]
		public void TestRedisFromCloudServicesDevConfig()
		{
			GXServices services=null;
			GXServices.LoadFromFile("CloudServices.test.config",ref services);
			GXServices.Instance = services;
			ISessionService session = GXSessionServiceFactory.GetProvider();
			Assert.NotNull(session);
			Assert.True(session is GxRedisSession);
		}
	}
}
