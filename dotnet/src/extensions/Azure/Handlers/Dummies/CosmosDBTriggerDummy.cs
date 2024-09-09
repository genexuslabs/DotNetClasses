using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using System.Linq;

namespace CosmosDBTriggerDummy
{
	public static class CosmosDBTriggerDummy
	{
        [Function("CosmosDBTrigger1")]
        public static void Run(
			[CosmosDBTrigger("%CosmosDb%", "%CosmosCollIn%",
			Connection = "CosmosConnection",
			LeaseConnection = "LeaseConnection",
			LeaseContainerName = "LeaseContainerName",
			LeaseContainerPrefix = "LeaseContainerPrefix",
			LeaseDatabaseName = "LeaseDatabaseName")] IReadOnlyList<object> input,
			FunctionContext context)
		{
           
        }
    }
}
