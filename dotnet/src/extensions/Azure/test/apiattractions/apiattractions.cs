using GeneXus.Application;
using GeneXus.Data;
using GeneXus.Http;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs
{
	public class apiattractions : GXProcedure
	{
		protected override bool IntegratedSecurityEnabled
		{
			get
			{
				return true;
			}

		}

		protected override GAMSecurityLevel IntegratedSecurityLevel
		{
			get
			{
				return GAMSecurityLevel.SecurityLow;
			}

		}

		public apiattractions()
		{
			context = new GxContext();
			dsGAM = context.GetDataStore("GAM");
			dsDefault = context.GetDataStore("Default");
			IsMain = true;
			IsApiObject = true;
		}

		public apiattractions(IGxContext context)
		{
			this.context = context;
			IsMain = false;
			IsApiObject = true;
			dsGAM = context.GetDataStore("GAM");
			dsDefault = context.GetDataStore("Default");
		}

		public void execute()
		{
			executePrivate();
		}

		void executePrivate()
		{
			/* GeneXus formulas */
			/* Output device settings */
			this.cleanup();
		}

		public void gxep_listattractions(out GXBaseCollection<SdtAttractionOut_Attraction> aP0_AttractionOut)
		{

			initialize();
			initialized = 1;
			GXBaseCollection<SdtAttractionOut_Attraction> Gxm2rootcol = new GXBaseCollection<SdtAttractionOut_Attraction>(context, "Attraction", "TestServerlessGAM");
			/* ListAttractions Constructor */
			SdtAttractionOut_Attraction Gxm1attractionout = new SdtAttractionOut_Attraction(context);
			Gxm2rootcol.Add(Gxm1attractionout, 0);
			Gxm1attractionout.gxTpr_Attractionid = 1;
			Gxm1attractionout.gxTpr_Attractionname = "AttractionName";

			aP0_AttractionOut=Gxm2rootcol;
		}

		public override void cleanup()
		{
			CloseOpenCursors();
		}

		protected void CloseOpenCursors()
		{
		}

		public override void initialize()
		{
			/* GeneXus formulas. */
			context.Gx_err = 0;
		}

		protected short initialized;
		protected IGxDataStore dsGAM;
		protected IGxDataStore dsDefault;
	}

}
