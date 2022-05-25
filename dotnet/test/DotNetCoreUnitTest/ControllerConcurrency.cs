using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Http;
using GeneXus.Metadata;
using GxClasses.Web.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;
namespace xUnitTesting
{
	public class ControllerConcurrency
	{

		[Fact]
		public async Task TestMultiCall()
		{
			HttpContext httpContext = CreateMulticallContext();
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();
			ClassLoader.FindType("apps.append", "GeneXus.Programs.apps", "append", Assembly.GetExecutingAssembly(), true);//Force loading assembly for append procedure
			MultiCallMiddleware multicall = new MultiCallMiddleware(next: (innerHttpContext) => Task.FromResult(0));
			await multicall.Invoke(httpContext);
		}
		private HttpContext CreateMulticallContext()
		{
			string body = "[[1, \"one\", 11], [2, \"two\", 22], [3, \"three\", 33]]";
			HttpContext httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
			httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
			httpContext.Request.Host= new HostString("localhost");
			httpContext.Request.IsHttps=false;
			httpContext.Request.Path = new PathString("/gxmulticall");
			httpContext.Request.QueryString = new QueryString("?apps.append");
			return httpContext;
		}
		[Fact]
		public async Task MultithreadRestServiceAccess_ContextDisposed()
		{
			ClassLoader.FindType("apps.append", "GeneXus.Programs.apps", "append", Assembly.GetExecutingAssembly(), true);//Force loading assembly for append procedure
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();
			TestServer server = new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>());
			List<Task> tasks = new List<Task>();
			int MAX = 3000;
			for (int i = 0; i < MAX; i++)
			{
				tasks.Add(Task.Run(() => RunController(server)));
			}
			await Task.WhenAll(tasks);

		}

		private async Task RunController(TestServer server)
		{
			HttpClient client = server.CreateClient();
			var response = await client.GetAsync("rest/apps/append");
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); //When failed, turn on log.config to see server side error.
		}
	}
	public class MultiCallMiddleware
	{
		private readonly RequestDelegate _next;

		public MultiCallMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				GXMultiCall multicall = new GXMultiCall();
				multicall.ProcessRequest(context);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
			await _next(context);
		}
	}

}
