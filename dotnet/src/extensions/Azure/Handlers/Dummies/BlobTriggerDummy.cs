using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GeneXus.Deploy.AzureFunctions.Handlers.Dummies
{
	public static class BlobTriggerDummy
	{
		[Function("BlobTrigger1")]
		public static void Run(
			[BlobTrigger("mycontainer/{name}", Connection = "AzureWebJobsStorage")] Stream myTriggerItem,
			FunctionContext context)
		{
			
		}
	}
}
