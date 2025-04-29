using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace GeneXus.Messaging.Core.Providers.Kafka
{

	public class KafkaProducer : IProducer, IDisposable
	{
		private IProducer<string, string> producer;
		private static List<Task<DeliveryResult<string, string>>> TaskList = new List<Task<DeliveryResult<string, string>>>();

		private DeliveryHandler DeliveryHandler;
		private List<MessageResponse> Messages = new List<MessageResponse>();

		public KafkaProducer(Dictionary<string, string> config)
		{
			var producerConfig = new ProducerConfig(config);

			producer = new ProducerBuilder<string, string>(producerConfig)
				.SetKeySerializer(Serializers.Utf8)   
				.SetValueSerializer(Serializers.Utf8) 
				.SetLogHandler(Producer_OnLog)       
				.Build();

			DeliveryHandler = new DeliveryHandler(Messages);
		}
		private void Producer_OnLog(object sender, LogMessage e)
		{			
			Console.WriteLine(e.ToString());
		}
		public bool ProduceAsync(string topic, string key, string value)
		{
			var message = new Message<string, string>
			{
				Key = key,
				Value = value
			};

			TaskList.Add(producer.ProduceAsync(topic, message));
			return true;
		}

		public List<MessageResponse> Finish(int milliseconds)
		{
			Task<DeliveryResult<string, string>>[] list = TaskList.ToArray();
			TaskList = new List<Task<DeliveryResult<string, string>>>();
			Task.WaitAll(list, milliseconds);
			foreach (var task in list)
			{
				DeliveryHandler.HandleDeliveryReport(task.Result);
			}
			return Messages;
		}


		private void Producer_OnError(object sender, Error e)
		{
			Console.WriteLine(e.ToString());
		}


#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					producer.Dispose();
				}
				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~KafkaConsumer() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
#endregion

	}

	
	public class DeliveryHandler
	{
		private List<MessageResponse> Messages;

		public DeliveryHandler(List<MessageResponse> messagesList)
		{
			Messages = messagesList;
		}

		public void HandleDeliveryReport(DeliveryResult<string, string> deliveryResult)
		{
			Messages.Add(Helper.ToMessageResponse(deliveryResult));
		}
	}

}
