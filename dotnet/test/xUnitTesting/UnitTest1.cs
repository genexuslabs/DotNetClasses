using Xunit;
using GeneXus.Application;
using System.Collections.Generic;
using System.Linq;

namespace xUnitTesting
{
    public class UnitTest1
    {
        [Fact]
		public void TestProcRestInModule()
        {
			string path = "module1/module2/aprocrest";
			List<ControllerInfo> cList = RouteController(path);
			Assert.Equal(2, cList.Count);
			Assert.Equal(path, cList.First().Name);
        }
		[Fact]
		public void TestProcRestInModuleWithParms()
		{
			string path = "module1/module2/invoice/5";
			List<ControllerInfo> cList = RouteController(path);
			Assert.Equal(2, cList.Count);
			Assert.Equal("module1/module2/invoice", cList[1].Name);
			Assert.Equal("5", cList[1].Parameters);
		}
		[Fact]
		public void TestProcRestInModuleWithParms2()
		{
			string path = "module1/module2/aprocrest?1,3,test";
			List<ControllerInfo> cList = RouteController(path);
			Assert.Single(cList);
			ControllerInfo c = cList.First();
			Assert.Equal("module1/module2/aprocrest", c.Name);
			Assert.Equal("1,3,test", c.Parameters);
		}
		[Fact]
		public void TestProcRest()
		{
			string path = "aprocrest";
			List<ControllerInfo> cList = RouteController(path);
			Assert.Single(cList);
			ControllerInfo c = cList.First();
			Assert.Equal("aprocrest", c.Name);
			Assert.Equal(string.Empty, c.Parameters);
		}
		[Fact]
		public void TestProcRest1()
		{
			string path = "aprocrest/";
			List<ControllerInfo> cList = RouteController(path);
			Assert.Single(cList);
			Assert.Equal("aprocrest", cList.First().Name);
			Assert.Equal(string.Empty, cList.First().Parameters);
		}
		[Fact]
		public void TestProcRestWithParms()
		{
			string path = "aprocrest/?1,3";
			List<ControllerInfo> cList = RouteController(path);
			Assert.Equal("aprocrest", cList.First().Name);
			Assert.Equal("1,3", cList.First().Parameters);
		}
		[Fact]
		public void TestProcRestWithMarkButNoParms()
		{
			string path = "aprocrest/?";
			List<ControllerInfo> cList = RouteController(path);
			Assert.Single(cList);
			Assert.Equal("aprocrest", cList.First().Name);
			Assert.Equal("", cList.First().Parameters);
		}
		[Fact]
		public void TestProcRestWithMarkButNoParms1()
		{
			string path = "aprocrest?";
			string controller = string.Empty;
			string parms = string.Empty;
			List<ControllerInfo> cList = RouteController(path);
			Assert.Single(cList);
			Assert.Equal("aprocrest", cList.First().Name);
			Assert.Equal("", cList.First().Parameters);
		}

		private List<ControllerInfo> RouteController(string path)
		{
			Dictionary<String, String> servicesPathUrl = new Dictionary<String, String>();
			Dictionary<String, Dictionary<String, String>> servicesMap = new Dictionary<String, Dictionary<string, string>>();
			return Startup.GetRouteController(servicesPathUrl,servicesMap, "",path);
		}
	}
}
