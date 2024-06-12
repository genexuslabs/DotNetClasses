using System;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Http.Server;
using GeneXus.Procedure;
using GeneXus.Utils;
using GeneXus.XML;
namespace GeneXus.Programs.apps
{
	public class getsdtcollection : GXWebProcedure
   {
      public override void webExecute( )
      {
         context.SetDefaultTheme("GeneXusXEv2", false);
         initialize();
         AV10CustomerSdtList = new GXBaseCollection<SdtCustomerSdtSimple>( context, "CustomerSdtSimple", "TestRestProcs") ;
         if ( ! context.isAjaxRequest( ) )
         {
            GXSoapHTTPResponse.AppendHeader("Content-type", "text/xml;charset=utf-8");
         }
         if ( StringUtil.StrCmp(StringUtil.Lower( GXSoapHTTPRequest.Method), "get") == 0 )
         {
            if ( StringUtil.StrCmp(StringUtil.Lower( GXSoapHTTPRequest.QueryString), "wsdl") == 0 )
            {
               GXSoapXMLWriter.OpenResponse(GXSoapHTTPResponse);
               GXSoapXMLWriter.WriteStartDocument("utf-8", 0);
               GXSoapXMLWriter.WriteStartElement("definitions");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTest");
               GXSoapXMLWriter.WriteAttribute("targetNamespace", "TestRestProcs");
               GXSoapXMLWriter.WriteAttribute("xmlns:wsdlns", "TestRestProcs");
               GXSoapXMLWriter.WriteAttribute("xmlns:soap", "http://schemas.xmlsoap.org/wsdl/soap/");
               GXSoapXMLWriter.WriteAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
               GXSoapXMLWriter.WriteAttribute("xmlns", "http://schemas.xmlsoap.org/wsdl/");
               GXSoapXMLWriter.WriteAttribute("xmlns:tns", "TestRestProcs");
               GXSoapXMLWriter.WriteStartElement("types");
               GXSoapXMLWriter.WriteStartElement("schema");
               GXSoapXMLWriter.WriteAttribute("targetNamespace", "TestRestProcs");
               GXSoapXMLWriter.WriteAttribute("xmlns", "http://www.w3.org/2001/XMLSchema");
               GXSoapXMLWriter.WriteAttribute("xmlns:SOAP-ENC", "http://schemas.xmlsoap.org/soap/encoding/");
               GXSoapXMLWriter.WriteAttribute("elementFormDefault", "qualified");
               GXSoapXMLWriter.WriteStartElement("complexType");
               GXSoapXMLWriter.WriteAttribute("name", "ArrayOfCustomerSdtSimple");
               GXSoapXMLWriter.WriteStartElement("sequence");
               GXSoapXMLWriter.WriteStartElement("element");
               GXSoapXMLWriter.WriteAttribute("minOccurs", "0");
               GXSoapXMLWriter.WriteAttribute("maxOccurs", "unbounded");
               GXSoapXMLWriter.WriteAttribute("name", "CustomerSdtSimple");
               GXSoapXMLWriter.WriteAttribute("type", "tns:CustomerSdtSimple");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("complexType");
               GXSoapXMLWriter.WriteAttribute("name", "CustomerSdtSimple");
               GXSoapXMLWriter.WriteStartElement("sequence");
               GXSoapXMLWriter.WriteStartElement("element");
               GXSoapXMLWriter.WriteAttribute("name", "CustomerId");
               GXSoapXMLWriter.WriteAttribute("type", "xsd:short");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("element");
               GXSoapXMLWriter.WriteAttribute("name", "CustomerName");
               GXSoapXMLWriter.WriteAttribute("type", "xsd:string");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("element");
               GXSoapXMLWriter.WriteAttribute("name", "CustomerBirthDate");
               GXSoapXMLWriter.WriteAttribute("type", "xsd:date");
               GXSoapXMLWriter.WriteAttribute("nillable", "true");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("element");
               GXSoapXMLWriter.WriteAttribute("name", "CustomerPayDate");
               GXSoapXMLWriter.WriteAttribute("type", "xsd:dateTime");
               GXSoapXMLWriter.WriteAttribute("nillable", "true");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("element");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTest.Execute");
               GXSoapXMLWriter.WriteStartElement("complexType");
               GXSoapXMLWriter.WriteStartElement("sequence");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("element");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTest.ExecuteResponse");
               GXSoapXMLWriter.WriteStartElement("complexType");
               GXSoapXMLWriter.WriteStartElement("sequence");
               GXSoapXMLWriter.WriteElement("element", "");
               GXSoapXMLWriter.WriteAttribute("minOccurs", "1");
               GXSoapXMLWriter.WriteAttribute("maxOccurs", "1");
               GXSoapXMLWriter.WriteAttribute("name", "Customersdtlist");
               GXSoapXMLWriter.WriteAttribute("type", "tns:ArrayOfCustomerSdtSimple");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("message");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTest.ExecuteSoapIn");
               GXSoapXMLWriter.WriteElement("part", "");
               GXSoapXMLWriter.WriteAttribute("name", "parameters");
               GXSoapXMLWriter.WriteAttribute("element", "tns:GetCustomersTest.Execute");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("message");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTest.ExecuteSoapOut");
               GXSoapXMLWriter.WriteElement("part", "");
               GXSoapXMLWriter.WriteAttribute("name", "parameters");
               GXSoapXMLWriter.WriteAttribute("element", "tns:GetCustomersTest.ExecuteResponse");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("portType");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTestSoapPort");
               GXSoapXMLWriter.WriteStartElement("operation");
               GXSoapXMLWriter.WriteAttribute("name", "Execute");
               GXSoapXMLWriter.WriteElement("input", "");
               GXSoapXMLWriter.WriteAttribute("message", "wsdlns:"+"GetCustomersTest.ExecuteSoapIn");
               GXSoapXMLWriter.WriteElement("output", "");
               GXSoapXMLWriter.WriteAttribute("message", "wsdlns:"+"GetCustomersTest.ExecuteSoapOut");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("binding");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTestSoapBinding");
               GXSoapXMLWriter.WriteAttribute("type", "wsdlns:"+"GetCustomersTestSoapPort");
               GXSoapXMLWriter.WriteElement("soap:binding", "");
               GXSoapXMLWriter.WriteAttribute("style", "document");
               GXSoapXMLWriter.WriteAttribute("transport", "http://schemas.xmlsoap.org/soap/http");
               GXSoapXMLWriter.WriteStartElement("operation");
               GXSoapXMLWriter.WriteAttribute("name", "Execute");
               GXSoapXMLWriter.WriteElement("soap:operation", "");
               GXSoapXMLWriter.WriteAttribute("soapAction", "TestRestProcsaction/"+"AGETCUSTOMERSTEST.Execute");
               GXSoapXMLWriter.WriteStartElement("input");
               GXSoapXMLWriter.WriteElement("soap:body", "");
               GXSoapXMLWriter.WriteAttribute("use", "literal");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("output");
               GXSoapXMLWriter.WriteElement("soap:body", "");
               GXSoapXMLWriter.WriteAttribute("use", "literal");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteStartElement("service");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTest");
               GXSoapXMLWriter.WriteStartElement("port");
               GXSoapXMLWriter.WriteAttribute("name", "GetCustomersTestSoapPort");
               GXSoapXMLWriter.WriteAttribute("binding", "wsdlns:"+"GetCustomersTestSoapBinding");
               GXSoapXMLWriter.WriteElement("soap:address", "");
               GXSoapXMLWriter.WriteAttribute("location", "http://"+context.GetServerName( )+((context.GetServerPort( )>0)&&(context.GetServerPort( )!=80)&&(context.GetServerPort( )!=443) ? ":"+StringUtil.LTrim( StringUtil.Str( (decimal)(context.GetServerPort( )), 6, 0)) : "")+context.GetScriptPath( )+"getcustomerstest.aspx");
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.WriteEndElement();
               GXSoapXMLWriter.Close();
               return  ;
            }
            else
            {
               currSoapErr = (short)(-20000);
               currSoapErrmsg = "No SOAP request found. Call " + "http://" + context.GetServerName( ) + ((context.GetServerPort( )>0)&&(context.GetServerPort( )!=80)&&(context.GetServerPort( )!=443) ? ":"+StringUtil.LTrim( StringUtil.Str( (decimal)(context.GetServerPort( )), 6, 0)) : "") + context.GetScriptPath( ) + "getcustomerstest.aspx" + "?wsdl to get the WSDL.";
            }
         }
         if ( currSoapErr == 0 )
         {
            GXSoapXMLReader.OpenRequest(GXSoapHTTPRequest);
            GXSoapXMLReader.ReadExternalEntities = 0;
            GXSoapXMLReader.IgnoreComments = 1;
            GXSoapError = GXSoapXMLReader.Read();
            while ( GXSoapError > 0 )
            {
               if ( StringUtil.StringSearch( GXSoapXMLReader.Name, "Envelope", 1) > 0 )
               {
                  this.SetPrefixesFromReader( GXSoapXMLReader);
               }
               if ( StringUtil.StringSearch( GXSoapXMLReader.Name, "Body", 1) > 0 )
               {
                  this.SetPrefixesFromReader( GXSoapXMLReader);
                  if (true) break;
               }
               GXSoapError = GXSoapXMLReader.Read();
            }
            if ( GXSoapError > 0 )
            {
               GXSoapError = GXSoapXMLReader.Read();
               if ( GXSoapError > 0 )
               {
                  this.SetPrefixesFromReader( GXSoapXMLReader);
                  currMethod = GXSoapXMLReader.Name;
                  if ( ( StringUtil.StringSearch( currMethod+"&", "Execute&", 1) > 0 ) || ( currSoapErr != 0 ) )
                  {
                     if ( currSoapErr == 0 )
                     {
                        AV10CustomerSdtList = new GXBaseCollection<SdtCustomerSdtSimple>( context, "CustomerSdtSimple", "TestRestProcs");
                     }
                  }
                  else
                  {
                     currSoapErr = (short)(-20002);
                     currSoapErrmsg = "Wrong method called. Expected method: " + "Execute";
                  }
               }
            }
            GXSoapXMLReader.Close();
         }
         if ( currSoapErr == 0 )
         {
            if ( GXSoapError < 0 )
            {
               currSoapErr = (short)(GXSoapError*-1);
               currSoapErrmsg = context.sSOAPErrMsg;
            }
            else
            {
               if ( GXSoapXMLReader.ErrCode > 0 )
               {
                  currSoapErr = (short)(GXSoapXMLReader.ErrCode*-1);
                  currSoapErrmsg = GXSoapXMLReader.ErrDescription;
               }
               else
               {
                  if ( GXSoapError == 0 )
                  {
                     currSoapErr = (short)(-20001);
                     currSoapErrmsg = "Malformed SOAP message.";
                  }
                  else
                  {
                     currSoapErr = 0;
                     currSoapErrmsg = "No error.";
                  }
               }
            }
         }
         if ( currSoapErr == 0 )
         {
            ExecutePrivate();
         }
         context.CloseConnections();
         GXSoapXMLWriter.OpenResponse(GXSoapHTTPResponse);
         GXSoapXMLWriter.WriteStartDocument("utf-8", 0);
         GXSoapXMLWriter.WriteStartElement("SOAP-ENV:Envelope");
         GXSoapXMLWriter.WriteAttribute("xmlns:SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");
         GXSoapXMLWriter.WriteAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
         GXSoapXMLWriter.WriteAttribute("xmlns:SOAP-ENC", "http://schemas.xmlsoap.org/soap/encoding/");
         GXSoapXMLWriter.WriteAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
         if ( ( StringUtil.StringSearch( currMethod+"&", "Execute&", 1) > 0 ) || ( currSoapErr != 0 ) )
         {
            GXSoapXMLWriter.WriteStartElement("SOAP-ENV:Body");
            GXSoapXMLWriter.WriteStartElement("GetCustomersTest.ExecuteResponse");
            GXSoapXMLWriter.WriteAttribute("xmlns", "TestRestProcs");
            if ( currSoapErr == 0 )
            {
               if ( AV10CustomerSdtList != null )
               {
                  AV10CustomerSdtList.writexmlcollection(GXSoapXMLWriter, "Customersdtlist", "TestRestProcs", "CustomerSdtSimple", "TestRestProcs");
               }
            }
            else
            {
               GXSoapXMLWriter.WriteStartElement("SOAP-ENV:Fault");
               GXSoapXMLWriter.WriteElement("faultcode", "SOAP-ENV:Client");
               GXSoapXMLWriter.WriteElement("faultstring", currSoapErrmsg);
               GXSoapXMLWriter.WriteElement("detail", StringUtil.Trim( StringUtil.Str( (decimal)(currSoapErr), 10, 0)));
               GXSoapXMLWriter.WriteEndElement();
            }
            GXSoapXMLWriter.WriteEndElement();
            GXSoapXMLWriter.WriteEndElement();
         }
         GXSoapXMLWriter.WriteEndElement();
         GXSoapXMLWriter.Close();
         cleanup();
      }

