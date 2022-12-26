using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Http;
using GeneXus.Http.Server;
using GeneXus.Procedure;

namespace GeneXus.Programs.apps
{
	public class createsession : GXWebProcedure
	{

		public createsession()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("Carmine");
		}

		public createsession(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute(string gxid, string Type, string title)
		{
			initialize();
			executePrivate(gxid, Type, title);
		}

		void executePrivate(string gxid, string Type, string Title)
		{
			string Gxids = "gxid_" + gxid;
			Gxwebsession.Set("GXID_" + gxid + "GXVAR_DATASDT", gxid);
			if (string.IsNullOrEmpty(Gxwebsession.Get(Gxids)))
			{
				
				Gxwebsession.Set(Gxids + "gxvar_Datasdt", $"Reccord 1 - type {Type}");
				Gxwebsession.Set(Gxids + "gxvar_Data1", "");
				Gxwebsession.Set(Gxids + "gxvar_Data2", "");
				Gxwebsession.Set(Gxids, "true");
			}
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
