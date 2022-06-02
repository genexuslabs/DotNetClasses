using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using DotNetUnitTest;
using GeneXus.Messaging.Common;
using GeneXus.Services;
using GeneXus.Storage;
using Xunit;


#pragma warning disable CA1031 // Do not catch general exception types
namespace UnitTesting
{
	[Collection("Sequential")]
	public abstract class QueueTest
	{

		private IQueue queue;

		public QueueTest(string queueName, Type queueType)
		{
			bool testEnabled = Environment.GetEnvironmentVariable("AZUREQUEUE_TEST_ENABLED") == "true";
			Skip.IfNot(testEnabled, "Environment variables not set");

			if (queueName == GeneXus.Messaging.Queue.AzureQueue.Name)
			{
				//testEnabled = true;
				//Environment variables needed here
				Environment.SetEnvironmentVariable("QUEUE_AZUREQUEUE_QUEUENAME", "");
				Environment.SetEnvironmentVariable("QUEUE_AZUREQUEUE_CONNECTIONSTRING", "");

				queue = (IQueue)Activator.CreateInstance(queueType);

				Assert.NotNull(queue);
			}
		}

		[SkippableFact]
		//Clear the Queue
		public void TestClearQueue()
		{
			bool success = false;
			queue.Clear(out success);
			Assert.True(success);

		}

		[SkippableFact]
		public void TestSimpleSendOneMessageMethod()
		{
			SimpleQueueMessage simpleQueueMessage = new SimpleQueueMessage();
			simpleQueueMessage.MessageId = "TestMsgId";
			simpleQueueMessage.MessageBody = "This is the message body";

			bool success= false;
			MessageQueueResult messageQueueResult = queue.SendMessage(simpleQueueMessage, out success);

			Assert.True(success);

			Assert.Equal(simpleQueueMessage.MessageId, messageQueueResult.MessageId);
			Assert.Equal(messageQueueResult.MessageStatus,MessageQueueResultStatus.Sent);

		}
		[SkippableFact]
		public void TestSimpleGetMessagesMethod()
		{
			bool success = false;
			IList<SimpleQueueMessage> simpleQueueMessages = queue.GetMessages(out success);

			Assert.True(success);
			Assert.True(simpleQueueMessages.Count == 1);
			Assert.Equal("This is the message body", simpleQueueMessages[0].MessageBody);
			
		}

		[SkippableFact]
		public void TestSendMessageOptionsMethod()
		{
			SimpleQueueMessage simpleQueueMessage = new SimpleQueueMessage();
			simpleQueueMessage.MessageId = "TestMsgId2";
			simpleQueueMessage.MessageBody = "This is the message body with options";

			MessageQueueOptions options = new MessageQueueOptions();
			//options.VisibilityTimeout = TimeSpan.FromSeconds(1);
			options.TimetoLive = 3600;

			bool success = false;
			MessageQueueResult messageQueueResult = queue.SendMessage(simpleQueueMessage, options, out success);

			Assert.True(success);
			Assert.Equal(simpleQueueMessage.MessageId, messageQueueResult.MessageId);
			Assert.Equal(MessageQueueResultStatus.Sent,messageQueueResult.MessageStatus);

		}
		[SkippableFact]
		public void TestGetMessageOptionsMethod()
		{
	
			bool success = false;
			MessageQueueOptions options = new MessageQueueOptions();
			options.MaxNumberOfMessages = 10;
			options.DeleteConsumedMessages = true;
			options.VisibilityTimeout = 1;
			IList<SimpleQueueMessage> simpleQueueMessages = queue.GetMessages(options, out success);

			Assert.True(success);
			Assert.True(simpleQueueMessages.Count == 2);

			Assert.Equal("TestMsgId1", simpleQueueMessages[0].MessageId);
			Assert.Equal("TestMsgId2", simpleQueueMessages[1].MessageId);

		}

		[SkippableFact]
		public void TestSendMessagesOptionsMethod()
		{

			IList<SimpleQueueMessage> simpleQueueMessages = new List<SimpleQueueMessage>();

			SimpleQueueMessage simpleQueueMessage = new SimpleQueueMessage();
			simpleQueueMessage.MessageId = "TestMsgId3";
			simpleQueueMessage.MessageBody = "This is the message body 3";
			simpleQueueMessages.Add(simpleQueueMessage);

			SimpleQueueMessage simpleQueueMessage2 = new SimpleQueueMessage();
			simpleQueueMessage2.MessageId = "TestMsgId4";
			simpleQueueMessage2.MessageBody = "This is the message body 4";
			simpleQueueMessages.Add(simpleQueueMessage2);

			MessageQueueOptions options = new MessageQueueOptions();

			//Azure VisibilityTimeout. If specified, the request must be made using an x-ms-version of 2011-08-18 or later. If not specified, the default value is 0. Specifies the new visibility timeout value, in seconds, relative to server time. The new value must be larger than or equal to 0, and cannot be larger than 7 days. The visibility timeout of a message cannot be set to a value later than the expiry time. visibilitytimeout should be set to a value smaller than the time-to-live value.
			//options.VisibilityTimeout = TimeSpan.FromSeconds(1);

			//TimeToLive.  Specifies the time-to-live interval for the message, in seconds
			//Azure -1 means not expires
			options.TimetoLive = 3600;

			bool success = false;
			IList<MessageQueueResult> messageQueueResults = queue.SendMessages(simpleQueueMessages,options, out success);

			Assert.True(success);
			Assert.True(simpleQueueMessages.Count == 2);

			System.Threading.Thread.Sleep(1000);
			Assert.Equal("TestMsgId3", simpleQueueMessages[0].MessageId);
			Assert.Equal("TestMsgId4", simpleQueueMessages[1].MessageId);

		}

		
		public void TestDeleteMessagesOptionsMethod()
		{
			bool success = false;
			IList<MessageQueueResult> messageQueueResults = new List<MessageQueueResult>();
			List<string> messageHandleId = new List<string>() { "TestMsgIdNotExists", "TestMsgId3" };
			MessageQueueOptions options = new MessageQueueOptions();
			options.MaxNumberOfMessages = 10;
			messageQueueResults = queue.DeleteMessages(messageHandleId, options, out success);

			Assert.True(success);
			Assert.True(messageQueueResults.Count == 1);

		}

		
		public void TestDeleteMessageMethod()
		{
			bool success = false;
			MessageQueueResult messageQueueResult = queue.DeleteMessage(out success);
			Assert.True(success);
		}

		[SkippableFact]
		public void TestGetQueueLength()
		{
			bool success = false;
			int length = queue.GetQueueLength(out success);

			Assert.True(success);
			Assert.Equal(0, length);

		}

	}
}
#pragma warning restore CA1031 // Do not catch general exception types