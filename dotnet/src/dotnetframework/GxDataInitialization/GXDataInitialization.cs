using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Data.NTier;
using GeneXus.Metadata;
using GeneXus.Procedure;
using GeneXus.Reorg;
using GeneXus.Resources;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace GeneXus.Utils
{
	[XmlType("Object")]
	public class DynTrnInitializer
	{
		public DynTrnInitializer() { }
		[XmlAttribute]
		public string Name { get; set; }
	}

	public class GXDataInitialization : GXProcedure
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GXDataInitialization));

		public GXDataInitialization()
		{
			context = GxContext.CreateDefaultInstance();
			IsMain = true;
		}
		public static int Main(string [] args)
		{
			try
			{
				string dynTrnInitializerFile = "DynTrnInitializers.xml";
				if (args!=null && args.Length > 0)
				{
					dynTrnInitializerFile = args[0];
				}
				else 
				{
					if (!File.Exists(dynTrnInitializerFile))
					{
						dynTrnInitializerFile = Path.Combine(GxContext.StaticPhysicalPath(), dynTrnInitializerFile);
					}
				}
				List<DynTrnInitializer> dataproviders = GXXmlSerializer.Deserialize<List<DynTrnInitializer>>(typeof(List<DynTrnInitializer>), File.ReadAllText(dynTrnInitializerFile), "Objects", string.Empty, out List<string> errors);
				GXDataInitialization dataInitilization = new GXDataInitialization();
				int result = dataInitilization.ExecDataInitialization(dataproviders);
				if (result == 0)
					File.Delete(dynTrnInitializerFile);
				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				GXLogging.Error(log, ex);
				return 1;
			}
		}
		public int ExecDataInitialization(List<DynTrnInitializer> dataProviders)
		{
			try
			{
				string ns;
				if (Config.GetValueOf("AppMainNamespace", out ns))
				{
					foreach (DynTrnInitializer dataProvider in dataProviders) {
						Console.WriteLine(GXResourceManager.GetMessage("GXM_runpgm", new object[] { dataProvider.Name }));

						string dataProviderClassName = dataProvider.Name.ToLower();
						string[] dataProviderClassNameTokens = dataProviderClassName.Split('.');
						dataProviderClassNameTokens[dataProviderClassNameTokens.Length - 1] = "a" + dataProviderClassNameTokens[dataProviderClassNameTokens.Length - 1];
						dataProviderClassName = string.Join(".", dataProviderClassNameTokens);
						var dp = ClassLoader.FindInstance(dataProviderClassName, ns, dataProviderClassName, new Object[] { context }, null);
						object[] parms = new object[1]; //Dataproviders receive 1 sdt collection parameter
						ClassLoader.ExecuteVoidRef(dp, "execute", parms);
						IGxCollection bcs = (IGxCollection)parms[0];

						int idx = 1;
						while (idx <= bcs.Count)
						{
							var bc = ((GxSilentTrnSdt)bcs.Item(idx));
							if (!bc.InsertOrUpdate() || !bc.Success())
							{
								Console.WriteLine(MessagesToString(bc.GetMessages()));
							}
							idx++;
						}
					}
					if (dataProviders.Count>0)
						context.CommitDataStores("ExecDataInitialization");
				}
				this.cleanup();
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				GXLogging.Error(log, ex);
				return 1;
			}
		}

		public override void initialize()
		{
		}

		public override void cleanup()
		{
			context.CloseConnections();
		}

		private enum MessageTypes
		{
			Warning = 0,
			Error = 1
		}

		public static string MessagesToString(GXBaseCollection<SdtMessages_Message> messages)
		{
			StringBuilder str = new StringBuilder();
			foreach(SdtMessages_Message msg in messages)
			{
				string msgType;
				switch(msg.gxTpr_Type)
				{
					case (int)MessageTypes.Warning: msgType = "Warning"; break;
					case (int)MessageTypes.Error: msgType = "Error"; break;
					default: goto case (int)MessageTypes.Warning;
				}
				str.AppendFormat("{0}:{3}  Code: {1}{3}  Description: {2}{3}", msgType, msg.gxTpr_Id, msg.gxTpr_Description, StringUtil.NewLine());
			}
			return str.ToString();
		}
	}
}
