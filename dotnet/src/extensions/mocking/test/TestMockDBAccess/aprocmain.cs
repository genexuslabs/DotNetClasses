using System;
using GeneXus.Application;
using GeneXus.Data;
using GeneXus.Data.NTier;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs
{
	public class aprocmain : GXProcedure
	{
		public int executeCmdLine(string[] args)
		{
			short aP0_clientid;
			short aP1_Number;
			string aP2_Message = new string(' ', 0);
			if (0 < args.Length)
			{
				aP0_clientid = ((short)(NumberUtil.Val((string)(args[0]), ".")));
			}
			else
			{
				aP0_clientid = 0;
			}
			if (1 < args.Length)
			{
				aP1_Number = ((short)(NumberUtil.Val((string)(args[1]), ".")));
			}
			else
			{
				aP1_Number = 0;
			}
			if (2 < args.Length)
			{
				aP2_Message = ((string)(args[2]));
			}
			else
			{
				aP2_Message = "";
			}
			execute(aP0_clientid, ref aP1_Number, out aP2_Message);
			return 0;
		}

		public aprocmain()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			dsDefault = context.GetDataStore("Default");
			IsMain = true;
			context.SetDefaultTheme("Gxtestprocs", true);
		}

		public aprocmain(IGxContext context)
		{
			this.context = context;
			IsMain = false;
			dsDefault = context.GetDataStore("Default");
		}

		public void execute(short aP0_clientid,
							 ref short aP1_Number,
							 out string aP2_Message)
		{
			this.AV8clientid = aP0_clientid;
			this.AV9Number = aP1_Number;
			this.AV10Message = "";
			initialize();
			ExecuteImpl();
			aP1_Number = this.AV9Number;
			aP2_Message = this.AV10Message;
		}

		public string executeUdp(short aP0_clientid,
								  ref short aP1_Number)
		{
			execute(aP0_clientid, ref aP1_Number, out aP2_Message);
			return AV10Message;
		}

		public void executeSubmit(short aP0_clientid,
								   ref short aP1_Number,
								   out string aP2_Message)
		{
			aprocmain objaprocmain;
			objaprocmain = new aprocmain();
			objaprocmain.AV8clientid = aP0_clientid;
			objaprocmain.AV9Number = aP1_Number;
			objaprocmain.AV10Message = "";
			objaprocmain.context.SetSubmitInitialConfig(context);
			objaprocmain.initialize();
			Submit(executePrivateCatch, objaprocmain);
			aP1_Number = this.AV9Number;
			aP2_Message = this.AV10Message;
		}

		void executePrivateCatch(object stateInfo)
		{
			try
			{
				((aprocmain)stateInfo).ExecutePrivate();
			}
			catch (Exception e)
			{
				GXUtil.SaveToEventLog("Design", e);
				Console.WriteLine(e.ToString());
			}
		}

		protected override string[] GetParameters()
		{
			return new string[] { "AV8clientid", "AV9Number", "AV10Message" }; ;
		}

		protected override void ExecutePrivate()
		{
			AV10Message = string.Empty;
			this.cleanup();
		}

		public override void cleanup()
		{
			CloseOpenCursors();
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
			AV10Message = "";
			/* GeneXus formulas. */
			context.Gx_err = 0;
		}

		private short AV8clientid;
		private short AV9Number;
		private string AV10Message;
		private IGxDataStore dsDefault;
		private string aP2_Message;
	}


}
