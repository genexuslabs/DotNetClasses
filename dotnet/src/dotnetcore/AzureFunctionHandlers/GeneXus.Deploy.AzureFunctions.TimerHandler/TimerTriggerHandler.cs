using System;
using Microsoft.Azure.WebJobs;
using System.IO;
using System.Reflection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Text;
using GeneXus.Application;
using GeneXus.Utils;
using GeneXus.Deploy.AzureFunctions.TimerHandler.Helpers;
using Newtonsoft.Json;

namespace GeneXus.Deploy.AzureFunctions.TimerHandler
{
    public static class TimerTriggerHandler
    {
        public static void Run(TimerInfo TimerInfo, ILogger log, Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            string functionName = context.FunctionName;
			string exMessage = "";
			Guid messageId = context.InvocationId;
			
			log.LogInformation($"GeneXus Timer trigger handler. Function processed: {functionName}. Message Id: {messageId}. Function executed at: {DateTime.Now}");

			//Get json file to know the GX procedure to call
			try
			{
				string mapfilepath = Path.Combine(context.FunctionDirectory, "gxazmappings.json");
				if (File.Exists(mapfilepath))
				{
					using (StreamReader r = new StreamReader(mapfilepath))
					{
						string gxazmappings = r.ReadToEnd();

						gxazMappings _gxmappings = Newtonsoft.Json.JsonConvert.DeserializeObject<gxazMappings>(gxazmappings);

						string gxprocedure = _gxmappings.GXEntrypoint;

						if (gxprocedure != "")
						{
							try
							{
								StringBuilder sb1 = new StringBuilder(gxprocedure);
								sb1.Append(".dll");
								string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), sb1.ToString());
								Assembly obj = Assembly.LoadFile(path);

								StringBuilder sb2 = new StringBuilder("GeneXus.Programs.");
								sb2.Append(gxprocedure);

								Type objexec = obj.GetType(sb2.ToString());

								if (objexec != null)
								{
									object objgxproc = Activator.CreateInstance(objexec);
									var method = objexec.GetMethod("execute");
									ParameterInfo[] parameters = method.GetParameters();
									
									//Check parameters

									//Two valid signatures for the GX proc:

									//parm(in:&EventMessageCollection, out:&ExternalEventMessageResponse );
									//parm(in:&rawData, out:&ExternalEventMessageResponse );

									if (parameters.Length != 2)
									{
										//Thrown to the Azure monitor
										exMessage = string.Format("{0} Error for Message Id {1}. The number of parameters in GeneXus procedure is not correct.", TimerExceptionType.SysRuntimeError, messageId);
										log.LogError(exMessage);
										
									}

									else

									{
										GxContext gxcontext = new GxContext();
										Object[] parametersdata;
										if (parameters[0].ParameterType == typeof(string))
										{
											string messageSerialized = JsonConvert.SerializeObject(TimerInfo);
											parametersdata = new object[] { messageSerialized, null };
										}
										else
										{ 

										GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
										GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem> CustomPayload = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem>(gxcontext, "CustomPayloadItem", "ServerlessAPI");
										GeneXus.Programs.genexusserverlessapi.SdtEventMessage EventMessageItem = new GeneXus.Programs.genexusserverlessapi.SdtEventMessage(gxcontext);
										GeneXus.Programs.genexusserverlessapi.SdtEventMessages EventMessages = new GeneXus.Programs.genexusserverlessapi.SdtEventMessages(gxcontext);

										//Payload

										CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
										CustomPayloadItem.gxTpr_Propertyid = "ScheduleStatusNext";
										CustomPayloadItem.gxTpr_Propertyvalue = TimerInfo.ScheduleStatus.Next.ToString();
										CustomPayload.Add(CustomPayloadItem, 0);

										CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
										CustomPayloadItem.gxTpr_Propertyid = "ScheduleStatusLast";
										CustomPayloadItem.gxTpr_Propertyvalue = TimerInfo.ScheduleStatus.Last.ToString();
										CustomPayload.Add(CustomPayloadItem, 0);

										CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
										CustomPayloadItem.gxTpr_Propertyid = "ScheduleStatusLastUpdated";
										CustomPayloadItem.gxTpr_Propertyvalue = TimerInfo.ScheduleStatus.LastUpdated.ToString();
										CustomPayload.Add(CustomPayloadItem, 0);

										CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
										CustomPayloadItem.gxTpr_Propertyid = "IsPastDue";
										CustomPayloadItem.gxTpr_Propertyvalue = TimerInfo.IsPastDue.ToString();
										CustomPayload.Add(CustomPayloadItem, 0);

										//Event
										EventMessageItem.gxTpr_Eventmessageid = messageId.ToString();
										EventMessageItem.gxTpr_Eventmessagesourcetype = "Timer";
										EventMessageItem.gxTpr_Eventmessageversion = "";
										EventMessageItem.gxTpr_Eventmessagecustompayload = CustomPayload;

										//List of Events
										EventMessages.gxTpr_Eventmessage.Add(EventMessageItem, 0);

										parametersdata = new object[] { EventMessages, null };
										}

										try
										{
											method.Invoke(objgxproc, parametersdata);
											
											GeneXus.Programs.genexusserverlessapi.SdtEventMessageResponse messageResponse = (GeneXus.Programs.genexusserverlessapi.SdtEventMessageResponse)parametersdata[1];
											//Error handling

											if (messageResponse.gxTpr_Handled == false) //Must retry
											{
												exMessage = string.Format("{0} {1}", TimerExceptionType.AppError, messageResponse.gxTpr_Errormessage);
												throw new Exception(exMessage);
											}
											else
											{
												log.LogInformation("(GX function handler) Function finished execution.");
											}
											
										}
										catch (Exception ex)
										{
											exMessage = string.Format("{0} Error invoking the GX procedure for Message Id {1}.", TimerExceptionType.SysRuntimeError, messageId);
											log.LogError(exMessage);
											throw (ex); //Throw the exception so the runtime can Retry the operation.
										}
									}		
								}
								else
								{
									exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", TimerExceptionType.SysRuntimeError, messageId);
									throw new Exception(exMessage);
								}
							}
							catch (Exception ex)
							{
								log.LogError("{0} Error processing Message Id {1}.", TimerExceptionType.SysRuntimeError, messageId);
								throw (ex); //Throw the exception so the runtime can Retry the operation.
							}
						}
						else
						{
							exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", TimerExceptionType.SysRuntimeError, messageId);
							throw new Exception(exMessage);
						}
					}
				}
				else
				{
					exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: configuration file not found.", TimerExceptionType.SysRuntimeError, messageId);
					throw new Exception(exMessage);
				}
			}
			catch (Exception ex) //Catch System exception and retry
			{
				log.LogError(ex.ToString());
				throw (ex);
			}
		}
	}
}
