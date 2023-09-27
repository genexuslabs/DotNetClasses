using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Metadata;
using Microsoft.Extensions.Logging;
using Xunit;
using xUnitTesting;

namespace DotNetCoreWebUnitTest
{
	public class TestOpenTelemetry : MiddlewareTest
	{
		public TestOpenTelemetry() {
			ClassLoader.FindType("TestApp", "GeneXus.Programs.apps", "testservice", Assembly.GetExecutingAssembly(), true);//Force loading assembly for testservice
			server.AllowSynchronousIO = true;

		}
		[Fact]
		public async Task TestCouldNotLoadSystemDiagnosticsDiagnosticSource7_0_0_0()
		{
			Environment.SetEnvironmentVariable(Startup.APPLICATIONINSIGHTS_CONNECTION_STRING, "DummyConnectionString", EnvironmentVariableTarget.Process);
			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();
			HttpResponseMessage response = await client.PostAsync("rest/apps/testservice", null);
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
			string responseBody = await response.Content.ReadAsStringAsync();
			Assert.Equal("{}", responseBody);
		}

	}
}
