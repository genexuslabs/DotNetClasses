using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Http.Client;
using GeneXus.Http.Server;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs
{
	public class aprochttpgetstatic : GXWebProcedure
   {
      public override async Task WebExecuteAsync( )
      {
         context.SetDefaultTheme("HttpClientTest", true);
         initialize();
         {
            await ExecutePrivateAsync();
         }
         await CleanupAsync();
      }

      public aprochttpgetstatic( )
      {
         context = new GxContext(  );
         DataStoreUtil.LoadDataStores( context);
         IsMain = true;
         context.SetDefaultTheme("HttpClientTest", true);
      }

      public aprochttpgetstatic( IGxContext context )
      {
         this.context = context;
         IsMain = false;
      }

      public void execute( )
      {
         initialize();
         ExecuteImpl();
      }

      public void executeSubmit( )
      {
         SubmitImpl();
      }

      protected override async Task ExecutePrivateAsync( )
      {
         /* GeneXus formulas */
         /* Output device settings */
         AV14grant_type = "refresh_token";
         AV12ClientId = AV10HttpRequest.GetValue("client_id");
         AV16Refresh_Token = AV10HttpRequest.GetValue("refresh_token");
         AV8baseUrl = "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png";
         AV17baseUrlWithParms = StringUtil.Format( "%1?grant_type=%2&client_id=%3&refresh_token=%4", AV8baseUrl, StringUtil.Trim( AV14grant_type), StringUtil.Trim( AV12ClientId), StringUtil.Trim( AV16Refresh_Token), "", "", "", "", "");
         AV9Httpclient.AddHeader("Content-Type", "application/x-www-form-urlencoded");
         AV9Httpclient.Execute("GET", StringUtil.Trim( AV17baseUrlWithParms));
         AV18Httpclientstr = AV9Httpclient.ToString();
         AV11HttpResponse.AddString(AV18Httpclientstr);
         if ( context.WillRedirect( ) )
         {
            context.Redirect( context.wjLoc );
            context.wjLoc = "";
         }
         await CleanupAsync();
      }

      protected override async Task CleanupAsync( )
      {
         CloseCursors();
         base.cleanup();
         if ( IsMain )
         {
            await CloseConnectionsAsync();
         }
         ExitApp();
      }

		
	  public override void initialize( )
      {
        // GXKey = "";
         //gxfirstwebparm = "";
         AV14grant_type = "";
         AV12ClientId = "";
         AV10HttpRequest = new GxHttpRequest( context);
         AV16Refresh_Token = "";
         AV8baseUrl = "";
         AV17baseUrlWithParms = "";
         AV9Httpclient = new GxHttpClient( context);
         AV18Httpclientstr = "";
         AV11HttpResponse = new GxHttpResponse( context);
         /* GeneXus formulas. */
      }

      //private short gxcookieaux ;
      //private string GXKey ;
      //private string gxfirstwebparm ;
      private string AV14grant_type ;
      private string AV12ClientId ;
      private string AV16Refresh_Token ;
      private string AV17baseUrlWithParms ;
      private string AV18Httpclientstr ;
      //private bool entryPointCalled ;
      private string AV8baseUrl ;
      private GxHttpRequest AV10HttpRequest ;
      private GxHttpClient AV9Httpclient ;
      private GxHttpResponse AV11HttpResponse ;
   }

}
