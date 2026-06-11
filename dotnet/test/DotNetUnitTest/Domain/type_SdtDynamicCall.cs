/*
               File: genexus.common.type_SdtDynamicCall
        Description: DynamicCall
             Author: GeneXus .NET Generator version 18_0_11-185337
       Generated on: 11/28/2024 9:54:49.46
       Program type: Callable routine
          Main DBMS: SQL Server
*/
using System;
using System.Collections;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Utils;
namespace GeneXus.Core.genexus.common
{
	[Serializable]
   public class SdtDynamicCall : GxUserType, IGxExternalObject
   {
      public SdtDynamicCall( )
      {
         /* Constructor for serialization */
      }

      public SdtDynamicCall( IGxContext context )
      {
         this.context = context;
         initialize();
      }

      private static Hashtable mapper;
      public override string JsonMap( string value )
      {
         if ( mapper == null )
         {
            mapper = new Hashtable();
         }
         return (string)mapper[value]; ;
      }

      public void execute( ref GxSimpleCollection<object> gxTp_Parameters ,
                           out GXBaseCollection<GeneXus.Utils.SdtMessages_Message> gxTp_Errors )
      {
         gxTp_Errors = new GXBaseCollection<GeneXus.Utils.SdtMessages_Message>( context, "Message", "GeneXus");
         if ( GeneXus_Common_DynamicCall_externalReference == null )
         {
            GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
         }
         System.Collections.Generic.IList<object> externalParm0;
         System.Collections.Generic.IList<GeneXus.Utils.SdtMessages_Message> externalParm1;
         externalParm0 = (System.Collections.Generic.IList<object>)CollectionUtils.ConvertToExternal( typeof(System.Collections.Generic.IList<object>), gxTp_Parameters.ExternalInstance);
         GeneXus_Common_DynamicCall_externalReference.Execute(ref externalParm0, out externalParm1);
         gxTp_Parameters.ExternalInstance = (IList)CollectionUtils.ConvertToInternal( typeof(System.Collections.Generic.IList<object>), externalParm0);
         gxTp_Errors.ExternalInstance = (IList)CollectionUtils.ConvertToInternal( typeof(System.Collections.Generic.IList<GeneXus.Utils.SdtMessages_Message>), externalParm1);
         return  ;
      }

      public object execute( ref GxSimpleCollection<object> gxTp_Parameters ,
                             GeneXus.Core.genexus.common.SdtDynamicCallMethodProperties gxTp_MethodInfo ,
                             out GXBaseCollection<GeneXus.Utils.SdtMessages_Message> gxTp_Errors )
      {
         object returnexecute;
         gxTp_Errors = new GXBaseCollection<GeneXus.Utils.SdtMessages_Message>( context, "Message", "GeneXus");
         if ( GeneXus_Common_DynamicCall_externalReference == null )
         {
            GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
         }
         object externalParm0;
         System.Collections.Generic.IList<object> externalParm1;
         GeneXus.DynamicCall.GxDynCallMethodConf externalParm2;
         System.Collections.Generic.IList<GeneXus.Utils.SdtMessages_Message> externalParm3;
         externalParm1 = (System.Collections.Generic.IList<object>)CollectionUtils.ConvertToExternal( typeof(System.Collections.Generic.IList<object>), gxTp_Parameters.ExternalInstance);
         externalParm2 = (GeneXus.DynamicCall.GxDynCallMethodConf)(gxTp_MethodInfo.ExternalInstance);
         externalParm0 = GeneXus_Common_DynamicCall_externalReference.Execute(ref externalParm1, externalParm2, out externalParm3);
         returnexecute = (object)(externalParm0);
         gxTp_Parameters.ExternalInstance = (IList)CollectionUtils.ConvertToInternal( typeof(System.Collections.Generic.IList<object>), externalParm1);
         gxTp_Errors.ExternalInstance = (IList)CollectionUtils.ConvertToInternal( typeof(System.Collections.Generic.IList<GeneXus.Utils.SdtMessages_Message>), externalParm3);
         return returnexecute ;
      }

