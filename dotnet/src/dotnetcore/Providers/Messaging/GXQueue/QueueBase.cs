using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Services;
using log4net;

namespace GeneXus.Messaging.Common
{
	public abstract class QueueBase
	{
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(QueueBase));
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
