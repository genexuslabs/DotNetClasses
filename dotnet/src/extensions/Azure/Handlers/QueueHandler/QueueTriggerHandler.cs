using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GeneXus.Application;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Metadata;
using GeneXus.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GeneXus.Deploy.AzureFunctions.QueueHandler
{
	public class QueueTriggerHandler
	{
		private ICallMappings _callmappings;

		public QueueTriggerHandler(ICallMappings callMappings)
		{
			_callmappings = callMappings;
		}

		public void Run(string myQueueItem, FunctionContext context)
		{
			var log = context.GetLogger("QueueTriggerHandler");
			string functionName = context.FunctionDefinition.Name;

			QueueMessage queueMessage = SetupMessage(context, myQueueItem);
			log.LogInformation($"GeneXus Queue trigger handler. Function processed: {functionName} Invocation Id: {context.InvocationId}. Queue item : {queueMessage.Id}");

			try
			{
				ProcessMessage(context, log, queueMessage);
			}
			catch (Exception ex) //Catch System exception and retry
			{
				log.LogError(ex.ToString());
				throw;
			}		
		}
		private QueueMessage SetupMessage(FunctionContext context, string item)
		{
			QueueMessage message = new QueueMessage();
			message.MessageProperties = new List<QueueMessage.MessageProperty>();
			message.Body = item;

			if (context.BindingContext.BindingData.TryGetValue("Id", out object messageIdObj) && messageIdObj != null)
			{
				message.Id = messageIdObj.ToString();
		
				foreach (string key in context.BindingContext.BindingData.Keys)
				{
					QueueMessage.MessageProperty messageProperty = new QueueMessage.MessageProperty();
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
			}
			else
			{
				throw new InvalidOperationException();
			}
			return message;
		}
		private void ProcessMessage(FunctionContext context, ILogger log, QueueMessage queueMessage)
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

							exMessage = string.Format("{0} Error for Message Id {1}: the number of parameters in GeneXus procedure is not correct.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
							throw new ArgumentException(exMessage); //Send to retry if possible.
						}
						else
						{
							//Two valid signatures for the GX procedure:
							//parm(in:&EventMessageCollection, out:&ExternalEventMessageResponse );
							//parm(in:&rawData, out:&ExternalEventMessageResponse );

							GxContext gxcontext = new GxContext();
							object[] parametersdata;
			
							if (parameters[0].ParameterType == typeof(string))
							{
								string queueMessageSerialized = JSONHelper.Serialize(queueMessage);
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

								foreach (var messageProp in queueMessage.MessageProperties)
								{
									CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, messageProp.key, messageProp.value, gxcontext);
									CustomPayload.Add(CustomPayloadItem);	
								}

								//Body

								CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "Body", queueMessage.Body, gxcontext);
								CustomPayload.Add(CustomPayloadItem);

								//Event

								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessageid", queueMessage.Id);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagedata", queueMessage.Body);

								QueueMessage.MessageProperty InsertionTimeProp = queueMessage.MessageProperties.Find(x => x.key == "InsertionTime");
								if (InsertionTimeProp != null)
								{
									DateTime InsertionTime;
									if (DateTime.TryParse(InsertionTimeProp.value, out InsertionTime))
									ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagedate", InsertionTime.ToUniversalTime());
								}
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.QueueMessage);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessageversion", string.Empty);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagecustompayload", CustomPayload);

								//List of Events
								EventMessage.Add(EventMessageItem);
								parametersdata = new object[] { EventMessages, null };
							}
							try
							{
								method.Invoke(objgxproc, parametersdata);
								GxUserType EventMessageResponse = parametersdata[1] as GxUserType;//SdtEventMessageResponse
								bool result = (bool)ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Handled");

								//Error handling

								if (result == false) //Must retry if possible.
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
								log.LogError("{0} Error invoking the GX procedure for Message Id {1}.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
								throw; //Throw the exception so the runtime can Retry the operation.
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
						throw new ApplicationException(exMessage);
					}
				}
				catch (Exception)
				{
					log.LogError("{0} Error processing Message Id {1}.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
					throw; //Throw the exception so the runtime can Retry the operation.
				}
			}
			else
			{
				exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
				throw new ApplicationException(exMessage);
			}
		}
		private GxUserType CreateCustomPayloadItem(Type customPayloadItemType, string propertyId, object propertyValue, GxContext gxContext)
		{
			GxUserType CustomPayloadItem = (GxUserType)Activator.CreateInstance(customPayloadItemType, new object[] { gxContext });
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyid", propertyId);
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyvalue", propertyValue);
			return CustomPayloadItem;

		}
		internal class QueueMessage
		{
			public QueueMessage()
			{ }

			public List<MessageProperty> MessageProperties;		
			public string Id { get; set; }
			public string Body { get; set; }
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
