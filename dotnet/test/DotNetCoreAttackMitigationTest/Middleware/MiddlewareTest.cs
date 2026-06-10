using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using GeneXus.Application;
using GxClasses.Web.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace xUnitTesting
{
	public class MiddlewareTest
	{
		protected TestServer server;

		public MiddlewareTest()
		{
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();

			var hostBuilder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
						.UseContentRoot(GXRouting.ContentRootPath)
						.UseStartup<Startup>()
						.UseTestServer();
				});

			var host = hostBuilder.Start();

			server = host.GetTestServer();
			server.PreserveExecutionContext = true;
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
