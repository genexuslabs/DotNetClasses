using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Http;
using GeneXus.Http.Client;
using GeneXus.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Xunit;
namespace xUnitTesting
{
	public class ChunkedServiceTest : MiddlewareTest
	{
		const string BufferedContent = $"Line 1\nLine 2\nLine 3\n";
		const string Chunk1 = @"Line 1";
		const string Chunk2 = @"Line 2";
		const string Chunk3 = @"Line 3";
		string fileName = Path.Combine(Directory.GetCurrentDirectory(), "tmp.txt");
		[Fact]
		public async Task TestChunkedResponse()
		{
			var hostBuilder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
						   {
							   webBuilder
								   .UseStartup<TestStartup>()
								   .UseTestServer();
						   });
			using var host = await hostBuilder.StartAsync();

			var server = host.GetTestServer();
			var client = server.CreateClient();
			var response = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);
			Assert.True(response.Headers.TransferEncodingChunked);
			response.EnsureSuccessStatusCode();
			GxHttpClient gxHttp = new GxHttpClient();
			gxHttp.ProcessResponse(response);
			Assert.False(gxHttp.Eof);
			Assert.Equal(Chunk1, gxHttp.ReadChunk());
			Assert.False(gxHttp.Eof);
			Assert.Equal(Chunk2, gxHttp.ReadChunk());
			Assert.False(gxHttp.Eof);
			Assert.Equal(Chunk3, gxHttp.ReadChunk());
			Assert.False(gxHttp.Eof);
			Assert.Equal(string.Empty, gxHttp.ReadChunk());
			Assert.True(gxHttp.Eof);
		}

		[Fact]
		public async Task TestBufferedResponse()
		{
			var hostBuilder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
							{
								webBuilder
									.UseStartup<TestStartup>()
									.UseTestServer();
							});

			using var host = await hostBuilder.StartAsync();

			var server = host.GetTestServer();
			var client = server.CreateClient();
			var response = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);
			Assert.True(response.Headers.TransferEncodingChunked);
			response.EnsureSuccessStatusCode();
			GxHttpClient gxHttp = new GxHttpClient();
			gxHttp.ProcessResponse(response);
			Assert.Equal(BufferedContent, gxHttp.ToString());
			gxHttp.ToFile(fileName);
			Assert.True(File.Exists(fileName));
			if (File.Exists(fileName))
			{
				string content = File.ReadAllText(fileName);
				Assert.Equal(BufferedContent, content);
			}

		}

	}
	public class TestStartup
	{
		public void ConfigureServices(IServiceCollection services)
		{
		}

		public void Configure(IApplicationBuilder app)
		{
			
			app.Run(async context =>
			{
				context.Response.Headers.Append(HttpHeader.TRANSFER_ENCODING, "chunked");

				var responseStream = context.Response.Body;

				for (int i = 0; i < 3; i++)
				{
					string line = $"Line {i + 1}\n";
					byte[] data = Encoding.UTF8.GetBytes(line);
					await responseStream.WriteAsync(data, 0, data.Length);
					await responseStream.FlushAsync();
					await Task.Delay(1000);
				}
			});
		}
	}
}
