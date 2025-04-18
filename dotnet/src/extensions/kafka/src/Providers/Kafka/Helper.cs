using Confluent.Kafka;
using System.Collections.Generic;

namespace GeneXus.Messaging.Core.Providers.Kafka
{
	public static class Helper
	{
		public static Dictionary<string, object> GetKafkaConfiguration(List<ProviderConfiguration> listConfig)
		{
			Dictionary<string, object> config = new Dictionary<string, object>();

			foreach (var item in listConfig)
			{
				if (item.NestedValue != null && item.NestedValue.Count > 0)
				{
					config.Add(item.Key, GetKafkaConfiguration(item.NestedValue));
				}
				else
				{
					if (!string.IsNullOrEmpty(item.Value))
					{
						config.Add(item.Key, item.Value);
					}
					else
					{
						config.Add(item.Key, item.IntValue);
					}
				}
			}
			return config;
		}
		public static MessageResponse ToMessageResponse(DeliveryResult<string, string> deliveryResult)
		{
			var result = new MessageResponse()
			{
				Key = deliveryResult.Message.Key,
				Value = deliveryResult.Message.Value,
				Topic = deliveryResult.Topic
			};

			if (deliveryResult.Status == PersistenceStatus.NotPersisted)
			{
				result.Error = new MessageResponseError(-1, "Message not persisted");
			}
			else
			{
				result.Error = new MessageResponseError(0, string.Empty);
			}

			return result;
		}
		public static MessageResponse ToMessageResponse(ConsumeResult<string, string> consumeResult)
		{
			var result = new MessageResponse()
			{
				Key = consumeResult.Message.Key,
				Value = consumeResult.Message.Value,
				Topic = consumeResult.Topic,
			};

			if (consumeResult.Message == null)
			{
				result.Error = new MessageResponseError(-1, "Message is null");
			}
			else
			{
				result.Error = new MessageResponseError(0, string.Empty);
			}

			return result;
		}


	}
}
