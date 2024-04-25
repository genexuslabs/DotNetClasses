using System.IO;
using GeneXus.Http.HttpModules;
using Xunit;

namespace xUnitTesting
{
	public class RoutingTest
    {
       
		[Fact]
		public void TestLoadGRPC()
		{
			GXAPIModule gxAPIModule = new GXAPIModule();
			gxAPIModule.ServicesGroupSetting(Path.Combine(Directory.GetCurrentDirectory()));
			Assert.NotEmpty(GXAPIModule.servicesMap);
		}
	}
}
