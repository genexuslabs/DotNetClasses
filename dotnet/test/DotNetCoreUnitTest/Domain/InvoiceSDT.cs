using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Utils;

namespace DotNetCoreUnitTest.Domain
{
	[XmlSerializerFormat]
	[XmlRoot(ElementName = "Invoice")]
	[XmlType(TypeName = "Invoice", Namespace = "TestRESTKB1")]
	[Serializable]
	public class SdtInvoice : GxSilentTrnSdt, System.Web.SessionState.IRequiresSessionState
	{
		public SdtInvoice()
		{
		}

		public SdtInvoice(IGxContext context)
		{
			this.context = context;
			constructorCallingAssembly = Assembly.GetCallingAssembly();
			initialize();
		}

		private static Hashtable mapper;
		public override string JsonMap(string value)
		{
			if (mapper == null)
			{
				mapper = new Hashtable();
			}
			return (string)mapper[value]; ;
		}

		public void Load(short AV4InvoiceId)
		{
			IGxSilentTrn obj;
			obj = getTransaction();
			obj.LoadKey(new Object[] { (short)AV4InvoiceId });
			return;
		}

		public void LoadStrParms(string sAV4InvoiceId)
		{
			short AV4InvoiceId;
			AV4InvoiceId = (short)(Math.Round(NumberUtil.Val(sAV4InvoiceId, "."), 18, MidpointRounding.ToEven));
			Load(AV4InvoiceId);
			return;
		}

		public override Object[][] GetBCKey()
		{
			return (Object[][])(new Object[][] { new Object[] { "InvoiceId", typeof(short) } });
		}

		public override GXProperties GetMetadata()
		{
			GXProperties metadata = new GXProperties();
			metadata.Set("Name", "Invoice");
			metadata.Set("BT", "Invoice");
			metadata.Set("PK", "[ \"InvoiceId\" ]");
			metadata.Set("Levels", "[ \"InvoiceLine\" ]");
			metadata.Set("Serial", "[ [ \"Same\",\"Invoice\",\"InvoiceLatestLine\",\"InvoiceLineId\",\"InvoiceId\",\"InvoiceId\" ] ]");
			metadata.Set("FKList", "[ { \"FK\":[ \"ClientId\" ],\"FKMap\":[  ] } ]");
			metadata.Set("AllowInsert", "True");
			metadata.Set("AllowUpdate", "True");
			metadata.Set("AllowDelete", "True");
			return metadata;
		}

		public override GeneXus.Utils.GxStringCollection StateAttributes()
		{
			GeneXus.Utils.GxStringCollection state = new GeneXus.Utils.GxStringCollection();
			state.Add("gxTpr_Mode");
			state.Add("gxTpr_Initialized");
			state.Add("gxTpr_Invoiceid_Z");
			state.Add("gxTpr_Invoicedate_Z_Nullable");
			state.Add("gxTpr_Invoicedescription_Z");
			state.Add("gxTpr_Clientid_Z");
			state.Add("gxTpr_Clientfirstname_Z");
			state.Add("gxTpr_Clientbalance_Z");
			state.Add("gxTpr_Clientaddress_Z");
			state.Add("gxTpr_Invoicelatestline_Z");
			state.Add("gxTpr_Invoicesubtotal_Z");
			state.Add("gxTpr_Invoicetaxes_Z");
			state.Add("gxTpr_Invoicetotal_Z");
			state.Add("gxTpr_Invoicesubtotal_N");
			return state;
		}



		public override void ToJSON()
		{
			ToJSON(true);
			return;
		}

		public override void ToJSON(bool includeState)
		{
			ToJSON(includeState, true);
			return;
		}

