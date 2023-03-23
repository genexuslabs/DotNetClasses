using GeneXus.Messaging.GXAzureServiceBus;
using UnitTesting;

namespace DotNetUnitTest
{
	public class AzureServiceBusTest : AzureMessageBrokerTest
	{
		public AzureServiceBusTest() : base(AzureServiceBus.Name, typeof(AzureServiceBus))
		{
		}

	}
}