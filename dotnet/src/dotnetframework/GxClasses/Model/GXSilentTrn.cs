namespace GeneXus.Utils
{
	using System;
	using System.Collections;
	using GeneXus.Metadata;
	using GeneXus.Application;
	using System.Xml.Serialization;
	using System.Collections.Generic;
	using Jayrock.Json;
	using System.Text;
	using System.Security.Cryptography;
	using System.Reflection;
	using log4net;
	using System.Runtime.Serialization;
#if !NETCORE
	using System.ServiceModel;
#endif
	using Configuration;
	using System.Globalization;
	using GeneXus.Http;

	public interface IGxSilentTrn
	{
		IGxContext context { get; set; }

		/*
		* Insert or update depending on the internal record mode.
		*/
		void Save();

        /*
         * Insert the actual record
         */
        bool Insert();

        /* 
         * Update the actual record
         */
        bool Update();
        /*
         * Try to insert the actual record, if fail then it try to update instead
         */
        bool InsertOrUpdate();

		void Check();
		void SetMode(string mode);
		string GetMode();
		int Errors();
		msglist GetMessages();
		void LoadKey(object[] parms);
		void initialize();
		void cleanup();
		void SetSDT( GxSilentTrnSdt sdt, short loadBC);
		void ReloadFromSDT();
        bool Reindex();
		void ForceCommitOnExit();
        
        string ToString();
        GxContentInfo GetContentInfo();
		void Load();
        void GetInsDefault();
	}
    public class GxContentInfo
    {
        private string m_entity;
        private string m_viewer;
        private string m_title;
        private string m_id;
        private IList<string> m_keys = new List<string>();

        public string Entity
        {
            get { return m_entity; }
            set { m_entity = value; }
        }
        public string Id
        {
            get { return m_id; }
            set { m_id = value; }
        }
        public string Viewer
        {
            get { return m_viewer; }
            set { m_viewer = value; }
        }
        public string Title
        {
            get { return m_title; }
            set { m_title = value; }
        }
        public void AddKey(string s)
        {
            m_keys.Add(s);
        }
        public IList<string> Keys
        {
            get { return m_keys; }
        }

    }

	public interface IGxSilentTrnGridItem
	{
		short gxTpr_Modified {get;set;}
		string gxTpr_Mode {get;set;}
		
	}
	public class TRANSACTION_MODE
	{
		public static string INSERT = "INS";
		public static string MODE_DELETE = "DLT";
		public static string MODE_UPDATE = "UPD";
	}
#if NETCORE
	public abstract class GxSilentTrn:GXBaseObject
	{
		public bool IsMain;

		public virtual void cleanup() { }

		protected int handle;


		public GxSilentTrn()
		{
		}

		public virtual void initialize() {  }
		public void flushBuffer(){	}

		public virtual object getParm(object[] parms, int index)
		{
			return parms[index];
		}

		

		public msglist GX_msglist
		{
			get { return context.GX_msglist; }
			set { context.GX_msglist = value; }
		}
	}
#endif
	[Serializable]
	public class GxSilentTrnSdt : GxUserType
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GxSilentTrnSdt));
		public GxSilentTrnSdt(IGxContext context){
            this.context = context;
            
        }
		public GxSilentTrnSdt() {
            //XML serialization
        }
		[XmlIgnore]
		[NonSerialized]
		public IGxSilentTrn trn;
        protected string assembly;
        protected string fullTypeName;

		protected Assembly constructorCallingAssembly;

		[XmlIgnore]
		public IGxSilentTrn Transaction
		{
			get	{return trn;}
			set {trn = value;}
		}
		public IGxSilentTrn getTransaction()
		{
			return trn;
		}
		public void setTransaction(IGxSilentTrn t)
		{
			dirties.Clear();
			trn = t;
		}
		public virtual void Save()
		{
			if( Transaction != null) 
				Transaction.Save();
		}

  
        public virtual bool Insert()
        {
            if (Transaction != null)
                return Transaction.Insert();
            return false;
        }

        public virtual bool Update()
        {
            if (Transaction != null)
                return Transaction.Update();
            return false;
        }

        public virtual bool InsertOrUpdate()
        {
            if (Transaction != null)
                return Transaction.InsertOrUpdate();
            return false;
        }

		public virtual void Check()
		{
			if( Transaction != null) 
				Transaction.Check();
		}
		public virtual void Delete( )
		{
			if( Transaction != null) 
			{
				Transaction.SetMode("DLT") ;
				Transaction.Save();
			}
		}
		public virtual bool Success()
		{
			if( Transaction != null) 
				return (Transaction.Errors() == 0);
			return false;
		}
		public virtual bool Fail()
		{
			if( Transaction != null) 
				return (Transaction.Errors() == 1);
			return false;
		}
		public virtual string GetMode()
		{
			if( Transaction != null) 
				return Transaction.GetMode();
			return "";
		}
		public override object Clone()
		{
			GxSilentTrnSdt o = (GxSilentTrnSdt) (base.MemberwiseClone());
			o.trn.SetSDT( o, 0 );
			return o;
		}
		public override bool FromXml(string s)
		{
			bool success = base.FromXml(s);
			if( Transaction != null) 
				Transaction.ReloadFromSDT();
			return success;
		}
		public static GxSilentTrnSdt Create(string Name, string Namespace, IGxContext context)
		{
			GxSilentTrnSdt ret;
			try
			{
#if NETCORE
				ret = ClassLoader.FindInstance(Config.CommonAssemblyName, Namespace, GxSdtNameToCsharpName(Name), new object[] { context }, Assembly.GetEntryAssembly()) as GxSilentTrnSdt;
#else
				ret = ClassLoader.FindInstance(Config.CommonAssemblyName, Namespace, GxSdtNameToCsharpName(Name), new object[] { context }, Assembly.GetCallingAssembly()) as GxSilentTrnSdt;
#endif
			}
			catch
			{
				ret = new GxSilentTrnSdt(context);
			}
			return ret;
		}

		public static IGxCollection CreateCollection(string Name, string Namespace, IGxContext context)
		{
#if NETCORE
			Type type = ClassLoader.FindType(Config.CommonAssemblyName, Namespace, GxSdtNameToCsharpName(Name), Assembly.GetEntryAssembly());
#else
			Type type = ClassLoader.FindType(Config.CommonAssemblyName, Namespace, GxSdtNameToCsharpName(Name), Assembly.GetCallingAssembly());
#endif
			var listType = typeof(GXBaseCollection<>);
			var concreteType = listType.MakeGenericType(type);
			return Activator.CreateInstance(concreteType, new object[] { context, GxSdtNameToCsharpName(Name), Namespace}) as IGxCollection;
		}
		public static string GxSdtNameToCsharpName(string GxFullSdtName)
		{
			string[] names = GxFullSdtName.Split('\\', '/');
			StringBuilder qualifiedName = new StringBuilder();
			if (names.Length > 1)
			{
				for (int i = 0; i < names.Length - 1; i++)
				{
					qualifiedName.Append(names[i].ToLower());
					if (i < names.Length - 1)
						qualifiedName.Append('.');
				}
			}
			qualifiedName.Append("Sdt");
			qualifiedName.Append(names[names.Length - 1]);
			return qualifiedName.ToString();
		}
		public string GetName()
		{
			return GetType().Name.Substring(3);
		}

		public void Load()
		{
			getTransaction().Load();
		}

		public void ForceCommitOnExit()
		{
			getTransaction().ForceCommitOnExit();
		}

		public void Load(IGxCollection Key)
		{
			Type me = GetType();
			for (int i = 1; i <= Key.Count; i++)
			{
				object LoadKeyItem = Key.Item(i);
				Type LoadKeyItemType = LoadKeyItem.GetType();
				string Name = LoadKeyItemType.InvokeMember("gxTpr_Name", BindingFlags.GetProperty, null, LoadKeyItem, Array.Empty<object>()) as string;
				string Value = LoadKeyItemType.InvokeMember("gxTpr_Value", BindingFlags.GetProperty, null, LoadKeyItem, Array.Empty<object>()) as string;

				string FieldName = "gxTpr_" + Name.Substring(0, 1).ToUpper() + Name.Substring(1).ToLower();
				Type MemberType = me.GetProperty(FieldName).PropertyType;
				me.InvokeMember(FieldName, BindingFlags.SetProperty, null, this, new object[] { Convert.ChangeType(Value, MemberType, System.Globalization.CultureInfo.CurrentCulture) });
			}
			getTransaction().Load();
		}