		public override void ToJSON(bool includeState,
									 bool includeNonInitialized)
		{
			AddObjectProperty("InvoiceId", gxTv_SdtInvoice_Invoiceid, false, includeNonInitialized);
			sDateCnv = "";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Year(gxTv_SdtInvoice_Invoicedate)), 10, 0));
			sDateCnv += StringUtil.Substring("0000", 1, 4 - StringUtil.Len(sNumToPad)) + sNumToPad;
			sDateCnv += "-";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Month(gxTv_SdtInvoice_Invoicedate)), 10, 0));
			sDateCnv += StringUtil.Substring("00", 1, 2 - StringUtil.Len(sNumToPad)) + sNumToPad;
			sDateCnv += "-";
			sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Day(gxTv_SdtInvoice_Invoicedate)), 10, 0));
			sDateCnv += StringUtil.Substring("00", 1, 2 - StringUtil.Len(sNumToPad)) + sNumToPad;
			AddObjectProperty("InvoiceDate", sDateCnv, false, includeNonInitialized);
			AddObjectProperty("InvoiceDescription", gxTv_SdtInvoice_Invoicedescription, false, includeNonInitialized);
			AddObjectProperty("ClientId", gxTv_SdtInvoice_Clientid, false, includeNonInitialized);
			AddObjectProperty("ClientFirstName", gxTv_SdtInvoice_Clientfirstname, false, includeNonInitialized);
			AddObjectProperty("ClientBalance", gxTv_SdtInvoice_Clientbalance, false, includeNonInitialized);
			AddObjectProperty("ClientAddress", gxTv_SdtInvoice_Clientaddress, false, includeNonInitialized);
			AddObjectProperty("InvoiceLatestLine", gxTv_SdtInvoice_Invoicelatestline, false, includeNonInitialized);
			if (gxTv_SdtInvoice_Line != null)
			{
				AddObjectProperty("Line", gxTv_SdtInvoice_Line, includeState, includeNonInitialized);
			}
			AddObjectProperty("InvoiceSubTotal", gxTv_SdtInvoice_Invoicesubtotal, false, includeNonInitialized);
			AddObjectProperty("InvoiceSubTotal_N", gxTv_SdtInvoice_Invoicesubtotal_N, false, includeNonInitialized);
			AddObjectProperty("InvoiceTaxes", gxTv_SdtInvoice_Invoicetaxes, false, includeNonInitialized);
			AddObjectProperty("InvoiceTotal", gxTv_SdtInvoice_Invoicetotal, false, includeNonInitialized);
			if (includeState)
			{
				AddObjectProperty("Mode", gxTv_SdtInvoice_Mode, false, includeNonInitialized);
				AddObjectProperty("Initialized", gxTv_SdtInvoice_Initialized, false, includeNonInitialized);
				AddObjectProperty("InvoiceId_Z", gxTv_SdtInvoice_Invoiceid_Z, false, includeNonInitialized);
				sDateCnv = "";
				sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Year(gxTv_SdtInvoice_Invoicedate_Z)), 10, 0));
				sDateCnv += StringUtil.Substring("0000", 1, 4 - StringUtil.Len(sNumToPad)) + sNumToPad;
				sDateCnv += "-";
				sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Month(gxTv_SdtInvoice_Invoicedate_Z)), 10, 0));
				sDateCnv += StringUtil.Substring("00", 1, 2 - StringUtil.Len(sNumToPad)) + sNumToPad;
				sDateCnv += "-";
				sNumToPad = StringUtil.Trim(StringUtil.Str((decimal)(DateTimeUtil.Day(gxTv_SdtInvoice_Invoicedate_Z)), 10, 0));
				sDateCnv += StringUtil.Substring("00", 1, 2 - StringUtil.Len(sNumToPad)) + sNumToPad;
				AddObjectProperty("InvoiceDate_Z", sDateCnv, false, includeNonInitialized);
				AddObjectProperty("InvoiceDescription_Z", gxTv_SdtInvoice_Invoicedescription_Z, false, includeNonInitialized);
				AddObjectProperty("ClientId_Z", gxTv_SdtInvoice_Clientid_Z, false, includeNonInitialized);
				AddObjectProperty("ClientFirstName_Z", gxTv_SdtInvoice_Clientfirstname_Z, false, includeNonInitialized);
				AddObjectProperty("ClientBalance_Z", gxTv_SdtInvoice_Clientbalance_Z, false, includeNonInitialized);
				AddObjectProperty("ClientAddress_Z", gxTv_SdtInvoice_Clientaddress_Z, false, includeNonInitialized);
				AddObjectProperty("InvoiceLatestLine_Z", gxTv_SdtInvoice_Invoicelatestline_Z, false, includeNonInitialized);
				AddObjectProperty("InvoiceSubTotal_Z", gxTv_SdtInvoice_Invoicesubtotal_Z, false, includeNonInitialized);
				AddObjectProperty("InvoiceTaxes_Z", gxTv_SdtInvoice_Invoicetaxes_Z, false, includeNonInitialized);
				AddObjectProperty("InvoiceTotal_Z", gxTv_SdtInvoice_Invoicetotal_Z, false, includeNonInitialized);
				AddObjectProperty("InvoiceSubTotal_N", gxTv_SdtInvoice_Invoicesubtotal_N, false, includeNonInitialized);
			}
			return;
		}

		public void UpdateDirties(SdtInvoice sdt)
		{
			if (sdt.IsDirty("InvoiceId"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoiceid = sdt.gxTv_SdtInvoice_Invoiceid;
			}
			if (sdt.IsDirty("InvoiceDate"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicedate = sdt.gxTv_SdtInvoice_Invoicedate;
			}
			if (sdt.IsDirty("InvoiceDescription"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicedescription = sdt.gxTv_SdtInvoice_Invoicedescription;
			}
			if (sdt.IsDirty("ClientId"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientid = sdt.gxTv_SdtInvoice_Clientid;
			}
			if (sdt.IsDirty("ClientFirstName"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientfirstname = sdt.gxTv_SdtInvoice_Clientfirstname;
			}
			if (sdt.IsDirty("ClientBalance"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientbalance = sdt.gxTv_SdtInvoice_Clientbalance;
			}
			if (sdt.IsDirty("ClientAddress"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientaddress = sdt.gxTv_SdtInvoice_Clientaddress;
			}
			if (sdt.IsDirty("InvoiceLatestLine"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicelatestline = sdt.gxTv_SdtInvoice_Invoicelatestline;
			}
			if (gxTv_SdtInvoice_Line != null)
			{
				GXBCLevelCollection<SdtInvoice_InvoiceLine> newCollectionLine = sdt.gxTpr_Line;
				SdtInvoice_InvoiceLine currItemLine;
				SdtInvoice_InvoiceLine newItemLine;
				short idx = 1;
				while (idx <= newCollectionLine.Count)
				{
					newItemLine = ((SdtInvoice_InvoiceLine)newCollectionLine.Item(idx));
					currItemLine = gxTv_SdtInvoice_Line.GetByKey(newItemLine.gxTpr_Invoicelineid);
					if (StringUtil.StrCmp(currItemLine.gxTpr_Mode, "UPD") == 0)
					{
						currItemLine.UpdateDirties(newItemLine);
						if (StringUtil.StrCmp(newItemLine.gxTpr_Mode, "DLT") == 0)
						{
							currItemLine.gxTpr_Mode = "DLT";
						}
						currItemLine.gxTpr_Modified = 1;
					}
					else
					{
						gxTv_SdtInvoice_Line.Add(newItemLine, 0);
					}
					idx = (short)(idx + 1);
				}
			}
			if (sdt.IsDirty("InvoiceSubTotal"))
			{
				gxTv_SdtInvoice_Invoicesubtotal_N = (short)(sdt.gxTv_SdtInvoice_Invoicesubtotal_N);
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicesubtotal = sdt.gxTv_SdtInvoice_Invoicesubtotal;
			}
			if (sdt.IsDirty("InvoiceTaxes"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicetaxes = sdt.gxTv_SdtInvoice_Invoicetaxes;
			}
			if (sdt.IsDirty("InvoiceTotal"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicetotal = sdt.gxTv_SdtInvoice_Invoicetotal;
			}
			return;
		}

		[SoapElement(ElementName = "InvoiceId")]
		[XmlElement(ElementName = "InvoiceId")]
		public short gxTpr_Invoiceid
		{
			get
			{
				return gxTv_SdtInvoice_Invoiceid;
			}

			set
			{
				sdtIsNull = 0;
				if (gxTv_SdtInvoice_Invoiceid != value)
				{
					gxTv_SdtInvoice_Mode = "INS";
					this.gxTv_SdtInvoice_Invoiceid_Z_SetNull();
					this.gxTv_SdtInvoice_Invoicedate_Z_SetNull();
					this.gxTv_SdtInvoice_Invoicedescription_Z_SetNull();
					this.gxTv_SdtInvoice_Clientid_Z_SetNull();
					this.gxTv_SdtInvoice_Clientfirstname_Z_SetNull();
					this.gxTv_SdtInvoice_Clientbalance_Z_SetNull();
					this.gxTv_SdtInvoice_Clientaddress_Z_SetNull();
					this.gxTv_SdtInvoice_Invoicelatestline_Z_SetNull();
					this.gxTv_SdtInvoice_Invoicesubtotal_Z_SetNull();
					this.gxTv_SdtInvoice_Invoicetaxes_Z_SetNull();
					this.gxTv_SdtInvoice_Invoicetotal_Z_SetNull();
					if (gxTv_SdtInvoice_Line != null)
					{
						GXBCLevelCollection<SdtInvoice_InvoiceLine> collectionLine = gxTv_SdtInvoice_Line;
						SdtInvoice_InvoiceLine currItemLine;
						short idx = 1;
						while (idx <= collectionLine.Count)
						{
							currItemLine = ((SdtInvoice_InvoiceLine)collectionLine.Item(idx));
							currItemLine.gxTpr_Mode = "INS";
							currItemLine.gxTpr_Modified = 1;
							idx = (short)(idx + 1);
						}
					}
				}
				gxTv_SdtInvoice_Invoiceid = value;
				SetDirty("Invoiceid");
			}

		}

		[SoapElement(ElementName = "InvoiceDate")]
		[XmlElement(ElementName = "InvoiceDate", IsNullable = true)]
		public string gxTpr_Invoicedate_Nullable
		{
			get
			{
				if (gxTv_SdtInvoice_Invoicedate == DateTime.MinValue)
					return null;
				return new GxDateString(gxTv_SdtInvoice_Invoicedate).value;
			}

			set
			{
				sdtIsNull = 0;
				if (String.IsNullOrEmpty(value) || value == GxDateString.NullValue)
					gxTv_SdtInvoice_Invoicedate = DateTime.MinValue;
				else
					gxTv_SdtInvoice_Invoicedate = DateTime.Parse(value);
			}

		}

		[SoapIgnore]
		[XmlIgnore]
		public DateTime gxTpr_Invoicedate
		{
			get
			{
				return gxTv_SdtInvoice_Invoicedate;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicedate = value;
				SetDirty("Invoicedate");
			}

		}

		[SoapElement(ElementName = "InvoiceDescription")]
		[XmlElement(ElementName = "InvoiceDescription")]
		public string gxTpr_Invoicedescription
		{
			get
			{
				return gxTv_SdtInvoice_Invoicedescription;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicedescription = value;
				SetDirty("Invoicedescription");
			}

		}

		public void gxTv_SdtInvoice_Invoicedescription_SetNull()
		{
			gxTv_SdtInvoice_Invoicedescription = "";
			SetDirty("Invoicedescription");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicedescription_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ClientId")]
		[XmlElement(ElementName = "ClientId")]
		public short gxTpr_Clientid
		{
			get
			{
				return gxTv_SdtInvoice_Clientid;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientid = value;
				SetDirty("Clientid");
			}

		}

		[SoapElement(ElementName = "ClientFirstName")]
		[XmlElement(ElementName = "ClientFirstName")]
		public string gxTpr_Clientfirstname
		{
			get
			{
				return gxTv_SdtInvoice_Clientfirstname;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientfirstname = value;
				SetDirty("Clientfirstname");
			}

		}

		[SoapElement(ElementName = "ClientBalance")]
		[XmlElement(ElementName = "ClientBalance")]
		public decimal gxTpr_Clientbalance
		{
			get
			{
				return gxTv_SdtInvoice_Clientbalance;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientbalance = value;
				SetDirty("Clientbalance");
			}

		}

		[SoapElement(ElementName = "ClientAddress")]
		[XmlElement(ElementName = "ClientAddress")]
		public string gxTpr_Clientaddress
		{
			get
			{
				return gxTv_SdtInvoice_Clientaddress;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientaddress = value;
				SetDirty("Clientaddress");
			}

		}

		[SoapElement(ElementName = "InvoiceLatestLine")]
		[XmlElement(ElementName = "InvoiceLatestLine")]
		public short gxTpr_Invoicelatestline
		{
			get
			{
				return gxTv_SdtInvoice_Invoicelatestline;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicelatestline = value;
				SetDirty("Invoicelatestline");
			}

		}

		[SoapElement(ElementName = "Line")]
		[XmlArray(ElementName = "Line")]
		[XmlArrayItemAttribute(ElementName = "Invoice.InvoiceLine", IsNullable = false)]
		public GXBCLevelCollection<SdtInvoice_InvoiceLine> gxTpr_Line_GXBCLevelCollection
		{
			get
			{
				if (gxTv_SdtInvoice_Line == null)
				{
					gxTv_SdtInvoice_Line = new GXBCLevelCollection<SdtInvoice_InvoiceLine>(context, "Invoice.InvoiceLine", "TestRESTKB1");
				}
				return gxTv_SdtInvoice_Line;
			}

			set
			{
				if (gxTv_SdtInvoice_Line == null)
				{
					gxTv_SdtInvoice_Line = new GXBCLevelCollection<SdtInvoice_InvoiceLine>(context, "Invoice.InvoiceLine", "TestRESTKB1");
				}
				sdtIsNull = 0;
				gxTv_SdtInvoice_Line = value;
			}

		}

		[SoapIgnore]
		[XmlIgnore]
		public GXBCLevelCollection<SdtInvoice_InvoiceLine> gxTpr_Line
		{
			get
			{
				if (gxTv_SdtInvoice_Line == null)
				{
					gxTv_SdtInvoice_Line = new GXBCLevelCollection<SdtInvoice_InvoiceLine>(context, "Invoice.InvoiceLine", "TestRESTKB1");
				}
				sdtIsNull = 0;
				return gxTv_SdtInvoice_Line;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Line = value;
				SetDirty("Line");
			}

		}

		public void gxTv_SdtInvoice_Line_SetNull()
		{
			gxTv_SdtInvoice_Line = null;
			SetDirty("Line");
			return;
		}

		public bool gxTv_SdtInvoice_Line_IsNull()
		{
			if (gxTv_SdtInvoice_Line == null)
			{
				return true;
			}
			return false;
		}

		[SoapElement(ElementName = "InvoiceSubTotal")]
		[XmlElement(ElementName = "InvoiceSubTotal")]
		public decimal gxTpr_Invoicesubtotal
		{
			get
			{
				return gxTv_SdtInvoice_Invoicesubtotal;
			}

			set
			{
				gxTv_SdtInvoice_Invoicesubtotal_N = 0;
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicesubtotal = value;
				SetDirty("Invoicesubtotal");
			}

		}

		public void gxTv_SdtInvoice_Invoicesubtotal_SetNull()
		{
			gxTv_SdtInvoice_Invoicesubtotal_N = 1;
			gxTv_SdtInvoice_Invoicesubtotal = 0;
			SetDirty("Invoicesubtotal");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicesubtotal_IsNull()
		{
			return (gxTv_SdtInvoice_Invoicesubtotal_N == 1);
		}

		[SoapElement(ElementName = "InvoiceTaxes")]
		[XmlElement(ElementName = "InvoiceTaxes")]
		public decimal gxTpr_Invoicetaxes
		{
			get
			{
				return gxTv_SdtInvoice_Invoicetaxes;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicetaxes = value;
				SetDirty("Invoicetaxes");
			}

		}

		public void gxTv_SdtInvoice_Invoicetaxes_SetNull()
		{
			gxTv_SdtInvoice_Invoicetaxes = 0;
			SetDirty("Invoicetaxes");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicetaxes_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceTotal")]
		[XmlElement(ElementName = "InvoiceTotal")]
		public decimal gxTpr_Invoicetotal
		{
			get
			{
				return gxTv_SdtInvoice_Invoicetotal;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicetotal = value;
				SetDirty("Invoicetotal");
			}

		}

		public void gxTv_SdtInvoice_Invoicetotal_SetNull()
		{
			gxTv_SdtInvoice_Invoicetotal = 0;
			SetDirty("Invoicetotal");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicetotal_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "Mode")]
		[XmlElement(ElementName = "Mode")]
		public string gxTpr_Mode
		{
			get
			{
				return gxTv_SdtInvoice_Mode;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Mode = value;
				SetDirty("Mode");
			}

		}

		public void gxTv_SdtInvoice_Mode_SetNull()
		{
			gxTv_SdtInvoice_Mode = "";
			SetDirty("Mode");
			return;
		}

		public bool gxTv_SdtInvoice_Mode_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "Initialized")]
		[XmlElement(ElementName = "Initialized")]
		public short gxTpr_Initialized
		{
			get
			{
				return gxTv_SdtInvoice_Initialized;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Initialized = value;
				SetDirty("Initialized");
			}

		}

		public void gxTv_SdtInvoice_Initialized_SetNull()
		{
			gxTv_SdtInvoice_Initialized = 0;
			SetDirty("Initialized");
			return;
		}

		public bool gxTv_SdtInvoice_Initialized_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceId_Z")]
		[XmlElement(ElementName = "InvoiceId_Z")]
		public short gxTpr_Invoiceid_Z
		{
			get
			{
				return gxTv_SdtInvoice_Invoiceid_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoiceid_Z = value;
				SetDirty("Invoiceid_Z");
			}

		}

		public void gxTv_SdtInvoice_Invoiceid_Z_SetNull()
		{
			gxTv_SdtInvoice_Invoiceid_Z = 0;
			SetDirty("Invoiceid_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Invoiceid_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceDate_Z")]
		[XmlElement(ElementName = "InvoiceDate_Z", IsNullable = true)]
		public string gxTpr_Invoicedate_Z_Nullable
		{
			get
			{
				if (gxTv_SdtInvoice_Invoicedate_Z == DateTime.MinValue)
					return null;
				return new GxDateString(gxTv_SdtInvoice_Invoicedate_Z).value;
			}

			set
			{
				sdtIsNull = 0;
				if (String.IsNullOrEmpty(value) || value == GxDateString.NullValue)
					gxTv_SdtInvoice_Invoicedate_Z = DateTime.MinValue;
				else
					gxTv_SdtInvoice_Invoicedate_Z = DateTime.Parse(value);
			}

		}

		[SoapIgnore]
		[XmlIgnore]
		public DateTime gxTpr_Invoicedate_Z
		{
			get
			{
				return gxTv_SdtInvoice_Invoicedate_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicedate_Z = value;
				SetDirty("Invoicedate_Z");
			}

		}

		public void gxTv_SdtInvoice_Invoicedate_Z_SetNull()
		{
			gxTv_SdtInvoice_Invoicedate_Z = (DateTime)(DateTime.MinValue);
			SetDirty("Invoicedate_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicedate_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceDescription_Z")]
		[XmlElement(ElementName = "InvoiceDescription_Z")]
		public string gxTpr_Invoicedescription_Z
		{
			get
			{
				return gxTv_SdtInvoice_Invoicedescription_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicedescription_Z = value;
				SetDirty("Invoicedescription_Z");
			}

		}

		public void gxTv_SdtInvoice_Invoicedescription_Z_SetNull()
		{
			gxTv_SdtInvoice_Invoicedescription_Z = "";
			SetDirty("Invoicedescription_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicedescription_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ClientId_Z")]
		[XmlElement(ElementName = "ClientId_Z")]
		public short gxTpr_Clientid_Z
		{
			get
			{
				return gxTv_SdtInvoice_Clientid_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientid_Z = value;
				SetDirty("Clientid_Z");
			}

		}

		public void gxTv_SdtInvoice_Clientid_Z_SetNull()
		{
			gxTv_SdtInvoice_Clientid_Z = 0;
			SetDirty("Clientid_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Clientid_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ClientFirstName_Z")]
		[XmlElement(ElementName = "ClientFirstName_Z")]
		public string gxTpr_Clientfirstname_Z
		{
			get
			{
				return gxTv_SdtInvoice_Clientfirstname_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientfirstname_Z = value;
				SetDirty("Clientfirstname_Z");
			}

		}

		public void gxTv_SdtInvoice_Clientfirstname_Z_SetNull()
		{
			gxTv_SdtInvoice_Clientfirstname_Z = "";
			SetDirty("Clientfirstname_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Clientfirstname_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ClientBalance_Z")]
		[XmlElement(ElementName = "ClientBalance_Z")]
		public decimal gxTpr_Clientbalance_Z
		{
			get
			{
				return gxTv_SdtInvoice_Clientbalance_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientbalance_Z = value;
				SetDirty("Clientbalance_Z");
			}

		}

		public void gxTv_SdtInvoice_Clientbalance_Z_SetNull()
		{
			gxTv_SdtInvoice_Clientbalance_Z = 0;
			SetDirty("Clientbalance_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Clientbalance_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ClientAddress_Z")]
		[XmlElement(ElementName = "ClientAddress_Z")]
		public string gxTpr_Clientaddress_Z
		{
			get
			{
				return gxTv_SdtInvoice_Clientaddress_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Clientaddress_Z = value;
				SetDirty("Clientaddress_Z");
			}

		}

		public void gxTv_SdtInvoice_Clientaddress_Z_SetNull()
		{
			gxTv_SdtInvoice_Clientaddress_Z = "";
			SetDirty("Clientaddress_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Clientaddress_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceLatestLine_Z")]
		[XmlElement(ElementName = "InvoiceLatestLine_Z")]
		public short gxTpr_Invoicelatestline_Z
		{
			get
			{
				return gxTv_SdtInvoice_Invoicelatestline_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicelatestline_Z = value;
				SetDirty("Invoicelatestline_Z");
			}

		}

		public void gxTv_SdtInvoice_Invoicelatestline_Z_SetNull()
		{
			gxTv_SdtInvoice_Invoicelatestline_Z = 0;
			SetDirty("Invoicelatestline_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicelatestline_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceSubTotal_Z")]
		[XmlElement(ElementName = "InvoiceSubTotal_Z")]
		public decimal gxTpr_Invoicesubtotal_Z
		{
			get
			{
				return gxTv_SdtInvoice_Invoicesubtotal_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicesubtotal_Z = value;
				SetDirty("Invoicesubtotal_Z");
			}

		}

		public void gxTv_SdtInvoice_Invoicesubtotal_Z_SetNull()
		{
			gxTv_SdtInvoice_Invoicesubtotal_Z = 0;
			SetDirty("Invoicesubtotal_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicesubtotal_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceTaxes_Z")]
		[XmlElement(ElementName = "InvoiceTaxes_Z")]
		public decimal gxTpr_Invoicetaxes_Z
		{
			get
			{
				return gxTv_SdtInvoice_Invoicetaxes_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicetaxes_Z = value;
				SetDirty("Invoicetaxes_Z");
			}

		}

		public void gxTv_SdtInvoice_Invoicetaxes_Z_SetNull()
		{
			gxTv_SdtInvoice_Invoicetaxes_Z = 0;
			SetDirty("Invoicetaxes_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicetaxes_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceTotal_Z")]
		[XmlElement(ElementName = "InvoiceTotal_Z")]
		public decimal gxTpr_Invoicetotal_Z
		{
			get
			{
				return gxTv_SdtInvoice_Invoicetotal_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicetotal_Z = value;
				SetDirty("Invoicetotal_Z");
			}

		}

		public void gxTv_SdtInvoice_Invoicetotal_Z_SetNull()
		{
			gxTv_SdtInvoice_Invoicetotal_Z = 0;
			SetDirty("Invoicetotal_Z");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicetotal_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceSubTotal_N")]
		[XmlElement(ElementName = "InvoiceSubTotal_N")]
		public short gxTpr_Invoicesubtotal_N
		{
			get
			{
				return gxTv_SdtInvoice_Invoicesubtotal_N;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_Invoicesubtotal_N = value;
				SetDirty("Invoicesubtotal_N");
			}

		}

		public void gxTv_SdtInvoice_Invoicesubtotal_N_SetNull()
		{
			gxTv_SdtInvoice_Invoicesubtotal_N = 0;
			SetDirty("Invoicesubtotal_N");
			return;
		}

		public bool gxTv_SdtInvoice_Invoicesubtotal_N_IsNull()
		{
			return false;
		}

		public void initialize()
		{
			sdtIsNull = 1;
			gxTv_SdtInvoice_Invoiceid = 0;
			gxTv_SdtInvoice_Invoicedate = DateTime.MinValue;
			gxTv_SdtInvoice_Invoicedescription = "";
			gxTv_SdtInvoice_Clientfirstname = "";
			gxTv_SdtInvoice_Clientaddress = "";
			gxTv_SdtInvoice_Mode = "";
			gxTv_SdtInvoice_Invoicedate_Z = DateTime.MinValue;
			gxTv_SdtInvoice_Invoicedescription_Z = "";
			gxTv_SdtInvoice_Clientfirstname_Z = "";
			gxTv_SdtInvoice_Clientaddress_Z = "";
			sDateCnv = "";
			sNumToPad = "";
			IGxSilentTrn obj;
			obj = (IGxSilentTrn)ClassLoader.FindInstance("invoice", "GeneXus.Programs.invoice_bc", new Object[] { context }, constructorCallingAssembly); ;
			obj.initialize();
			obj.SetSDT(this, 1);
			setTransaction(obj);
			obj.SetMode("INS");
			return;
		}

		public short isNull()
		{
			return sdtIsNull;
		}

		private short gxTv_SdtInvoice_Invoiceid;
		private short sdtIsNull;
		private short gxTv_SdtInvoice_Clientid;
		private short gxTv_SdtInvoice_Invoicelatestline;
		private short gxTv_SdtInvoice_Initialized;
		private short gxTv_SdtInvoice_Invoiceid_Z;
		private short gxTv_SdtInvoice_Clientid_Z;
		private short gxTv_SdtInvoice_Invoicelatestline_Z;
		private short gxTv_SdtInvoice_Invoicesubtotal_N;
		private decimal gxTv_SdtInvoice_Clientbalance;
		private decimal gxTv_SdtInvoice_Invoicesubtotal;
		private decimal gxTv_SdtInvoice_Invoicetaxes;
		private decimal gxTv_SdtInvoice_Invoicetotal;
		private decimal gxTv_SdtInvoice_Clientbalance_Z;
		private decimal gxTv_SdtInvoice_Invoicesubtotal_Z;
		private decimal gxTv_SdtInvoice_Invoicetaxes_Z;
		private decimal gxTv_SdtInvoice_Invoicetotal_Z;
		private string gxTv_SdtInvoice_Invoicedescription;
		private string gxTv_SdtInvoice_Clientfirstname;
		private string gxTv_SdtInvoice_Clientaddress;
		private string gxTv_SdtInvoice_Mode;
		private string gxTv_SdtInvoice_Invoicedescription_Z;
		private string gxTv_SdtInvoice_Clientfirstname_Z;
		private string gxTv_SdtInvoice_Clientaddress_Z;
		private string sDateCnv;
		private string sNumToPad;
		private DateTime gxTv_SdtInvoice_Invoicedate;
		private DateTime gxTv_SdtInvoice_Invoicedate_Z;
		private GXBCLevelCollection<SdtInvoice_InvoiceLine> gxTv_SdtInvoice_Line = null;
	}

	[DataContract(Name = @"Invoice", Namespace = "TestRESTKB1")]
	public class SdtInvoice_RESTInterface : GxGenericCollectionItem<SdtInvoice>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtInvoice_RESTInterface() : base()
		{
		}

		public SdtInvoice_RESTInterface(SdtInvoice psdt) : base(psdt)
		{
		}

		[DataMember(Name = "InvoiceId", Order = 0)]
		[GxSeudo()]
		public Nullable<short> gxTpr_Invoiceid
		{
			get
			{
				return sdt.gxTpr_Invoiceid;
			}

			set
			{
				sdt.gxTpr_Invoiceid = (short)(value.HasValue ? value.Value : 0);
			}

		}

		[DataMember(Name = "InvoiceDate", Order = 1)]
		[GxSeudo()]
		public string gxTpr_Invoicedate
		{
			get
			{
				return DateTimeUtil.DToC2(sdt.gxTpr_Invoicedate);
			}

			set
			{
				sdt.gxTpr_Invoicedate = DateTimeUtil.CToD2(value);
			}

		}

		[DataMember(Name = "InvoiceDescription", Order = 2)]
		[GxSeudo()]
		public string gxTpr_Invoicedescription
		{
			get
			{
				return StringUtil.RTrim(sdt.gxTpr_Invoicedescription);
			}

			set
			{
				sdt.gxTpr_Invoicedescription = value;
			}

		}

		[DataMember(Name = "ClientId", Order = 3)]
		[GxSeudo()]
		public Nullable<short> gxTpr_Clientid
		{
			get
			{
				return sdt.gxTpr_Clientid;
			}

			set
			{
				sdt.gxTpr_Clientid = (short)(value.HasValue ? value.Value : 0);
			}

		}

		[DataMember(Name = "ClientFirstName", Order = 4)]
		[GxSeudo()]
		public string gxTpr_Clientfirstname
		{
			get
			{
				return StringUtil.RTrim(sdt.gxTpr_Clientfirstname);
			}

			set
			{
				sdt.gxTpr_Clientfirstname = value;
			}

		}

		[DataMember(Name = "ClientBalance", Order = 5)]
		[GxSeudo()]
		public string gxTpr_Clientbalance
		{
			get
			{
				return StringUtil.LTrim(StringUtil.Str(sdt.gxTpr_Clientbalance, 10, 2));
			}

			set
			{
				sdt.gxTpr_Clientbalance = NumberUtil.Val(value, ".");
			}

		}

		[DataMember(Name = "ClientAddress", Order = 6)]
		[GxSeudo()]
		public string gxTpr_Clientaddress
		{
			get
			{
				return StringUtil.RTrim(sdt.gxTpr_Clientaddress);
			}

			set
			{
				sdt.gxTpr_Clientaddress = value;
			}

		}

		[DataMember(Name = "InvoiceLatestLine", Order = 7)]
		[GxSeudo()]
		public Nullable<short> gxTpr_Invoicelatestline
		{
			get
			{
				return sdt.gxTpr_Invoicelatestline;
			}

			set
			{
				sdt.gxTpr_Invoicelatestline = (short)(value.HasValue ? value.Value : 0);
			}

		}

		[DataMember(Name = "Line", Order = 8)]
		public GxGenericCollection<SdtInvoice_InvoiceLine_RESTInterface> gxTpr_Line
		{
			get
			{
				return new GxGenericCollection<SdtInvoice_InvoiceLine_RESTInterface>(sdt.gxTpr_Line);
			}

			set
			{
				value.LoadCollection(sdt.gxTpr_Line);
			}

		}

		[DataMember(Name = "InvoiceSubTotal", Order = 9)]
		[GxSeudo()]
		public string gxTpr_Invoicesubtotal
		{
			get
			{
				return StringUtil.LTrim(StringUtil.Str(sdt.gxTpr_Invoicesubtotal, 10, 2));
			}

			set
			{
				sdt.gxTpr_Invoicesubtotal = NumberUtil.Val(value, ".");
			}

		}

		[DataMember(Name = "InvoiceTaxes", Order = 10)]
		[GxSeudo()]
		public string gxTpr_Invoicetaxes
		{
			get
			{
				return StringUtil.LTrim(StringUtil.Str(sdt.gxTpr_Invoicetaxes, 10, 2));
			}

			set
			{
				sdt.gxTpr_Invoicetaxes = NumberUtil.Val(value, ".");
			}

		}

		[DataMember(Name = "InvoiceTotal", Order = 11)]
		[GxSeudo()]
		public string gxTpr_Invoicetotal
		{
			get
			{
				return StringUtil.LTrim(StringUtil.Str(sdt.gxTpr_Invoicetotal, 10, 2));
			}

			set
			{
				sdt.gxTpr_Invoicetotal = NumberUtil.Val(value, ".");
			}

		}

		public SdtInvoice sdt
		{
			get
			{
				return (SdtInvoice)Sdt;
			}

			set
			{
				Sdt = value;
			}

		}

		[OnDeserializing]
		void checkSdt(StreamingContext ctx)
		{
			if (sdt == null)
			{
				sdt = new SdtInvoice();
			}
		}

		[DataMember(Name = "gx_md5_hash", Order = 12)]
		public string Hash
		{
			get
			{
				if (StringUtil.StrCmp(md5Hash, null) == 0)
				{
					md5Hash = (string)(getHash());
				}
				return md5Hash;
			}

			set
			{
				md5Hash = value;
			}

		}

		private string md5Hash;
	}

	[DataContract(Name = @"Invoice", Namespace = "TestRESTKB1")]
	public class SdtInvoice_RESTLInterface : GxGenericCollectionItem<SdtInvoice>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtInvoice_RESTLInterface() : base()
		{
		}

		public SdtInvoice_RESTLInterface(SdtInvoice psdt) : base(psdt)
		{
		}

		[DataMember(Name = "InvoiceDescription", Order = 0)]
		[GxSeudo()]
		public string gxTpr_Invoicedescription
		{
			get
			{
				return StringUtil.RTrim(sdt.gxTpr_Invoicedescription);
			}

			set
			{
				sdt.gxTpr_Invoicedescription = value;
			}

		}

		[DataMember(Name = "uri", Order = 1)]
		public static string Uri
		{
			get
			{
				return "";
			}

			set
			{
			}

		}

		public SdtInvoice sdt
		{
			get
			{
				return (SdtInvoice)Sdt;
			}

			set
			{
				Sdt = value;
			}

		}

		[OnDeserializing]
		void checkSdt(StreamingContext ctx)
		{
			if (sdt == null)
			{
				sdt = new SdtInvoice();
			}
		}

	}

	[XmlSerializerFormat]
	[XmlRoot(ElementName = "Invoice.InvoiceLine")]
	[XmlType(TypeName = "Invoice.InvoiceLine", Namespace = "TestRESTKB1")]
	[Serializable]
	public class SdtInvoice_InvoiceLine : GxSilentTrnSdt, IGxSilentTrnGridItem
	{
		public SdtInvoice_InvoiceLine()
		{
		}

		public SdtInvoice_InvoiceLine(IGxContext context)
		{
			this.context = context;
			constructorCallingAssembly = Assembly.GetCallingAssembly();
			initialize();
		}

		private static Hashtable mapper;
		public override string JsonMap(string value)
		{
			if (mapper == null)
			{
				mapper = new Hashtable();
			}
			return (string)mapper[value]; ;
		}

		public override Object[][] GetBCKey()
		{
			return (Object[][])(new Object[][] { new Object[] { "InvoiceLineId", typeof(short) } });
		}

		public override GXProperties GetMetadata()
		{
			GXProperties metadata = new GXProperties();
			metadata.Set("Name", "InvoiceLine");
			metadata.Set("BT", "InvoiceLine");
			metadata.Set("PK", "[ \"InvoiceLineId\" ]");
			metadata.Set("PKAssigned", "[ \"InvoiceLineId\" ]");
			metadata.Set("FKList", "[ { \"FK\":[ \"InvoiceId\" ],\"FKMap\":[  ] },{ \"FK\":[ \"ProductId\" ],\"FKMap\":[  ] } ]");
			metadata.Set("AllowInsert", "True");
			metadata.Set("AllowUpdate", "True");
			metadata.Set("AllowDelete", "True");
			return metadata;
		}

		public override GeneXus.Utils.GxStringCollection StateAttributes()
		{
			GeneXus.Utils.GxStringCollection state = new GeneXus.Utils.GxStringCollection();
			state.Add("gxTpr_Mode");
			state.Add("gxTpr_Modified");
			state.Add("gxTpr_Initialized");
			state.Add("gxTpr_Invoicelineid_Z");
			state.Add("gxTpr_Productid_Z");
			state.Add("gxTpr_Productname_Z");
			state.Add("gxTpr_Productstock_Z");
			state.Add("gxTpr_Productprice_Z");
			state.Add("gxTpr_Invoicelineqty_Z");
			state.Add("gxTpr_Invoicelineamount_Z");
			return state;
		}

		public override void Copy(GxUserType source)
		{
			SdtInvoice_InvoiceLine sdt;
			sdt = (SdtInvoice_InvoiceLine)(source);
			gxTv_SdtInvoice_InvoiceLine_Invoicelineid = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineid;
			gxTv_SdtInvoice_InvoiceLine_Productid = sdt.gxTv_SdtInvoice_InvoiceLine_Productid;
			gxTv_SdtInvoice_InvoiceLine_Productname = sdt.gxTv_SdtInvoice_InvoiceLine_Productname;
			gxTv_SdtInvoice_InvoiceLine_Productstock = sdt.gxTv_SdtInvoice_InvoiceLine_Productstock;
			gxTv_SdtInvoice_InvoiceLine_Productprice = sdt.gxTv_SdtInvoice_InvoiceLine_Productprice;
			gxTv_SdtInvoice_InvoiceLine_Invoicelineqty = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineqty;
			gxTv_SdtInvoice_InvoiceLine_Invoicelineamount = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineamount;
			gxTv_SdtInvoice_InvoiceLine_Mode = sdt.gxTv_SdtInvoice_InvoiceLine_Mode;
			gxTv_SdtInvoice_InvoiceLine_Modified = sdt.gxTv_SdtInvoice_InvoiceLine_Modified;
			gxTv_SdtInvoice_InvoiceLine_Initialized = sdt.gxTv_SdtInvoice_InvoiceLine_Initialized;
			gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z;
			gxTv_SdtInvoice_InvoiceLine_Productid_Z = sdt.gxTv_SdtInvoice_InvoiceLine_Productid_Z;
			gxTv_SdtInvoice_InvoiceLine_Productname_Z = sdt.gxTv_SdtInvoice_InvoiceLine_Productname_Z;
			gxTv_SdtInvoice_InvoiceLine_Productstock_Z = sdt.gxTv_SdtInvoice_InvoiceLine_Productstock_Z;
			gxTv_SdtInvoice_InvoiceLine_Productprice_Z = sdt.gxTv_SdtInvoice_InvoiceLine_Productprice_Z;
			gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z;
			gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z;
			return;
		}

		public override void ToJSON()
		{
			ToJSON(true);
			return;
		}

		public override void ToJSON(bool includeState)
		{
			ToJSON(includeState, true);
			return;
		}

		public override void ToJSON(bool includeState,
									 bool includeNonInitialized)
		{
			AddObjectProperty("InvoiceLineId", gxTv_SdtInvoice_InvoiceLine_Invoicelineid, false, includeNonInitialized);
			AddObjectProperty("ProductId", gxTv_SdtInvoice_InvoiceLine_Productid, false, includeNonInitialized);
			AddObjectProperty("ProductName", gxTv_SdtInvoice_InvoiceLine_Productname, false, includeNonInitialized);
			AddObjectProperty("ProductStock", gxTv_SdtInvoice_InvoiceLine_Productstock, false, includeNonInitialized);
			AddObjectProperty("ProductPrice", gxTv_SdtInvoice_InvoiceLine_Productprice, false, includeNonInitialized);
			AddObjectProperty("InvoiceLineQty", gxTv_SdtInvoice_InvoiceLine_Invoicelineqty, false, includeNonInitialized);
			AddObjectProperty("InvoiceLineAmount", gxTv_SdtInvoice_InvoiceLine_Invoicelineamount, false, includeNonInitialized);
			if (includeState)
			{
				AddObjectProperty("Mode", gxTv_SdtInvoice_InvoiceLine_Mode, false, includeNonInitialized);
				AddObjectProperty("Modified", gxTv_SdtInvoice_InvoiceLine_Modified, false, includeNonInitialized);
				AddObjectProperty("Initialized", gxTv_SdtInvoice_InvoiceLine_Initialized, false, includeNonInitialized);
				AddObjectProperty("InvoiceLineId_Z", gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z, false, includeNonInitialized);
				AddObjectProperty("ProductId_Z", gxTv_SdtInvoice_InvoiceLine_Productid_Z, false, includeNonInitialized);
				AddObjectProperty("ProductName_Z", gxTv_SdtInvoice_InvoiceLine_Productname_Z, false, includeNonInitialized);
				AddObjectProperty("ProductStock_Z", gxTv_SdtInvoice_InvoiceLine_Productstock_Z, false, includeNonInitialized);
				AddObjectProperty("ProductPrice_Z", gxTv_SdtInvoice_InvoiceLine_Productprice_Z, false, includeNonInitialized);
				AddObjectProperty("InvoiceLineQty_Z", gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z, false, includeNonInitialized);
				AddObjectProperty("InvoiceLineAmount_Z", gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z, false, includeNonInitialized);
			}
			return;
		}

		public void UpdateDirties(SdtInvoice_InvoiceLine sdt)
		{
			if (sdt.IsDirty("InvoiceLineId"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineid = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineid;
			}
			if (sdt.IsDirty("ProductId"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productid = sdt.gxTv_SdtInvoice_InvoiceLine_Productid;
			}
			if (sdt.IsDirty("ProductName"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productname = sdt.gxTv_SdtInvoice_InvoiceLine_Productname;
			}
			if (sdt.IsDirty("ProductStock"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productstock = sdt.gxTv_SdtInvoice_InvoiceLine_Productstock;
			}
			if (sdt.IsDirty("ProductPrice"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productprice = sdt.gxTv_SdtInvoice_InvoiceLine_Productprice;
			}
			if (sdt.IsDirty("InvoiceLineQty"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineqty = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineqty;
			}
			if (sdt.IsDirty("InvoiceLineAmount"))
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineamount = sdt.gxTv_SdtInvoice_InvoiceLine_Invoicelineamount;
			}
			return;
		}

		[SoapElement(ElementName = "InvoiceLineId")]
		[XmlElement(ElementName = "InvoiceLineId")]
		public short gxTpr_Invoicelineid
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Invoicelineid;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineid = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Invoicelineid");
			}

		}

		[SoapElement(ElementName = "ProductId")]
		[XmlElement(ElementName = "ProductId")]
		public short gxTpr_Productid
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Productid;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productid = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Productid");
			}

		}

		[SoapElement(ElementName = "ProductName")]
		[XmlElement(ElementName = "ProductName")]
		public string gxTpr_Productname
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Productname;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productname = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Productname");
			}

		}

		[SoapElement(ElementName = "ProductStock")]
		[XmlElement(ElementName = "ProductStock")]
		public short gxTpr_Productstock
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Productstock;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productstock = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Productstock");
			}

		}

		[SoapElement(ElementName = "ProductPrice")]
		[XmlElement(ElementName = "ProductPrice")]
		public decimal gxTpr_Productprice
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Productprice;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productprice = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Productprice");
			}

		}

		[SoapElement(ElementName = "InvoiceLineQty")]
		[XmlElement(ElementName = "InvoiceLineQty")]
		public short gxTpr_Invoicelineqty
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Invoicelineqty;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineqty = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Invoicelineqty");
			}

		}

		[SoapElement(ElementName = "InvoiceLineAmount")]
		[XmlElement(ElementName = "InvoiceLineAmount")]
		public decimal gxTpr_Invoicelineamount
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Invoicelineamount;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineamount = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Invoicelineamount");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Invoicelineamount = 0;
			SetDirty("Invoicelineamount");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "Mode")]
		[XmlElement(ElementName = "Mode")]
		public string gxTpr_Mode
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Mode;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Mode = value;
				SetDirty("Mode");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Mode_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Mode = "";
			SetDirty("Mode");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Mode_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "Modified")]
		[XmlElement(ElementName = "Modified")]
		public short gxTpr_Modified
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Modified;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Modified = value;
				SetDirty("Modified");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Modified_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Modified = 0;
			SetDirty("Modified");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Modified_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "Initialized")]
		[XmlElement(ElementName = "Initialized")]
		public short gxTpr_Initialized
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Initialized;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Initialized = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Initialized");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Initialized_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Initialized = 0;
			SetDirty("Initialized");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Initialized_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceLineId_Z")]
		[XmlElement(ElementName = "InvoiceLineId_Z")]
		public short gxTpr_Invoicelineid_Z
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Invoicelineid_Z");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z = 0;
			SetDirty("Invoicelineid_Z");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ProductId_Z")]
		[XmlElement(ElementName = "ProductId_Z")]
		public short gxTpr_Productid_Z
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Productid_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productid_Z = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Productid_Z");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Productid_Z_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Productid_Z = 0;
			SetDirty("Productid_Z");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Productid_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ProductName_Z")]
		[XmlElement(ElementName = "ProductName_Z")]
		public string gxTpr_Productname_Z
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Productname_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productname_Z = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Productname_Z");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Productname_Z_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Productname_Z = "";
			SetDirty("Productname_Z");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Productname_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ProductStock_Z")]
		[XmlElement(ElementName = "ProductStock_Z")]
		public short gxTpr_Productstock_Z
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Productstock_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productstock_Z = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Productstock_Z");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Productstock_Z_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Productstock_Z = 0;
			SetDirty("Productstock_Z");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Productstock_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "ProductPrice_Z")]
		[XmlElement(ElementName = "ProductPrice_Z")]
		public decimal gxTpr_Productprice_Z
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Productprice_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Productprice_Z = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Productprice_Z");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Productprice_Z_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Productprice_Z = 0;
			SetDirty("Productprice_Z");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Productprice_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceLineQty_Z")]
		[XmlElement(ElementName = "InvoiceLineQty_Z")]
		public short gxTpr_Invoicelineqty_Z
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Invoicelineqty_Z");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z = 0;
			SetDirty("Invoicelineqty_Z");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z_IsNull()
		{
			return false;
		}

		[SoapElement(ElementName = "InvoiceLineAmount_Z")]
		[XmlElement(ElementName = "InvoiceLineAmount_Z")]
		public decimal gxTpr_Invoicelineamount_Z
		{
			get
			{
				return gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z;
			}

			set
			{
				sdtIsNull = 0;
				gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z = value;
				gxTv_SdtInvoice_InvoiceLine_Modified = 1;
				SetDirty("Invoicelineamount_Z");
			}

		}

		public void gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z_SetNull()
		{
			gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z = 0;
			SetDirty("Invoicelineamount_Z");
			return;
		}

		public bool gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z_IsNull()
		{
			return false;
		}

		public void initialize()
		{
			sdtIsNull = 1;
			gxTv_SdtInvoice_InvoiceLine_Productname = "";
			gxTv_SdtInvoice_InvoiceLine_Mode = "";
			gxTv_SdtInvoice_InvoiceLine_Productname_Z = "";
			return;
		}

		public short isNull()
		{
			return sdtIsNull;
		}

		private short gxTv_SdtInvoice_InvoiceLine_Invoicelineid;
		private short sdtIsNull;
		private short gxTv_SdtInvoice_InvoiceLine_Productid;
		private short gxTv_SdtInvoice_InvoiceLine_Productstock;
		private short gxTv_SdtInvoice_InvoiceLine_Invoicelineqty;
		private short gxTv_SdtInvoice_InvoiceLine_Modified;
		private short gxTv_SdtInvoice_InvoiceLine_Initialized;
		private short gxTv_SdtInvoice_InvoiceLine_Invoicelineid_Z;
		private short gxTv_SdtInvoice_InvoiceLine_Productid_Z;
		private short gxTv_SdtInvoice_InvoiceLine_Productstock_Z;
		private short gxTv_SdtInvoice_InvoiceLine_Invoicelineqty_Z;
		private decimal gxTv_SdtInvoice_InvoiceLine_Productprice;
		private decimal gxTv_SdtInvoice_InvoiceLine_Invoicelineamount;
		private decimal gxTv_SdtInvoice_InvoiceLine_Productprice_Z;
		private decimal gxTv_SdtInvoice_InvoiceLine_Invoicelineamount_Z;
		private string gxTv_SdtInvoice_InvoiceLine_Productname;
		private string gxTv_SdtInvoice_InvoiceLine_Mode;
		private string gxTv_SdtInvoice_InvoiceLine_Productname_Z;
	}

	[DataContract(Name = @"Invoice.InvoiceLine", Namespace = "TestRESTKB1")]
	public class SdtInvoice_InvoiceLine_RESTInterface : GxGenericCollectionItem<SdtInvoice_InvoiceLine>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtInvoice_InvoiceLine_RESTInterface() : base()
		{
		}

		public SdtInvoice_InvoiceLine_RESTInterface(SdtInvoice_InvoiceLine psdt) : base(psdt)
		{
		}

		[DataMember(Name = "InvoiceLineId", Order = 0)]
		[GxSeudo()]
		public Nullable<short> gxTpr_Invoicelineid
		{
			get
			{
				return sdt.gxTpr_Invoicelineid;
			}

			set
			{
				sdt.gxTpr_Invoicelineid = (short)(value.HasValue ? value.Value : 0);
			}

		}

		[DataMember(Name = "ProductId", Order = 1)]
		[GxSeudo()]
		public Nullable<short> gxTpr_Productid
		{
			get
			{
				return sdt.gxTpr_Productid;
			}

			set
			{
				sdt.gxTpr_Productid = (short)(value.HasValue ? value.Value : 0);
			}

		}

		[DataMember(Name = "ProductName", Order = 2)]
		[GxSeudo()]
		public string gxTpr_Productname
		{
			get
			{
				return StringUtil.RTrim(sdt.gxTpr_Productname);
			}

			set
			{
				sdt.gxTpr_Productname = value;
			}

		}

		[DataMember(Name = "ProductStock", Order = 3)]
		[GxSeudo()]
		public Nullable<short> gxTpr_Productstock
		{
			get
			{
				return sdt.gxTpr_Productstock;
			}

			set
			{
				sdt.gxTpr_Productstock = (short)(value.HasValue ? value.Value : 0);
			}

		}

		[DataMember(Name = "ProductPrice", Order = 4)]
		[GxSeudo()]
		public string gxTpr_Productprice
		{
			get
			{
				return StringUtil.LTrim(StringUtil.Str(sdt.gxTpr_Productprice, 10, 2));
			}

			set
			{
				sdt.gxTpr_Productprice = NumberUtil.Val(value, ".");
			}

		}

		[DataMember(Name = "InvoiceLineQty", Order = 5)]
		[GxSeudo()]
		public Nullable<short> gxTpr_Invoicelineqty
		{
			get
			{
				return sdt.gxTpr_Invoicelineqty;
			}

			set
			{
				sdt.gxTpr_Invoicelineqty = (short)(value.HasValue ? value.Value : 0);
			}

		}

		[DataMember(Name = "InvoiceLineAmount", Order = 6)]
		[GxSeudo()]
		public string gxTpr_Invoicelineamount
		{
			get
			{
				return StringUtil.LTrim(StringUtil.Str(sdt.gxTpr_Invoicelineamount, 10, 2));
			}

			set
			{
				sdt.gxTpr_Invoicelineamount = NumberUtil.Val(value, ".");
			}

		}

		public SdtInvoice_InvoiceLine sdt
		{
			get
			{
				return (SdtInvoice_InvoiceLine)Sdt;
			}

			set
			{
				Sdt = value;
			}

		}

		[OnDeserializing]
		void checkSdt(StreamingContext ctx)
		{
			if (sdt == null)
			{
				sdt = new SdtInvoice_InvoiceLine();
			}
		}

	}

	[DataContract(Name = @"Invoice.InvoiceLine", Namespace = "TestRESTKB1")]
	public class SdtInvoice_InvoiceLine_RESTLInterface : GxGenericCollectionItem<SdtInvoice_InvoiceLine>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtInvoice_InvoiceLine_RESTLInterface() : base()
		{
		}

		public SdtInvoice_InvoiceLine_RESTLInterface(SdtInvoice_InvoiceLine psdt) : base(psdt)
		{
		}

		public SdtInvoice_InvoiceLine sdt
		{
			get
			{
				return (SdtInvoice_InvoiceLine)Sdt;
			}

			set
			{
				Sdt = value;
			}

		}

		[OnDeserializing]
		void checkSdt(StreamingContext ctx)
		{
			if (sdt == null)
			{
				sdt = new SdtInvoice_InvoiceLine();
			}
		}

	}
}
