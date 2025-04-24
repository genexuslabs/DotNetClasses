using System.Collections.Generic;

namespace GeneXus.Messaging.Core
{
	public interface IConsumer
	{
		List<MessageResponse> Consume(string topic, int timeout);
	}
}
