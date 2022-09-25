using System;
using System.Collections.Generic;
using GeneXus.Messaging.Common;
using Xunit;


#pragma warning disable CA1031 // Do not catch general exception types
namespace UnitTesting
{
	[Collection("Sequential")]
	public abstract class AzureMessageBrokerTest
	{

		private IMessageBroker messageBroker;

		public AzureMessageBrokerTest(string queueName, Type queueType)
		{
			bool testEnabled = Environment.GetEnvironmentVariable("AZURESB_TEST_ENABLED") == "true";
			Skip.IfNot(testEnabled, "Environment variables not set");

			if (queueName == GeneXus.Messaging.GXAzureServiceBus.AzureServiceBus.Name)
			{
				//Environment variables needed here
				Environment.SetEnvironmentVariable("MESSAGEBROKER_AZURESB_QUEUENAME", "");
				Environment.SetEnvironmentVariable("MESSAGEBROKER_AZURESB_QUEUECONNECTION", "");

				messageBroker = (IMessageBroker)Activator.CreateInstance(queueType);
				Assert.NotNull(messageBroker);
			}
		}

		[SkippableFact]
		public void TestSendOneMessageMethod()
		{
			BrokerMessage brokerMessage = new BrokerMessage();
			brokerMessage.MessageId = "TestMsgId";
			brokerMessage.MessageBody = "This is the message body";

			bool success = messageBroker.SendMessage(brokerMessage,String.Empty);

			Assert.True(success);

		}

		[SkippableFact]
		public void TestSendBatchMessagesMethod()
		{
			BrokerMessage brokerMessage1 = new BrokerMessage();
			brokerMessage1.MessageId = "TestMsgId1";
			brokerMessage1.MessageBody = "This is the message body 1";

			BrokerMessage brokerMessage2 = new BrokerMessage();
			brokerMessage2.MessageId = "TestMsgId2";
			brokerMessage2.MessageBody = "This is the message body 2";

			IList<BrokerMessage> messages = new List<BrokerMessage>();
			messages.Add(brokerMessage1);
			messages.Add(brokerMessage2);

			BrokerMessageOptions options = new BrokerMessageOptions();

			bool success = messageBroker.SendMessages(messages, String.Empty);

			Assert.True(success);
		}
		[SkippableFact]
		public void TestGetBatchMessagesMethod()
		{	
			BrokerMessageOptions options = new BrokerMessageOptions();
			IList<BrokerMessage> messages = messageBroker.GetMessages(String.Empty, out bool success);
			Assert.True(success);
		}
	}
}
#pragma warning restore CA1031 // Do not catch general exception types