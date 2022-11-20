using System;
using GeneXus.Services;
using log4net;

namespace GeneXus.Messaging.Common
{
	public abstract class MessageBrokerBase
	{
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(MessageBrokerBase));
		internal GXService service;
		public MessageBrokerBase()
		{
		}

		public MessageBrokerBase(GXService s)
		{
			if (s == null)
			{
				try
				{
					s = ServiceFactory.GetGXServices().Get(GXServices.MESSAGEBROKER_SERVICE);
				}
				catch (Exception)
				{
					GXLogging.Warn(logger, "MESSAGEBROKER_SERVICE is not activated");
				}
			}

			service = s;
		}
		public abstract String GetName();
	}
}
