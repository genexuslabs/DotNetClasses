using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Http.Server;
using GeneXus.Procedure;
using Stubble.Core.Contexts;
namespace GeneXus.Programs.apps
{
	public class saveimage : GXProcedure
	{
		public saveimage()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
		}

		public saveimage(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute(string aP0_ImageDescription, string aP1_Image, out string aP2_ImagePath)
		{
			System.Console.WriteLine("SaveImage executed:" + aP0_ImageDescription);
			aP2_ImagePath= context.GetScriptPath() + aP1_Image;
		}

		public override bool UploadEnabled()
		{
			return true;
		}


		public override void initialize()
		{
			httpResponse = new GxHttpResponse(context);
		}
		GxHttpResponse httpResponse;
	}

}
