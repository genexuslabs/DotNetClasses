using System;
using System.IO;
using System.Reflection;
using System.Text;
using GeneXus.Application;
using GeneXus.Deploy.AzureFunctions.QueueHandler.Helpers;
using GeneXus.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GeneXus.Deploy.AzureFunctions.QueueHandler
{
	public static class QueueTriggerHandler
	{
		private const string MappingsFile = "gxazmappings.json";
		public static void Run(CloudQueueMessage myQueueItem, ILogger log, Microsoft.Azure.WebJobs.ExecutionContext context)
		{
			string functionName = context.FunctionName;
			string exMessage = "";

			log.LogInformation($"GeneXus Queue trigger handler. Function processed: {functionName}. Queue item Id : {myQueueItem.Id}");
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
										exMessage = string.Format("{0} Error for Message Id {1}: the number of parameters in GeneXus procedure is not correct.", QueueExceptionType.SysRuntimeError, myQueueItem.Id);
										throw new Exception(exMessage); //Send to retry if possible.
									}
									else
									{
										//Two valid signatures for the GX procedure:
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

											CustomPayloadItem.gxTpr_Propertyid = "Id";
											CustomPayloadItem.gxTpr_Propertyvalue = myQueueItem.Id;
											CustomPayload.Add(CustomPayloadItem, 0);

											CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
											CustomPayloadItem.gxTpr_Propertyid = "DequeueCount";
											CustomPayloadItem.gxTpr_Propertyvalue = myQueueItem.DequeueCount.ToString();
											CustomPayload.Add(CustomPayloadItem, 0);

											CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
											CustomPayloadItem.gxTpr_Propertyid = "ExpirationTime";
											CustomPayloadItem.gxTpr_Propertyvalue = myQueueItem.ExpirationTime.Value.UtcDateTime.ToString();
											CustomPayload.Add(CustomPayloadItem, 0);

											CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
											CustomPayloadItem.gxTpr_Propertyid = "InsertionTime";
											CustomPayloadItem.gxTpr_Propertyvalue = myQueueItem.InsertionTime.Value.UtcDateTime.ToString();
											CustomPayload.Add(CustomPayloadItem, 0);

											CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
											CustomPayloadItem.gxTpr_Propertyid = "NextVisibleTime";
											CustomPayloadItem.gxTpr_Propertyvalue = myQueueItem.NextVisibleTime.Value.UtcDateTime.ToString();
											CustomPayload.Add(CustomPayloadItem, 0);

											CustomPayloadItem = new GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem(gxcontext);
											CustomPayloadItem.gxTpr_Propertyid = "PopReceipt";
											CustomPayloadItem.gxTpr_Propertyvalue = myQueueItem.PopReceipt;
											CustomPayload.Add(CustomPayloadItem, 0);

											//Event
											EventMessageItem.gxTpr_Eventmessageid = myQueueItem.Id;
											EventMessageItem.gxTpr_Eventmessagedata = myQueueItem.AsString;

											//Insertion DateTime?
											EventMessageItem.gxTpr_Eventmessagedate = myQueueItem.InsertionTime.Value.UtcDateTime;

											EventMessageItem.gxTpr_Eventmessagesourcetype = "QueueMessage";
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

										if (messageResponse.gxTpr_Handled == false) //Must retry if possible.
											{
												exMessage = string.Format("{0} {1}", QueueExceptionType.AppError, messageResponse.gxTpr_Errormessage);
												throw new Exception(exMessage);
											}
										else
											{
												log.LogInformation("(GX function handler) Function finished execution.");
											}
										}
										catch (Exception ex)
										{
											log.LogError("{0} Error invoking the GX procedure for Message Id {1}.", QueueExceptionType.SysRuntimeError, myQueueItem.Id);
											throw (ex); //Throw the exception so the runtime can Retry the operation.
										}
									}
								}
								else
								{
									exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", QueueExceptionType.SysRuntimeError, myQueueItem.Id);
									throw new Exception(exMessage);
								}
							}
							catch (Exception ex)
							{
								log.LogError("{0} Error processing Message Id {1}.", QueueExceptionType.SysRuntimeError, myQueueItem.Id);
								throw (ex); //Throw the exception so the runtime can Retry the operation.
							}
						}
						else
						{
							exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", QueueExceptionType.SysRuntimeError, myQueueItem.Id);
							throw new Exception(exMessage);
						}
					}
				}
				else
				{
					exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: configuration file not found.", QueueExceptionType.SysRuntimeError, myQueueItem.Id);
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
