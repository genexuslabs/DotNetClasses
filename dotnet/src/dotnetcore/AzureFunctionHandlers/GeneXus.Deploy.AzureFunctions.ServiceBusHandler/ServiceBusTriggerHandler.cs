using System;
using System.IO;
using System.Reflection;
using System.Text;
using GeneXus.Application;
using GeneXus.Deploy.AzureFunctions.ServiceBusHandler.Helpers;
using GeneXus.Utils;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GeneXus.Deploy.AzureFunctions.ServiceBusHandler
{
	public static class ServiceBusTriggerHandler
	{
		private const string MappingsFile = "gxazmappings.json";
		public static void Run(Message myQueueItem, ILogger log, Microsoft.Azure.WebJobs.ExecutionContext context)
		{
			string functionName = context.FunctionName;
			string exMessage = "";

			log.LogInformation($"GeneXus Service Bus trigger handler. Function processed: {functionName}. Queue item Id: {myQueueItem.MessageId}");

			try
			{ 
				//Get json file to know the GX procedure to call
				string mapfilepath = Path.Combine(context.FunctionDirectory, MappingsFile);
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

									if (parameters.Length != 2)
									{
										//Thrown to the Azure monitor
										exMessage = string.Format("{0} Error for Message Id {1}. The number of parameters in GeneXus procedure is not correct.",ServiceBusExceptionType.SysRuntimeError,myQueueItem.MessageId);
										throw new Exception(exMessage); //Send to retry if possible.
									}
									else
									{

										//Two valid signatures for the GX proc:

										//parm(in:&EventMessageCollection, out:&ExternalEventMessageResponse );
										//parm(in:&rawData, out:&ExternalEventMessageResponse );

										GxContext gxcontext = new GxContext();
										Object[] parametersdata;

										if (parameters[0].ParameterType == typeof(string))
										{
											string queueMessageSerialized = JsonConvert.SerializeObject(myQueueItem);
											parametersdata = new object[] { queueMessageSerialized, null };
										}
										else
										{

											//Initialization

											GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
											GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem> CustomPayload = new GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem>(gxcontext, "CustomPayloadItem", "ServerlessAPI");
											GeneXus.Programs.genexusserverlessapi.SdtEventMessage EventMessageItem = new GeneXus.Programs.genexusserverlessapi.SdtEventMessage(gxcontext);
											GeneXus.Programs.genexusserverlessapi.SdtEventMessages EventMessages = new GeneXus.Programs.genexusserverlessapi.SdtEventMessages(gxcontext);

											//Payload

											Type t = myQueueItem.GetType();
											PropertyInfo[] props = t.GetProperties();

											foreach (var prop in props)
												if (prop.GetIndexParameters().Length == 0)
												{
													if ((prop.Name != "Body")  & (prop.Name != "UserProperties") & (prop.Name != "SystemProperties"))
													{ 
													CustomPayloadItem.gxTpr_Propertyid = prop.Name;
													CustomPayloadItem.gxTpr_Propertyvalue = Convert.ToString(prop.GetValue(myQueueItem));
													CustomPayload.Add(CustomPayloadItem, 0);
													CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
													}
												}
											CustomPayloadItem.gxTpr_Propertyid = "Body";
											CustomPayloadItem.gxTpr_Propertyvalue = System.Text.Encoding.UTF8.GetString(myQueueItem.Body); 
											
											CustomPayload.Add(CustomPayloadItem, 0);
											CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);

											//user Properties
											
											foreach (string key in myQueueItem.UserProperties.Keys)
											{
												CustomPayloadItem.gxTpr_Propertyid = key;
												CustomPayloadItem.gxTpr_Propertyvalue = JsonConvert.SerializeObject(myQueueItem.UserProperties[key]);
												CustomPayload.Add(CustomPayloadItem, 0);
												CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
											}

											//System Properties or broker properties

											Type syst = myQueueItem.SystemProperties.GetType();
											PropertyInfo[] sysProps = syst.GetProperties();

											foreach (var prop in sysProps)
												if (prop.GetIndexParameters().Length == 0)
												{
													CustomPayloadItem.gxTpr_Propertyid = prop.Name;
													CustomPayloadItem.gxTpr_Propertyvalue = Convert.ToString(prop.GetValue(myQueueItem.SystemProperties));
													CustomPayload.Add(CustomPayloadItem, 0);
													CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
												}

											//Event
											EventMessageItem.gxTpr_Eventmessageid = myQueueItem.MessageId;
											EventMessageItem.gxTpr_Eventmessagedate = myQueueItem.SystemProperties.EnqueuedTimeUtc;
											EventMessageItem.gxTpr_Eventmessagesourcetype = "ServiceBusMessage";
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
												exMessage = string.Format("{0} {1}", ServiceBusExceptionType.AppError, messageResponse.gxTpr_Errormessage);
												throw new Exception(exMessage);
											}
											else
											{
												log.LogInformation("(GX function handler) Function finished execution.");
											}
										}
										catch (Exception ex)
										{
											exMessage = string.Format("{0} Error invoking the GX procedure for Message Id {1}.", ServiceBusExceptionType.SysRuntimeError, myQueueItem.MessageId);
											log.LogError(exMessage);
											throw (ex); //Throw the exception so the runtime can Retry the operation.
										}
									}

								}
								else
								{
									exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", ServiceBusExceptionType.SysRuntimeError, myQueueItem.MessageId);
									throw new Exception(exMessage);
								}	
							}
							catch (Exception ex)
							{
								log.LogError("{0} Error processing Message Id {1}.", ServiceBusExceptionType.SysRuntimeError, myQueueItem.MessageId);
								throw (ex); //Throw the exception so the runtime can Retry the operation.
							}
						}
						else
						{
							exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", ServiceBusExceptionType.SysRuntimeError, myQueueItem.MessageId);
							throw new Exception(exMessage);
						}
					}
				}
				else
				{
					exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: configuration file not found.", ServiceBusExceptionType.SysRuntimeError, myQueueItem.MessageId);
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
