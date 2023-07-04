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

			if (context.BindingContext.BindingData.TryGetValue("Id", out var messageIdObj) && messageIdObj != null)
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

							exMessage = string.Format("{0} Error for Message Id {1}: the number of parameters in GeneXus procedure is not correct.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
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
								string queueMessageSerialized = JSONHelper.Serialize(queueMessage);
								parametersdata = new object[] { queueMessageSerialized, null };
							}
							else
							{
								//Initialization

								Type eventMessagesType = parameters[0].ParameterType; //SdtEventMessages
								GxUserType EventMessages = (GxUserType)Activator.CreateInstance(eventMessagesType, new object[] { gxcontext }); // instance of SdtEventMessages

								IList eventMessage = (IList)ClassLoader.GetPropValue(EventMessages, "gxTpr_Eventmessage");//instance of GXBaseCollection<SdtEventMessage>
								Type EventMessageItemType = eventMessage.GetType().GetGenericArguments()[0];//SdtEventMessage

								GxUserType eventMessageItem = (GxUserType)Activator.CreateInstance(EventMessageItemType, new object[] { gxcontext }); // instance of SdtEventMessage

								IList eventMessageProperties = (IList)ClassLoader.GetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties");//instance of GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty>
								Type eventMessPropsItemType = eventMessageProperties.GetType().GetGenericArguments()[0];//SdtEventMessageProperty								

								//Event message properties

								GxUserType eventMessageProperty;

								foreach (var messageProp in queueMessage.MessageProperties)
								{
									eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, messageProp.key, messageProp.value, gxcontext);
									eventMessageProperties.Add(eventMessageProperty);
								}

								//Body

								eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "Body", queueMessage.Body, gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								//Event

								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageid", queueMessage.Id);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedata", queueMessage.Body);

								QueueMessage.MessageProperty InsertionTimeProp = queueMessage.MessageProperties.Find(x => x.key == "InsertionTime");
								if (InsertionTimeProp != null)
								{
									DateTime InsertionTime;
									if (DateTime.TryParse(InsertionTimeProp.value, out InsertionTime))
									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedate", InsertionTime.ToUniversalTime());
								}
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.QueueMessage);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageversion", string.Empty);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties", eventMessageProperties);

								//List of Events
								eventMessage.Add(eventMessageItem);
								parametersdata = new object[] { EventMessages, null };
							}
							try
							{
								method.Invoke(objgxproc, parametersdata);
								GxUserType EventMessageResponse = parametersdata[1] as GxUserType;//SdtEventMessageResponse
								bool result = (bool)ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Handlefailure");


								//Error handling

								if (result == true) //Must retry if possible.
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
								log.LogError("{0} Error invoking the GX procedure for Message Id {1}.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
								throw; //Throw the exception so the runtime can Retry the operation.
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
						throw new Exception(exMessage);
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
				throw new Exception(exMessage);
			}
		}

		[DataContract]
		internal class QueueMessage
		{
			public QueueMessage()
			{ }
			[DataMember]
			public List<MessageProperty> MessageProperties;

			[DataMember]
			public string Id { get; set; }

			[DataMember]
			public string Body { get; set; }

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
