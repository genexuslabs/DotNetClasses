using System.Collections.Generic;
using GeneXus.Application;
using log4net;
namespace GeneXus.Mock
{
	public interface IGxMock
	{
		void Handle<T>(IGxContext context, T objectInstance, List<GxObjectParameter> parameters) where T : GXBaseObject;
		bool CanHandle<T>(T objectInstance) where T : GXBaseObject;
	}
	public class GxMockProvider 
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxMockProvider));

		private static volatile IGxMock provider;

		public static IGxMock Provider {
			get
			{
				if (provider == null)
				{
					provider = new GxMock();
				}
				GXLogging.Debug(log, "Mock provider: " + provider.GetType().FullName);
				return provider;
			}
			set{
				provider = value;
			}
		}
	}

	internal class GxMock : IGxMock
	{
		public bool CanHandle<T>(T objectInstance) where T : GXBaseObject
		{
			return false;
		}

		public void Handle<T>(IGxContext context, T objectInstance, List<GxObjectParameter> parameters) where T : GXBaseObject
		{
			
		}
	}

}