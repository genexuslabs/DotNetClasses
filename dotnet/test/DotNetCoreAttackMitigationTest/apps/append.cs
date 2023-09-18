using GeneXus.Utils;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Procedure;
using System;
using GX;

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
							 out string aP3_character2,
							 [GxJsonFormat("yyyy-MM-dd'T'HH:mm:ss")] out DateTime aP0_datetime,
							 out Sdtrappo00b aP2_rappo00B
							 )
		{
			this.AV8datetime = DateTime.MinValue;
			this.AV18numeric1 = aP0_numeric1;
			this.AV17Character = aP1_Character;
			this.AV19numeric2 = aP2_numeric2;
			this.AV20character2 = "";
			this.AV8rappo00B = new Sdtrappo00b(context);
			initialize();
			executePrivate();
			aP3_character2=this.AV20character2;
			aP0_datetime=this.AV8datetime;
			aP2_rappo00B=this.AV8rappo00B;
		}

		void executePrivate()
		{
			/* GeneXus formulas */
			/* Output device settings */
			AV20character2 = StringUtil.Str((decimal)(AV18numeric1), 10, 0) + AV17Character + StringUtil.Str(AV19numeric2, 10, 0) + ClientInformation.Id.ToString();
			AV8datetime = DateTimeUtil.ResetTime(context.localUtil.YMDToD(2022, 5, 24));
			AV8rappo00B.gxTpr_Rapdaa = DateTimeUtil.ResetTime(context.localUtil.YMDToD(2022, 5, 24));
			AV8rappo00B.gxTpr_Raporafin = context.localUtil.YMDHMSToT(2022, 5, 24, 10, 32, 15);
			AV8rappo00B.gxTpr_Raporaini = context.localUtil.YMDHMSToT(2022, 5, 24, 10, 30, 15);
			AV8rappo00B.gxTpr_Raporafinall = context.localUtil.YMDHMSToT(2022, 5, 24, 10, 40, 15);
			AV8rappo00B.gxTpr_Raporainiall = context.localUtil.YMDHMSToT(2022, 5, 24, 0, 40, 15);
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
			AV8rappo00B = new Sdtrappo00b(context);
			AV8datetime = (DateTime.MinValue);
			/* GeneXus formulas. */
			context.Gx_err = 0;
		}
		private short AV18numeric1;
		private decimal AV19numeric2;
		private string AV17Character;
		private string AV20character2;
		private DateTime AV8datetime;
		private Sdtrappo00b AV8rappo00B;
	}

}
