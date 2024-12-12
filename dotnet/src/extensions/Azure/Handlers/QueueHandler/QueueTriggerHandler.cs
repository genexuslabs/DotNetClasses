using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Azure.Storage.Queues.Models;
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

		public void Run(QueueMessage queueMessage, FunctionContext context)
		{
			var log = context.GetLogger("QueueTriggerHandler");
			string functionName = context.FunctionDefinition.Name;

			log.LogInformation($"GeneXus Queue trigger handler. Function processed: {functionName} Invocation Id: {context.InvocationId}. Queue item : {StringUtil.Sanitize(queueMessage.MessageId, StringUtil.LogUserEntryWhiteList)}");

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
		private void ProcessMessage(FunctionContext context, ILogger log, QueueMessage queueMessage)
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
				gxProcedure = (map != null && map is object) ? map.GXEntrypoint : string.Empty;
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

							exMessage = string.Format("{0} Error for Message Id {1}: the number of parameters in GeneXus procedure is not correct.", FunctionExceptionType.SysRuntimeError, queueMessage.MessageId);
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

								eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType,"Id", queueMessage.MessageId, gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								if (queueMessage.InsertedOn != null)
								{
									eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "InsertionTime", queueMessage.InsertedOn.Value.UtcDateTime.ToString(), gxcontext);
									eventMessageProperties.Add(eventMessageProperty);
								}

								if (queueMessage.ExpiresOn != null)
								{
									eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "ExpirationTime", queueMessage.ExpiresOn.Value.UtcDateTime.ToString(), gxcontext);
									eventMessageProperties.Add(eventMessageProperty);
								}

								if (queueMessage.NextVisibleOn != null)
								{
									eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "NextVisibleTime", queueMessage.NextVisibleOn.Value.UtcDateTime.ToString(), gxcontext);
									eventMessageProperties.Add(eventMessageProperty);
								}

								eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "DequeueCount", queueMessage.DequeueCount.ToString(), gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								if (queueMessage.PopReceipt != null)
								{ 
								eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "PopReceipt", queueMessage.PopReceipt, gxcontext);
								eventMessageProperties.Add(eventMessageProperty);
								}

								//Body

								eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "Body", queueMessage.Body.ToString(), gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								//Event

								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageid", queueMessage.MessageId);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedata", queueMessage.Body.ToString());

								if (queueMessage.InsertedOn != null	)
									ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedate", queueMessage.InsertedOn.Value.UtcDateTime);

								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.QueueMessage);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageversion", "0.1.0");
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
								log.LogError("{0} Error invoking the GX procedure for Message Id {1}.", FunctionExceptionType.SysRuntimeError, StringUtil.Sanitize(queueMessage.MessageId, StringUtil.LogUserEntryWhiteList));
								throw; //Throw the exception so the runtime can Retry the operation.
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", FunctionExceptionType.SysRuntimeError, StringUtil.Sanitize(queueMessage.MessageId, StringUtil.LogUserEntryWhiteList));
						throw new Exception(exMessage);
					}
				}
				catch (Exception)
				{
					log.LogError("{0} Error processing Message Id {1}.", FunctionExceptionType.SysRuntimeError, StringUtil.Sanitize(queueMessage.MessageId, StringUtil.LogUserEntryWhiteList));
					throw; //Throw the exception so the runtime can Retry the operation.
				}
			}
			else
			{
				exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError, queueMessage.MessageId);
				throw new Exception(exMessage);
			}
		}
	}
}
