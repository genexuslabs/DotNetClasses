using System;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Http.Server;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs
{
	public class webhook : GXWebProcedure
	{
		public override void webExecute()
		{
			context.SetDefaultTheme("Carmine");
			initialize();
			executePrivate();
			cleanup();
		}

		public webhook()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("Carmine");
		}

		public webhook(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute()
		{
			initialize();
			executePrivate();
		}

		public void executeSubmit()
		{
			webhook objwebhook;
			objwebhook = new webhook();
			objwebhook.context.SetSubmitInitialConfig(context);
			objwebhook.initialize();
			Submit(executePrivateCatch, objwebhook);
		}

		void executePrivateCatch(object stateInfo)
		{
			try
			{
				((webhook)stateInfo).executePrivate();
			}
			catch (Exception e)
			{
				GXUtil.SaveToEventLog("Design", e);
				throw;
			}
		}

		void executePrivate()
		{
			/* GeneXus formulas */
			/* Output device settings */
			AV9body = AV8httprequest.ToString();
			AV10httpresponse.AddString(AV9body);
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
			AV8httprequest = new GxHttpRequest(context);
			AV10httpresponse = new GxHttpResponse(context);
			/* GeneXus formulas. */
			context.Gx_err = 0;
		}

		private string AV9body;
		private GxHttpRequest AV8httprequest;
		private GxHttpResponse AV10httpresponse;
	}

}
