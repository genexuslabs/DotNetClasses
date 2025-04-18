using GeneXus.Messaging.Core;
using System;
using System.Collections.Generic;

namespace TesterConsumer
{
	class Program
	{
		static void Main(string[] args)
		{
			GXMessaging messaging = new GXMessaging();
			messaging.Configuration = "{'bootstrap.servers': 'localhost:9092', 'group.id': 'mi-nuevo-grupo', 'auto.offset.reset': 'earliest', 'session.timeout.ms': 500}";


			while (true)
			{
				var list = new List<MessageResponse>();

				list = messaging.Consume("gxtopic", 1000 * 10);

				foreach (var item in list)
				{
					Console.WriteLine($"{item.Key} - {item.Value} - {item.Topic}");
				}
			}
		}
	}
}
