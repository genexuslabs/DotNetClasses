using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBusQueueDummy
{
	// This is a dummy class.
	// The purpose of this class is to force the Net SDK to generate all the configuration files
	// needed for running under Net 5 framework.
	public static class ServiceBusQueueDummy
    {
        [Function("ServiceBusQueueTrigger1")]
        public static void Run([ServiceBusTrigger("myqueue", Connection = "")] string myQueueItem, FunctionContext context)
        {
           
        }
    }
}
