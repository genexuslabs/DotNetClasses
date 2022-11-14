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
		const string DOTNET_ENVIRONMENT = "Development";

		protected TestServer server;
		public MiddlewareTest()
		{
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();
			server = new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>().UseEnvironment(DOTNET_ENVIRONMENT));
			server.CreateClient();
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
