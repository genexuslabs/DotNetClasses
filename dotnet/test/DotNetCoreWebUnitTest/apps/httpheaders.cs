using System;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Data.NTier.ADO;
using GeneXus.Procedure;
using GeneXus.Utils;

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
			result = (context.GetHttpSecure() == 1 ? "https://" : "http://") + context.GetRemoteAddress();
			result += StringUtil.NewLine() + GXUtil.UserId(string.Empty, context, pr_default);
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
			pr_default = new DataStoreProvider(context, new httpheaders__default(),
			new Object[][] {
			}
		 );
		}
		private IDataStoreProvider pr_default;
	}
	public class httpheaders__default : DataStoreHelperBase, IDataStoreHelper
	{
		public ICursor[] getCursors()
		{
			cursorDefinitions();
			return new Cursor[] {
	   };
		}

		private static CursorDef[] def;
		private void cursorDefinitions()
		{
			if (def == null)
			{
				def = new CursorDef[] {
		  };
			}
		}

		public void getResults(int cursor,
								IFieldGetter rslt,
								Object[] buf)
		{
		}

	}
}
