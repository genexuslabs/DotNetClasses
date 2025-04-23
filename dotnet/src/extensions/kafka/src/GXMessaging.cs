using System.Collections.Generic;
using GeneXus.Messaging.Core.Exceptions;
using GeneXus.Messaging.Core.Providers.Kafka;

namespace GeneXus.Messaging.Core
{
	public class GXMessaging
	{

		private IConsumer consumer;
		private IProducer producer;

		public string Configuration { set; get; }
		
		public int ErrCode { get; set; }

		public string ErrDescription { get; set; }

		public GXMessaging()
		{
		}

		private bool Initialize()
		{
			bool ok = Configuration != null;
			return ok;
		}
		public List<MessageResponse> Consume(string topic, int timeout)
		{
			List<MessageResponse> list = new List<MessageResponse>();

			bool ok = false;

			if (!Initialize())
			{
				return list;
			}
			if (consumer == null)
			{
				
				consumer = new KafkaConsumer(JsonHelper.Deserialize(Configuration));
			}
			try
			{
				var resultList = consumer.Consume(topic, timeout);
				list.AddRange(resultList);
				ok = resultList.Count > 0;
				ErrDescription = "";
				ErrCode = 0;
			}
			catch (MessagingConsumeException e)
			{
				ErrDescription = e.Message;
				ErrCode = e.ErrCode;
			}

			return list;
		}
		public bool ProduceAsync(string topic, string key, string value)
		{
			if (!Initialize())
			{
				return false;
			}

			if (producer == null)
			{
				producer = new KafkaProducer(JsonHelper.Deserialize(Configuration));
			}

			return producer.ProduceAsync(topic,key, value);
		}

		public List<MessageResponse> Finish(int timeout)
		{
			if (!Initialize())
			{
				return new List<MessageResponse>();
			}
			return producer.Finish(timeout);
		}

	}
}