      public getsdtcollection( )
      {
         context = new GxContext(  );
         DataStoreUtil.LoadDataStores( context);
         IsMain = true;
         context.SetDefaultTheme("GeneXusXEv2", false);
      }

      public getsdtcollection( IGxContext context )
      {
         this.context = context;
         IsMain = false;
      }

      public void execute( out GXBaseCollection<SdtCustomerSdtSimple> aP0_CustomerSdtList )
      {
         this.AV10CustomerSdtList = new GXBaseCollection<SdtCustomerSdtSimple>( context, "CustomerSdtSimple", "TestRestProcs") ;
         initialize();
         ExecuteImpl();
         aP0_CustomerSdtList=this.AV10CustomerSdtList;
      }

      public GXBaseCollection<SdtCustomerSdtSimple> executeUdp( )
      {
         execute(out aP0_CustomerSdtList);
         return AV10CustomerSdtList ;
      }

      public void executeSubmit( out GXBaseCollection<SdtCustomerSdtSimple> aP0_CustomerSdtList )
      {
         this.AV10CustomerSdtList = new GXBaseCollection<SdtCustomerSdtSimple>( context, "CustomerSdtSimple", "TestRestProcs") ;
         SubmitImpl();
         aP0_CustomerSdtList=this.AV10CustomerSdtList;
      }

