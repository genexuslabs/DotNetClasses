using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using GeneXus.Deploy.AzureFunctions.HttpHandler;
using GxClasses.Web.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Extensions.AzureFunctions.Test
{
	public class HttpTriggerTest
	{
		[Fact]
		public void HttpApiObjectTest()
		{
			try
			{
				ServiceCollection serviceCollection = new ServiceCollection();
				serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
				ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

				Mock<FunctionContext> context = new Mock<FunctionContext>();
				context.SetupProperty(c => c.InstanceServices, serviceProvider);

				context.SetupGet(c => c.FunctionId).Returns("6202c88748614a51851a40fa6a4366e6");
				context.SetupGet(c => c.FunctionDefinition.Name).Returns("listattractions");
				context.SetupGet(c => c.InvocationId).Returns("6a871dbc3cb74a9fa95f05ae63505c2c");

				MockHttpRequestData request = new MockHttpRequestData(
								context.Object,
								new Uri("http://localhost/APIAttractions/ListAttractions"));

				HttpTriggerHandler function = new HttpTriggerHandler(new GXRouting(String.Empty), null);
				HttpResponseData response = function.Run(request, context.Object);
				Assert.NotNull(response);
				Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
				response.Body.Seek(0, SeekOrigin.Begin);
				StreamReader reader = new StreamReader(response.Body);
				string responseBody = reader.ReadToEnd();
				Assert.NotEmpty(responseBody);

			}
			catch (Exception ex)
			{
				throw new Exception("Exception should not be thrown.", ex);
			}

		}
		[Fact]
		public void HttpTest()
		{			
			try
			{
				ServiceCollection serviceCollection = new ServiceCollection();
				serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
				ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

				Mock<FunctionContext> context = new Mock<FunctionContext>();
				context.SetupProperty(c => c.InstanceServices, serviceProvider);

				context.SetupGet(c => c.FunctionId).Returns("6202c88748614a51851a40fa6a4366e6");
				context.SetupGet(c => c.FunctionDefinition.Name).Returns("timerTest");
				context.SetupGet(c => c.InvocationId).Returns("6a871dbc3cb74a9fa95f05ae63505c2c");

				MemoryStream body = new MemoryStream(Encoding.ASCII.GetBytes("{ \"test\": true }"));

				MockHttpRequestData request = new MockHttpRequestData(
								context.Object,
								new Uri("http://localhost/rest/amyprochandler"),
								body);

				HttpTriggerHandler function = new HttpTriggerHandler(new GXRouting("rest"), null);
				HttpResponseData response = function.Run(request, context.Object);
				Assert.NotNull(response);
				//Assert.Equal(HttpStatusCode.OK, response.StatusCode);
				response.Body.Seek(0, SeekOrigin.Begin);
				StreamReader reader = new StreamReader(response.Body);
				string responseBody = reader.ReadToEnd();
				Assert.NotEmpty(responseBody);

			} catch(Exception ex)
			{
				throw new Exception("Exception should not be thrown.", ex);
			}
				
		}
	}
	public class MockHttpRequestData : HttpRequestData
	{
		public MockHttpRequestData(FunctionContext functionContext, Uri url, Stream body = null) : base(functionContext)
		{
			Url = url;
			Body = body ?? new MemoryStream();
		}

		public override Stream Body { get; } = new MemoryStream();

		public override HttpHeadersCollection Headers { get; } = new HttpHeadersCollection();

		public override IReadOnlyCollection<IHttpCookie> Cookies { get; }

		public override Uri Url { get; }

		public override IEnumerable<ClaimsIdentity> Identities { get; }

		public override string Method
		{
			get { return HttpMethod.Get.Method; }
		}

		public override HttpResponseData CreateResponse()
		{
			return new MockHttpResponseData(FunctionContext);
		}
	}

	public class MockHttpResponseData : HttpResponseData
	{
		public MockHttpResponseData(FunctionContext functionContext) : base(functionContext)
		{
		}

		public override HttpStatusCode StatusCode { get; set; }
		public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();
		public override Stream Body { get; set; } = new MemoryStream();
		public override HttpCookies Cookies { get; }
	}
}