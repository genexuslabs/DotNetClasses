using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Application;
using GeneXus.Utils;
using System.Collections;
using GeneXus.Metadata;

namespace GeneXus.Deploy.AzureFunctions.QueueHandler
{
	public static class QueueTriggerHandler
	{
		public static void Run(string myQueueItem, FunctionContext context)
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
				throw ex;
			}		
		}
		private static QueueMessage SetupMessage(FunctionContext context, string item)
		{
			QueueMessage message = new QueueMessage();
			message.AsString = item;

			if (context.BindingContext.BindingData.TryGetValue("Id", out var messageIdObj) && messageIdObj != null)
			{
				message.Id = messageIdObj.ToString();

				if (context.BindingContext.BindingData.TryGetValue("DequeueCount", out var DequeueCountObj) && DequeueCountObj != null)
				{
					message.DequeueCount = DequeueCountObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("AsBytes", out var AsBytesObj) && AsBytesObj != null)
				{
					message.AsBytes = System.Text.Encoding.UTF8.GetBytes(AsBytesObj.ToString());
				}
				if (context.BindingContext.BindingData.TryGetValue("ExpirationTime", out var ExpiresOnObj) && ExpiresOnObj != null)
				{
					message.ExpirationTime = ExpiresOnObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("InsertionTime", out var InsertedOnObj) && InsertedOnObj != null)
				{
					message.InsertionTime = InsertedOnObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("NextVisibleTime", out var NextVisibleOnObj) && NextVisibleOnObj != null)
				{
					message.NextVisibleTime = NextVisibleOnObj.ToString();
				}
				if (context.BindingContext.BindingData.TryGetValue("PopReceipt", out var PopReceiptObj) && PopReceiptObj != null)
				{
					message.PopReceipt = PopReceiptObj.ToString();
				}
			}
			else
			{
				throw new InvalidOperationException();
			}
			return message;
		}
		private static void ProcessMessage(FunctionContext context, ILogger log, QueueMessage queueMessage)
		{
			string gxProcedure = FunctionReferences.GetFunctionEntryPoint(context, log, queueMessage.Id);
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
								//string queueMessageSerialized = JSONHelper.Serialize(myQueueItem);
								string queueMessageSerialized = JsonConvert.SerializeObject(queueMessage);
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
								GxUserType CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "Id", queueMessage.Id, gxcontext);
								CustomPayload.Add(CustomPayloadItem);

								CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "DequeueCount", queueMessage.DequeueCount, gxcontext);
								CustomPayload.Add(CustomPayloadItem);

								CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "ExpirationTime", queueMessage.ExpirationTime, gxcontext);
								CustomPayload.Add(CustomPayloadItem);

								CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "InsertionTime", queueMessage.InsertionTime, gxcontext);
								CustomPayload.Add(CustomPayloadItem);

								CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "NextVisibleTime", queueMessage.NextVisibleTime, gxcontext);
								CustomPayload.Add(CustomPayloadItem);

								CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "PopReceipt", queueMessage.PopReceipt, gxcontext);
								CustomPayload.Add(CustomPayloadItem);

								//Event

								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessageid", queueMessage.Id);
								ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagedata", queueMessage.AsString);


								//Insertion DateTime?

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
									throw new Exception(exMessage);
								}
								else
								{
									log.LogInformation("(GX function handler) Function finished execution.");
								}
							}
							catch (Exception ex)
							{
								log.LogError("{0} Error invoking the GX procedure for Message Id {1}.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
								throw ex; //Throw the exception so the runtime can Retry the operation.
							}
						}
					}
					else
					{
						exMessage = string.Format("{0} GeneXus procedure could not be executed for Message Id {1}.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
						throw new Exception(exMessage);
					}
				}
				catch (Exception ex)
				{
					log.LogError("{0} Error processing Message Id {1}.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
					throw ex; //Throw the exception so the runtime can Retry the operation.
				}
			}
			else
			{
				exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: procedure not specified in configuration file.", FunctionExceptionType.SysRuntimeError, queueMessage.Id);
				throw new Exception(exMessage);
			}
		}
		private static GxUserType CreateCustomPayloadItem(Type customPayloadItemType, string propertyId, object propertyValue, GxContext gxContext)
		{
			GxUserType CustomPayloadItem = (GxUserType)Activator.CreateInstance(customPayloadItemType, new object[] { gxContext });
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyid", propertyId);
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyvalue", propertyValue);
			return CustomPayloadItem;

		}
		private class QueueMessage
		{
			public QueueMessage()
			{ }
			public static long MaxMessageSize { get; set; }
			public static TimeSpan MaxVisibilityTimeout { get; set; }
			public static int MaxNumberOfMessagesToPeek { get; set; }
			public byte[] AsBytes { get; set; }
			public string Id { get; set; }
			public string PopReceipt { get; set; }
			public string InsertionTime { get; set; }
			public string ExpirationTime { get; set; }
			public string NextVisibleTime { get; set; }
			public string AsString { get; set; }
			public string DequeueCount { get; set; }
		}
	}
}
