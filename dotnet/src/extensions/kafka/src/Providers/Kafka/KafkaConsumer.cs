using System;
using System.Collections.Generic;
using Confluent.Kafka;
using GeneXus.Messaging.Core.Exceptions;

namespace GeneXus.Messaging.Core.Providers.Kafka
{

	public class KafkaConsumer : IConsumer, IDisposable
	{

		private IConsumer<string, string> consumer;
		public Error LastError { get; set; }
		private Dictionary<string, string> _config;

		public KafkaConsumer(Dictionary<string, string> config)
		{
			_config = config;
			Initialize();

		}
		private void Initialize()
		{
			var consumerConfig = new ConsumerConfig();
			foreach (var kvp in _config)
			{
				consumerConfig.Set(kvp.Key, kvp.Value.ToString());
			}

			consumer = new ConsumerBuilder<string, string>(consumerConfig)
				.SetErrorHandler(Consumer_OnError)
				.SetKeyDeserializer(Deserializers.Utf8)   
				.SetValueDeserializer(Deserializers.Utf8) 
				.Build();
		}
		public List<MessageResponse> Consume(string topic, int timeout)
		{
			LastError = null;
			var list = new List<MessageResponse>();
			consumer.Subscribe(topic);

			try
			{
				while (true)
				{
					ConsumeResult<string, string> consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeout));
					if (consumeResult == null) // Si no hay más mensajes, sal del bucle
						break;

					list.Add(Helper.ToMessageResponse(consumeResult));
				}
			}
			catch (ObjectDisposedException) // Bug en KafkaClient
			{
				Initialize();
			}
			catch (ConsumeException ex) // Manejo de errores específicos de consumo
			{
				LastError = ex.Error;
			}

			if (LastError != null)
			{
				throw new MessagingConsumeException((int)LastError.Code, LastError.ToString());
			}

			return list;
		}

		
		private void Consumer_OnError(object sender, Error e)
		{
			LastError = e;
		}


#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					try
					{
						consumer.Dispose();
					}
					catch (Exception)
					{

					}
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
}
