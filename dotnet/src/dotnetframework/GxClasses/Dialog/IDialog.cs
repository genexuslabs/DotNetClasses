using ConnectionBuilder;
using log4net;
using System;

namespace GeneXus.Utils
{
	public interface IGxMessageFactory
	{
		IGxMessage GetMessageDialog();
	}
	public interface IGxMessage
	{
		void Show(string erroInfo, string message);
	}
	public class Dialogs : OSHelper
	{
		public static IConnectionDialogFactory provider;
		internal static string ProcessFactoryProvider = "ConnectionBuilder.ConnectionDialogFactory";

		public static IConnectionDialogFactory DialogFactory
		{
			get
			{
				if (provider == null)
				{
					try
					{

						lock (syncRoot)
						{
							if (provider == null)
							{
								provider = (IConnectionDialogFactory)Metadata.ClassLoader.GetInstance(OsProviderAssembly, ProcessFactoryProvider, null);
							}
						}
					}
					catch (Exception ex)
					{
						var strErrorMsg = $"Error loading Dialogs provider. Check if {OsProviderAssembly}.dll exists ";
						GXLogging.Debug(log, strErrorMsg, ex);
						throw new Exception(strErrorMsg, ex);
					}
				}
				return provider;
			}
			set
			{
				provider = value;
			}
		}

		public static IGxMessageFactory Message;
	}

}
