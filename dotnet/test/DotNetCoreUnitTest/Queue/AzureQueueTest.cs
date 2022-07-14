using GeneXus.Messaging.Queue;
using UnitTesting;

namespace DotNetUnitTest
{
	public class AzureQueueTest : QueueTest
	{
		public AzureQueueTest() : base(AzureQueue.Name, typeof(AzureQueue))
		{
		}

	}
}