using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using GeneXus.Application;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Metadata;
using GeneXus.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GeneXus.Deploy.AzureFunctions.EventGridHandler
{
	public class EventGridTriggerHandler
	{
		private ICallMappings _callmappings;

		public EventGridTriggerHandler(ICallMappings callMappings)
		{
			_callmappings = callMappings;
		}
		public void Run(CustomEventType input, FunctionContext context)
		{
			var logger = context.GetLogger("EventGridTriggerHandler");
			string functionName = context.FunctionDefinition.Name;
			Guid eventId = new Guid(context.InvocationId);
			logger.LogInformation($"GeneXus Event Grid trigger handler. Function processed: {functionName}. Event Id: {eventId}. Function executed at: {DateTime.Now}.");

			try
			{
				ProcessEvent(context, logger, input, eventId.ToString());
			}
			catch (Exception ex) //Catch System exception and retry
			{
				logger.LogError(ex.ToString());
				throw;
			}
		}
		private void ProcessEvent(FunctionContext context, ILogger log, CustomEventType input, string eventId)
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
							//Two valid signatures for the GX procedure:
							//parm(in:&EventMessageCollection, out:&ExternalEventMessageResponse );
							//parm(in:&rawData, out:&ExternalEventMessageResponse );

							GxContext gxcontext = new GxContext();
							object[] parametersdata;
							parametersdata = new object[] { null };

							if (parameters[0].ParameterType == typeof(string))
							{
								string eventMessageSerialized = string.Empty;
								eventMessageSerialized = JsonSerializer.Serialize(input);
								parametersdata = new object[] { eventMessageSerialized, null };
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

								GxUserType eventMessageProperty;

								if (input.Subject != null)
								{
									eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "Subject", input.Subject, gxcontext);
									eventMessageProperties.Add(eventMessageProperty);
								}

								if (input.Topic != null)
								{
									eventMessageProperty = EventMessagePropertyMapping.CreateEventMessageProperty(eventMessPropsItemType, "Topic", input.Topic, gxcontext);
									eventMessageProperties.Add(eventMessageProperty);
								}

								//Event

								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageid", input.Id);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagesourcetype", input.EventType);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageversion", input.DataVersion);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessageproperties", eventMessageProperties);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedate", input.EventTime);
								ClassLoader.SetPropValue(eventMessageItem, "gxTpr_Eventmessagedata", toJsonString(input.Data));


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
		private static string toJsonString(IDictionary<string, JsonElement> keyValues)
		{
			Dictionary<string, object> jsonDictionary = new Dictionary<string, object>();
			foreach (var keyvaluepair in keyValues)
			{
				jsonDictionary[keyvaluepair.Key] = GetJsonValue(keyvaluepair.Value);
			}

			return JsonSerializer.Serialize(jsonDictionary);
		}

		static object GetJsonValue(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					Dictionary<string, object> obj = new Dictionary<string, object>();
					foreach (var prop in element.EnumerateObject())
					{
						obj[prop.Name] = GetJsonValue(prop.Value);
					}
					return obj;

				case JsonValueKind.Array:
					List<object> list = new List<object>();
					foreach (var item in element.EnumerateArray())
					{
						list.Add(GetJsonValue(item));
					}
					return list;

				case JsonValueKind.String:
					return element.GetString();

				case JsonValueKind.Number:
					return element.GetDecimal();

				case JsonValueKind.True:
					return true;

				case JsonValueKind.False:
					return false;

				case JsonValueKind.Null:
				default:
					return null;
			}
		}
	}
}

