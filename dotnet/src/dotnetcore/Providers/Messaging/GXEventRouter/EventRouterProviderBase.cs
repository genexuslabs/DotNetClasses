using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GeneXus.Services;
using GeneXus.Utils;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Messaging.Common
{
	public class EventRouterProviderBase
	{
		internal IEventRouter eventRouter = null;
		public static Assembly assembly;
		static readonly ILog logger = LogManager.GetLogger(typeof(EventRouterProviderBase));
		private const string MODULE_DLL = @"GeneXusEventMessaging";

		public EventRouterProviderBase()
		{
		}
		public EventRouterProviderBase(EventRouterProviderBase other)
		{
			eventRouter = other.eventRouter;
		}
		void ValidQueue()
		{
			if (eventRouter == null)
			{
				GXLogging.Error(logger, "Event Router was not instantiated.");
				throw new Exception("Event Router was not instantiated.");
			}
		}
		private static Assembly LoadAssembly(string fileName)
		{
			if (File.Exists(fileName))
			{
				Assembly assemblyLoaded = Assembly.LoadFrom(fileName);
				return assemblyLoaded;
			}
			else
				return null;
		}

		private static void LoadAssemblyIfRequired()
		{
			if (assembly == null)
			{
				assembly = AssemblyLoader.LoadAssembly(new AssemblyName(MODULE_DLL));
			}
		}

		public bool SendEvent(GxUserType evt, bool binaryData, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{
			bool success = false;
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			try
			{
				GXCloudEvent gxCloudEvent = ToGXCloudEvent(evt);
				LoadAssemblyIfRequired();
				try
				{
					ValidQueue();
					if (eventRouter != null)
						return(eventRouter.SendEvent(gxCloudEvent, binaryData));
				}
				catch (Exception ex)
				{
					EventRouterErrorMessagesSetup(ex, out errorMessages);
					success = false;
					GXLogging.Error(logger, ex);
				}
			}
			catch (Exception ex)
			{
				success = false;
				GXLogging.Error(logger,ex);
				throw ex;
			}
			return success;
		}

		public bool SendCustomEvents(string evts, bool isBinary, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			bool success;
			try
			{
				ValidQueue();
				success = eventRouter.SendCustomEvents(evts, isBinary);
			}
			catch (Exception ex)
			{
				EventRouterErrorMessagesSetup(ex, out errorMessages);
				success = false;
				GXLogging.Error(logger, ex);
			}
			return success;
		}

		public bool SendEvents(IList evts, bool binaryData, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{	
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			bool success = false;
			LoadAssemblyIfRequired();
			try
			{
				IList<GXCloudEvent> gxCloudEvents = new List<GXCloudEvent>();	
				foreach (GxUserType e in evts)
				{
					if (ToGXCloudEvent(e) is GXCloudEvent gxCloudEvent)
						gxCloudEvents.Add(gxCloudEvent);
				}
				try
				{
					ValidQueue();
					success = eventRouter.SendEvents(gxCloudEvents, binaryData);
				}
				catch (Exception ex)
				{
					EventRouterErrorMessagesSetup(ex, out errorMessages);
					success = false;
					GXLogging.Error(logger, ex);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(logger, ex);
				throw ex;
			}
			return success;
		}

		#region Transform operations
		protected void EventRouterErrorMessagesSetup(Exception ex, out GXBaseCollection<SdtMessages_Message> errorMessages)
		{
			errorMessages = new GXBaseCollection<SdtMessages_Message>();
			bool foundGeneralException = false;
			if (ex != null)
			{
				SdtMessages_Message msg = new SdtMessages_Message();
				if (eventRouter != null && ex.InnerException != null)
				{
					do
					{
						if (eventRouter.GetMessageFromException(ex.InnerException, msg))
						{
							msg.gxTpr_Type = 1;
							errorMessages.Add(msg);
						}
						else
						{
							foundGeneralException = true;
							break;
						}
						ex = ex.InnerException;
					}
					while (ex.InnerException != null);
					if (foundGeneralException)
						GXUtil.ErrorToMessages("GXEventRouter", ex, errorMessages);
				}
				else
				{
					GXUtil.ErrorToMessages("GXEventRouter", ex, errorMessages);
				}
			}
		}		
		private GXCloudEvent ToGXCloudEvent(GxUserType evt)
		{
			if (evt != null)
			{
				GXCloudEvent gxCloudEvent = new GXCloudEvent();
				gxCloudEvent.type = evt.GetPropertyValue<string>("Type");
				gxCloudEvent.source = evt.GetPropertyValue<string>("Source");
				gxCloudEvent.data = evt.GetPropertyValue<string>("Data");
				gxCloudEvent.datacontenttype = evt.GetPropertyValue<string>("Datacontenttype");
				gxCloudEvent.id = evt.GetPropertyValue<string>("Id");
				gxCloudEvent.subject = evt.GetPropertyValue<string>("Subject");
				gxCloudEvent.dataschema = evt.GetPropertyValue<string>("Dataschema");
				gxCloudEvent.data_base64 = evt.GetPropertyValue<string>("Data_base64");
				gxCloudEvent.time = evt.GetPropertyValue<DateTime>("Time");
				return gxCloudEvent;
			}
			return null;
		}
		#endregion

	}
	internal class ServiceFactory
	{
		private static IEventRouter eventRouter;
		private static readonly ILog log = LogManager.GetLogger(typeof(Services.ServiceFactory));

		public static GXServices GetGXServices()
		{
			return GXServices.Instance;
		}

		public static IEventRouter GetEventRouter()
		{
			if (eventRouter == null)
			{
				eventRouter = GetRouterImpl(GXServices.EVENTROUTER_SERVICE);
			}
			return eventRouter;
		}

		public static IEventRouter GetRouterImpl(string service)
		{
			IEventRouter eventRouterImpl = null;
			if (GetGXServices() != null)
			{
				GXService providerService = GetGXServices()?.Get(service);
				if (providerService != null)
				{
					try
					{
						string typeFullName = providerService.ClassName;
						GXLogging.Debug(log, "Loading Event Router settings:", typeFullName);
						Type type = AssemblyLoader.GetType(typeFullName);
						eventRouterImpl = (IEventRouter)Activator.CreateInstance(type);
					}
					catch (Exception e)
					{
						GXLogging.Error(log, "Couldn't connect to the Event Router.", e.Message, e);
						throw e;
					}
				}
			}
			return eventRouterImpl;
		}
	}
}

	

