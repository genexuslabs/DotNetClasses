/*
               File: genexus.common.type_SdtDynamicCallMethodProperties
        Description: DynamicCallMethodProperties
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
   public class SdtDynamicCallMethodProperties : GxUserType, IGxExternalObject
   {
      public SdtDynamicCallMethodProperties( )
      {
         /* Constructor for serialization */
      }

      public SdtDynamicCallMethodProperties( IGxContext context )
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

      public bool gxTpr_Isstatic
      {
         get {
            if ( GeneXus_Common_DynamicCallMethodProperties_externalReference == null )
            {
               GeneXus_Common_DynamicCallMethodProperties_externalReference = new GeneXus.DynamicCall.GxDynCallMethodConf();
            }
            return GeneXus_Common_DynamicCallMethodProperties_externalReference.IsStatic ;
         }

         set {
            if ( GeneXus_Common_DynamicCallMethodProperties_externalReference == null )
            {
               GeneXus_Common_DynamicCallMethodProperties_externalReference = new GeneXus.DynamicCall.GxDynCallMethodConf();
            }
            GeneXus_Common_DynamicCallMethodProperties_externalReference.IsStatic = value;
            SetDirty("Isstatic");
         }

      }

      public string gxTpr_Methodname
      {
         get {
            if ( GeneXus_Common_DynamicCallMethodProperties_externalReference == null )
            {
               GeneXus_Common_DynamicCallMethodProperties_externalReference = new GeneXus.DynamicCall.GxDynCallMethodConf();
            }
            return GeneXus_Common_DynamicCallMethodProperties_externalReference.MethodName ;
         }

         set {
            if ( GeneXus_Common_DynamicCallMethodProperties_externalReference == null )
            {
               GeneXus_Common_DynamicCallMethodProperties_externalReference = new GeneXus.DynamicCall.GxDynCallMethodConf();
            }
            GeneXus_Common_DynamicCallMethodProperties_externalReference.MethodName = value;
            SetDirty("Methodname");
         }

      }

      public Object ExternalInstance
      {
         get {
            if ( GeneXus_Common_DynamicCallMethodProperties_externalReference == null )
            {
               GeneXus_Common_DynamicCallMethodProperties_externalReference = new GeneXus.DynamicCall.GxDynCallMethodConf();
            }
            return GeneXus_Common_DynamicCallMethodProperties_externalReference ;
         }

         set {
            GeneXus_Common_DynamicCallMethodProperties_externalReference = (GeneXus.DynamicCall.GxDynCallMethodConf)(value);
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

      protected GeneXus.DynamicCall.GxDynCallMethodConf GeneXus_Common_DynamicCallMethodProperties_externalReference=null ;
   }

}
