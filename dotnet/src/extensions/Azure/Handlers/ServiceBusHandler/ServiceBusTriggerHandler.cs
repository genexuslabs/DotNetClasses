using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using GeneXus.Application;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Utils;
using GeneXus.Metadata;
using System.Collections;
using System.Linq;

//https://github.com/Azure/azure-functions-dotnet-worker/issues/384

namespace GeneXus.Deploy.AzureFunctions.ServiceBusHandler
{
	public class ServiceBusTriggerHandler
	{
		private ICallMappings _callmappings;

		public ServiceBusTriggerHandler(ICallMappings callMappings)
		{
			_callmappings = callMappings;
		}
		public void Run(string myQueueItem, FunctionContext context)
		{
			var log = context.GetLogger("ServiceBusTriggerHandler");
			string functionName = context.FunctionDefinition.Name;

			Message message = SetupMessage(context, myQueueItem);
			log.LogInformation($"GeneXus Service Bus trigger handler. Function processed: {functionName}. Queue item Id: {message.MessageId}");

			try
			{
				ProcessMessage(context, log, message);
			}
			catch (Exception ex) //Catch System exception and retry
			{
				log.LogError(ex.ToString());
				throw;
			}
		}
		private GxUserType CreateCustomPayloadItem(Type customPayloadItemType, string propertyId, object propertyValue, GxContext gxContext)
		{
			GxUserType CustomPayloadItem = (GxUserType)Activator.CreateInstance(customPayloadItemType, new object[] { gxContext });
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyid", propertyId);
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyvalue", propertyValue);
			return CustomPayloadItem;

		}
		private Message SetupMessage(FunctionContext context, string item)
		{
			Message message = new Message();
			message.MessageProperties = new List<Message.MessageProperty>();
			message.Body = item;

			if (context.BindingContext.BindingData.TryGetValue("MessageId", out var MessageIdObj) && MessageIdObj != null)
			{
				message.MessageId = MessageIdObj.ToString();

				foreach (string key in context.BindingContext.BindingData.Keys)
				{
					Message.MessageProperty messageProperty = new Message.MessageProperty();
					messageProperty.key = key;

					string valueStr = context.BindingContext.BindingData[key].ToString().Trim('\"');
					DateTime valueDateTime;
					if (DateTime.TryParse(valueStr, out valueDateTime))
					{						
						messageProperty.value = valueDateTime.ToUniversalTime().ToString();
					}
					else
						messageProperty.value = context.BindingContext.BindingData[key].ToString();

					message.MessageProperties.Add(messageProperty);
				}

				if (context.BindingContext.BindingData.TryGetValue("UserProperties", out var customProperties) && customProperties != null)
				{
					var customHeaders = JSONHelper.Deserialize<Dictionary<string, Object>>(customProperties.ToString());

					if (customHeaders != null)
						message.UserProperties = customHeaders;
				}
				if (context.BindingContext.BindingData.TryGetValue("SystemProperties", out var systemProperties) && systemProperties != null)
				{
					Message.SystemPropertiesCollection systemHeaders = new Message.SystemPropertiesCollection();
					systemHeaders = JSONHelper.Deserialize<Message.SystemPropertiesCollection>(systemProperties.ToString());
					if (systemHeaders != null)
						message.SystemProperties = systemHeaders;
				}
			}
			else
			{
				throw new InvalidOperationException();
			}
			return message;
		}
		private void ProcessMessage(FunctionContext context, ILogger log, Message message)
		{
			CallMappings callmap = (CallMappings)_callmappings;
			GxAzMappings map = callmap.mappings is object ? callmap.mappings.First(m => m.FunctionName == context.FunctionDefinition.Name) : null;
			string gxProcedure = map is object ? map.GXEntrypoint : string.Empty;

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

							exMessage = string.Format("{0} Error for Message Id {1}: the number of parameters in GeneXus procedure is not correct.", FunctionExceptionType.SysRuntimeError, message.MessageId);
							throw new ArgumentException(exMessage); //Send to retry if possible.
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
								string queueMessageSerialized = JSONHelper.Serialize(message);
								parametersdata = new object[] { queueMessageSerialized, null };
							}
							else
							{
								//Initialization

								Type EventMessagesType = parameters[0].ParameterType; //SdtEventMessages
								GxUserType EventMessages = (GxUserType)Activator.CreateInstance(EventMessagesType, new object[] { gxcontext }); // instance of SdtEventMessages

								IList EventMessage = (IList)ClassLoader.GetPropValue(EventMessages, "gxTpr_Eventmessage");//instance of GXBaseCollection<SdtEventMessage>
								Type EventMessageItemType = EventMessage.GetType().GetGenericArguments()[0];//SdtEventMessage

								GxUserType EventMessageItem = (GxUserType)Activator.CreateInstance(EventMessageItemType, new object[] { gxcontext }); // instance of SdtEventMessage
								IList CustomPayload = (IList)ClassLoader.GetPropValue(EventMessageItem, "gxTpr_Eventmessagecustompayload");//instance of GXBaseCollection<SdtEventCustomPayload_CustomPayloadItem>

								Type CustomPayloadItemType = CustomPayload.GetType().GetGenericArguments()[0];//SdtEventCustomPayload_CustomPayloadItem

								//Payload
								GxUserType CustomPayloadItem;

								foreach (var messageProp in message.MessageProperties)
								{
									if ((messageProp.key != "UserProperties") & (messageProp.key != "SystemProperties"))
									{
										CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, messageProp.key, Convert.ToString(messageProp.value), gxcontext);
										CustomPayload.Add(CustomPayloadItem);
									}
								}

