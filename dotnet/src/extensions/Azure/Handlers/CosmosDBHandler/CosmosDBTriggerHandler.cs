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
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeneXus.Deploy.AzureFunctions.CosmosDBHandler
{
	public class CosmosDBTriggerHandler
    {
		private ICallMappings _callmappings;

		public CosmosDBTriggerHandler(ICallMappings callMappings)
		{
			_callmappings = callMappings;
		}
		public void Run(IReadOnlyList<Dictionary<string, object>> doc, FunctionContext context) 
		{
			var logger = context.GetLogger("CosmosDBTriggerHandler");
			string functionName = context.FunctionDefinition.Name;
			Guid eventId = new Guid(context.InvocationId);
			logger.LogInformation($"GeneXus CosmosDB trigger handler. Function processed: {functionName}. Event Id: {eventId}. Function executed at: {DateTime.Now}.");

			try
			{
				ProcessEvent(context, logger, doc, eventId.ToString());
			}
			catch (Exception ex) //Catch System exception and retry
			{
				logger.LogError(ex.ToString());
				throw;
			}
		}
		private void ProcessEvent(FunctionContext context, ILogger log, IReadOnlyList<Dictionary<string,object>> doc, string eventId)
		{
			CallMappings callmap = (CallMappings)_callmappings;

			GxAzMappings map = callmap.mappings is object ? callmap.mappings.First(m => m.FunctionName == context.FunctionDefinition.Name) : null;
			string gxProcedure = (map != null && map is object) ? map.GXEntrypoint : string.Empty;
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

							exMessage = string.Format("{0} Error for Event Id {1}: the number of parameters in GeneXus procedure is not correct.", FunctionExceptionType.SysRuntimeError, eventId);
							throw new Exception(exMessage); //Send to retry if possible.
						}
						else
						{
							//Three valid signatures for the GX procedure:
							//parm(in:&EventMessageCollection, out:&ExternalEventMessageResponse );
							//parm(in:&rawData, out:&ExternalEventMessageResponse );
							//parm(in:&EventMessagesList, out:&ExternalEventMessageResponse );

							GxContext gxcontext = new GxContext();
							object[] parametersdata;
							parametersdata = new object[] { null };

							if (parameters[0].ParameterType == typeof(string))
							{
								StringBuilder st = new StringBuilder();
								foreach (Dictionary<string, object> singleDoc in doc)
									st.Append(ConvertToJsonObject(singleDoc).ToJsonString());
								
								parametersdata = new object[] { st.ToString(), null };
							}
							else
								try
								{ 			
									string jsonDocuments = string.Empty;
									Type eventMessagesListType = parameters[0].ParameterType; //SdtEventMessagesList
									GxUserType eventMessagesList = (GxUserType)Activator.CreateInstance(eventMessagesListType, new object[] { gxcontext }); // instance of SdtEventMessagesList

									IList eventMessageItems = (IList)ClassLoader.GetPropValue(eventMessagesList, "gxTpr_Items_GxSimpleCollection");
									foreach (Dictionary<string, object> singleDoc in doc)
										eventMessageItems.Add(ConvertToJsonObject(singleDoc).ToJsonString());
									
									parametersdata = new object[] { eventMessagesList, null };
								
								}
								catch (Exception)
								{ 					
									Type eventMessagesType = parameters[0].ParameterType; //SdtEventMessages
									GxUserType eventMessages = (GxUserType)Activator.CreateInstance(eventMessagesType, new object[] { gxcontext }); // instance of SdtEventMessages

									IList eventMessage = (IList)ClassLoader.GetPropValue(eventMessages, "gxTpr_Eventmessage");//instance of GXBaseCollection<SdtEventMessage>
									Type eventMessageItemType = eventMessage.GetType().GetGenericArguments()[0];//SdtEventMessage									

									GxUserType eventMessageProperty;

									foreach (Dictionary<string,object> singleDoc in doc)
									{
										string idValue = string.Empty;
										GxUserType eventMessageItem = (GxUserType)Activator.CreateInstance(eventMessageItemType, new object[] { gxcontext }); // instance of SdtEventMessage

										IList eventMessageProperties = (IList)ClassLoader.GetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties");//instance of GXBaseCollection<GeneXus.Programs.genexusserverlessapi.SdtEventMessageProperty>
										Type eventMessPropsItemType = eventMessageProperties.GetType().GetGenericArguments()[0];//SdtEventMessageProperty

										foreach (string key in singleDoc.Keys)
										{
											if (singleDoc[key] != null)
											{ 
												JsonElement jsonElement = (JsonElement)singleDoc[key];
												string strValue = jsonElement.ToString().Trim(); 
				
												if (key == "id")
													idValue = strValue;

												eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType,key, strValue, gxcontext);
												eventMessageProperties.Add(eventMessageProperty);
											}
										}

										//Event

										ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageid", $"{eventId.ToString()}_{idValue}");
										ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagesourcetype", EventSourceType.CosmosDB);
										ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedate", DateTime.UtcNow);
										ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageversion", string.Empty);
										ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties", eventMessageProperties);

										//List of Events
										eventMessage.Add(eventMessageItem);
									}
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
								log.LogError("{0} Error invoking the GX procedure for Event Id {1}.", FunctionExceptionType.SysRuntimeError, eventId);
								throw; //Throw the exception so the runtime can Retry the operation.
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Event Id {1}.", FunctionExceptionType.SysRuntimeError, eventId);
						throw new Exception(exMessage);
					}
				}
				catch (Exception)
				{
					log.LogError("{0} Error processing Event Id {1}.", FunctionExceptionType.SysRuntimeError, eventId);
					throw; //Throw the exception so the runtime can Retry the operation.
				}
			}
			else
			{
				exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Event Id {1}. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError, eventId);
				throw new Exception(exMessage);
			}
		}
		private JsonArray ConvertToJsonArray(JsonElement jsonElement)
		{
			JsonArray jsonArray = new JsonArray();
			JsonElement.ArrayEnumerator jsonElements = jsonElement.EnumerateArray();
			foreach (JsonElement j in jsonElements)
				jsonArray.Add(ConvertToJsonObject(j));
			return jsonArray;
		}
		private JsonNode ConvertToJsonObject(JsonElement jsonElement)
		{
			JsonNode jsonValue = null;		
			if (jsonElement.ValueKind == JsonValueKind.Array)
				jsonValue = ConvertToJsonArray(jsonElement);
			else
				if (jsonElement.ValueKind == JsonValueKind.Object)
				{
					JsonSerializerOptions options = new JsonSerializerOptions
					{
						Converters = { new JsonStringEnumConverter()}
					};
					jsonValue = JsonSerializer.Deserialize<JsonNode>(jsonElement.GetRawText(), options);
				}
				else
					jsonValue = JsonValue.Create(jsonElement);
			
			return jsonValue;
		}
		private JsonObject ConvertToJsonObject(Dictionary<string,object> jsondoc)
		{	
			JsonObject keyValuePairs = new JsonObject();
			foreach (KeyValuePair<string,object> element in jsondoc)
			{
				//Json Null Serialization: include non null
				if (element.Value != null)
				{ 
					string key = element.Key;
					JsonElement value = (JsonElement)element.Value;
					keyValuePairs.Add(key, ConvertToJsonObject(value));
				}
			}
			return keyValuePairs;
		}
	}	
}
