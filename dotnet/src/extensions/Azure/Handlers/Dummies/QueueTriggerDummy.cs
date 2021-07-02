using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace QueueTriggerDummy
{
	// This is a dummy class.
	// The purpose of this class is to force the Net SDK to generate all the configuration files
	// needed for running under Net 5 framework.
	public static class QueueTriggerDummy
    {
        [Function("QueueTrigger1")]
        public static void Run([QueueTrigger("myqueue-items", Connection = "")] string myQueueItem,
            FunctionContext context)
        {
           
        }
    }
}
