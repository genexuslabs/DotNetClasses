using System;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using GeneXus.Application;
using GeneXus.Utils;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using System.Collections;
using GeneXus.Metadata;
using System.Linq;

namespace GeneXus.Deploy.AzureFunctions.TimerHandler
{
    public class TimerTriggerHandler
    {
		private ICallMappings _callmappings;

		public TimerTriggerHandler(ICallMappings callMappings)
		{
			_callmappings = callMappings;
		}
		public void Run(TimerInfo timerInfo, FunctionContext context) 
		{
			var logger = context.GetLogger("TimerTriggerHandler");
			string functionName = context.FunctionDefinition.Name;
			Guid messageId = new Guid(context.InvocationId);
			logger.LogInformation($"GeneXus Timer trigger handler. Function processed: {functionName}. Message Id: {messageId}. Function executed at: {DateTime.Now}. Next timer schedule at: {timerInfo.ScheduleStatus.Next}");

			try
			{
				ProcessMessage(context, logger, timerInfo, messageId.ToString());
			}
			catch (Exception ex) //Catch System exception and retry
			{
				logger.LogError(ex.ToString());
				throw;
			}
		}
		private void ProcessMessage(FunctionContext context, ILogger log, TimerInfo TimerInfo, string messageId)
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

							exMessage = string.Format("{0} Error for Message Id {1}: the number of parameters in GeneXus procedure is not correct.", FunctionExceptionType.SysRuntimeError, messageId);
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
								string queueMessageSerialized = JSONHelper.Serialize(TimerInfo);
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

								GxUserType eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "ScheduleStatusNext", TimerInfo.ScheduleStatus.Next.ToUniversalTime().ToString(), gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "ScheduleStatusLast", TimerInfo.ScheduleStatus.Last.ToUniversalTime().ToString(), gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "ScheduleStatusLastUpdated", TimerInfo.ScheduleStatus.LastUpdated.ToUniversalTime().ToString(), gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "IsPastDue", TimerInfo.IsPastDue.ToString(), gxcontext);
								eventMessageProperties.Add(eventMessageProperty);

								//Event

								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageid", messageId.ToString());
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.Timer);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedate", DateTime.UtcNow);
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
								bool result = (bool)ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Handlefailure");

								//Error handling

								if (result == true) //Must retry
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
								log.LogError("{0} Error invoking the GX procedure for Message Id {1}.", FunctionExceptionType.SysRuntimeError, messageId);
								throw; //Throw the exception so the runtime can Retry the operation.
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", FunctionExceptionType.SysRuntimeError, messageId);
						throw new Exception(exMessage);
					}
				}
				catch (Exception)
				{
					log.LogError("{0} Error processing Message Id {1}.", FunctionExceptionType.SysRuntimeError, messageId);
					throw; //Throw the exception so the runtime can Retry the operation.
				}
			}
			else
			{
				exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError, messageId);
				throw new Exception(exMessage);
			}
		}
	}	
}