      protected override void ExecutePrivate( )
      {
         /* GeneXus formulas */
         /* Output device settings */
         AV9CustomerSdt.gxTpr_Customerid = 1;
         AV9CustomerSdt.gxTpr_Customername = "CustomerName";
         AV9CustomerSdt.gxTpr_Customerbirthdate = DateTimeUtil.Today( context);
         GXt_dtime1 = DateTimeUtil.ResetTime( DateTimeUtil.Today( context) ) ;
         AV9CustomerSdt.gxTpr_Customerpaydate = GXt_dtime1;
         AV10CustomerSdtList.Add(AV9CustomerSdt, 0);
         AV9CustomerSdt = new SdtCustomerSdtSimple(context);
         if ( context.WillRedirect( ) )
         {
            context.Redirect( context.wjLoc );
            context.wjLoc = "";
         }
         cleanup();
      }

      public override void cleanup( )
      {
         CloseCursors();
         base.cleanup();
         if ( IsMain )
         {
            context.CloseConnections();
         }
         ExitApp();
      }

      public override void initialize( )
      {
         GXSoapHTTPRequest = new GxSoapRequest(context) ;
         GXSoapXMLReader = new GXXMLReader(context.GetPhysicalPath());
         GXSoapHTTPResponse = new GxHttpResponse(context) ;
         GXSoapXMLWriter = new GXXMLWriter(context.GetPhysicalPath());
         currSoapErrmsg = "";
         currMethod = "";
         AV9CustomerSdt = new SdtCustomerSdtSimple(context);
         GXt_dtime1 = (DateTime)(DateTime.MinValue);
         /* GeneXus formulas. */
      }

      private short GXSoapError ;
      private short currSoapErr ;
      private string currSoapErrmsg ;
      private string currMethod ;
      private DateTime GXt_dtime1 ;
      private GXXMLReader GXSoapXMLReader ;
      private GXXMLWriter GXSoapXMLWriter ;
      private GxSoapRequest GXSoapHTTPRequest ;
      private GxHttpResponse GXSoapHTTPResponse ;
      private GXBaseCollection<SdtCustomerSdtSimple> AV10CustomerSdtList ;
      private SdtCustomerSdtSimple AV9CustomerSdt ;
      private GXBaseCollection<SdtCustomerSdtSimple> aP0_CustomerSdtList ;
   }

}
