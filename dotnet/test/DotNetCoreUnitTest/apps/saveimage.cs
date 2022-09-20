using GeneXus.Application;
using GeneXus.Data.NTier;

using GeneXus.Procedure;
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

		public void execute(string aP0_ImageDescription, string aP1_Image)
		{
			System.Console.WriteLine("SaveImage executed:" + aP0_ImageDescription);
		}

		public override bool UploadEnabled()
		{
			return true;
		}


		public override void initialize()
		{

		}
	}

}
