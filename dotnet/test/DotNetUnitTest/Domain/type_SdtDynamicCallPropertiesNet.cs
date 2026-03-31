/*
               File: genexus.common.type_SdtDynamicCallPropertiesNet
        Description: DynamicCallPropertiesNet
             Author: GeneXus .NET Generator version 18_0_11-185337
       Generated on: 11/28/2024 9:54:49.49
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
   public class SdtDynamicCallPropertiesNet : GxUserType, IGxExternalObject
   {
      public SdtDynamicCallPropertiesNet( )
      {
         /* Constructor for serialization */
      }

      public SdtDynamicCallPropertiesNet( IGxContext context )
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

      public string gxTpr_Externalname
      {
         get {
            if ( GeneXus_Common_DynamicCallPropertiesNet_externalReference == null )
            {
               GeneXus_Common_DynamicCallPropertiesNet_externalReference = new GeneXus.DynamicCall.GxDynCallProperties();
            }
            return GeneXus_Common_DynamicCallPropertiesNet_externalReference.ExternalName ;
         }

         set {
            if ( GeneXus_Common_DynamicCallPropertiesNet_externalReference == null )
            {
               GeneXus_Common_DynamicCallPropertiesNet_externalReference = new GeneXus.DynamicCall.GxDynCallProperties();
            }
            GeneXus_Common_DynamicCallPropertiesNet_externalReference.ExternalName = value;
            SetDirty("Externalname");
         }

      }

      public string gxTpr_Assemblyname
      {
         get {
            if ( GeneXus_Common_DynamicCallPropertiesNet_externalReference == null )
            {
               GeneXus_Common_DynamicCallPropertiesNet_externalReference = new GeneXus.DynamicCall.GxDynCallProperties();
            }
            return GeneXus_Common_DynamicCallPropertiesNet_externalReference.AssemblyName ;
         }

         set {
            if ( GeneXus_Common_DynamicCallPropertiesNet_externalReference == null )
            {
               GeneXus_Common_DynamicCallPropertiesNet_externalReference = new GeneXus.DynamicCall.GxDynCallProperties();
            }
            GeneXus_Common_DynamicCallPropertiesNet_externalReference.AssemblyName = value;
            SetDirty("Assemblyname");
         }

      }

      public Object ExternalInstance
      {
         get {
            if ( GeneXus_Common_DynamicCallPropertiesNet_externalReference == null )
            {
               GeneXus_Common_DynamicCallPropertiesNet_externalReference = new GeneXus.DynamicCall.GxDynCallProperties();
            }
            return GeneXus_Common_DynamicCallPropertiesNet_externalReference ;
         }

         set {
            GeneXus_Common_DynamicCallPropertiesNet_externalReference = (GeneXus.DynamicCall.GxDynCallProperties)(value);
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

      protected GeneXus.DynamicCall.GxDynCallProperties GeneXus_Common_DynamicCallPropertiesNet_externalReference=null ;
   }

}
