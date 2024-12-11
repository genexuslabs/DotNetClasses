using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs.apps
{
	public class getcollection : GXProcedure
   {
      public getcollection( )
      {
         context = new GxContext(  );
         DataStoreUtil.LoadDataStores( context);
         IsMain = true;
         context.SetDefaultTheme("FromString", true);
      }

      public getcollection( IGxContext context )
      {
         this.context = context;
         IsMain = false;
      }

      public void execute( out short aP0_CliType ,
                           out GxSimpleCollection<int> aP1_CliCode )
      {
         this.clitype = 0 ;
         this.cliCod = new GxSimpleCollection<int>() ;
         initialize();
         ExecuteImpl();
         aP0_CliType=this.clitype;
         aP1_CliCode=this.cliCod;
      }

      public GxSimpleCollection<int> executeUdp( out short aP0_CliType )
      {
         execute(out aP0_CliType, out aP1_CliCode);
         return cliCod ;
      }

      public void executeSubmit( out short aP0_CliType ,
                                 out GxSimpleCollection<int> aP1_CliCode )
      {
         this.clitype = 0 ;
         this.cliCod = new GxSimpleCollection<int>() ;
         SubmitImpl();
         aP0_CliType=this.clitype;
         aP1_CliCode=this.cliCod;
      }

      protected override void ExecutePrivate( )
      {
         /* GeneXus formulas */
         /* Output device settings */
         clitype = 1;
         cliCod.Add(1, 0);
         cliCod.Add(2, 0);
         this.cleanup();
      }

      public override void cleanup( )
      {
         CloseCursors();
         if ( IsMain )
         {
            context.CloseConnections();
         }
         ExitApp();
      }

      public override void initialize( )
      {
         cliCod = new GxSimpleCollection<int>();

      }

      private short clitype ;
      private GxSimpleCollection<int> cliCod ;
      private GxSimpleCollection<int> aP1_CliCode ;
   }

}
