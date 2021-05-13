using System;
using System.Collections;
using System.IO;
using System.Reflection;
using GeneXus.Application;
using GeneXus.Metadata;
using Xunit;
using GeneXus.Utils;

namespace Extensiones.AzureFunctions
{
	public class AzureInteropTests
	{
		[Fact]
		public void TestAzureFunctionsInterface()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "amyprochandler.dll");
			Assembly obj = Assembly.LoadFile(path);
			Type objexec = obj.GetType("GeneXus.Programs.amyprochandler");
			object objgxproc= Activator.CreateInstance(objexec);

			var method = objexec.GetMethod("execute");
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters.Length != 2)
			{
				//Thrown to the Azure monitor
				string exMessage = "The number of parameters in GeneXus procedure is not correct.";
				Console.WriteLine(exMessage);
			}
			else
			{
				GxContext gxcontext = new GxContext();
				Object[] parametersdata;
				Type EventMessagesType = parameters[0].ParameterType; //SdtEventMessages
				GxUserType EventMessages = (GxUserType)Activator.CreateInstance(EventMessagesType, new object[] { gxcontext }); // instance of SdtEventMessages

				IList EventMessage = (IList)ClassLoader.GetPropValue(EventMessages, "gxTpr_Eventmessage");//instance of GXBaseCollection<SdtEventMessage>
				Type EventMessageItemType = EventMessage.GetType().GetGenericArguments()[0];//SdtEventMessage

				GxUserType EventMessageItem = (GxUserType)Activator.CreateInstance(EventMessageItemType, new object[] { gxcontext }); // instance of SdtEventMessage
				IList CustomPayload = (IList)ClassLoader.GetPropValue(EventMessageItem, "gxTpr_Eventmessagecustompayload");//instance of GXBaseCollection<SdtEventCustomPayload_CustomPayloadItem>

				Type CustomPayloadItemType = CustomPayload.GetType().GetGenericArguments()[0];//SdtEventCustomPayload_CustomPayloadItem


				//Payload

				GxUserType CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "ScheduleStatusNext", "TimerInfo.ScheduleStatus.Next.ToString()", gxcontext);
				CustomPayload.Add(CustomPayloadItem);

				CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "ScheduleStatusLast", "TimerInfo.ScheduleStatus.Last.ToString()", gxcontext);
				CustomPayload.Add(CustomPayloadItem);

				CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "ScheduleStatusLastUpdated", "TimerInfo.ScheduleStatus.LastUpdated.ToString()", gxcontext);
				CustomPayload.Add(CustomPayloadItem);

				CustomPayloadItem = CreateCustomPayloadItem(CustomPayloadItemType, "IsPastDue", "TimerInfo.IsPastDue.ToString()", gxcontext);
				CustomPayload.Add(CustomPayloadItem);

				//Event

				ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessageid", "messageId.ToString()");
				ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagesourcetype", "EventSourceType.Timer");
				ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessageversion", string.Empty);
				ClassLoader.SetPropValue(EventMessageItem, "gxTpr_Eventmessagecustompayload", CustomPayload);

				//List of Events
				EventMessage.Add(EventMessageItem);

				parametersdata = new object[] { EventMessages, null };

				method.Invoke(objgxproc, parametersdata);

				GxUserType EventMessageResponse = parametersdata[1] as GxUserType;//SdtEventMessageResponse
				bool result = (bool)ClassLoader.GetPropValue(EventMessageResponse, "gxTpr_Handled");
				Assert.True(result, "Handled returned false");
			}
		}
		private static GxUserType CreateCustomPayloadItem(Type customPayloadItemType, string propertyId, object propertyValue, GxContext gxContext)
		{
			GxUserType CustomPayloadItem = (GxUserType)Activator.CreateInstance(customPayloadItemType, new object[] { gxContext });
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyid", propertyId);
			ClassLoader.SetPropValue(CustomPayloadItem, "gxTpr_Propertyvalue", propertyValue);
			return CustomPayloadItem;

		}
	}
}
	

