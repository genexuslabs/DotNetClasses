using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Http;
using GeneXus.Metadata;
using Microsoft.AspNetCore.Http;
using Xunit;
namespace xUnitTesting
{
	public class RestServiceTest : MiddlewareTest
	{
		public RestServiceTest():base()
		{
			ClassLoader.FindType("apps.append", "GeneXus.Programs.apps", "append", Assembly.GetExecutingAssembly(), true);//Force loading assembly for append procedure
		}
		[Fact]
		public async Task TestMultiCall()
		{
			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();
			StringContent body = new StringContent("[[1, \"one\", 11], [2, \"two\", 22], [3, \"three\", 33]]");
			HttpResponseMessage response = await client.PostAsync("gxmulticall?apps.append", body);
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
			string responseBody = await response.Content.ReadAsStringAsync();
			Assert.Empty(responseBody);
		}
		[Fact]
		public async Task MultithreadRestServiceAccess_ContextDisposed()
		{
			HttpClient client = server.CreateClient();
			List<Task> tasks = new List<Task>();
			int MAX_THREADS = 200;
			for (int i = 0; i < MAX_THREADS; i++)
			{
				tasks.Add(RunController(client));
			}
			
			await Task.WhenAll(tasks);
		}


		private async Task<HttpResponseMessage> RunController(HttpClient client)
		{
			HttpResponseMessage response = await client.GetAsync("rest/apps/append");
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); //When failed, turn on log.config to see server side error.
			return response;
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
