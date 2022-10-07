using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using GeneXus.Application;
using GxClasses.Web.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace xUnitTesting
{
	public class MiddlewareTest
	{
		protected TestServer server;
		public MiddlewareTest()
		{
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();
			server = new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>());
		}
		public MiddlewareTest(string environment)
		{
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();
			server = new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>().UseEnvironment(environment));
		}
		protected string GetHeader(HttpResponseMessage response, string headerName)
		{
			if (response.Headers.TryGetValues(headerName, out IEnumerable<string> value))
				return value.First();
			else
				return string.Empty;

		}

	}
}