								//Body

								CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "Body", message.Body, gxcontext);
								CustomPayload.Add(CustomPayloadItem);

								//user Properties
								if (message.UserProperties.Count > 0)
								{
									foreach (string key in message.UserProperties.Keys)
									{
										CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, key, JSONHelper.Serialize(message.UserProperties[key]), gxcontext);
										CustomPayload.Add(CustomPayloadItem);
									}
								}

								//System Properties or broker properties
								if (message.SystemProperties != null)
								{
									Type syst = message.SystemProperties.GetType();
									PropertyInfo[] sysProps = syst.GetProperties();

									foreach (var prop in sysProps)
										if (prop.GetIndexParameters().Length == 0)
										{
											CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, prop.Name, Convert.ToString(prop.GetValue(message.SystemProperties)), gxcontext);
											CustomPayload.Add(CustomPayloadItem);
										}
								}

								//Event

								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessageid", message.MessageId);	
								Message.MessageProperty enqueuedTimeUtcProp = message.MessageProperties.Find(x => x.key == "EnqueuedTimeUtc");
								if (enqueuedTimeUtcProp != null)
								{
									DateTime enqueuedTimeUtc;
									if (DateTime.TryParse(enqueuedTimeUtcProp.value, out enqueuedTimeUtc))
									ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagedate", enqueuedTimeUtc);
								}
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.ServiceBusMessage);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessageversion", string.Empty);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagecustompayload", CustomPayload);

								//List of Events
								EventMessage.Add(EventMessageItem);
								parametersdata = new object[] { EventMessages, null };
								
								try
								{
									method.Invoke(objgxproc, parametersdata);
									GxUserType EventMessageResponse = parametersdata[1] as GxUserType;//SdtEventMessageResponse

									//Error handling

									if ((bool)ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Handled") == false) //Must retry
									{
										exMessage = string.Format("{0} {1}", FunctionExceptionType.AppError, ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Errormessage"));
										throw new ArgumentException(exMessage);
									}
									else
									{
										log.LogInformation("(GX function handler) Function finished execution.");
									}
								}
								catch (Exception)
								{
									exMessage = string.Format("{0} Error invoking the GX procedure for Message Id {1}.", FunctionExceptionType.SysRuntimeError, message.MessageId);
									log.LogError(exMessage);
									throw; //Throw the exception so the runtime can Retry the operation.
								}
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", FunctionExceptionType.SysRuntimeError, message.MessageId);
						throw new ApplicationException(exMessage);
					}
				}
				catch (Exception)
				{
					log.LogError("{0} Error processing Message Id {1}.", FunctionExceptionType.SysRuntimeError, message.MessageId);
					throw; //Throw the exception so the runtime can Retry the operation.
				}
			}
			else
			{
				exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError, message.MessageId);
				throw new ApplicationException(exMessage);
			}
		}
		internal class Message
		{
			public Message()
			{ }

			public string MessageId;
			public IDictionary<string, object> UserProperties { get; set; }
			public SystemPropertiesCollection SystemProperties { get; set; }
			public string Body { get; set; }

			public List<MessageProperty> MessageProperties;
			internal class SystemPropertiesCollection
			{
				public bool IsLockTokenSet { get; }
				public string LockToken { get; }
				public bool IsReceived { get; }
				public int DeliveryCount { get; }
				public DateTime LockedUntilUtc { get; }
				public long SequenceNumber { get; }
				public string DeadLetterSource { get; }
				public long EnqueuedSequenceNumber { get; }
				public DateTime EnqueuedTimeUtc { get; }
			}
			internal class MessageProperty
			{
				public string key { get; set; }
				public string value { get; set; }
				public MessageProperty()
				{ }
			}
		}
	}
}