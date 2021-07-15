using System;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Deploy.AzureFunctions.TimerHandler;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Extensions.AzureFunctions.Test
{
	public class TimerTriggerTest
	{
		[Fact]
		public void TimerTest()
		{			
			try
			{
				var serviceCollection = new ServiceCollection();
				serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
				var serviceProvider = serviceCollection.BuildServiceProvider();

				var context = new Mock<FunctionContext>();
				context.SetupProperty(c => c.InstanceServices, serviceProvider);

				context.SetupGet(c => c.FunctionId).Returns("6202c88748614a51851a40fa6a4366e6");
				context.SetupGet(c => c.FunctionDefinition.Name).Returns("timerTest");
				context.SetupGet(c => c.InvocationId).Returns("6a871dbc3cb74a9fa95f05ae63505c2c");

				ICallMappings callMappings = new CallMappings(".");

				MyScheduleStatus scheduleStatus = new MyScheduleStatus();
				scheduleStatus.Next = DateTime.Now.AddMinutes(10);
				MyInfo myinfo = new();
				myinfo.IsPastDue = false;
				myinfo.ScheduleStatus = scheduleStatus;
				
				var exception = Record.Exception(() => new TimerTriggerHandler(callMappings).Run(myinfo,context.Object));
				Assert.Null(exception);
				
			} catch(Exception ex)
			{
				throw new Exception("Exception should not be thrown.", ex);
			}
				
		}
	}

}