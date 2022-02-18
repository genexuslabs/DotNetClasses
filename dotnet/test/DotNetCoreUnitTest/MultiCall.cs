using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Http;
using GeneXus.Metadata;
using GxClasses.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;
namespace xUnitTesting
{
	public class MultiCall
	{
		[Fact]
		public async Task TestMultiCall()
		{
			string body = "[[1, \"one\", 11], [2, \"two\", 22], [3, \"three\", 33]]";
			var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
			httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
			httpContext.Request.Host= new HostString("localhost");
			httpContext.Request.IsHttps=false;
			httpContext.Request.Path = new PathString("/gxmulticall");
			httpContext.Request.QueryString = new QueryString("?apps.append");
			GXRouting.ContentRootPath = Directory.GetCurrentDirectory();
			ClassLoader.FindType("apps.append", "GeneXus.Programs.apps", "append", Assembly.GetExecutingAssembly(), true);//Force loading assembly for append procedure
			var multicall = new MultiCallMiddleware(next: (innerHttpContext) => Task.FromResult(0));

			await multicall.Invoke(httpContext);
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
