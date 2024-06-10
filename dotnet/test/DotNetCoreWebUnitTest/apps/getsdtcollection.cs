using System;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs.apps
{
	public class getsdtcollection : GXProcedure
	{
		public getsdtcollection()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("GeneXusXEv2", false);
		}

		public getsdtcollection(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute(DateTime aP0_invoicedate,
							 short aP1_CustomerId,
							 string aP2_Customername,
							 out GXBCCollection<SdtInvoice> aP3_Gxm2rootcol)
		{
			this.AV6invoicedate = aP0_invoicedate;
			this.AV5CustomerId = aP1_CustomerId;
			this.AV7Customername = aP2_Customername;
			this.Gxm2rootcol = new GXBCCollection<SdtInvoice>(context, "Invoice", "TestRestProcs");
			initialize();
			ExecuteImpl();
			aP3_Gxm2rootcol = this.Gxm2rootcol;
		}

		public GXBCCollection<SdtInvoice> executeUdp(DateTime aP0_invoicedate,
													  short aP1_CustomerId,
													  string aP2_Customername)
		{
			execute(aP0_invoicedate, aP1_CustomerId, aP2_Customername, out aP3_Gxm2rootcol);
			return Gxm2rootcol;
		}

		public void executeSubmit(DateTime aP0_invoicedate,
								   short aP1_CustomerId,
								   string aP2_Customername,
								   out GXBCCollection<SdtInvoice> aP3_Gxm2rootcol)
		{
			this.AV6invoicedate = aP0_invoicedate;
			this.AV5CustomerId = aP1_CustomerId;
			this.AV7Customername = aP2_Customername;
			this.Gxm2rootcol = new GXBCCollection<SdtInvoice>(context, "Invoice", "TestRestProcs");
			SubmitImpl();
			aP3_Gxm2rootcol = this.Gxm2rootcol;
		}

		protected override void ExecutePrivate()
		{
			/* GeneXus formulas */
			/* Output device settings */
			Gxm1invoice = new SdtInvoice(context);
			Gxm2rootcol.Add(Gxm1invoice, 0);
			Gxm1invoice.gxTpr_Invoiceid = 1;
			Gxm1invoice.gxTpr_Invoicedate = context.localUtil.YMDToD(2024, 1, 1);
			Gxm1invoice.gxTpr_Customerid = 1;
			Gxm3invoice_level = new SdtInvoice_Level(context);
			Gxm1invoice.gxTpr_Level.Add(Gxm3invoice_level, 0);
			Gxm3invoice_level.gxTpr_Invoicelevelid = 1;
			Gxm3invoice_level.gxTpr_Productid = 1;
			Gxm3invoice_level.gxTpr_Invoicelevelqty = 10;


			Gxm1invoice = new SdtInvoice(context);
			Gxm2rootcol.Add(Gxm1invoice, 0);
			Gxm1invoice.gxTpr_Invoiceid = 2;
			Gxm1invoice.gxTpr_Invoicedate = context.localUtil.YMDToD(2024, 2, 2);
			Gxm1invoice.gxTpr_Customerid = 2;
			Gxm3invoice_level = new SdtInvoice_Level(context);
			Gxm1invoice.gxTpr_Level.Add(Gxm3invoice_level, 0);
			Gxm3invoice_level.gxTpr_Invoicelevelid = 2;
			Gxm3invoice_level.gxTpr_Productid = 2;
			Gxm3invoice_level.gxTpr_Invoicelevelqty = 20;
			cleanup();
		}

		public override void cleanup()
		{
			CloseCursors();
			if (IsMain)
			{
				context.CloseConnections();
			}
			ExitApp();
		}

		public override void initialize()
		{
			Gxm1invoice = new SdtInvoice(context);
			Gxm3invoice_level = new SdtInvoice_Level(context);
			/* GeneXus formulas. */
		}

		private short AV5CustomerId;
		private string AV7Customername;
		private DateTime AV6invoicedate;
		private GXBCCollection<SdtInvoice> Gxm2rootcol;
		private SdtInvoice Gxm1invoice;
		private SdtInvoice_Level Gxm3invoice_level;
		private GXBCCollection<SdtInvoice> aP3_Gxm2rootcol;
	}

}
