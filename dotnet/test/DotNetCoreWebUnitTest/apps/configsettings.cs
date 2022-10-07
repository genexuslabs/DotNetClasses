using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Data.NTier;
using GeneXus.Http.Server;
using GeneXus.Procedure;

namespace GeneXus.Programs.apps
{
	public class configsettings : GXWebProcedure
	{

		public configsettings()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("Carmine");
		}

		public configsettings(IGxContext context)
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
			GxHttpRequest AV24httpRequest = new GxHttpRequest(context);
			GxHttpResponse AV25httpResponse = new GxHttpResponse(context);
			string AV26character = AV24httpRequest.GetHeader("ConfigurationSetting");
			string confValue = ConfigurationManager.GetValue(AV26character);
			AV25httpResponse.AppendHeader("ConfigurationSettingValue", confValue);

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
		}
	}

}
