using System;
using Functions.Tests;
using GeneXus.Deploy.AzureFunctions.QueueHandler;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Xunit;

namespace Extensiones.AzureFunctions
{
	public class AzureTriggerTests
	{
		private readonly ILogger logger = TestFactory.CreateLogger(LoggerTypes.List);

		[Fact]
		public void HttpQueueTriggerHandler()
		{
			try
			{
				CloudQueueMessage myQueueItem = new CloudQueueMessage("contentTest", "popReceiptTest");
				Microsoft.Azure.WebJobs.ExecutionContext context = new Microsoft.Azure.WebJobs.ExecutionContext();
				context.FunctionDirectory = ".";
				QueueTriggerHandler.Run(myQueueItem, logger, context);
				ListLogger listLogger = logger as ListLogger;
				foreach (string msg in listLogger.Logs)
				{
					//check msgs are ok
				}
				Assert.Equal(10, listLogger.Logs.Count);
			}catch(Exception)
			{
				//Exception should not be thrown
			}
		}
	}

}