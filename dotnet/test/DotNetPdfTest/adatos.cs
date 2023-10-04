using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Http.Server;
using GeneXus.Procedure;
using GeneXus.Utils;
namespace GeneXus.Programs
{
	public class adatos : GXProcedure
   {
      public int executeCmdLine( string[] args )
      {
         return ExecuteCmdLine(args); ;
      }

      protected override int ExecuteCmdLine( string[] args )
      {
         string aP0_staticDir = new string(' ',0)  ;
         GXBaseCollection<SdtFCKTstCollection_FCKTst> aP1_Gxm2rootcol = new GXBaseCollection<SdtFCKTstCollection_FCKTst>()  ;
         if ( 0 < args.Length )
         {
            aP0_staticDir=((string)(args[0]));
         }
         else
         {
            aP0_staticDir="";
         }
         execute(aP0_staticDir, out aP1_Gxm2rootcol);
         return GX.GXRuntime.ExitCode ;
      }

      public adatos( )
      {
         context = new GxContext(  );
         DataStoreUtil.LoadDataStores( context);
         IsMain = true;
         context.SetDefaultTheme("Carmine", false);
      }

      public adatos( IGxContext context )
      {
         this.context = context;
         IsMain = false;
      }

      public void execute( string aP0_staticDir ,
                           out GXBaseCollection<SdtFCKTstCollection_FCKTst> aP1_Gxm2rootcol )
      {
         this.AV6staticDir = aP0_staticDir;
         this.Gxm2rootcol = new GXBaseCollection<SdtFCKTstCollection_FCKTst>( context, "FCKTst", "TestReportes") ;
         initialize();
         ExecutePrivate();
         aP1_Gxm2rootcol=this.Gxm2rootcol;
      }

      public GXBaseCollection<SdtFCKTstCollection_FCKTst> executeUdp( string aP0_staticDir )
      {
         execute(aP0_staticDir, out aP1_Gxm2rootcol);
         return Gxm2rootcol ;
      }

      public void executeSubmit( string aP0_staticDir ,
                                 out GXBaseCollection<SdtFCKTstCollection_FCKTst> aP1_Gxm2rootcol )
      {
         this.AV6staticDir = aP0_staticDir;
         this.Gxm2rootcol = new GXBaseCollection<SdtFCKTstCollection_FCKTst>( context, "FCKTst", "TestReportes") ;
         SubmitImpl();
         aP1_Gxm2rootcol=this.Gxm2rootcol;
      }

      protected override void ExecutePrivate( )
      {
         /* GeneXus formulas */
         /* Output device settings */
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 1;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "tabla basica";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" style=\"width: 500px;\">	<caption>Tabla basica</caption><thead><tr>"+"<th scope=\"col\"><span style=\"color: red;\">row 1 cell 1 bold rojo</span></th><th scope=\"col\"><span style=\"color: blue;\">row1 cell 2 bold azul</span></th>"+"</tr></thead><tbody><tr><td>row 2 cell 1</td><td>row2 cell 2</td></tr><tr>	<td>row 3 cell 1</td><td>row 3 cell 2</td></tr></tbody>"+"</table><p>&nbsp;</p>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 1;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 2;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "lista de elementos";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<ol><li>Coffee</li><li>Milk</li></ol>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 2;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 3;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "parrafos";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<p>This is a paragraph 1<p>This is a paragraph 2";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 3;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 4;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "link";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<a href=\"http://www.w3schools.com/\">Visit W3Schools</a>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 4;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 5;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "imagen";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = StringUtil.Format( "<img src=\"%1%2Resources/w3schools.jpg\" width=\"52\" height=\"71\" />", AV5Httprequest.BaseURL, AV6staticDir, "", "", "", "", "", "", "");
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 5;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 6;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "horizontal rule";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<html><body><p>The hr tag defines a horizontal rule:</p><hr /><p>This is a paragraph</p><hr /><p>This is a paragraph</p>"+"<hr /><p>This is a paragraph</p></body></html>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 6;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 7;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "fonts";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<html><body><h1 style=\"font-family:verdana;\">A heading</h1><p style=\"font-family:arial;color:red;font-size:20px;\">A paragraph.</p>"+"</body></html>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 7;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 8;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "nested lists";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<html><body><h4>A nested List:</h4><ul>  <li>Coffee</li>  <li>Tea    <ul>    <li>Black tea</li>    <li>Green tea"+"  <ul>      <li>China</li>      <li>Africa</li>      </ul>    </li>    </ul>  </li>  <li>Milk</li></ul></body></html>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 8;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 9;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "radio buttons";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<html><body><form action=\"\"><input type=\"radio\" name=\"sex\" value=\"male\" /> Male<br /><input type=\"radio\" name=\"sex\" value=\"female\" /> Female"+"<input type=\"checkbox\" name=\"vehicle\" value=\"Bike\" /> I have a bike<br /><input type=\"checkbox\" name=\"vehicle\" value=\"Car\" /> I have a car </form>"+"</body></html>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 9;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 10;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "color en texto parag";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<h1 style=\"font-family:verdana;\">A heading</h1><p style=\"font-family:verdana;font-size:12px;color:green\">"+"This is a paragraph with some text in it. This is a paragraph with some text in it. This is a paragraph with some text in it. This is a paragraph with some text in it.</p>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 10;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 12;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "mail to con link";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<p>This is another mailto link:<a href=\"mailto:someone@example.com?cc=someoneelse@example.com&bcc=andsomeoneelse@example.com&subject=Summer%20Party&body=You%20are%20invited%20to%20a%20big%20summer%20party!\">Send mail!</a></p>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 12;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 13;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "table borders";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<html><body><h4>With a normal border:</h4>  <table border=\"1\"><tr>  <td>First</td>  <td>Row</td></tr>   <tr>"+"<td>Second</td>  <td>Row</td></tr></table><h4>With a thick border:</h4>  <table border=\"8\"><tr>  <td>First</td>  <td>Row</td>"+"</tr>   <tr>  <td>Second</td>  <td>Row</td></tr></table><h4>With a very thick border:</h4>  <table border=\"15\"><tr>  <td>First</td>"+"<td>Row</td></tr>   <tr>  <td>Second</td>  <td>Row</td></tr></table></body></html>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 13;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 14;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "espacios";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<p>	&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; espacios &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;en blanco &nbsp; &nbsp; fin</p>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 14;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 15;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "color rgb";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<p>	<span>test </span></p><p>	<strong><span style=\"color:#ff0000;\">test sin span</span></strong></p>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 15;
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         Gxm2rootcol.Add(Gxm1fcktstcollection, 0);
         Gxm1fcktstcollection.gxTpr_Fcktstid = 16;
         Gxm1fcktstcollection.gxTpr_Fcktstdsc = "Test centralizado";
         Gxm1fcktstcollection.gxTpr_Fcktstfck = "<h1 style=\"text-align:center\">Teste de centralizado</h1> <p style=\"text-align:right\">A direita</p> <p>Esquerda</p>";
         Gxm1fcktstcollection.gxTpr_Fcktstotro = 15;
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
         Gxm1fcktstcollection = new SdtFCKTstCollection_FCKTst(context);
         AV5Httprequest = new GxHttpRequest( context);
         /* GeneXus formulas. */
      }

      private string AV6staticDir ;
      private GXBaseCollection<SdtFCKTstCollection_FCKTst> aP1_Gxm2rootcol ;
      private GxHttpRequest AV5Httprequest ;
      private GXBaseCollection<SdtFCKTstCollection_FCKTst> Gxm2rootcol ;
      private SdtFCKTstCollection_FCKTst Gxm1fcktstcollection ;
   }

}
