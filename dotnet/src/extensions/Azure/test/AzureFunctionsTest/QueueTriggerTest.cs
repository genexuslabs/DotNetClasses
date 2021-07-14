using System;
using System.Collections.Generic;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Deploy.AzureFunctions.QueueHandler;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Extensions.AzureFunctions.Test
{
	public class QueueTriggerTest
	{

		[Fact]
		public void QueueTest()
		{			
			try
			{
				var serviceCollection = new ServiceCollection();
				serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
				var serviceProvider = serviceCollection.BuildServiceProvider();

				var context = new Mock<FunctionContext>();
				context.SetupProperty(c => c.InstanceServices, serviceProvider);

				context.SetupGet(c => c.FunctionId).Returns("6202c88748614a51851a40fa6a4366e6");
				context.SetupGet(c => c.FunctionDefinition.Name).Returns("queueTest");
				context.SetupGet(c => c.InvocationId).Returns("6a871dbc3cb74a9fa95f05ae63505c2c");

				ICallMappings callMappings = new CallMappings(".");

			
				IReadOnlyDictionary<string, object> bindingData = new Dictionary<string, object>()
				{ 
					{
						"Id", new PropertyId { Id = "b50d8f85-3d42-4904-bfdf-dd006ac5ec6a"}
					},
				};

				string message = "This is a sample message";
				context.SetupGet(c => c.BindingContext.BindingData).Returns(bindingData);
				var ex = Record.Exception(() => new QueueTriggerHandler(callMappings).Run(message, context.Object));
				Assert.Null(ex);
				
			} catch(Exception ex)
			{
				throw new Exception("Exception should not be thrown.", ex);
			}
				
		}
	}

	public class PropertyId
	{
		public string Id;
	}

}