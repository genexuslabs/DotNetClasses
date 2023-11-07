using System;
using GeneXus.Services;
using log4net;

namespace GeneXus.Messaging.Common
{
	public abstract class EventRouterBase
	{
		static readonly IGXLogger logger = GXLoggerFactory.GetLogger<EventRouterBase>();
		internal GXService service;
		public EventRouterBase()
		{
		}

		public EventRouterBase(GXService s)
		{
			if (s == null)
			{
				try
				{
					s = ServiceFactory.GetGXServices()?.Get(GXServices.EVENTROUTER_SERVICE);
				}
				catch (Exception)
				{
					GXLogging.Warn(logger, "EVENTROUTER_SERVICE is not activated");
				}
			}

			service = s;
		}
		public abstract string GetName();
	}
}
