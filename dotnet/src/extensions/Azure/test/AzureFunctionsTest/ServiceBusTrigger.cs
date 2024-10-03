using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Deploy.AzureFunctions.ServiceBusHandler;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Extensions.AzureFunctions.Test
{
	public class ServiceBusTrigger
	{
		[Fact]
		public async Task ServiceBusTriggerTest()
		{
			try
			{
				var serviceCollection = new ServiceCollection();
				serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
				var serviceProvider = serviceCollection.BuildServiceProvider();

				var context = new Mock<FunctionContext>();
				context.SetupProperty(c => c.InstanceServices, serviceProvider);

				context.SetupGet(c => c.FunctionId).Returns("815504e67081459c9ef1d0399a4aeda4");
				context.SetupGet(c => c.FunctionDefinition.Name).Returns("serviceBusTest");
				context.SetupGet(c => c.InvocationId).Returns("d1934e45f752478d899ddb87d262f314");

				ICallMappings callMappings = new CallMappings(".");

				IReadOnlyDictionary<string, object> bindingData = new Dictionary<string, object>()
				{
					{
						"Id", new PropertyId { Id = "75c7ead6-6485-4aeb-9b31-a0778b4d64a4"}
					},
				};

				ServiceBusReceivedMessage[] messages = CreateMockServiceBusReceivedMessages();

				Exception ex = await Record.ExceptionAsync(() => new ServiceBusTriggerHandler(callMappings).Run(messages, context.Object));
				Assert.Null(ex);

			}
			catch (Exception ex)
			{
				throw new Exception("Exception should not be thrown.", ex);
			}

		}
		private ServiceBusReceivedMessage[] CreateMockServiceBusReceivedMessages()
		{

			ServiceBusReceivedMessage[] messages = new ServiceBusReceivedMessage[2];

			BinaryData messageBody = BinaryData.FromString("Mocked message content");

			ServiceBusReceivedMessage message = ServiceBusModelFactory.ServiceBusReceivedMessage(
				body: messageBody,
				messageId: "mocked-message-id",
				contentType: "application/json",
				partitionKey: "1234",
				sessionId: null,
				replyToSessionId: null,
				replyTo: null,
				subject: null,
				correlationId: null,
				to: null,
				timeToLive: TimeSpan.FromMinutes(5),
				deliveryCount: 1,
				enqueuedTime: DateTimeOffset.UtcNow,
				lockTokenGuid: Guid.NewGuid(),
				sequenceNumber: 1
			);

			messages[0] = message;

			BinaryData messageBody2 = BinaryData.FromString("Mocked message content 2");
			ServiceBusReceivedMessage message2 = ServiceBusModelFactory.ServiceBusReceivedMessage(
				body: messageBody2,
				messageId: "mocked-message-id-2",
				contentType: "application/json",
				partitionKey: "1234",
				sessionId: null,
				replyToSessionId: null,
				replyTo: null,
				subject: null,
				correlationId: null,
				to: null,
				timeToLive: TimeSpan.FromMinutes(5),
				deliveryCount: 1,
				enqueuedTime: DateTimeOffset.UtcNow,
				lockTokenGuid: Guid.NewGuid(),
				sequenceNumber: 1
			);

			messages[1] = message2;
			return messages;
		}
	}
	
}