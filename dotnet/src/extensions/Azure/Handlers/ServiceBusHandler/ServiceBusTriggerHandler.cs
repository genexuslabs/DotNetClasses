using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using GeneXus.Application;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Metadata;
using GeneXus.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GeneXus.Deploy.AzureFunctions.ServiceBusHandler
{
	public class ServiceBusTriggerHandler
	{
		private ICallMappings _callmappings;

		public ServiceBusTriggerHandler(ICallMappings callMappings)
		{
			_callmappings = callMappings;
		}
		public async Task Run(ServiceBusReceivedMessage[] myQueueItem, FunctionContext context)
		{
			var log = context.GetLogger("ServiceBusTriggerHandler");
			string functionName = context.FunctionDefinition.Name;

			log.LogInformation($"GeneXus Service Bus trigger handler. Function processed: {functionName}.");

			try
			{
				await ProcessMessages(context, log, myQueueItem);
			}
			catch (Exception ex) //Catch System exception and retry
			{
				log.LogError(ex.ToString());
				throw;
			}
		}

		public async Task RunSingle(ServiceBusReceivedMessage myQueueItem, FunctionContext context)
		{

			var log = context.GetLogger("ServiceBusTriggerHandler");
			string functionName = context.FunctionDefinition.Name;

			log.LogInformation($"GeneXus Service Bus trigger handler. Function processed: {functionName}.");

			try
			{
				ServiceBusReceivedMessage[] queueItems = new ServiceBusReceivedMessage[] { myQueueItem };
				await ProcessMessages(context, log, queueItems);
			}
			catch (Exception ex) //Catch System exception and retry
			{
				log.LogError(ex.ToString());
				throw;
			}
		}
		private Task ProcessMessages(FunctionContext context, ILogger log, ServiceBusReceivedMessage[] messages)
		{
			string envVar = $"GX_AZURE_{context.FunctionDefinition.Name.ToUpper()}_CLASS";
			string envVarValue = Environment.GetEnvironmentVariable(envVar);
			string gxProcedure = string.Empty;
			if (!string.IsNullOrEmpty(envVarValue))
				gxProcedure = envVarValue;
			else
			{
				CallMappings callmap = (CallMappings)_callmappings;
				GxAzMappings map = callmap != null && callmap.mappings is object ? callmap.mappings.SingleOrDefault(m => m.FunctionName == context.FunctionDefinition.Name) : null;
				gxProcedure = map is object ? map.GXEntrypoint : string.Empty;
			}

			string exMessage;

			if (!string.IsNullOrEmpty(gxProcedure))
			{
				try
				{
					StringBuilder sb1 = new StringBuilder(gxProcedure);
					sb1.Append(".dll");
					string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), sb1.ToString());
					Assembly obj = Assembly.LoadFile(path);

					StringBuilder sb2 = new StringBuilder("GeneXus.Programs.");
					sb2.Append(gxProcedure);

					Type objexec = obj.GetType(sb2.ToString());
					if (objexec != null)
					{

						object objgxproc = Activator.CreateInstance(objexec);
						var method = objexec.GetMethod("execute");
						ParameterInfo[] parameters = method.GetParameters();

						//Check parameters

						if (parameters.Length != 2)
						{
							//Thrown to the Azure monitor

							exMessage = string.Format("The number of parameters in GeneXus procedure is not correct.", FunctionExceptionType.SysRuntimeError);
							throw new Exception(exMessage); //Send to retry if possible.
						}
						else
						{
							//Two valid signatures for the GX procedure:
							//parm(in:&EventMessageCollection, out:&ExternalEventMessageResponse );
							//parm(in:&rawData, out:&ExternalEventMessageResponse );

							GxContext gxcontext = new GxContext();
							object[] parametersdata;
							parametersdata = new object[] { null };

							if (parameters[0].ParameterType == typeof(string))
							{
								string queueMessageSerialized = GetSerializedMessages(messages);
								parametersdata = new object[] { queueMessageSerialized, null };
							}
							else
							{
								//Initialization
								Type eventMessagesType = parameters[0].ParameterType; //SdtEventMessages
																					  //GxUserType eventMessages = (GxUserType)Activator.CreateInstance(eventMessagesType, new object[] { gxcontext }); // instance of SdtEventMessages
								GxUserType eventMessages = (GxUserType)Activator.CreateInstance(eventMessagesType, new object[] { gxcontext }); // instance of SdtEventMessages

								foreach (ServiceBusReceivedMessage serviceBusReceivedMessage in messages)
								{

									IList eventMessage = (IList)ClassLoader.GetPropValue(eventMessages, "gxTpr_Eventmessage");//instance of GXBaseCollection<SdtEventMessage>
									Type eventMessageItemType = eventMessage.GetType().GetGenericArguments()[0];//SdtEventMessage

									GxUserType eventMessageItem = (GxUserType)Activator.CreateInstance(eventMessageItemType, new object[] { gxcontext }); // instance of SdtEventMessage

									IList eventMessageProperties = (IList)ClassLoader.GetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties");//instance of GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty>
									Type eventMessPropsItemType = eventMessageProperties.GetType().GetGenericArguments()[0];//SdtEventMessageProperty								

									//Payload
									GxUserType eventMessageProperty;

									foreach (var messageProp in serviceBusReceivedMessage.ApplicationProperties)
									{
										eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, messageProp.Key, Convert.ToString(messageProp.Value), gxcontext);
										eventMessageProperties.Add(eventMessageProperty);

									}

									eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "Body", serviceBusReceivedMessage.Body?.ToString(), gxcontext);
									eventMessageProperties.Add(eventMessageProperty);

									eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "ContentType", serviceBusReceivedMessage.ContentType, gxcontext);
									eventMessageProperties.Add(eventMessageProperty);

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.CorrelationId))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "CorrelationId", serviceBusReceivedMessage.CorrelationId, gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.SessionId))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "SessionId", serviceBusReceivedMessage.SessionId, gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.Subject))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "Subject", serviceBusReceivedMessage.Subject, gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.DeadLetterErrorDescription))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "DeadLetterErrorDescription", serviceBusReceivedMessage.DeadLetterErrorDescription, gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.DeadLetterReason))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "DeadLetterReason", serviceBusReceivedMessage.DeadLetterReason, gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.DeadLetterSource))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "DeadLetterSource", serviceBusReceivedMessage.DeadLetterSource, gxcontext));

									eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "DeliveryCount", serviceBusReceivedMessage.DeliveryCount.ToString(), gxcontext));
									eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "EnqueuedSequenceNumber", serviceBusReceivedMessage.EnqueuedSequenceNumber.ToString(), gxcontext));
									eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "SequenceNumber", serviceBusReceivedMessage.SequenceNumber.ToString(), gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.ReplyTo))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "ReplyTo", serviceBusReceivedMessage.ReplyTo, gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.ReplyToSessionId))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "ReplyToSessionId", serviceBusReceivedMessage.ReplyToSessionId, gxcontext));

									eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "State", serviceBusReceivedMessage.State.ToString(), gxcontext));

									eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "EnqueuedTime", serviceBusReceivedMessage.EnqueuedTime.ToString(), gxcontext));

									eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "LockedUntil", serviceBusReceivedMessage.LockedUntil.ToString(), gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.LockToken))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "LockToken", serviceBusReceivedMessage.LockToken, gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.PartitionKey))
										eventMessageProperties.Add(eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "PartitionKey", serviceBusReceivedMessage.PartitionKey, gxcontext));

									eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "ScheduledEnqueueTime", serviceBusReceivedMessage.ScheduledEnqueueTime.ToString(), gxcontext));

									eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "TimeToLive", serviceBusReceivedMessage.TimeToLive.ToString(), gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.To))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "To", serviceBusReceivedMessage.To, gxcontext));

									if (!string.IsNullOrEmpty(serviceBusReceivedMessage.TransactionPartitionKey))
										eventMessageProperties.Add(EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "TransactionPartitionKey", serviceBusReceivedMessage.TransactionPartitionKey, gxcontext));

									//Event

									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageid", serviceBusReceivedMessage.MessageId);
									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedate", serviceBusReceivedMessage.EnqueuedTime.UtcDateTime);
									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedata", serviceBusReceivedMessage.Body.ToString());
									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.ServiceBusMessage);
									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageversion", string.Empty);
									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties", eventMessageProperties);

									//List of Events
									eventMessage.Add(eventMessageItem);

								}
								parametersdata = new object[] { eventMessages, null };
							}

							try
							{
								method.Invoke(objgxproc, parametersdata);
								GxUserType EventMessageResponse = parametersdata[1] as GxUserType;//SdtEventMessageResponse

								//Error handling

								if ((bool)ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Handlefailure") == true) //Must retry
								{
									exMessage = string.Format("{0} {1}", FunctionExceptionType.AppError, ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Errormessage"));
									throw new Exception(exMessage);
								}
								else
								{
									log.LogInformation("(GX function handler) Function finished execution.");
								}
							}
							catch (Exception)
							{
								exMessage = string.Format("{0} Error invoking the GX procedure for processing a batch of messages.", FunctionExceptionType.SysRuntimeError);
								log.LogError(exMessage);
								throw; //Throw the exception so the runtime can Retry the operation.
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed while processing a batch of messages. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError);
						throw new Exception(exMessage);
					}
				}
				catch (Exception)
				{
					log.LogError("{0} Error processing a batch of messages.", FunctionExceptionType.SysRuntimeError);
					throw; //Throw the exception so the runtime can Retry the operation.
				}
			}
			else
			{
				exMessage = string.Format("{0} GeneXus procedure could not be executed while processing a batch of messages. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError);
				throw new Exception(exMessage);
			}

			return Task.CompletedTask;
		}
		internal string GetSerializedMessages(ServiceBusReceivedMessage[] messages)
		{
			
			SerializableServiceBusMessages serializableServiceBusMessages = new SerializableServiceBusMessages();
			foreach (ServiceBusReceivedMessage serviceBusReceivedMessage in messages)
			{
				SerializableServiceBusMessage serializableMessage = new SerializableServiceBusMessage
				{
					MessageId = serviceBusReceivedMessage.MessageId,
					Body = serviceBusReceivedMessage.Body != null ? serviceBusReceivedMessage.Body.ToString() : string.Empty,
					EnqueuedTime = serviceBusReceivedMessage.EnqueuedTime.UtcDateTime,
					ContentType = serviceBusReceivedMessage.ContentType ?? "application/json",
					CorrelationId = serviceBusReceivedMessage.CorrelationId ?? string.Empty,
					SessionId = serviceBusReceivedMessage.SessionId ?? string.Empty,
					DeliveryCount = serviceBusReceivedMessage.DeliveryCount,
					PartitionKey = serviceBusReceivedMessage.PartitionKey ?? string.Empty,
					ReplyToSessionId = serviceBusReceivedMessage.ReplyToSessionId ?? string.Empty,
					ExpiresAt = serviceBusReceivedMessage.ExpiresAt.UtcDateTime,
					LockedUntil = serviceBusReceivedMessage.LockedUntil.UtcDateTime,
					LockToken = serviceBusReceivedMessage.LockToken ?? string.Empty,
					ApplicationProperties = serviceBusReceivedMessage.ApplicationProperties,
					EnqueuedSequenceNumber = serviceBusReceivedMessage.EnqueuedSequenceNumber,
					ScheduledEnqueueTime = serviceBusReceivedMessage.ScheduledEnqueueTime.UtcDateTime,
					TimeToLive = serviceBusReceivedMessage.TimeToLive,
					Subject = serviceBusReceivedMessage.Subject ?? string.Empty

				};
				serializableServiceBusMessages.messages.Add(serializableMessage);
			}
			return JsonSerializer.Serialize(serializableServiceBusMessages);
		}
	}
	[Serializable]
	public class SerializableServiceBusMessages
	{
		public SerializableServiceBusMessages() {
			messages = new List<SerializableServiceBusMessage>();
		}
		public List<SerializableServiceBusMessage> messages { get; set; }
	}

	[Serializable]
	public class SerializableServiceBusMessage
	{
		public SerializableServiceBusMessage()
		{}
		public string MessageId { get; set; }
		public string Body { get; set; }
		public DateTime EnqueuedTime { get; set; }
		public string ContentType { get; set; }
		public string CorrelationId { get; set; }
		public string SessionId { get; set; }
		public int DeliveryCount { get; set; }
		public string PartitionKey { get; set; }
		public string ReplyToSessionId {  get; set; }
		public DateTime ExpiresAt { get; set; }
		public DateTime LockedUntil { get; set; }
		public string LockToken { get; set; }
		public IReadOnlyDictionary<string, object> ApplicationProperties {  get; set; }
		public long EnqueuedSequenceNumber { get; set; }
		public DateTime ScheduledEnqueueTime { get; set; }
		public TimeSpan TimeToLive { get; set; }
		public string Subject { get; set; }
	}
}