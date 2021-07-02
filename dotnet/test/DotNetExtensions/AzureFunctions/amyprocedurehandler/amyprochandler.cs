using GeneXus.Application;
using GeneXus.Procedure;

namespace GeneXus.Programs
{
	public class amyprochandler : GXProcedure
	{
		public amyprochandler()
		{
			context = new GxContext();
			IsMain = true;
		}

		public override void cleanup()
		{
			if (IsMain)
			{
				context.CloseConnections();
			}
			ExitApp();
		}

		public void execute(genexusserverlessapi.SdtEventMessages aP0_EventMessages,
						   out genexusserverlessapi.SdtEventMessageResponse aP1_ExternalEventMessageResponse)
		{
			aP1_ExternalEventMessageResponse = new GeneXus.Programs.genexusserverlessapi.SdtEventMessageResponse(context);
			aP1_ExternalEventMessageResponse.gxTpr_Handled = true;
		}

		public override void initialize()
		{
			context.Gx_err = 0;
		}
	}
}
