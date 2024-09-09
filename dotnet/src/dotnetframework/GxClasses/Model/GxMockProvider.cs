using System.Collections.Generic;
using GeneXus.Application;
namespace GeneXus.Mock
{
	public interface IGxMock
	{
		bool Handle<T>(IGxContext context, T objectInstance, List<GxObjectParameter> parameters) where T : GXBaseObject;
	}
	public class GxMockProvider 
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxMockProvider>();

		private static volatile IGxMock provider;

		public static IGxMock Provider{
			get
			{
				return provider;
			}
			set{
				provider = value;
				if (provider != null)
					GXLogging.Debug(log, "Mock provider: " + provider.GetType().FullName);
				else
					GXLogging.Debug(log, "Mock provider set to null ");
			}
		}
	}

}