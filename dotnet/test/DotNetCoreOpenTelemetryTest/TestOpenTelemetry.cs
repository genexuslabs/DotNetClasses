using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Metadata;
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
		protected override void SetEnvironmentVars()
		{
			Environment.SetEnvironmentVariable(Startup.APPLICATIONINSIGHTS_CONNECTION_STRING, "InstrumentationKey=dummykey;IngestionEndpoint=https://dummyendpoint;", EnvironmentVariableTarget.Process);
		}
		[Fact]
		public async Task TestCouldNotLoadSystemDiagnosticsDiagnosticSource7_0_0_0()
		{
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
