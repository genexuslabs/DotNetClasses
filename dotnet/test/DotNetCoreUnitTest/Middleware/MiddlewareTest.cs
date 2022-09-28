using System.IO;
using GeneXus.Application;
using GxClasses.Web.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace xUnitTesting
{
	[Collection("Non-Parallel Collection")]

	public class MiddlewareTest
	{
		protected TestServer server;
		public MiddlewareTest()
		{
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();
			server = new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>());
		}

	}
}
