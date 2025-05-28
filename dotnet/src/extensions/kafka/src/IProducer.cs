using System.Collections.Generic;

namespace GeneXus.Messaging.Core
{
	public interface IProducer
	{
		bool ProduceAsync(string topic, string key, string value);
		List<MessageResponse> Finish(int miliseconds);
	}
}
