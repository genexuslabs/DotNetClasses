using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TimerTriggerDummy
{
	// This is a dummy class.
	// The purpose of this class is to force the Net SDK to generate all the configuration files
	// needed for running under Net 5 framework.
	public static class TimerTriggerDummy
    {
        [Function("TimerTrigger1")]
        public static void Run([TimerTrigger("0 */5 * * * *")] string myTimer, FunctionContext context)
        {
        }
    }
}
