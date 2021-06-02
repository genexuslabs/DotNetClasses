using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using GeneXus.Application;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Utils;
using GeneXus.Metadata;
using System.Collections;

//https://github.com/Azure/azure-functions-dotnet-worker/issues/384

namespace GeneXus.Deploy.AzureFunctions.ServiceBusHandler
{
	public static class ServiceBusTriggerHandler
	{
		public static void Run(string myQueueItem, FunctionContext context)
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
				throw ex;
			}
		}
		private static GxUserType CreateCustomPayloadItem(Type customPayloadItemType, string propertyId, object propertyValue, GxContext gxContext)
		{
			GxUserType CustomPayloadItem = (GxUserType)Activator.CreateInstance(customPayloadItemType, new object[] { gxContext });
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyid", propertyId);
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyvalue", propertyValue);
			return CustomPayloadItem;

		}
		private static Message SetupMessage(FunctionContext context, string item)
		{
			Message message = new Message();
			message.Body = System.Text.Encoding.UTF8.GetBytes(item);

			if (context.BindingContext.BindingData.TryGetValue("MessageId", out var messageIdObj) && messageIdObj != null)
			{
				message.MessageId = messageIdObj.ToString();

				if (context.BindingContext.BindingData.TryGetValue("Label", out var LabelObj) && LabelObj != null)
				{
					message.Label = LabelObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("Size", out var SizeObj) && SizeObj != null)
				{
					message.Size = SizeObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("ScheduledEnqueueTimeUtc", out var ScheduledEnqueueTimeUtcObj) && ScheduledEnqueueTimeUtcObj != null)
				{
					message.ScheduledEnqueueTimeUtc = ScheduledEnqueueTimeUtcObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("ReplyTo", out var ReplyToObj) && ReplyToObj != null)
				{
					message.ReplyTo = ReplyToObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("ContentType", out var ContentTypeObj) && ContentTypeObj != null)
				{
					message.ContentType = ContentTypeObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("To", out var ToObj) && ToObj != null)
				{
					message.To = ToObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("CorrelationId", out var CorrelationIdObj) && CorrelationIdObj != null)
				{
					message.CorrelationId = CorrelationIdObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("TimeToLive", out var TimeToLiveObj) && TimeToLiveObj != null)
				{
					message.TimeToLive = TimeToLiveObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("ReplyToSessionId", out var ReplyToSessionIdObj) && ReplyToSessionIdObj != null)
				{
					message.ReplyToSessionId = ReplyToSessionIdObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("SessionId", out var SessionIdObj) && SessionIdObj != null)
				{
					message.SessionId = SessionIdObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("ViaPartitionKey", out var ViaPartitionKeyObj) && ViaPartitionKeyObj != null)
				{
					message.ViaPartitionKey = ViaPartitionKeyObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("PartitionKey", out var PartitionKeyObj) && PartitionKeyObj != null)
				{
					message.PartitionKey = PartitionKeyObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("ExpiresAtUtc", out var ExpiresAtUtcObj) && ExpiresAtUtcObj != null)
				{
					message.ExpiresAtUtc = ExpiresAtUtcObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("UserProperties", out var customProperties) && customProperties != null)
				{
					var customHeaders = JsonConvert.DeserializeObject<Dictionary<string, Object>>(customProperties.ToString());

					if (customHeaders != null)
						message.UserProperties = customHeaders;
				}
				if (context.BindingContext.BindingData.TryGetValue("SystemProperties", out var systemProperties) && systemProperties != null)
				{
					Message.SystemPropertiesCollection systemHeaders = new Message.SystemPropertiesCollection();
					systemHeaders = System.Text.Json.JsonSerializer.Deserialize<Message.SystemPropertiesCollection>(systemProperties.ToString());
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
		private static void ProcessMessage(FunctionContext context, ILogger log, Message message)
		{
			string gxProcedure = FunctionReferences.GetFunctionEntryPoint(context, log, message.MessageId);
			log.LogInformation($"gxprocedure {gxProcedure}");

			string exMessage;
			if (string.IsNullOrEmpty(gxProcedure))
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
								string queueMessageSerialized = JsonConvert.SerializeObject(message);
								parametersdata = new object[] { queueMessageSerialized, null };
							}
							else
							{
								//Initialization

								Type CustomPayloadItemType = ClassLoader.FindType(FunctionReferences.GeneXusServerlessAPIAssembly, FunctionReferences.EventCustomPayloadItemFullClassName, null);
								Type baseCollection = typeof(GXBaseCollection<>);
								IList CustomPayload = (IList)Activator.CreateInstance(baseCollection.MakeGenericType(CustomPayloadItemType), new object[] { gxcontext, "CustomPayloadItem", "ServerlessAPI" });

								GxUserType EventMessageItem = (GxUserType)ClassLoader.GetInstance(FunctionReferences.GeneXusServerlessAPIAssembly, FunctionReferences.EventMessageFullClassName, new object[] { gxcontext });
								GxUserType EventMessages = (GxUserType)ClassLoader.GetInstance(FunctionReferences.GeneXusServerlessAPIAssembly, FunctionReferences.EventMessagesFullClassName, new object[] { gxcontext });

								//Payload

								Type t = message.GetType();
								PropertyInfo[] props = t.GetProperties();
								GxUserType CustomPayloadItem;

								foreach (var prop in props)
									if (prop.GetIndexParameters().Length == 0)
									{
										if ((prop.Name != "Body") & (prop.Name != "UserProperties") & (prop.Name != "SystemProperties"))
										{
											CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, prop.Name, Convert.ToString(prop.GetValue(message)), gxcontext);
											CustomPayload.Add(CustomPayloadItem);
										}
									}

								CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "Body", Encoding.UTF8.GetString(message.Body), gxcontext);
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
								if (message.SystemProperties != null)
									ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagedate", message.SystemProperties.EnqueuedTimeUtc);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.ServiceBusMessage);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessageversion", string.Empty);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagecustompayload", CustomPayload);

								//List of Events
								IList events = (IList)ClassLoader.GetPropValue(EventMessages, "gxTpr_Eventmessage");
								parametersdata = new object[] { EventMessages, null };
								
								try
								{
									method.Invoke(objgxproc, parametersdata);
									GxUserType EventMessageResponse = (GxUserType)ClassLoader.GetInstance(FunctionReferences.GeneXusServerlessAPIAssembly, FunctionReferences.EventMessageResponseFullClassName, new object[] { gxcontext });
									EventMessageResponse = (GxUserType)parametersdata[1];

									//Error handling

									if ((bool)ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Handled") == false) //Must retry
									{
										exMessage = string.Format("{0} {1}", FunctionExceptionType.AppError, ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Errormessage"));
										throw new Exception(exMessage);
									}
									else
									{
										log.LogInformation("(GX function handler) Function finished execution.");
									}
								}
								catch (Exception ex)
								{
									exMessage = string.Format("{0} Error invoking the GX procedure for Message Id {1}.", FunctionExceptionType.SysRuntimeError, message.MessageId);
									log.LogError(exMessage);
									throw (ex); //Throw the exception so the runtime can Retry the operation.
								}
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", FunctionExceptionType.SysRuntimeError, message.MessageId);
						throw new Exception(exMessage);
					}
				}
				catch (Exception ex)
				{
					log.LogError("{0} Error processing Message Id {1}.", FunctionExceptionType.SysRuntimeError, message.MessageId);
					throw (ex); //Throw the exception so the runtime can Retry the operation.
				}
			}
			else
			{
				exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError, message.MessageId);
				throw new Exception(exMessage);
			}
		}
		internal class Message
		{
			public IDictionary<string, object> UserProperties { get; set; }
			public string Size { get; set; }
			public string ScheduledEnqueueTimeUtc { get; set; }
			public string ReplyTo { get; set; }
			public string ContentType { get; set; }
			public string To { get; set; }
			public string Label { get; set; }
			public string CorrelationId { get; set; }
			public string TimeToLive { get; set; }
			public string ReplyToSessionId { get; set; }
			internal SystemPropertiesCollection SystemProperties { get; set; }
			public string SessionId { get; set; }
			public string ViaPartitionKey { get; set; }
			public string PartitionKey { get; set; }
			public string MessageId { get; set; }
			public byte[] Body { get; set; }
			public string ExpiresAtUtc { get; set; }
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
		}
	}
}