#if !NETCORE
		public void SetContent(string jsonContent)
		{
			Type me = GetType();

			JsonTextReader reader = new JsonTextReader(new System.IO.StringReader(jsonContent));

			while (!reader.EOF)
			{
				object obj = reader.DeserializeNext();

				JObject jobj = obj as JObject;
				JArray jlevel = obj as JArray;
				JArray jlevels = null;
				if (jobj != null)
				{
					if (!jobj.Contains("Fields")) continue;
					jlevel = (jobj["Fields"]) as JArray;
					if (jobj.Contains("Levels"))
						jlevels = (jobj["Levels"]) as JArray;

				}
				System.Globalization.CultureInfo Culture = GxContext.Current.localUtil.CultureInfo;
				SetContent(Culture, GetName(), me, this, jlevel, jlevels);
				break;
			}
		}
#endif
		public string GetValue(string Name)
		{
			if (string.IsNullOrEmpty(Name))
			{
				GXLogging.Warn(log, "GetValue of empty SDT property at " + this.GetType());
				return string.Empty;
			}
			else
			{
				Type me = GetType();
				string FieldName = "gxTpr_" + Name.Substring(0, 1).ToUpper() + Name.Substring(1).ToLower();
				object objValue = me.GetProperty(FieldName).GetValue(this, Array.Empty<object>());
				if (objValue is DateTime)
				{ 
					DateTime dt = (DateTime)objValue;
					if (dt.Year > 200)
						objValue = new DateTime(dt.Year - 1900, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
					
				}
				else if (objValue is Guid)
				{
					objValue = objValue.ToString();
				}
				else
				{
					IGxCollection objColValue = objValue as IGxCollection;
					if (objColValue != null)
					{
						return objColValue.ToJSonString();
					}
				}

				return Convert.ChangeType(objValue, typeof(string), System.Globalization.CultureInfo.InvariantCulture) as string;
			}
		}

		public void SetValue(string Name, string Value)
		{
			Type me = GetType();
			string FieldName = "gxTpr_" + Name.Substring(0, 1).ToUpper() + Name.Substring(1).ToLower();
			Type MemberType = me.GetProperty(FieldName).PropertyType;
			if(MemberType.IsAssignableFrom(typeof(Guid)))
				me.InvokeMember(FieldName, BindingFlags.SetProperty, null, this, new object[] { new Guid(Value) });
			else if(typeof(IGxCollection).IsAssignableFrom(MemberType))
			{
				Object levelInstance = me.InvokeMember(FieldName, BindingFlags.GetProperty, null, this, Array.Empty<object>());
				IGxCollection Col = levelInstance as IGxCollection;
				Col.FromJSonString(Value);
			}
			else
				me.InvokeMember(FieldName, BindingFlags.SetProperty, null, this, new object[] { Convert.ChangeType(Value, MemberType, System.Globalization.CultureInfo.InvariantCulture) });
		}
#if !NETCORE
		protected void SetContent(System.Globalization.CultureInfo Culture, string CurrentLevelName, Type me, Object meInstance, JArray level, JArray levels)
		{
			if (level != null)
			{
				foreach (IDictionary item in level)
				{
					string Name = item["Name"] as string;
					object Value = item["Value"];
					string FieldName = "gxTpr_" + Name.Substring(0, 1).ToUpper() + Name.Substring(1).ToLower();
					Type MemberType = me.GetProperty(FieldName).PropertyType;
					if (string.IsNullOrEmpty(Value as string))
					{
						if (MemberType == typeof(DateTime))
						{
							Value = DateTime.MinValue;
						}
						else if (MemberType.IsPrimitive)
						{
							Value = "0";
						}
						else if (MemberType == typeof(Decimal))
						{
							Value = Decimal.Zero;
						}
					}
					me.InvokeMember(FieldName, BindingFlags.SetProperty, null, meInstance, new object[] { Convert.ChangeType(Value, MemberType, Culture) });
				}
			}

			if (levels != null)
			{
				foreach (IDictionary nlevel in levels)
				{
					string Name = nlevel["Name"] as string;
					string LevelName = "gxTpr_" + Name.Substring(0, 1).ToUpper() + Name.Substring(1).ToLower();
					Type LevelType = me.GetProperty(LevelName).PropertyType;
					Object levelInstance = me.InvokeMember(LevelName, BindingFlags.GetProperty, null, meInstance, Array.Empty<object>());
					GxObjectCollectionBase Col = levelInstance as GxObjectCollectionBase;
					string NewLevelName = CurrentLevelName + "_" + Name;
					if (Col != null)
					{
						string Namespace = Col._containedTypeNamespace;
						string className = "Sdt" + NewLevelName;
						levelInstance = ClassLoader.FindInstance(Config.CommonAssemblyName, Namespace, className, new object[] { Col.context}, Assembly.GetCallingAssembly());
						LevelType = levelInstance.GetType();
						Col.addNew(levelInstance);
					}

					JArray LevelFields = nlevel["Fields"] as JArray;
					JArray LevelLevels = nlevel["Levels"] as JArray;
					SetContent(Culture, NewLevelName, LevelType, levelInstance, LevelFields, LevelLevels);
				}
			}
		}
#endif
		public virtual IGxCollection GetAll(int Start, int Count)
		{
			IGxSilentTrn trn = getTransaction();
			Type me = trn.GetType();
			return me.InvokeMember("GetAll", BindingFlags.InvokeMethod, null, trn, new object[] { Start, Count}) as IGxCollection;
		}

		public virtual object[][] GetBCKey()
		{
			return null;
		}

		public virtual GXProperties GetMetadata()
		{
			return new GXProperties();
		}
		public GXBaseCollection<SdtMessages_Message> GetMessages()
		{
			short item = 1;
			GXBaseCollection<SdtMessages_Message> msgs = new GXBaseCollection<SdtMessages_Message>(context, "Messages.Message", "Genexus");
			SdtMessages_Message m1;
			IGxSilentTrn trn = getTransaction();
			if (trn != null)
			{
				msglist msgList = trn.GetMessages();
				while (item <= msgList.ItemCount)
				{
					m1 = new SdtMessages_Message(context);
					m1.gxTpr_Id = msgList.getItemValue(item);
					m1.gxTpr_Description = msgList.getItemText(item);
					m1.gxTpr_Type = msgList.getItemType(item);
					msgs.Add(m1, 0);
					item = (short)(item + 1);
				}
			}
			return msgs;
		}
	}
	public class GxGenericCollection<T> : List<T> where T : new()
	{
		public GxGenericCollection()
			: base()
		{
		}
		public GxGenericCollection(IGxCollection x)
		{
			IList xarr = (IList)x;
			foreach (GxUserType x1 in xarr)
			{
				IGxGenericCollectionItem x2 = (IGxGenericCollectionItem)new T();
				x2.Sdt = x1;
				Add((T)x2);
			}
		}
		public void LoadCollection(IGxCollection x)
		{
			foreach (IGxGenericCollectionItem x1 in this)
				x.Add(x1.Sdt);
		}
		public override string ToString()
		{
			string s = "";
			foreach (IGxGenericCollectionItem x1 in this)
			{
				s += x1.ToString();
			}
			return s;
		}
	}
	public interface IGxGenericCollectionItem
	{
		GxUserType Sdt { get; set; }
	}

	[DataContract]
	public class GxGenericCollectionItem<T> : IGxGenericCollectionItem where T : GxUserType, new()
	{
		T sdt1;
		public GxGenericCollectionItem()
		{
			sdt1 = new T();
		}
		public GxGenericCollectionItem(T s)
		{
			sdt1 = s;
		}
		public GxUserType Sdt
		{
			get { return sdt1; }
			set { sdt1 = (T)value; }
		}
		public IGxContext context
		{
			get { return sdt1.context; }
		}
		public override string ToString()
		{
			string s = "";
			object o;
			foreach (PropertyInfo propInfo in this.GetType().GetProperties())
			{
#if NETCORE
				if (propInfo.CanRead && propInfo.Name.StartsWith("gxTpr_") && propInfo.GetCustomAttribute(typeof(GxSeudo), false) != null)
#else
				if (propInfo.CanRead && propInfo.Name.StartsWith("gxTpr_") && propInfo.GetCustomAttributes(typeof(GxSeudo), false).Length > 0)
#endif
				{
					PropertyInfo zPropInfo = sdt1.GetType().GetProperty(propInfo.Name + "_Z");
					if (zPropInfo != null)
						o = zPropInfo.GetValue(sdt1, null);
					else
						o = propInfo.GetValue(this, null);
					if (o != null)
					{
#if NETCORE
					var fixedPoint = "F";
					if (o is decimal)
						s += ((decimal)o).ToString(fixedPoint, CultureInfo.InvariantCulture);
					else
						s += o.ToString();
#else
						s += o.ToString();
#endif
					}
				}
			}
			return s;
		}
		public void CopyFrom(GxGenericCollectionItem<T> source)
		{

			foreach (PropertyInfo info in source.GetType().GetProperties())
			{
				if (info.CanRead && info.Name.StartsWith("gxTpr_"))
				{
					if (info.PropertyType.BaseType.Name.StartsWith("List"))
					{
						object thisCollection = info.GetValue(this, null);
						foreach (object item in (IList)thisCollection)
						{
							PropertyInfo itemInfo = item.GetType().GetProperty("sdt");
							IGxSilentTrnGridItem bcColItem = (IGxSilentTrnGridItem)itemInfo.GetValue(item, null);
							bcColItem.gxTpr_Mode = "DLT";
							bcColItem.gxTpr_Modified = 1;
						}
						object sourceCollection = info.GetValue(source, null);
						info.SetValue(this, sourceCollection, null);
					}
					else
					{
                        if (info.GetCustomAttributes(typeof(GxUpload), false).Length > 0)
                        {
                            string uploadPath = (string)info.GetValue(source, null);
							if (GxRestUtil.IsUpload(uploadPath)) //File upload from SD
							{
								info.SetValue(this, GxRestUtil.UploadPath(uploadPath), null);
								PropertyInfo info_gxi = source.Sdt.GetType().GetProperty(info.Name + "_gxi");//gxi reset
								if (info_gxi != null)
									info_gxi.SetValue(this.Sdt, string.Empty, null);
							}
							else if (PathUtil.IsAbsoluteUrl(uploadPath)) //External url from SD
							{
								PropertyInfo info_gxi = source.Sdt.GetType().GetProperty(info.Name + "_gxi");
								if (info_gxi != null)
									info_gxi.SetValue(this.Sdt, uploadPath, null);
								info.SetValue(this, string.Empty, null);
							}
							else if (string.IsNullOrEmpty(uploadPath))
							{
								PropertyInfo info_gxi = source.Sdt.GetType().GetProperty(info.Name + "_gxi");
								if (info_gxi != null)
									info_gxi.SetValue(this.Sdt, string.Empty, null);
								info.SetValue(this, string.Empty, null);
							}
                        }
                        else
                        {
                            object o = info.GetValue(source, null);
                            info.SetValue(this, o, null);
                        }
					}
				}
			}
		}
		protected string getHash()
		{
			string str = ToString();
			byte[] result;
			MD5 md5;
			Encoder enc = System.Text.Encoding.Unicode.GetEncoder();
			byte[] unicodeText = new byte[str.Length * 2];
			enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);
#if NETCORE
			using (md5 = MD5.Create())
#else
#pragma warning disable SCS0006 // Weak hashing function
			using (md5 = new MD5CryptoServiceProvider())
#pragma warning restore SCS0006 // Weak hashing function
#endif
			{
				result = md5.ComputeHash(unicodeText);
			}
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < result.Length; i++)
				sb.Append(result[i].ToString("X2"));
			return sb.ToString();




		}
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes"), AttributeUsage(AttributeTargets.Property)]
	public class GxSeudo : Attribute
	{
		public GxSeudo()
		{
		}
	}
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes"), AttributeUsage(AttributeTargets.Property)]
    public class GxUpload : Attribute
    {
        public GxUpload()
        {
        }
    }
