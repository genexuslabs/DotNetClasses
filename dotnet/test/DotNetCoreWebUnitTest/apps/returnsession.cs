using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Http;
using GeneXus.Http.Server;
using GeneXus.Procedure;
using Newtonsoft.Json.Linq;

namespace GeneXus.Programs.apps
{
	public class returnsession : GXWebProcedure
	{

		public returnsession()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("Carmine");
		}

		public returnsession(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute(string gxid, out string result)
		{
			initialize();
			executePrivate(gxid, out result);
		}

		void executePrivate(string gxid, out string result)
		{
			string Gxids = "gxid_" + gxid;
			string actual = Gxwebsession.Get(Gxids + "gxvar_Datasdt");
			result = $"{actual},{gxid}";
			this.cleanup();
		}

		public override void cleanup()
		{
			CloseOpenCursors();
			base.cleanup();
			if (IsMain)
			{
				context.CloseConnections();
			}
			ExitApp();
		}

		protected void CloseOpenCursors()
		{
		}

		public override void initialize()
		{
			Gxwebsession = context.GetSession();
		}
		private IGxSession Gxwebsession;
	}

}
