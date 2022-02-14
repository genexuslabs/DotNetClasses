using GeneXus.Utils;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Procedure;
namespace GeneXus.Programs.apps
{
	public class append : GXWebProcedure
	{

		public append()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("Carmine");
		}

		public append(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute(short aP0_numeric1,
							 string aP1_Character,
							 decimal aP2_numeric2,
							 out string aP3_character2)
		{
			this.AV18numeric1 = aP0_numeric1;
			this.AV17Character = aP1_Character;
			this.AV19numeric2 = aP2_numeric2;
			this.AV20character2 = "";
			initialize();
			executePrivate();
			aP3_character2=this.AV20character2;
		}

		void executePrivate()
		{
			/* GeneXus formulas */
			/* Output device settings */
			AV20character2 = StringUtil.Str((decimal)(AV18numeric1), 10, 0) + AV17Character + StringUtil.Str(AV19numeric2, 10, 0);
			if (context.WillRedirect())
			{
				context.Redirect(context.wjLoc);
				context.wjLoc = "";
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
			/* GeneXus formulas. */
			context.Gx_err = 0;
		}
		private short AV18numeric1;
		private decimal AV19numeric2;
		private string AV17Character;
		private string AV20character2;
	}

}
