/*
				   File: type_SdtItem
			Description: Item
				 Author: Nemo for C# version 15.0.12.124546
		   Generated on: 31/7/2018 12:06:48
		   Program type: Callable routine
			  Main DBMS: SQL Server
*/
using System;
using System.Collections;
using GeneXus.Utils;
using GeneXus.Resources;
using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Cryptography;
using GeneXus.Encryption;
using GeneXus.Http.Client;
using GeneXus.Http.Server;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.ServiceModel;



namespace GeneXus.Programs{

	[Serializable]
	public class SdtItem : GxUserType
	{
		public SdtItem( )
		{
			/* Constructor for serialization */
			gxTv_SdtItem_Name = "";

		}

		public SdtItem(IGxContext context)
		{
			this.context = context;
			initialize();
		}

		#region Json
		private static Hashtable mapper;
		public override String JsonMap(String value)
		{
			if (mapper == null)
			{
				mapper = new Hashtable();
			}
			return (String)mapper[value]; ;
		}

		public override void ToJSON()
		{
			ToJSON(true) ;
			return;
		}

		public override void ToJSON(bool includeState)
		{
			AddObjectProperty("Name", gxTpr_Name, false);
			return;
		}
		#endregion

		#region Properties

#pragma warning disable CA1707 // Identifiers should not contain underscores
		public String gxTpr_Name
#pragma warning restore CA1707 // Identifiers should not contain underscores
		{
			get { 
				return gxTv_SdtItem_Name; 
			}
			set { 
				gxTv_SdtItem_Name = value;
				SetDirty("Name");
			}
		}


		#endregion

		#region Initialization

		public void initialize( )
		{
			gxTv_SdtItem_Name = "";
			return  ;
		}



		#endregion

		#region Declaration

		protected String gxTv_SdtItem_Name;



		#endregion
	}
	
}