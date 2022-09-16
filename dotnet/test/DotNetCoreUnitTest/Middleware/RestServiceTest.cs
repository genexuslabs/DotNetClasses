using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using GeneXus.Http;
using GeneXus.Metadata;
using GeneXus.Services;
using GeneXus.Storage.GXAmazonS3;
using GeneXus.Utils;
using Microsoft.AspNetCore.Http;
using Xunit;
namespace xUnitTesting
{
	public class RestServiceTest : MiddlewareTest
	{
		public RestServiceTest() : base()
		{
			ClassLoader.FindType("apps.append", "GeneXus.Programs.apps", "append", Assembly.GetExecutingAssembly(), true);//Force loading assembly for append procedure
			ClassLoader.FindType("apps.saveimage", "GeneXus.Programs.apps", "saveimage", Assembly.GetExecutingAssembly(), true);//Force loading assembly for saveimage procedure
			server.AllowSynchronousIO = true;
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
		public async Task TestSimpleRestPost()
		{
			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();			
			StringContent body = new StringContent("{\"Image\":\"imageName\",\"ImageDescription\":\"imageDescription\"}");
			HttpResponseMessage response = await client.PostAsync("rest/apps/saveimage", body);
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
			string responseBody = await response.Content.ReadAsStringAsync();
			Assert.Equal("{}", responseBody);
		}

		[Fact(Skip = "Non deterministic")]
		public async Task TestGxObjectUploads()
		{
			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();
			using (Stream s = System.IO.File.Open(@"uruguay.flag.png", FileMode.Open))
			{
				StreamContent streamContent = new StreamContent(s);				
				HttpResponseMessage response = await client.PutAsync("rest/apps/saveimage/gxobject", streamContent);
				response.EnsureSuccessStatusCode();
				string uploadPath = await GetObjectIdToken(response);
				Assert.True(System.IO.File.Exists(uploadPath));				
			}
		}

		[SkippableFact]
		public async Task TestGxObjectUploadsWithS3Storage()
		{
			bool testEnabled = Environment.GetEnvironmentVariable("AWSS3" + "_TEST_ENABLED") == "true";
			Skip.IfNot(testEnabled, "Environment variables not set");

			server.AllowSynchronousIO = true;
			HttpClient client = server.CreateClient();

			ExternalProviderS3 s3Provider = new ExternalProviderS3();
			ServiceFactory.SetExternalProvider(s3Provider);
			
			using (Stream s = System.IO.File.Open(@"uruguay.flag.png", FileMode.Open))
			{
				StreamContent streamContent = new StreamContent(s);
				HttpResponseMessage response = await client.PutAsync("rest/apps/saveimage/gxobject", streamContent);
				response.EnsureSuccessStatusCode();

				string uploadUrl = await GetObjectIdToken(response);				
				response = await new HttpClient().GetAsync(uploadUrl);
				response.EnsureSuccessStatusCode();
				Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
			}
		}

		private static async Task<string> GetObjectIdToken(HttpResponseMessage response)
		{
			
			Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
			string responseBody = await response.Content.ReadAsStringAsync();
			Dictionary<string, string> jsonObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);

			Assert.True(jsonObj.ContainsKey("object_id"));
			string objId = jsonObj["object_id"];
			Assert.NotEmpty(objId);
			string uploadUrl = GxUploadHelper.UploadPath(objId);
			return uploadUrl;
		}

		[Fact(Skip = "Non deterministic")]
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
