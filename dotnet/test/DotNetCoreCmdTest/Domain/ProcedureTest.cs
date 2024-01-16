using System;
using System.IO;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Data;
using GeneXus.Data.NTier;
using GeneXus.Data.NTier.ADO;
using GeneXus.Procedure;
using UnitTesting;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class ProcedureInitializationTest : FileSystemTest
	{
		[Fact]
		public void PublicTempStorageNotCreated()
		{
			bool isKey = Config.GetValueOf("CS_BLOB_PATH", out string blobPath);
			Assert.True(isKey);
			string path = Path.Combine(BaseDir, blobPath);
			aprocedure1 aprocedure1 = new aprocedure1();
			aprocedure1.execute();
			Assert.False(Directory.Exists(path), $"The directory {blobPath} should only be created when necessary.");

			isKey = Config.GetValueOf("TMPMEDIA_DIR", out string tempMediaDir);
			Assert.True(isKey);
			path = Path.Combine(BaseDir, tempMediaDir);
			Assert.False(Directory.Exists(path), $"The directory {tempMediaDir} should only be created when necessary.");
		}
	}
	public class aprocedure1 : GXProcedure
	{

		public int executeCmdLine(string[] args)
		{
			return ExecuteCmdLine(args); ;
		}

		protected override int ExecuteCmdLine(string[] args)
		{
			execute();
			return GX.GXRuntime.ExitCode;
		}

		public aprocedure1()
		{
			context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			IsMain = true;
			context.SetDefaultTheme("CmdLine", true);
		}

		public aprocedure1(IGxContext context)
		{
			this.context = context;
			IsMain = false;
		}

		public void execute()
		{
			initialize();
			try
			{
				ExecutePrivate();
			}
			catch (ReturnInErrorHandlerException)
			{
				this.cleanup();
				return;
			}
		}

		public void executeSubmit()
		{
			SubmitImpl();
		}

		protected override void ExecutePrivate()
		{
			/* GeneXus formulas */
			/* Output device settings */
			pr_default.close(0);
			this.cleanup();
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
			P000D2_A1GXM01_CtlName = new string[] { "" };
			pr_default = new DataStoreProvider(context, new aprocedure1__default(),
			   new Object[][] {
				new Object[] {
			   P000D2_A1GXM01_CtlName
			   }
			   }
			);
			/* GeneXus formulas. */
		}

		private IDataStoreProvider pr_default;
		private string[] P000D2_A1GXM01_CtlName;
	}

	public class aprocedure1__default : DataStoreHelperBase, IDataStoreHelper
	{
		public ICursor[] getCursors()
		{
			cursorDefinitions();
			return new Cursor[] {
		  new ForEachCursor(def[0])
	   };
		}

		private static CursorDef[] def;
		private void cursorDefinitions()
		{
			if (def == null)
			{
				Object[] prmP000D2;
				prmP000D2 = new Object[] {
		  };
				def = new CursorDef[] {
			  new CursorDef("P000D2", "SELECT TOP 1 [GXM01_CtlName] FROM [GXM01] WHERE [GXM01_CtlName] = 'FCA010_DL_Name' ORDER BY [GXM01_CtlName] ",false, GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK, false, this,prmP000D2,1, GxCacheFrequency.OFF ,false,true )
		  };
			}
		}

		public void getResults(int cursor,
								IFieldGetter rslt,
								Object[] buf)
		{
			switch (cursor)
			{
				case 0:
					((string[])buf[0])[0] = rslt.getVarchar(1);
					return;
			}
		}

	}

}