#if !NETCORE
	[XmlSerializerFormat]
#endif
	[XmlRoot(ElementName = "Message")]
	[XmlType(TypeName = "Message", Namespace = "GeneXus")]
	[Serializable]
	public class SdtMessages_Message : GxUserType
	{
		public SdtMessages_Message()
		{
			/* Constructor for serialization */
		}

		public SdtMessages_Message(IGxContext context)
		{
			this.context = context;
			initialize();
		}

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
			ToJSON(true);
			return;
		}

		public override void ToJSON(bool includeState)
		{
			AddObjectProperty("Id", gxTv_SdtMessages_Message_Id, false);
			AddObjectProperty("Type", gxTv_SdtMessages_Message_Type, false);
			AddObjectProperty("Description", gxTv_SdtMessages_Message_Description, false);
			return;
		}

		[SoapElement(ElementName = "Id")]
		[XmlElement(ElementName = "Id")]
		public String gxTpr_Id
		{
			get
			{
				return gxTv_SdtMessages_Message_Id;
			}

			set
			{
				gxTv_SdtMessages_Message_Id = value;
				SetDirty("Id");
			}

		}

		[SoapElement(ElementName = "Type")]
		[XmlElement(ElementName = "Type")]
		public short gxTpr_Type
		{
			get
			{
				return gxTv_SdtMessages_Message_Type;
			}

			set
			{
				gxTv_SdtMessages_Message_Type = value;
				SetDirty("Type");
			}

		}

		[SoapElement(ElementName = "Description")]
		[XmlElement(ElementName = "Description")]
		public String gxTpr_Description
		{
			get
			{
				return gxTv_SdtMessages_Message_Description;
			}

			set
			{
				gxTv_SdtMessages_Message_Description = value;
				SetDirty("Description");
			}

		}

		public void initialize()
		{
			gxTv_SdtMessages_Message_Id = "";
			gxTv_SdtMessages_Message_Description = "";
			return;
		}

		protected short gxTv_SdtMessages_Message_Type;
		protected String gxTv_SdtMessages_Message_Id;
		protected String gxTv_SdtMessages_Message_Description;
	}

	[DataContract(Name = @"Messages.Message", Namespace = "GeneXus")]
	public class SdtMessages_Message_RESTInterface : GxGenericCollectionItem<SdtMessages_Message>, System.Web.SessionState.IRequiresSessionState
	{
		public SdtMessages_Message_RESTInterface()
			: base()
		{
		}

		public SdtMessages_Message_RESTInterface(SdtMessages_Message psdt)
			: base(psdt)
		{
		}

		[DataMember(Name = "Id", Order = 0)]
		public String gxTpr_Id
		{
			get
			{
				return sdt.gxTpr_Id;
			}

			set
			{
				sdt.gxTpr_Id = value;
			}

		}

		[DataMember(Name = "Type", Order = 1)]
		public Nullable<short> gxTpr_Type
		{
			get
			{
				return sdt.gxTpr_Type;
			}

			set
			{
				sdt.gxTpr_Type = (short)(value.HasValue ? value.Value : 0);
			}

		}

		[DataMember(Name = "Description", Order = 2)]
		public String gxTpr_Description
		{
			get
			{
				return sdt.gxTpr_Description;
			}

			set
			{
				sdt.gxTpr_Description = value;
			}

		}

		public SdtMessages_Message sdt
		{
			get
			{
				return (SdtMessages_Message)Sdt;
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
				sdt = new SdtMessages_Message();
			}
		}

	}

}