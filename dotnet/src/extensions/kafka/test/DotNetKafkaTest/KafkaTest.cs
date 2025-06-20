using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using GeneXus.Messaging.Core;
using Xunit;
namespace KafkaIntegrationTest
{

	public class KafkaIntegrationTests :IDisposable
	{
		private readonly string _topic;
		public KafkaIntegrationTests()
		{
			bool testEnabled = Environment.GetEnvironmentVariable("KAFKA_TEST_ENABLED") == "true";
			Skip.IfNot(testEnabled, "Environment variables not set");
#if NETCORE
			_topic = $"gxtopic_netcore_{Guid.NewGuid()}";
#else
			_topic = $"gxtopic_netfw_{Guid.NewGuid()}";
#endif

		}
		[SkippableFact]
		public void Test_Producer_And_Consumer_With_Kafka()
		{
			GXMessaging producer = new GXMessaging();
			producer.Configuration = "{'bootstrap.servers': 'localhost:9092', 'default.topic.config': {'message.timeout.ms': 10000}}";

			GXMessaging consumer = new GXMessaging();
			consumer.Configuration = "{'bootstrap.servers': 'localhost:9092', 'group.id': 'test-group', 'auto.offset.reset': 'earliest', 'session.timeout.ms': 10000}";

			string key = "msgkey_1";
			string message = "Integration test message";
			Console.WriteLine($"Producing message with key: {key}");
			//Produce a message
			producer.ProduceAsync(_topic, key, message);
			Console.WriteLine($"Message produced: {message}");
			producer.Finish(10000);

			Console.WriteLine($"Consuming message from topic: {_topic}");
			var messages = consumer.Consume(_topic, 10000);
			Console.WriteLine($"Messages consumed: {messages.Count}");

			//Verify the message was consumed
			Assert.Single(messages);
			Assert.Equal(key, messages[0].Key);
			Assert.Equal(message, messages[0].Value);
		}
		public void Dispose()
		{
		}

	}

}