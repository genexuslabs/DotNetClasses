using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Http.Server;
using GeneXus.Procedure;

namespace GeneXus.Programs.apps
{
	public class testservice : GXWebProcedure
	{

		public testservice()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("Carmine");
		}

		public testservice(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute()
		{
			initialize();
			executePrivate();
		}

		void executePrivate()
		{
			GxHttpResponse AV25httpResponse = new GxHttpResponse(context);
			AV25httpResponse.AppendHeader("Result", "OK");

			cleanup();
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
		}
	}

}
