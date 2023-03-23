using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using GeneXus.Application;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Metadata;
using GeneXus.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
		private GxUserType CreateEventMessageProperty(Type eventMessPropsItemType, string propertyId, object propertyValue, GxContext gxContext)
		{
			GxUserType eventMessageProperty = (GxUserType)Activator.CreateInstance(eventMessPropsItemType, new object[] { gxContext }); // instance of SdtEventMessageProperty
			ClassLoader.SetPropValue(eventMessageProperty, "gxTpr_Propertyid", propertyId);
			ClassLoader.SetPropValue(eventMessageProperty, "gxTpr_Propertyvalue", propertyValue);
			return eventMessageProperty;
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
			GxAzMappings map = (callmap!=null && callmap.mappings is object) ? callmap.mappings.First(m => m.FunctionName == context.FunctionDefinition.Name) : null;
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
							throw new Exception(exMessage); //Send to retry if possible.
						}
						else
						{
							//Two valid signatures for the GX procedure:
							//parm(in:&EventMessageCollection, out:&ExternalEventMessageResponse );
							//parm(in:&rawData, out:&ExternalEventMessageResponse );

							GxContext gxcontext = new GxContext();
							Object[] parametersdata;
							parametersdata = new object[] { null };

							if (parameters[0].ParameterType == typeof(string))
							{
								string queueMessageSerialized = JSONHelper.Serialize(message);
								parametersdata = new object[] { queueMessageSerialized, null };
							}
							else
							{
								//Initialization

								Type eventMessagesType = parameters[0].ParameterType; //SdtEventMessages
								GxUserType eventMessages = (GxUserType)Activator.CreateInstance(eventMessagesType, new object[] { gxcontext }); // instance of SdtEventMessages

								IList eventMessage = (IList)ClassLoader.GetPropValue(eventMessages, "gxTpr_Eventmessage");//instance of GXBaseCollection<SdtEventMessage>
								Type eventMessageItemType = eventMessage.GetType().GetGenericArguments()[0];//SdtEventMessage

								GxUserType eventMessageItem = (GxUserType)Activator.CreateInstance(eventMessageItemType, new object[] { gxcontext }); // instance of SdtEventMessage

								IList eventMessageProperties = (IList)ClassLoader.GetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties");//instance of GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty>
								Type eventMessPropsItemType = eventMessageProperties.GetType().GetGenericArguments()[0];//SdtEventMessageProperty								

								//Payload
								GxUserType eventMessageProperty;

								foreach (var messageProp in message.MessageProperties)
								{
									if ((messageProp.key != "UserProperties") & (messageProp.key != "SystemProperties"))
									{
										eventMessageProperty = CreateEventMessageProperty(eventMessPropsItemType, messageProp.key, Convert.ToString(messageProp.value), gxcontext);
										eventMessageProperties.Add(eventMessageProperty);
									}
								}

								//Body

								eventMessageProperty = CreateEventMessageProperty(eventMessPropsItemType, "Body", message.Body, gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								//user Properties
								if (message.UserProperties.Count > 0)
								{
									foreach (string key in message.UserProperties.Keys)
									{
										eventMessageProperty = CreateEventMessageProperty(eventMessPropsItemType, key, JSONHelper.Serialize(message.UserProperties[key]), gxcontext);
										eventMessageProperties.Add(eventMessageProperty);
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
											eventMessageProperty = CreateEventMessageProperty(eventMessPropsItemType, prop.Name, Convert.ToString(prop.GetValue(message.SystemProperties)), gxcontext);
											eventMessageProperties.Add(eventMessageProperty);
										}
								}

								//Event

								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageid", message.MessageId);	
								Message.MessageProperty enqueuedTimeUtcProp = message.MessageProperties.Find(x => x.key == "EnqueuedTimeUtc");
								if (enqueuedTimeUtcProp != null)
								{
									DateTime enqueuedTimeUtc;
									if (DateTime.TryParse(enqueuedTimeUtcProp.value, out enqueuedTimeUtc))
									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedate", enqueuedTimeUtc);
								}
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.ServiceBusMessage);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageversion", string.Empty);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties", eventMessageProperties);

								//List of Events
								eventMessage.Add(eventMessageItem);
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
								exMessage = string.Format("{0} Error invoking the GX procedure for Message Id {1}.", FunctionExceptionType.SysRuntimeError, message.MessageId);
								log.LogError(exMessage);
								throw; //Throw the exception so the runtime can Retry the operation.
							}	
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", FunctionExceptionType.SysRuntimeError, message.MessageId);
						throw new Exception(exMessage);
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
				throw new Exception(exMessage);
			}
		}
		[DataContract]
		internal class Message
		{
			public Message()
			{ }

			[DataMember]
			public string MessageId;

			[DataMember]
			public IDictionary<string, object> UserProperties { get; set; }

			[DataMember]
			public SystemPropertiesCollection SystemProperties { get; set; }

			[DataMember]
			public string Body { get; set; }

			[DataMember]
			public List<MessageProperty> MessageProperties;

			[DataContract]
			internal class SystemPropertiesCollection
			{
				[DataMember]
				public bool IsLockTokenSet { get; }

				[DataMember]
				public string LockToken { get; }

				[DataMember]
				public bool IsReceived { get; }

				[DataMember]
				public int DeliveryCount { get; }

				[DataMember]
				public DateTime LockedUntilUtc { get; }

				[DataMember]
				public long SequenceNumber { get; }

				[DataMember]
				public string DeadLetterSource { get; }

				[DataMember]
				public long EnqueuedSequenceNumber { get; }

				[DataMember]
				public DateTime EnqueuedTimeUtc { get; }
			}

			[DataContract]
			internal class MessageProperty
			{
				[DataMember]
				public string key { get; set; }

				[DataMember]
				public string value { get; set; }
				public MessageProperty()
				{ }
			}
		}
	}
}