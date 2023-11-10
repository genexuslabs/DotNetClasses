using System;
using GeneXus.Services;

namespace GeneXus.Messaging.Common
{
	public abstract class MessageBrokerBase
	{
		static readonly IGXLogger logger = GXLoggerFactory.GetLogger<MessageBrokerBase>();
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
					s = ServiceFactory.GetGXServices()?.Get(GXServices.MESSAGEBROKER_SERVICE);
				}
				catch (Exception)
				{
					GXLogging.Warn(logger, "MESSAGEBROKER_SERVICE is not activated");
				}
			}

			service = s;
		}
		public abstract string GetName();
	}
}
