using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Procedure;

namespace GeneXus.Programs.apps
{
	public class httpheaders : GXWebProcedure
	{

		public httpheaders()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("Carmine");
		}

		public httpheaders(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute(out string result)
		{
			initialize();
			executePrivate(out result);
		}

		void executePrivate(out string result)
		{
			result = context.GetRemoteAddress();

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
