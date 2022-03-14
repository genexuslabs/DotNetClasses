using log4net;
using System;
using System.Diagnostics;

namespace GeneXus.Utils
{
	public interface IProcessFactory
	{
		IProcessHelper GetProcessHelper();
	}
	public interface IProcessHelper
	{
		int ExecProcess(string filename, string[] args, string basePath, string executable, DataReceivedEventHandler dataReceived);
		short OpenPrintDocument(string commandString);
		int Shell(string commandString, int modal);

		int Shell(string commandString, int modal, int redirectOutput);
	}
	public class OSHelper
	{
		protected static readonly ILog log = log4net.LogManager.GetLogger(typeof(OSHelper));
		protected static string OsProviderAssembly = "GxClasses.Win";
		protected static object syncRoot = new Object();

	}
	public class GXProcessHelper: OSHelper
	{
		private static volatile IProcessFactory provider;
		internal static string ProcessFactoryProvider = "GeneXus.Utils.GxProcessFactory";

		public static IProcessFactory ProcessFactory
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
									provider = (IProcessFactory)Metadata.ClassLoader.GetInstance(OsProviderAssembly, ProcessFactoryProvider, null);
								}
							}
						}
						catch (Exception ex)
						{
							string strErrorMsg = $"Error loading Process provider. Check if {OsProviderAssembly}.dll exists ";
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
	}
}