      public void create( GxSimpleCollection<object> gxTp_Parameters ,
                          out GXBaseCollection<GeneXus.Utils.SdtMessages_Message> gxTp_Errors )
      {
         gxTp_Errors = new GXBaseCollection<GeneXus.Utils.SdtMessages_Message>( context, "Message", "GeneXus");
         if ( GeneXus_Common_DynamicCall_externalReference == null )
         {
            GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
         }
         System.Collections.Generic.IList<object> externalParm0;
         System.Collections.Generic.IList<GeneXus.Utils.SdtMessages_Message> externalParm1;
         externalParm0 = (System.Collections.Generic.IList<object>)CollectionUtils.ConvertToExternal( typeof(System.Collections.Generic.IList<object>), gxTp_Parameters.ExternalInstance);
         GeneXus_Common_DynamicCall_externalReference.Create(externalParm0, out externalParm1);
         gxTp_Errors.ExternalInstance = (IList)CollectionUtils.ConvertToInternal( typeof(System.Collections.Generic.IList<GeneXus.Utils.SdtMessages_Message>), externalParm1);
         return  ;
      }

      public void setoption( ref string gxTp_ObjectName ,
                             ref string gxTp_CallOption ,
                             ref string gxTp_Value )
      {
         if ( GeneXus_Common_DynamicCall_externalReference == null )
         {
            GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
         }
         return  ;
      }

      public string gxTpr_Objectname
      {
         get {
            if ( GeneXus_Common_DynamicCall_externalReference == null )
            {
               GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
            }
            return GeneXus_Common_DynamicCall_externalReference.ObjectName ;
         }

         set {
            if ( GeneXus_Common_DynamicCall_externalReference == null )
            {
               GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
            }
            GeneXus_Common_DynamicCall_externalReference.ObjectName = value;
            SetDirty("Objectname");
         }

      }

      public GeneXus.Core.genexus.common.SdtDynamicCallPropertiesNet gxTpr_Net
      {
         get {
            if ( GeneXus_Common_DynamicCall_externalReference == null )
            {
               GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
            }
            GeneXus.Core.genexus.common.SdtDynamicCallPropertiesNet intValue;
            intValue = new GeneXus.Core.genexus.common.SdtDynamicCallPropertiesNet(context);
            GeneXus.DynamicCall.GxDynCallProperties externalParm0;
            externalParm0 = GeneXus_Common_DynamicCall_externalReference.Properties;
            intValue.ExternalInstance = externalParm0;
            return intValue ;
         }

         set {
            if ( GeneXus_Common_DynamicCall_externalReference == null )
            {
               GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
            }
            GeneXus.Core.genexus.common.SdtDynamicCallPropertiesNet intValue;
            GeneXus.DynamicCall.GxDynCallProperties externalParm1;
            intValue = value;
            externalParm1 = (GeneXus.DynamicCall.GxDynCallProperties)(intValue.ExternalInstance);
            GeneXus_Common_DynamicCall_externalReference.Properties = externalParm1;
            SetDirty("Net");
         }

      }

      public Object ExternalInstance
      {
         get {
            if ( GeneXus_Common_DynamicCall_externalReference == null )
            {
               GeneXus_Common_DynamicCall_externalReference = new GeneXus.DynamicCall.GxDynamicCall();
            }
            return GeneXus_Common_DynamicCall_externalReference ;
         }

         set {
            GeneXus_Common_DynamicCall_externalReference = (GeneXus.DynamicCall.GxDynamicCall)(value);
         }

      }

      [XmlIgnore]
      private static GXTypeInfo _typeProps;
      protected override GXTypeInfo TypeInfo
      {
         get {
            return _typeProps ;
         }

         set {
            _typeProps = value ;
         }

      }

      public void initialize( )
      {
         return  ;
      }

      protected GeneXus.DynamicCall.GxDynamicCall GeneXus_Common_DynamicCall_externalReference=null ;
   }

}
