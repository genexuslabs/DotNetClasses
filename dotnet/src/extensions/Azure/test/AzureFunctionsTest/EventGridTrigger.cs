using System;
using System.Collections.Generic;
using System.Text.Json;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using GeneXus.Deploy.AzureFunctions.EventGridHandler;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Extensions.AzureFunctions.Test
{
	public class EventGridTriggerTest
	{

		[Fact]
		public void EventGridTestCloudEventSchema()
		{			
			try
			{
				var serviceCollection = new ServiceCollection();
				serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
				var serviceProvider = serviceCollection.BuildServiceProvider();

				var context = new Mock<FunctionContext>();
				context.SetupProperty(c => c.InstanceServices, serviceProvider);

				context.SetupGet(c => c.FunctionId).Returns("815504e67081459c9ef1d0399a4aeda3");
				context.SetupGet(c => c.FunctionDefinition.Name).Returns("eventgridTest");
				context.SetupGet(c => c.InvocationId).Returns("d1934e45f752478d899ddb87d262f313");

				ICallMappings callMappings = new CallMappings(".");

			
				IReadOnlyDictionary<string, object> bindingData = new Dictionary<string, object>()
				{ 
					{
						"Id", new PropertyId { Id = "75c7ead6-6485-4aeb-9b31-a0778b4d64a3"}
					},
				};

				Object data = "{\r\n        \"api\": \"PutBlockList\",\r\n        \"clientRequestId\": \"4c5dd7fb-2c48-4a27-bb30-5361b5de920a\",\r\n        \"requestId\": \"9aeb0fdf-c01e-0131-0922-9eb549000000\",\r\n        \"eTag\": \"0x8D76C39E4407333\",\r\n        \"contentType\": \"image/png\",\r\n        \"contentLength\": 30699,\r\n        \"blobType\": \"BlockBlob\",\r\n        \"url\": \"https://gridtesting.blob.core.windows.net/testcontainer/{new-file}\",\r\n        \"sequencer\": \"000000000000000000000000000099240000000000c41c18\",\r\n        \"storageDiagnostics\": {\r\n            \"batchId\": \"681fe319-3006-00a8-0022-9e7cde000000\"\r\n        }";
				context.SetupGet(c => c.BindingContext.BindingData).Returns(bindingData);

				CloudEvent cloudEvent = new("\"/subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.Storage/storageAccounts/{storage-account}\"", "Microsoft.Storage.BlobCreated", data);
				var ex = Record.Exception(() => new EventGridTriggerHandlerCloud(callMappings).Run(cloudEvent, context.Object));
				Assert.Null(ex);
				
			} catch(Exception ex)
			{
				throw new Exception("Exception should not be thrown.", ex);
			}
				
		}
	
		[Fact]
		public void EventGridTestAzureSchema()
		{
			try
			{
				var serviceCollection = new ServiceCollection();
				serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
				var serviceProvider = serviceCollection.BuildServiceProvider();

				var context = new Mock<FunctionContext>();
				context.SetupProperty(c => c.InstanceServices, serviceProvider);

				context.SetupGet(c => c.FunctionId).Returns("39e90db641d94d81b0e854016dcffef9");
				context.SetupGet(c => c.FunctionDefinition.Name).Returns("eventgridTestAzureSchema");
				context.SetupGet(c => c.InvocationId).Returns("4ceabb102cbc48378f58bbc3911d348d");

				ICallMappings callMappings = new CallMappings(".");


				IReadOnlyDictionary<string, object> bindingData = new Dictionary<string, object>()
				{
					{
						"Id", new PropertyId { Id = "514fa418-08fc-4aab-ba41-ed00375b6928"}
					},
				};


				context.SetupGet(c => c.BindingContext.BindingData).Returns(bindingData);

				Object data = "{\r\n        \"api\": \"PutBlockList\",\r\n        \"clientRequestId\": \"4c5dd7fb-2c48-4a27-bb30-5361b5de920a\",\r\n        \"requestId\": \"9aeb0fdf-c01e-0131-0922-9eb549000000\",\r\n        \"eTag\": \"0x8D76C39E4407333\",\r\n        \"contentType\": \"image/png\",\r\n        \"contentLength\": 30699,\r\n        \"blobType\": \"BlockBlob\",\r\n        \"url\": \"https://gridtesting.blob.core.windows.net/testcontainer/{new-file}\",\r\n        \"sequencer\": \"000000000000000000000000000099240000000000c41c18\",\r\n        \"storageDiagnostics\": {\r\n            \"batchId\": \"681fe319-3006-00a8-0022-9e7cde000000\"\r\n        }";
				EventGridEvent eventGridEvent = new EventGridEvent("TestEventAzureEventGridSchema", "GXTest", "1.0", data);

				var ex = Record.Exception(() => new EventGridTriggerHandlerAzure(callMappings).Run(eventGridEvent, context.Object));
				Assert.Null(ex);

			}
			catch (Exception ex)
			{
				throw new Exception("Exception should not be thrown.", ex);
			}

		}
	}
}