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
			SetEnvironmentVars();
			BeforeStartup();
			server = new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>().UseEnvironment(DOTNET_ENVIRONMENT));
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();
			server.PreserveExecutionContext= true;
			server.CreateClient();
		}
		protected virtual void SetEnvironmentVars()
		{

		}
		protected virtual void BeforeStartup()
		{

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
