using System;
using System.Threading;
using GeneXus.Application;
using GeneXus.Data;
using GeneXus.Data.ADO;
using GeneXus.Data.NTier;
using GeneXus.Data.NTier.ADO;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs
{
	public class aabmtest : GXProcedure
   {

      public int executeCmdLine( string[] args )
      {
         execute();
         return 0 ;
      }

      public aabmtest( )
      {
         context = new GxContext(  );
         DataStoreUtil.LoadDataStores( context);
         dsDefault = context.GetDataStore("Default");
         IsMain = true;
         context.SetDefaultTheme("Carmine");
      }

      public aabmtest( IGxContext context )
      {
         this.context = context;
         IsMain = false;
         dsDefault = context.GetDataStore("Default");
      }

      public void execute( )
      {
         initialize();
         executePrivate();
      }

      public void executeSubmit( )
      {
         aabmtest objaabmtest;
         objaabmtest = new aabmtest();
         objaabmtest.context.SetSubmitInitialConfig(context);
         objaabmtest.initialize();
         ThreadPool.QueueUserWorkItem( PropagateCulture(new WaitCallback( executePrivateCatch )),objaabmtest);
      }

      void executePrivateCatch( object stateInfo )
      {
         try
         {
            ((aabmtest)stateInfo).executePrivate();
         }
         catch ( Exception e )
         {
            GXUtil.SaveToEventLog( "Design", e);
            Console.WriteLine( e.ToString() );
         }
      }

      void executePrivate( )
      {
         /* GeneXus formulas */
         /* Output device settings */
         AV8date = DateTimeUtil.ServerDate( context, pr_default);
         /*
            INSERT RECORD ON TABLE genres

         */
         A3Genreid = 99;
         A4Name = "pop";
         /* Using cursor P000C2 */
         pr_default.execute(0, new Object[] {A3Genreid, A4Name});
         pr_default.close(0);
         dsDefault.SmartCacheProvider.SetUpdated("genres");
         if ( (pr_default.getStatus(0) == 1) )
         {
            context.Gx_err = 1;
            Gx_emsg = (string)(context.GetMessage( "GXM_noupdate", ""));
         }
         else
         {
            context.Gx_err = 0;
            Gx_emsg = "";
         }
         /* End Insert */
         /* Optimized UPDATE. */
         /* Using cursor P000C3 */
         pr_default.execute(1);
         pr_default.close(1);
         dsDefault.SmartCacheProvider.SetUpdated("genres");
         /* End optimized UPDATE. */
         /* Using cursor P000C4 */
         pr_default.execute(2);
         while ( (pr_default.getStatus(2) != 101) )
         {
            A3Genreid = P000C4_A3Genreid[0];
            A4Name = P000C4_A4Name[0];
            context.StatusMessage( "GenreName:"+A4Name );
            /* Exiting from a For First loop. */
            if (true) break;
         }
         pr_default.close(2);
         /* Optimized DELETE. */
         /* Using cursor P000C5 */
         pr_default.execute(3);
         pr_default.close(3);
         dsDefault.SmartCacheProvider.SetUpdated("genres");
         /* End optimized DELETE. */
         context.RollbackDataStores("abmtest",pr_default);
         this.cleanup();
      }

      public override void cleanup( )
      {
         context.CommitDataStores("abmtest",pr_default);
         CloseOpenCursors();
         if ( IsMain )
         {
            context.CloseConnections();
         }
         ExitApp();
      }

      protected void CloseOpenCursors( )
      {
      }

      public override void initialize( )
      {
         AV8date = DateTime.MinValue;
         A4Name = "";
         Gx_emsg = "";
         P000C4_A3Genreid = new short[1] ;
         P000C4_A4Name = new string[] {""} ;
         pr_default = new DataStoreProvider(context, new GeneXus.Programs.aabmtest__default(),
            new Object[][] {
                new Object[] {
               }
               , new Object[] {
               }
               , new Object[] {
               P000C4_A3Genreid, P000C4_A4Name
               }
               , new Object[] {
               }
            }
         );
         /* GeneXus formulas. */
         context.Gx_err = 0;
      }

      private short A3Genreid ;
      private string Gx_emsg ;
      private DateTime AV8date ;
      private string A4Name ;
      private IGxDataStore dsDefault ;
      private IDataStoreProvider pr_default ;
      private short[] P000C4_A3Genreid ;
      private string[] P000C4_A4Name ;
   }

   public class aabmtest__default : DataStoreHelperBase, IDataStoreHelper
   {
      public ICursor[] getCursors( )
      {
         cursorDefinitions();
         return new Cursor[] {
          new UpdateCursor(def[0])
         ,new UpdateCursor(def[1])
         ,new ForEachCursor(def[2])
         ,new UpdateCursor(def[3])
       };
    }

    private static CursorDef[] def;
    private void cursorDefinitions( )
    {
       if ( def == null )
       {
          Object[] prmP000C2;
          prmP000C2 = new Object[] {
          new ParDef("@Genreid",GXType.Int16,4,0) ,
          new ParDef("@Name",GXType.NVarChar,120,0)
          };
          Object[] prmP000C3;
          prmP000C3 = new Object[] {
          };
          Object[] prmP000C4;
          prmP000C4 = new Object[] {
          };
          Object[] prmP000C5;
          prmP000C5 = new Object[] {
          };
          def= new CursorDef[] {
              new CursorDef("P000C2", "INSERT INTO genres (Genreid, Name) VALUES (@Genreid, @Name)", GxErrorMask.GX_NOMASK,prmP000C2)
             ,new CursorDef("P000C3", "UPDATE genres SET Name='new pop'  WHERE Genreid = 99", GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK,prmP000C3)
             ,new CursorDef("P000C4", "SELECT Genreid, Name FROM genres WHERE Genreid = 99 ORDER BY Genreid ",false, GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK, false, this,prmP000C4,1, GxCacheFrequency.OFF ,false,true )
             ,new CursorDef("P000C5", "DELETE FROM genres  WHERE Genreid = 99", GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK,prmP000C5)
          };
       }
    }

    public void getResults( int cursor ,
                            IFieldGetter rslt ,
                            Object[] buf )
    {
       switch ( cursor )
       {
             case 2 :
                ((short[]) buf[0])[0] = rslt.getShort(1);
                ((string[]) buf[1])[0] = rslt.getVarchar(2);
                return;
       }
    }

 }

}
