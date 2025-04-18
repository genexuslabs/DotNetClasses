using System;
using GeneXus.Messaging.Core;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			GXMessaging messaging = new GXMessaging();
			messaging.Configuration = "{'bootstrap.servers': 'localhost:9092', 'default.topic.config': {'message.timeout.ms': 10000}}";

		
			int key = 250;
			while (true)
			{
				key += 1;
				Console.WriteLine("");
				Console.WriteLine("Enter the Message (END to finish):");
				string msg = "";
				string msg2 = "";

				while (msg2 != "END")
				{
					msg2 = Console.ReadLine();
					msg += msg2;
				}


				//for (int i = 1; i < 10; i++)
				//{
					messaging.ProduceAsync("gxtopic", "msgkey_" + key.ToString(), msg);
				//}
				//}				
				var list = messaging.Finish(5000);
				Console.WriteLine("");
				Console.WriteLine("*********Publish RESPONSE**********");
				foreach (var item in list)
				{
					Console.WriteLine($"{item.Key} - {item.Value} - {item.Error.Code} - HasError: {item.Error.HasError}");
				}
				Console.WriteLine("");
				Console.WriteLine("*********Finished**********");
				
			}

		}
	}
}
