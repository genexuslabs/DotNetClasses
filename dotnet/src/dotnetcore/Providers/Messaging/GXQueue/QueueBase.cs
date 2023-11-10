using System;
using GeneXus.Services;

namespace GeneXus.Messaging.Common
{
	public abstract class QueueBase
	{
		static readonly IGXLogger logger = GXLoggerFactory.GetLogger<QueueBase>();
		internal GXService service;
		public QueueBase()
		{
		}

		public QueueBase(GXService s)
		{
			if (s == null)
			{
				try
				{
					s = ServiceFactory.GetGXServices()?.Get(GXServices.QUEUE_SERVICE);
				}
				catch (Exception)
				{
					GXLogging.Warn(logger, "QUEUE_SERVICE is not activated");
				}
			}

			service = s;
		}
		public abstract String GetName();
	}
}
