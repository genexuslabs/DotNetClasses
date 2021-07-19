using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GeneXus.Deploy.AzureFunctions.Handlers.Helpers
{
	public class FunctionMetadata
		{
			public const string directionIn = "In";
			public const string directionOut = "Out";
			public class TriggerTypes
			{
				public const string TimerTrigger = "timerTrigger";
				public const string QueueTrigger = "queueTrigger";
				public const string ServiceBusTrigger = "serviceBusTrigger";
				public const string HttpTrigger = "HttpTrigger";
				public const string Http = "http";
			}
			public string name { get; set; }
			public string scriptFile { get; set; }
			public string entryPoint { get; set; }
			public string language { get; set; }

			public PropertyList properties;

			[JsonIgnore]
			public Binding binding;

			public List<Binding> bindings;
			public class PropertyList
			{
				public PropertyList(bool isCodeless, string confSource)
				{
					IsCodeless = isCodeless;
					configurationSource = confSource;
				}
				public bool IsCodeless { get; set; }
				public string configurationSource { get; set; }
			}
			public class Binding
			{
				public Binding(string trigger)
				{
					type = trigger;
				}
				public string type { get; set; }
			}
			public class TimerBinding : Binding
			{
				public TimerBinding(string trigger, string scheduleTime, bool monitor, bool runStartUp, string nameVariable)
					: base(trigger)
				{
					schedule = scheduleTime;
					useMonitor = monitor;
					runOnStartup = runStartUp;
					name = nameVariable;
				}
				public TimerBinding(string trigger)
					: base(trigger)
				{
				}

				public string schedule { get; set; }
				public bool useMonitor { get; set; }
				public bool runOnStartup { get; set; }
				public string name { get; set; }
			}
			public class QueueBinding : Binding
			{
				public QueueBinding(string trigger, string connectionQueue, string queue, string nameVariable) : base(trigger)
				{
					connection = connectionQueue;
					queueName = queue;
					name = nameVariable;
				}
				public QueueBinding(string trigger) : base(trigger)
				{
				}
				public string connection { get; set; }
				public string queueName { get; set; }
				public string name { get; set; }
			}
			public class ServiceBusQueueBinding : QueueBinding
			{
				public ServiceBusQueueBinding(string trigger, string connectionQueue, string queue, string nameVariable) : base(trigger, connectionQueue, queue, nameVariable)
				{ }
				public ServiceBusQueueBinding(string trigger) : base(trigger)
				{ }

				public bool isSessionsEnabled { get; set; }
				public bool autoComplete { get; set; }
			}
			public class ServiceBusTopicBinding : Binding
			{
				public ServiceBusTopicBinding(string trigger) : base(trigger)
				{
				}
				public string connection { get; set; }
				public string topicName { get; set; }
				public string subscriptionName { get; set; }
				public string name { get; set; }
				public bool isSessionsEnabled { get; set; }
				public bool autoComplete { get; set; }
			}
			public class HttpInBinding : Binding
			{
				public HttpInBinding(string nameFunc, string trigger, string authLevelFunc, List<string> methodsSupported, string routeFunc) : base(trigger)
				{
					name = nameFunc;
					direction = directionIn;
					authLevel = authLevelFunc;
					methods = methodsSupported;
					route = routeFunc;
				}
				public HttpInBinding(string trigger) : base(trigger)
				{
				}
				public string name { get; set; }
				public string direction { get; set; }
				public string authLevel { get; set; }
				public string route { get; set; }
				public List<string> methods { get; set; }
			}
			public class HttpOutBinding : Binding
			{
				public HttpOutBinding(string trigger) : base(trigger)
				{
					name = "$return";
					direction = directionOut;
				}
				public string name { get; set; }
				public string direction { get; set; }
			}
		}
}
