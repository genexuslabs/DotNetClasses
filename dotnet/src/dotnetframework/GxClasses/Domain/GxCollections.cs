namespace GeneXus.Utils
{
	using System;
	using System.Collections.Specialized;
	using System.Collections;
	using System.Collections.Generic;
	using GeneXus.XML;
	using System.Reflection;
	using System.Xml.Serialization;
	using System.Data;
	using GeneXus.Application;
	using System.Text;
	using System.Globalization;
	using Jayrock.Json;
	using log4net;
	using System.ComponentModel;
	using System.Xml;
	using System.Runtime.Serialization;
	using System.Linq;
	using GeneXus.Http;
	using GeneXus.Configuration;
	using GeneXus.Metadata;
	using System.Collections.Concurrent;

	public class GxParameterCollection : IDataParameterCollection
	{
		ArrayList parameters;
		public void Dispose() { }

		public GxParameterCollection()
		{
			parameters = new ArrayList();
		}

		public void CopyTo(Array array, int index)
		{
			parameters.CopyTo(array, index);
		}
		public int Count
		{
			get { return parameters.Count; }
		}
		public bool IsSynchronized
		{
			get { return false; }
		}
		public object SyncRoot
		{
			get { return null; }
		}
		public IEnumerator GetEnumerator()
		{
			return parameters.GetEnumerator();
		}
		int IList.Add(object value)
		{
			return parameters.Add(value);
		}
		public int Add(IDataParameter value)
		{
			return parameters.Add(value);
		}

		public void Clear()
		{
			parameters.Clear();
		}
		public bool Contains(object value)
		{
			return parameters.Contains(value);
		}
		public bool Contains(string value)
		{
			foreach (IDataParameter p in parameters)
			{
				if (p.ParameterName == value) return true;
			}
			return false;
		}
		public int IndexOf(object value)
		{
			return parameters.IndexOf(value);
		}
		public int IndexOf(string value)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (((IDataParameter)parameters[i]).ParameterName == value)
					return i;

			}
			return -1;
		}
		public void Insert(int index, object value)
		{
			parameters.Insert(index, value);
		}
		public bool IsFixedSize
		{
			get { return parameters.IsFixedSize; }
		}
		public bool IsReadOnly
		{
			get { return parameters.IsReadOnly; }
		}
		public void Remove(object value)
		{
			parameters.Remove(value);
		}
		public void RemoveAt(int index)
		{
			parameters.RemoveAt(index);
		}
		public void RemoveAt(string sObject)
		{
			int index = parameters.IndexOf(sObject);
			if (index == -1)
				parameters.RemoveAt(index);
		}
		public IDbDataParameter this[int i]
		{
			get { return (IDbDataParameter)parameters[i]; }
			set { parameters[i] = value; }
		}

		object IList.this[int i]
		{
			get { return parameters[i]; }
			set { parameters[i] = value; }
		}
		public object this[string sObject]
		{
			get
			{
				int index = this.IndexOf(sObject);
				if (index == -1)
					return null;
				return parameters[index];
			}
			set
			{
				int index = this.IndexOf(sObject);
				if (index == -1)
					parameters[index] = value;
			}
		}
		public GxParameterCollection Distinct()
		{
			if (Count > 1)
			{
				HashSet<string> parms = new HashSet<string>();
				GxParameterCollection uniqueParms = new GxParameterCollection();
				for (int j = Count - 1; j >= 0; j--)
				{
					if (!parms.Contains(this[j].ParameterName))
					{
						uniqueParms.Add(this[j]);
						parms.Add(this[j].ParameterName);
					}
				}
				return uniqueParms;
			}
			else
			{
				return this;
			}
		}
	}

	public class GxStringCollection : StringCollection, IGxJSONSerializable, IGxJSONAble
	{
		public GxStringCollection()
			: base()
		{
		}
		public string Item(int i)
		{
			if (i - 1 < Count)
				return this[i - 1];
			else
				return string.Empty;
		}
		public void addNew(string s)
		{
			Add(s);
		}
		public void InsertAt(int position, string s)
		{
			Insert(position, s);
		}
		#region MetodosPorCompatibJava
		public int getCount()
		{
			return this.Count;
		}
		public string item(int i)
		{
			return Item(i);
		}
		public static GxStringCollection getFromString(string list)
		{
			GxStringCollection ret = new GxStringCollection();
			char[] sep = { ';' };
			string[] tokens = list.Split(sep);

			foreach (string s in tokens)
			{
				ret.Add(s);
			}
			return ret;
		}
		#endregion

		#region IGxJSONSerializable Members

		public string ToJSonString()
		{
			return JSONHelper.Serialize<GxStringCollection>(this, Encoding.UTF8);
		}
		public bool FromJSonString(string s)
		{
			return FromJSonString(s, null);
		}
		public bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages)
		{
			JArray jarray = JSONHelper.ReadJSON<JArray>(s, Messages);
			bool result = jarray != null;
			try
			{
				FromJSONObject(jarray);
				return result;
			}
			catch (Exception ex)
			{
				GXUtil.ErrorToMessages("FromJson Error", ex, Messages);
				return false;
			}
		}
		public virtual void FromJSONObject(dynamic obj)
		{
			base.Clear();
			JArray jobj = obj as JArray;
			if (jobj != null)
			{
				for (int i = 0; i < jobj.Length; i++)
				{
					Add((string)jobj[i]);
				}
			}
		}
		#endregion

		#region IGxJSONAble Members

		public void AddObjectProperty(string name, object prop)
		{

		}

		public object GetJSONObject()
		{
			return new JArray(this);
		}

		public object GetJSONObject(bool includeState)
		{
			return GetJSONObject();
		}

		public string ToJavascriptSource()
		{
			return GetJSONObject().ToString();
		}

		#endregion
		public bool FromJSonFile(GxFile file)
		{
			return FromJSonFile(file, null);
		}
		public bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages)
		{
			if (GXUtil.CheckFile(file, Messages))
				return FromJSonString(file.ReadAllText(string.Empty), Messages);
			else
				return false;
		}
	}

	[Serializable]
	[CollectionDataContract(Name = "GxSimpleCollection")]
	public class GxSimpleCollection<T> : List<T>, IGxXMLSerializable, ICloneable, IGxJSONAble, IGxCollection<T>, IGxJSONSerializable
	{

		protected CollectionBase _jsonArr;
		protected CollectionBase jsonArr
		{
			get
			{
				if (_jsonArr == null)
					_jsonArr = new JArray();
				return _jsonArr;
			}
		}
		protected object currentItem;

		public GxSimpleCollection(List<T> value)
		{
			this.AddRange(value);
		}
		public GxSimpleCollection()
			: base()
		{
		}
		public Object Item(int i)
		{
			if (i - 1 < GetCount())
				return GetItem(i - 1);
			else
				return null;
		}
		protected virtual T GetItem(int index)
		{
			return base[index];
		}
		protected virtual int GetCount()
		{
			return base.Count;
		}
		public new int Count
		{
			get
			{
				return GetCount();
			}
		}

		public virtual IGxXMLSerializable CurrentItem
		{
			get
			{
				return (IGxXMLSerializable)currentItem;
			}
			set
			{
				currentItem = value;
			}
		}
		public string GetString(int i)
		{
			return Convert.ToString(this[i - 1]);
		}
		public T GetNumeric(int i)
		{
			return this[i - 1];
		}
		public DateTime GetDatetime(int i)
		{
			return Convert.ToDateTime(this[i - 1]);
		}
		public Geospatial GetGeospatial(int i)
		{
			return new Geospatial(this[i - 1]);
		}
		public int Add(Object o)
		{
			base.Add((T)o);
			return base.Count;
		}
		public void addNew(object o)
		{
			Add(o, 0);
		}
		public void Add(Object o, int idx)
		{
			T TObject = ConvertToT(o);
			if (idx == 0)
				Add(TObject);
			else
				Insert(idx - 1, TObject);
		}
		private T ConvertToT(Object o)
		{
			T TObject=default(T);
			if (o != null)
			{
				if (typeof(T).IsAssignableFrom(o.GetType()))
					TObject = (T)o;
				else if (typeof(IGxJSONAble).IsAssignableFrom(typeof(T)))
				{

					TObject = (T)Activator.CreateInstance(typeof(T));
					((IGxJSONAble)TObject).FromJSONObject(o);
				}
				else
				{
					if (typeof(T) == typeof(Geospatial))
					{
						object g = (Geospatial)(string)o;
						TObject = (T)g;
					}
					else if (typeof(T) == typeof(Guid))
					{
						object g = new Guid(o.ToString());
						TObject = (T)g;
					}
					else if (o is IConvertible)
					{
						TObject = (T)Convert.ChangeType(o, typeof(T));
					}
					else
						TObject = (T)Convert.ChangeType(o.ToString(), typeof(T));
				}
			}
			return TObject;
		}
		public void RemoveItem(int idx)
		{
			if (idx <= 0 || idx > Count)
				return;
			RemoveAt(idx - 1);
		}
		public void RemoveElement(int idx)
		{
			if (idx <= 0 || idx > Count)
				return;
			base.RemoveAt(idx - 1);
		}
		public virtual void ClearCollection()
		{
			base.Clear();
		}
		public virtual object Clone()
		{
			GxSimpleCollection<T> collection = new GxSimpleCollection<T>();
			collection.AddRange(this);
			return collection;
		}
		public int IndexOf(object value)
		{
			T TObject = ConvertToT(value);
			return base.IndexOf(TObject) + 1;
		}

		public virtual void writexml(GXXMLWriter oWriter, string sName)
		{
			writexml(oWriter, sName, "");
		}
		public virtual void writexml(GXXMLWriter oWriter, string sName, string sNamespace)
		{
			writexmlcollection(oWriter, sName, sNamespace, "item", sNamespace);
		}
		public virtual void writexml(GXXMLWriter oWriter, string sName, string sNamespace, bool includeState)
		{
			writexmlcollection(oWriter, sName, sNamespace, "item", sNamespace, includeState);
		}
		public virtual void writexmlcollection(GXXMLWriter oWriter, string sName, string sNamespace, string itemName, string itemNamespace)
		{
			writexmlcollection(oWriter, sName, sNamespace, itemName, itemNamespace, true);
		}

		public virtual void writexmlcollection(GXXMLWriter oWriter, string sName, string sNamespace, string itemName, string itemNamespace, bool includeState)
		{
			if (sName.Trim().Length > 0)
			{
				oWriter.WriteStartElement(sName);
				if (!sNamespace.StartsWith("[*:nosend]"))
					oWriter.WriteAttribute("xmlns", sNamespace);
				else
					sNamespace = sNamespace.Substring(10, sNamespace.Length - 10);
			}
			string itemName1 = "item";
			if (itemName.Trim().Length > 0)
				itemName1 = itemName;
			if (itemNamespace.StartsWith("[*:nosend]"))
				itemNamespace = itemNamespace.Substring(10, itemNamespace.Length - 10);

			bool sendItemNamespace = false;
			if (sName.Trim().Length == 0)
			{
				sendItemNamespace = true;
			}
			else if (sNamespace != itemNamespace)
			{
				sendItemNamespace = true;
			}

			foreach (Object obj in this)
			{
				oWriter.WriteElement(itemName1, obj.ToString());
				if (sendItemNamespace)
					oWriter.WriteAttribute("xmlns", itemNamespace);
			}
			if (sName.Trim().Length != 0)
				oWriter.WriteEndElement();
		}
		public virtual short readxml(GXXMLReader oReader)
		{
			return readxml(oReader, "");
		}
		public virtual short readxml(GXXMLReader oReader, string sName)
		{
			return readxmlcollection(oReader, sName, "item");
		}
		public virtual short readxmlcollection(GXXMLReader oReader, string sName, string itemName)
		{
			short currError = 1;
			if (!string.IsNullOrEmpty(sName))  // unwrapped collection 
				oReader.Read();
			string sTagName = oReader.Name;
			base.Clear();

			while ((string.Compare(oReader.Name.TrimEnd(' '), sTagName.TrimEnd(' ')) == 0) && (oReader.NodeType == 1) && (currError > 0))
			{
				if (oReader.IsSimple == 0)
				{
					// CDATA
					currError = oReader.Read();
					if (oReader.NodeType == GXXMLReader.CDataType && currError > 0)
					{
						addNew(oReader.Value);
						currError = oReader.Read();
					}
				}
				else
					addNew(oReader.Value);
				currError = oReader.Read();
			}
			return currError;
		}
		public bool FromXmlFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages, string sName, string sNamespace)
		{
			if (GXUtil.CheckFile(file, Messages))
				return FromXml(file.ReadAllText(string.Empty), Messages, sName, sNamespace);
			else
				return false;
		}

		public virtual bool FromXml(string s)
		{
			return FromXml(s, "");
		}
		public virtual bool FromXml(string s, string sName)
		{
			return FromXml(s, null, sName, "");
		}
		public virtual bool FromXml(string s, GXBaseCollection<SdtMessages_Message> Messages, string sName, string sNamespace)
		{
			try
			{
				GXXMLReader xmlReader = new GXXMLReader();
				if (!string.IsNullOrEmpty(s))
				{
					base.Clear();
					xmlReader.OpenFromString(s);
					xmlReader.Read();
					xmlReader.RemoveWhiteSpaces = 0;
					readxml(xmlReader, sName);
					xmlReader.Close();
					if (xmlReader.ErrCode > 0)
					{
						GXUtil.ErrorToMessages(xmlReader.ErrCode.ToString(), xmlReader.ErrDescription, Messages);
						return false;
					}
					else
						return true;
				}
			}
			catch (Exception ex)
			{
				GXUtil.ErrorToMessages("FromXml Error", ex, Messages);
			}

			return false;
		}
		public string ToXml(string name)
		{
			return ToXml(name, "");
		}
		public string ToXml(string name, string sNameSpace)
		{
			return ToXml(false, name, sNameSpace);
		}
		public string ToXml(bool includeHeader, string name, string sNameSpace)
		{
			return ToXml(includeHeader, true, name, sNameSpace);
		}
		public virtual string ToXml(bool includeHeader, bool includeState, string name, string sNameSpace)
		{
			GXXMLWriter xmlWriter = new GXXMLWriter();
			xmlWriter.OpenToString();
			if (includeHeader)
				xmlWriter.WriteStartDocument("UTF-8");
			if (string.IsNullOrEmpty(sNameSpace))
				sNameSpace = "[*:nosend]";
			writexml(xmlWriter, name, sNameSpace, includeState);
			string s = xmlWriter.ResultingString;
			xmlWriter.Close();
			return s;
		}
		public virtual bool IsSimpleCollection()
		{
			return true;
		}
		private XMLPrefixes currentNamespacePrefixes = new XMLPrefixes();

		public void SetNamedPrefixesFromReader(GXXMLReader rdr)
		{
			currentNamespacePrefixes.SetNamedPrefixesFromReader(rdr);
		}
		public void SetPrefixesFromReader(GXXMLReader rdr)
		{
			currentNamespacePrefixes.SetPrefixesFromReader(rdr);
		}
		public Dictionary<string, string> GetPrefixesInContext()
		{
			return currentNamespacePrefixes.GetPrefixes();
		}
		public void SetPrefixes(Dictionary<string, string> pfxs, GXXMLReader reader)
		{
			currentNamespacePrefixes.SetPrefixes(new Dictionary<string, string>(pfxs));
			if (reader != null)
				SetPrefixesFromReader(reader);
		}
		public IList ExternalInstance
		{
			get
			{
				return (IList)this;
			}
			set
			{
				if (value != this)
				{
					base.Clear();
					foreach (object item in value)
						this.Add(item);
				}
			}
		}
		public virtual void Sort(string order)
		{
			this.Sort();
		}
		public string ToJavascriptSource(bool includeState)
		{
			return JSONHelper.WriteJSON<dynamic>(GetJSONObject(includeState));
		}
		public string ToJavascriptSource()
		{
			return ToJavascriptSource(true);
		}
		public void ToJSON(bool includeState)
		{
			jsonArr.Clear();
			for (int i = 0; i < this.Count; i++)
			{
				AddObjectProperty(this[i], includeState);
			}
		}
		public GxDictionary Difference(GxDictionary dic)
		{
			return null;
		}

		public GxDictionary ToDictionary()
		{
			GxDictionary dic = new GxDictionary();
			for (int i = 0; i < this.Count; i++)
			{
				GxStringCollection strColl = this[i] as GxStringCollection;
				dic[((string)strColl[0]).TrimEnd()] = strColl[1];
			}
			return dic;
		}

		public void ToJSON()
		{
			ToJSON(true);
		}
		public string ToJSonString()
		{
			return ToJSonString(true);
		}
		public string ToJSonString(bool includeState)
		{
			return ToJavascriptSource(includeState);
		}
		public bool FromJSonString(string s)
		{
			return FromJSonString(s, null);
		}
		public bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages)
		{
			_jsonArr = JSONHelper.ReadJSON<JArray>(s, Messages);
			bool result = _jsonArr != null;
			try
			{
				FromJSONObject(jsonArr);
				return result;
			}
			catch (Exception ex)
			{
				GXUtil.ErrorToMessages("FromJson Error", ex, Messages);
				return false;
			}
		}

		public bool FromJSonFile(GxFile file)
		{
			return FromJSonFile(file, null);
		}

		public bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages)
		{
			if (GXUtil.CheckFile(file, Messages))
				return FromJSonString(file.ReadAllText(string.Empty), Messages);
			else
				return false;
		}

		public void AddObjectProperty(string name, object prop)
		{
			throw new Exception("Method Not implemented");
		}
		public void AddObjectProperty(object prop)
		{
			AddObjectProperty(prop, true);
		}
		public void AddObjectProperty(object prop, bool includeState)
		{
			IGxJSONAble ijsonprop;
			JArray jarray = jsonArr as JArray;
			if ((ijsonprop = prop as IGxJSONAble) != null)
			{
				GxUserType bc = ijsonprop as GxUserType;
				if (bc != null)
					jarray.Add(bc.GetJSONObject(includeState));
				else
					jarray.Add(ijsonprop.GetJSONObject());
			}
			else if (prop is DateTime)
			{
				jarray.Add(DateTimeUtil.TToC2((DateTime)prop, false));
			}
			else
			{
				jarray.Add(prop);
			}
		}
		public Object GetJSONObject(bool includeState)
		{
			ToJSON(includeState);
			return jsonArr;
		}
		public Object GetJSONObject()
		{
			return GetJSONObject(true);
		}
		public virtual void FromJSONObject(dynamic obj)
		{
			base.Clear();
			JArray jobj = obj as JArray;
			if (jobj != null)
			{
				for (int i = 0; i < jobj.Length; i++)
				{
					addNew(jobj[i]);
				}
			}
		}
		public GxSimpleCollection<string> ToStringCollection(int digits, int decimals)
		{
			GxSimpleCollection<string> result = new GxSimpleCollection<string>();
			foreach (T item in this)
			{
				decimal value = (decimal)Convert.ChangeType(item, typeof(decimal));
				result.Add(StringUtil.LTrim(StringUtil.Str(value, digits, decimals)));
			}
			return result;
		}
		public void FromStringCollection(GxSimpleCollection<string> value)
		{
			foreach (string item in value)
			{
				Add(Convert.ChangeType(NumberUtil.Val(item.ToString()), typeof(T)));
			}
		}

	}
#if !NETCORE
	[Serializable]

	public class GxObjectCollectionBase : GxSimpleCollection<object>
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GxObjectCollectionBase));

		public string _containedName;
		public string _containedXmlNamespace;
		public string _containedType;
		public string _containedTypeNamespace;
		Type _containedObjType;
		public IGxContext context;

		public GxObjectCollectionBase() : base() { }

		public GxObjectCollectionBase(IGxContext context, string containedName, string containedXmlNamespace, string containedType, string containedTypeNamespace)
			: base()
		{
			this.context = context;
			_containedName = containedName;
			_containedXmlNamespace = containedXmlNamespace;
			_containedType = containedType;
			_containedTypeNamespace = containedTypeNamespace;

			if (_containedType.Length > 0)
			{
				if (_containedType.Equals("GxSilentTrnSdt", StringComparison.OrdinalIgnoreCase))
					_containedObjType = typeof(GxSilentTrnSdt);
				else
					_containedObjType = Assembly.GetCallingAssembly().GetType(_containedTypeNamespace + "." + _containedType);
				if (_containedObjType == null)
					throw new Exception("GxObjectCollectionBase error: Could not load type: " + _containedTypeNamespace + "." + _containedType);
			}
		}
		public override void writexml(GXXMLWriter oWriter, string sName, string sNameSpace, bool includeState)
		{
			writexmlcollection(oWriter, sName, sNameSpace, _containedName, _containedXmlNamespace, includeState);
		}
		public override void writexml(GXXMLWriter oWriter, string sName, string sNameSpace)
		{
			writexmlcollection(oWriter, sName, sNameSpace, _containedName, _containedXmlNamespace, true);
		}
		public override void writexmlcollection(GXXMLWriter oWriter, string sName, string sNamespace, string itemName, string itemNamespace)
		{
			writexmlcollection(oWriter, sName, sNamespace, itemName, itemNamespace, true);
		}
		public override void writexmlcollection(GXXMLWriter oWriter, string sName, string sNamespace, string itemName, string itemNamespace, bool includeState)
		{
			if (sName.Trim().Length != 0)
			{
				oWriter.WriteStartElement(sName);
				if (!sNamespace.StartsWith("[*:nosend]"))
					oWriter.WriteAttribute("xmlns", sNamespace);
				else
					sNamespace = sNamespace.Substring(10, sNamespace.Length - 10);
			}
			string itemName1 = GetContainedName();
			if (!string.IsNullOrEmpty(itemName) && itemName.Trim().Length > 0)
				itemName1 = itemName;
			foreach (IGxXMLSerializable obj in this)
				((IGxXMLSerializable)obj).writexml(oWriter, itemName1, itemNamespace, includeState);
			if (!string.IsNullOrEmpty(sName) && sName.Trim().Length != 0)
				oWriter.WriteEndElement();
		}
		public override short readxml(GXXMLReader oReader, string sName)
		{
			return readxmlcollection(oReader, sName, GetContainedName());
		}
		public override short readxmlcollection(GXXMLReader oReader, string sName, string itemName)
		{
			string itemName1 = GetContainedName();
			if (!string.IsNullOrEmpty(itemName) && itemName.Length > 0)
				itemName1 = itemName;
			short currError = 1;
			while (oReader.LocalName == itemName1 && currError > 0)
			{
				currError = AddObjectInstance(oReader);
				oReader.Read();
			}
			return currError;
		}
		public short AddObjectInstance(GXXMLReader oReader)
		{
			short currError;
			IGxXMLSerializable obj;

			obj = (IGxXMLSerializable)Activator.CreateInstance(_containedObjType, new object[] { context });

			Add(obj);   // sets properties that read method modifies (Mode, Modified).
			currError = obj.readxml(oReader, "");
			return currError;
		}
		public string GetContainedName()
		{
			return _containedName;
		}

		public string GetContainedXmlNamespace()
		{
			return _containedXmlNamespace;
		}
		public Type ContainedType
		{
			get { return _containedObjType; }
			set { _containedObjType = value; }
		}
		public override IGxXMLSerializable CurrentItem
		{
			get
			{
				if (currentItem == null)
				{
					GXLogging.Warn(log, "CurrentItem of type ", _containedObjType.ToString(), " is null");
					try
					{
						currentItem = (IGxXMLSerializable)Activator.CreateInstance(_containedObjType, new object[] { context });
					}
					catch (Exception e)
					{
						GXLogging.Warn(log, "Error creating CurrentItem", e);
					}
				}
				return (IGxXMLSerializable)currentItem;
			}
			set
			{
				base.CurrentItem = value;
			}
		}
		public override void Sort(string order)
		{

		}
		public override bool IsSimpleCollection()
		{
			return false;
		}

		public override void FromJSONObject(dynamic obj)
		{
			base.Clear();
			JArray jobj = obj as JArray;
			for (int i = 0; i < jobj.Length; i++)
			{
				IGxJSONAble obj1 = (IGxJSONAble)Activator.CreateInstance(_containedObjType, new object[] { context });
				obj1.FromJSONObject(jobj[i]);
				Add(obj1);
			}
		}
	}

#endif
	public class GxSDTComparer<T> : IComparer<T> where T : GxUserType, IGxXMLSerializable, IGxJSONAble, new()
	{
		ArrayList _keys;
		ArrayList _order;
		public GxSDTComparer(string order)
		{
			_keys = new ArrayList();
			_order = new ArrayList();
			string[] orders;
			orders = order.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string x in orders)
			{
				string key;
				string trimmed = x.Trim();
				if (trimmed.StartsWith("[") || trimmed.StartsWith("("))
				{ // Descending order
					key = x.Trim().Substring(1, x.Trim().Length - 2);
					_order.Add(-1);
				}
				else
				{
					key = x.Trim();
					_order.Add(1);
				}
				string[] splittedKey = key.Split('.');
				for (int i = 0; i < splittedKey.Length; i++)

					splittedKey[i] = "gxTpr_" + splittedKey[i].Trim().Substring(0, 1).ToUpper() + splittedKey[i].Trim().Substring(1, splittedKey[i].Trim().Length - 1).ToLower();
				_keys.Add(splittedKey);
			}
		}
		object getPropertyValue(object o, string[] ptys, int pos)
		{
			if (pos > ptys.Length - 1)
				return o;
			PropertyInfo p1 = o.GetType().GetProperty(ptys[pos]);
			return getPropertyValue(p1.GetValue(o, null), ptys, pos + 1);
		}
		public int Compare(T a, T b)
		{
			int result = 0;
			int i = 0;
			foreach (string[] ptys in _keys)
			{
				object value_a = getPropertyValue(a, ptys, 0);
				object value_b = getPropertyValue(b, ptys, 0);
				string svalue_a = value_a as string;
				string svalue_b = value_b as string;
				if (svalue_a != null && svalue_b != null)
					result = string.CompareOrdinal(svalue_a, svalue_b) * (int)(_order[i++]);
				else
					result = new CaseInsensitiveComparer().Compare(value_a, value_b) * (int)(_order[i++]);
				if (result != 0)
					break;
			}
			return result;
		}
	}
	public class GxItemStmt : GxCursorBase
	{
		public int cursorId;
	}
	public class GxItemCommand : GxCursorBase
	{
		public IDbCommand command;
	}

	public class GxCursorBase
	{
		public short opened;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class GxJsonName : Attribute
	{
		string _jsonName;
		public GxJsonName(String name)
		{
			_jsonName = name;
		}
		public string Name
		{
			get { return this._jsonName;  }
			set { this._jsonName = value; }
		}
	}

	[Serializable]
	[XmlType(IncludeInSchema = false)]
	public class GxUserType : IGxXMLSerializable, ICloneable, IGxJSONAble, IGxJSONSerializable
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GxUserType));
		protected GXProperties dirties = new GXProperties();

		static object setupChannelObject = null;
		static bool setupChannelInitialized;
		
		static void loadConfigurator()
		{
			if (GxUserType.setupChannelObject == null && !GxUserType.setupChannelInitialized)
			{
				GxUserType.setupChannelInitialized = true;
				string asmName = null;
				if (Config.GetValueOf("NativeChannelConfigurator", out asmName))
				{
					GxUserType.setupChannelObject = ClassLoader.FindInstance(asmName, null, asmName, null, null);
				}
			}
		}

		static public bool CustomChannelSetup(string name, GxLocation location, object serviceClient)
		{
			loadConfigurator();
			if (setupChannelObject != null)
			{
				ClassLoader.ExecuteVoidRef(setupChannelObject, "Setup", new Object[] { name, location, serviceClient });
				return true;
			}
			return false;
		}

		[NonSerialized]
		[XmlIgnore]
		public IGxContext context;

		public virtual void setContext(IGxContext context)
		{
			this.context = context;
		}
		[NonSerialized]
		[XmlIgnore]
		private JObject _jsonObj;
		JObject JsonObj
		{
			get
			{
				if (_jsonObj == null)
					_jsonObj = new JObject();
				return _jsonObj;
			}
		}				

		public GxUserType()
		{
		}

		public virtual void SetDirty(string fieldName)
		{
			dirties[fieldName.ToLower()] = "true";
		}
		public virtual bool IsDirty(string fieldName)
		{
			if (dirties.ContainsKey(fieldName.ToLower()))
				return true;
			return false;
		}
		public virtual void Copy(GxUserType source)
		{
			foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(source))
			{
				item.SetValue(this, item.GetValue(source));
			}
		}

		public static bool IsEqual(object thisObj, object soureObj)
		{
			IGxCollection thisColl = thisObj as IGxCollection;
			IGxCollection sourceColl = soureObj as IGxCollection;
			if (thisColl != null && sourceColl != null)
			{
				return IsEqual(thisColl, sourceColl);
			}
			GxUserType gxthisObj = thisObj as GxUserType;
			GxUserType gxsourceObj = soureObj as GxUserType;
			if (gxthisObj != null && gxsourceObj != null)
			{
				return IsEqual(gxthisObj, gxsourceObj);
			}
			return true;
		}
		private static bool IsEqual(IGxCollection thisColl, IGxCollection soureColl)
		{
			if (thisColl == null || soureColl == null)
				return true;
			if (thisColl.Count != soureColl.Count)
				return false;
			for (int i = 1; i <= thisColl.Count; i++)
			{
				GxUserType thisEl = thisColl.Item(i) as GxUserType;
				GxUserType sourceEl = soureColl.Item(i) as GxUserType;
				if (thisEl != null)
				{
					if (!thisEl.IsEqual(sourceEl))
						return false;
				}
				else
				{
					object thisObj = thisColl.Item(i);
					object sourceObj = soureColl.Item(i);
					if (thisObj != null && sourceObj != null && !thisObj.Equals(sourceObj))
						return false;
				}
			}
			return true;
		}

		private static bool IsEqual(GxUserType thisObj, GxUserType sourceObj)
		{
			return thisObj.IsEqual(sourceObj);
		}

		public bool IsEqual(GxUserType source)
		{
			try
			{
				if (source == null)
					return false;
				foreach (FieldInfo item in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
				{
					if (item.Name.StartsWith("gxTv_"))
					{
						GxUserType thisGxUserType = item.GetValue(this) as GxUserType;
						GxUserType sourceGxUserType = item.GetValue(source) as GxUserType;
						if (thisGxUserType != null)
						{
							if (!thisGxUserType.IsEqual(sourceGxUserType))
								return false;
						}
						else if (item.GetValue(this) as IGxCollection != null)
						{
							IGxCollection thisColl = item.GetValue(this) as IGxCollection;
							IGxCollection sourceColl = item.GetValue(source) as IGxCollection;
							if (!IsEqual(thisColl, sourceColl))
								return false;
						}
						else
						{
							object thisObj = item.GetValue(this);
							object sourceObj = item.GetValue(source);
							if (thisObj != null && sourceObj != null && !thisObj.Equals(sourceObj))
								return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, ex.ToString());
				return false;
			}
			return true;
		}
		internal static Dictionary<Type, GxStringCollection> StateAttributesTypeMap(Type sdtType)
		{
			Dictionary<Type, GxStringCollection> attrisToIgnore = new Dictionary<Type, GxStringCollection>();
			GxSilentTrnSdt sdt = (GxSilentTrnSdt)Activator.CreateInstance(sdtType);
			GxStringCollection stateAttrs = sdt.StateAttributes();
			attrisToIgnore.Add(sdtType, stateAttrs);

			IEnumerable<PropertyInfo> levels = sdtType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.PropertyType.IsGenericType && typeof(IGXBCCollection).IsAssignableFrom(x.PropertyType));
			foreach (PropertyInfo level in levels)
			{
				Type levelType = level.PropertyType.GetGenericArguments()[0];
				Dictionary<Type, GxStringCollection> levelAttrisToIgnore = StateAttributesTypeMap(levelType);
				foreach (var key in levelAttrisToIgnore.Keys)
					attrisToIgnore[key] = levelAttrisToIgnore[key];
			}
			return attrisToIgnore;
		}

		public virtual GxStringCollection StateAttributes()
		{
			return null;

		}
		public string ToJavascriptSource(bool includeState)
		{
			return ToJavascriptSource(includeState, true);
		}
		public string ToJavascriptSource(bool includeState, bool includeNoInitialized)
		{
			return JSONHelper.WriteJSON<dynamic>(GetJSONObject(includeState, includeNoInitialized));
		}
		public string ToJavascriptSource()
		{
			return GetJSONObject().ToString();
		}
		public string ToJSonString(bool includeState)
		{
			return ToJavascriptSource(includeState);
		}
		public string ToJSonString(bool includeState, bool includeNoInitialized)
		{
			return ToJavascriptSource(includeState, includeNoInitialized);
		}
		public string ToJSonString()
		{
			return ToJSonString(true);
		}
		public bool FromJSonString(string s)
		{
			return FromJSonString(s, null);
		}
		public bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages)
		{
			_jsonObj = JSONHelper.ReadJSON<JObject>(s, Messages);
			bool result = _jsonObj != null;
			try
			{
				FromJSONObject(_jsonObj);
				return result;
			}
			catch (Exception ex)
			{
				GXUtil.ErrorToMessages("FromJson Error", ex, Messages);
				return false;
			}
		}
		public virtual bool ShouldSerializeSdtJson()
		{
			return true;
		}
		public virtual bool SdtSerializeAsNull()
		{
			return false;
		}
		public virtual object Clone()
		{
			return base.MemberwiseClone();
		}
		public virtual short readxml(GXXMLReader oReader)
		{
			return readxml(oReader, "");
		}
		public virtual short readxml(GXXMLReader oReader, string sName)
		{
			try
			{
				string xml = UpdateNodeDefaultNamespace(oReader.ReadRawXML(), null, false, this.GetPrefixesInContext());
				FromXml(xml, sName, oReader.NamespaceURI);
				return 1;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "readxml error", ex);
				return -1;
			}
		}

		private XMLPrefixes currentNamespacePrefixes = new XMLPrefixes();

		public void SetNamedPrefixesFromReader(GXXMLReader rdr)
		{
			currentNamespacePrefixes.SetNamedPrefixesFromReader(rdr);
		}
		public void SetPrefixesFromReader(GXXMLReader rdr)
		{
			currentNamespacePrefixes.SetPrefixesFromReader(rdr);
		}
		public Dictionary<string, string> GetPrefixesInContext()
		{
			return currentNamespacePrefixes.GetPrefixes();
		}
		public void SetPrefixes(Dictionary<string, string> pfxs, GXXMLReader reader)
		{
			currentNamespacePrefixes.SetPrefixes(new Dictionary<string, string>(pfxs));
			if (reader != null)
				SetPrefixesFromReader(reader);
		}
		public static string UpdateNodeDefaultNamespace(string xml, string defaultNamespace, bool forceDefaultNamespace, Dictionary<string, string> prefixes)
		{
			if (!string.IsNullOrEmpty(xml))
			{
				int tagOpenIndex = xml.IndexOf('<');
				if (tagOpenIndex == -1)
				{
					return xml;
				}

				if (xml[tagOpenIndex + 1] == '/')
				{
					return xml;
				}

				int tagCloseIndex = xml.IndexOf('>', tagOpenIndex);
				if (tagCloseIndex > 0 && xml[tagCloseIndex - 1] == '/')
				{
					tagCloseIndex--;
				}

				if (!string.IsNullOrEmpty(defaultNamespace) || forceDefaultNamespace || prefixes.ContainsKey(""))
				{
					string currentTagSubstring = xml.Substring(tagOpenIndex, tagCloseIndex - tagOpenIndex);
					int nsIndex = currentTagSubstring.IndexOf("xmlns=");

					if (nsIndex == -1)
					{
						string contextNS = null;
						if (defaultNamespace == null)
						{
							if (prefixes != null && prefixes[""] != null)
							{

								contextNS = prefixes[""];
							}
						}
						else
						{
							if (string.IsNullOrEmpty(defaultNamespace) && forceDefaultNamespace)
							{

								contextNS = "";
							}
							else
							{

								contextNS = defaultNamespace;
							}
						}
						if (contextNS != null)
						{
							xml = xml.Insert(tagCloseIndex, string.Format(" xmlns=\"{0}\"", contextNS));
						}
					}
				}

				if (prefixes != null)
				{
					foreach (string k in prefixes.Keys)
					{
						if (!string.IsNullOrEmpty(k) && !xml.Contains(string.Format("xmlns:{0}=", k)))
							xml = xml.Insert(tagCloseIndex, string.Format(" xmlns:{0}=\"{1}\"", k, prefixes[k]));
					}
				}
			}
			return xml;
		}

		public virtual void writexml(GXXMLWriter oWriter, string sName)
		{
			writexml(oWriter, sName, string.Empty);
		}
		public virtual void writexml(GXXMLWriter oWriter, string sName, string sNameSpace)
		{
			writexml(oWriter, sName, sNameSpace, true);
		}
		public virtual void writexml(GXXMLWriter oWriter, string sName, string sNameSpace, bool includeState)
		{
			if (StringUtil.StrCmp(StringUtil.Left(sNameSpace, 10), "[*:nosend]") == 0)
			{
				sNameSpace = StringUtil.Right(sNameSpace, (short)(StringUtil.Len(sNameSpace) - 10));
			}
			string xml = ToXml(false, includeState, sName, sNameSpace);
			if (string.IsNullOrWhiteSpace(sNameSpace))
			{
				// Force the namespace when it is "".
				xml = UpdateNodeDefaultNamespace(xml, "", true, null);
			}

			oWriter.WriteRawText(xml);
		}

		public bool FromXmlFile(GxFile file)
		{
			if (file != null && file.Exists())
			{
				string s = file.ReadAllText(string.Empty);
				return FromXml(s);
			}
			return false;
		}

		public virtual bool FromXml(string s)
		{
			return FromXml(s, "");
		}
		public virtual bool FromXml(string s, string sName)
		{
			return FromXml(s, sName, "");
		}
		public virtual bool FromXml(string s, string sName, string sNamespace)
		{
			return FromXml(s, null, sName, sNamespace);
		}
		public virtual bool FromXml(string s, GXBaseCollection<SdtMessages_Message> Messages, string sName, string sNamespace)
		{
			if (string.IsNullOrEmpty(s))
				return false;
			try
			{
				if (string.IsNullOrEmpty(sName))
				{
					sName = XmlNameAttribute(GetType());
					sNamespace = XmlNameSpaceAttribute(GetType());
				}

				GxUserType deserialized = GXXmlSerializer.Deserialize<GxUserType>(this.GetType(), s, sName, sNamespace, out List<string> serializationErrors);
				GXXmlSerializer.SetSoapError(context, serializationErrors);
				deserialized.context = this.context;
				Copy(deserialized);
				return true;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "FromXml error:" + this.GetType().ToString(), ex);
				GXXmlSerializer.SetSoapError(context, string.Format("Error reading {0}", this.GetType()));
				Exception innerEx = ex;
				while (innerEx != null)
				{
					GXXmlSerializer.SetSoapError(context, string.Format(" -> '{0}'", innerEx.Message));
					innerEx = innerEx.InnerException;
				}
				GXUtil.ErrorToMessages("FromXML Error", ex, Messages);
				return false;
			}

		}
		static void CopyProperties(GxUserType dest, GxUserType src)
		{
			foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(src))
			{
				item.SetValue(dest, item.GetValue(src));
			}
		}
		public virtual string ToXml(string name)
		{
			return ToXml(name, "");
		}
		public virtual string ToXml(string name, string sNameSpace)
		{
			return ToXml(false, name, sNameSpace);
		}
		internal static string XmlNameAttribute(Type sdtType)
		{
			XmlTypeAttribute typeAtt = sdtType.GetCustomAttributes<XmlTypeAttribute>().FirstOrDefault();
			if (typeAtt != null)
				return typeAtt.TypeName;
			else
				return string.Empty;
		}
		internal static string XmlNameSpaceAttribute(Type sdtType)
		{
			XmlTypeAttribute typeAtt = sdtType.GetCustomAttributes<XmlTypeAttribute>().FirstOrDefault();
			if (typeAtt != null)
				return typeAtt.Namespace;
			else
				return string.Empty;
		}
		public virtual string ToXml(bool includeHeader, bool includeState, string rootName, string sNameSpace)
		{
			if (string.IsNullOrEmpty(rootName))
				rootName = XmlNameAttribute(GetType());
			if (string.IsNullOrEmpty(sNameSpace))
				sNameSpace = XmlNameSpaceAttribute(GetType());

			XmlAttributeOverrides ov = null;

			if (!includeState)
			{
				ov = GXXmlSerializer.IgnoredAttributes(GxUserType.StateAttributesTypeMap(this.GetType()));
			}
			return GXXmlSerializer.Serialize(rootName, sNameSpace, ov, includeHeader, this);

		}
		public virtual string ToXml(bool includeHeader, string name, string sNameSpace)
		{
			return ToXml(includeHeader, true, name, sNameSpace);
		}
		public virtual void ToJSON()
		{
		}
		public virtual void ToJSON(bool includeState)
		{
			ToJSON();
		}
		public virtual void ToJSON(bool includeState, bool includeNoInitialized)
		{
			ToJSON();
		}

		public virtual string JsonMap(string value)
		{
			return null;
		}

		public void AddObjectProperty(string name, object prop)
		{
			AddObjectProperty(name, prop, true);
		}
		public void AddObjectProperty(string name, object prop, bool includeState)
		{
			AddObjectProperty(name, prop, includeState, true);
		}
		public void AddObjectProperty(string name, object prop, bool includeState, bool includeNonInitialized)
		{
			IGxJSONAble ijsonProp = prop as IGxJSONAble;
			if (ijsonProp != null)
			{
				GxSilentTrnSdt silentTrn = prop as GxSilentTrnSdt;
				if (silentTrn != null)
					silentTrn.GetJSONObject(includeState, includeNonInitialized);
				else
					JsonObj.Put(name, ijsonProp.GetJSONObject(includeState));
			}
			else
			{
				if (this is GxSilentTrnSdt)
				{
					if (includeNonInitialized || (!includeNonInitialized && IsDirty(name)))
					{
						JsonObj.Put(name, prop);
					}
				}
				else
				{
					JsonObj.Put(name, prop);
				}
			}
		}
		public Object GetJSONObject(bool includeState, bool includeNoInitialized)
		{
			JsonObj.Clear();
			ToJSON(includeState, includeNoInitialized);
			return JsonObj;
		}
		public Object GetJSONObject(bool includeState)
		{
			JsonObj.Clear();
			ToJSON(includeState);
			return JsonObj;
		}
		public Object GetJSONObject()
		{
			ToJSON();
			return JsonObj;
		}

		private ICollection getFromJSONObjectOrderIterator(ICollection it)
		{
			List<string> v = new List<string>();
			List<string> vAtEnd = new List<string>();
			foreach (string name in it)
			{
				string map = JsonMap(name);
				PropertyInfo objProperty = GetTypeProperty("gxtpr_" + (!string.IsNullOrEmpty(map) ? map : name).ToLower());
				if (name.EndsWith("_N") || objProperty != null && IsGxUploadAttribute(objProperty))
				{
					vAtEnd.Add(name);
				}
				else
				{
					v.Add(name);//keep the order of attributes that do not end with _N.
				}
			}
			if (vAtEnd.Count > 0)
				v.AddRange(vAtEnd);
			return v;
		}

		public void FromJSONObject(dynamic obj)
		{

			JObject jobj = obj as JObject;
			if (jobj == null)
				return;
			ICollection jsonIterator = getFromJSONObjectOrderIterator(jobj.Names);
			foreach (string name in jsonIterator)
			{
				object currObj = jobj[name];
				string map = JsonMap(name);
				PropertyInfo objProperty = GetTypeProperty("gxtpr_" + (map != null ? map : name).ToLower());

				if (objProperty != null)
				{
					if (!JSONHelper.IsJsonNull(currObj))
					{
						object newValue;
						if (TryConvertValueToProperty(currObj, objProperty, out newValue))
						{
							if (IsGxUploadAttribute(objProperty))
							{
								string uploadPath = (string)newValue;
								if (uploadPath.StartsWith(GXFormData.FORMDATA_REFERENCE))
								{

									string sVar = uploadPath.Replace(GXFormData.FORMDATA_REFERENCE, string.Empty);
									MethodInfo setBlob = GetMethodInfo("gxtv_" + GetType().Name + "_" + name + "_setblob");
									if (setBlob != null)
									{
										if (HttpHelper.GetHttpRequestPostedFile(context, sVar, out uploadPath))
										{
											string fileName = HttpHelper.GetHttpRequestPostedFileName(this.context.HttpContext, sVar);
											string fileType = HttpHelper.GetHttpRequestPostedFileType(this.context.HttpContext, sVar);
											string[] parms = { uploadPath, fileName, fileType };
											setBlob.Invoke(this, parms);
										}
									}
								}
								else if (GxUploadHelper.IsUpload(uploadPath)) //File upload from SD
								{
									objProperty.SetValue(this, uploadPath, null);
									PropertyInfo info_gxi = this.GetType().GetProperty(objProperty.Name + "_gxi");//gxi reset
									if (info_gxi != null)
										info_gxi.SetValue(this, string.Empty, null);
								}
								else if (!jobj.Contains(name + "_GXI") && !string.IsNullOrEmpty(uploadPath)) //If uploadPath is empty it is ignored (offline sync does not send unchanged images)
								{
									objProperty.SetValue(this, uploadPath, null);
									PropertyInfo info_gxi = this.GetType().GetProperty(objProperty.Name + "_gxi");//gxi reset
									if (info_gxi != null)
										info_gxi.SetValue(this, string.Empty, null);
								}
								else if (PathUtil.IsAbsoluteUrl(uploadPath)) //External url from SD
								{
									PropertyInfo info_gxi = this.GetType().GetProperty(objProperty.Name + "_gxi");
									if (info_gxi != null)
										info_gxi.SetValue(this, uploadPath, null);
									objProperty.SetValue(this, string.Empty, null);
								}
								else
								{
									objProperty.SetValue(this, uploadPath, null);
								}
							}
							else
							{
								objProperty.SetValue(this, newValue, null);
							}
						}
						else
						{
							object currProp = objProperty.GetValue(this, null);
							IGXBCCollection bcColl;
							GxSimpleCollection<object> currSimpleColl;
							IGxJSONAble currJsonProp;
							CollectionBase currObjColl = currObj as CollectionBase;
#if !NETCORE
							GxObjectCollectionBase currColl;
							if ((currColl = currProp as GxObjectCollectionBase) != null)
							{
								currColl.ClearCollection();
								if (currObjColl == null) //arays with 1 item often send the item itself and not the whole array
								{
									object collItem = currColl.ContainedType.GetConstructor(new Type[] { typeof(IGxContext) }).Invoke(new object[] { currColl.context });
									((IGxJSONAble)collItem).FromJSONObject(currObj);
									currColl.Add(collItem);
								}
								else
								{
									foreach (object item in currObjColl)
									{
										object collItem = currColl.ContainedType.GetConstructor(new Type[] { typeof(IGxContext) }).Invoke(new object[] { currColl.context });
										((IGxJSONAble)collItem).FromJSONObject(item);
										currColl.Add(collItem);
									}
								}
							}
							else if ((bcColl = currProp as IGXBCCollection) != null && currProp.GetType().IsGenericType)
#else
							if ((bcColl = currProp as IGXBCCollection) != null && currProp.GetType().GetTypeInfo().IsGenericType)
#endif
							{
								bcColl.ClearCollection();
								Type BCType = currProp.GetType().GetGenericArguments()[0];
								if (currObjColl != null)
								{
									foreach (object item in currObjColl)
									{
										object bcItem = BCType.GetConstructor(new Type[] { typeof(IGxContext) }).Invoke(new object[] { this.context });
										((IGxJSONAble)bcItem).FromJSONObject(item);
										bcColl.BaseAdd(bcItem); //BaseAdd doesn´t change mode in BCs levels.
									}
								}
							}
							else if ((currSimpleColl = currProp as GxSimpleCollection<object>) != null)
							{
								currSimpleColl.ClearCollection();
								foreach (object item in currObjColl)
								{
									currSimpleColl.Add(item);
								}
							}
							else if ((currJsonProp = currProp as IGxJSONAble) != null)
							{
								currJsonProp.FromJSONObject(currObj);
							}
							else if (objProperty.PropertyType == typeof(string) && !(currObj is String))
							{
								objProperty.SetValue(this, currObj.ToString(), null);
							}
							else
							{
								objProperty.SetValue(this, currObj, null);
							}
						}
					}
#if NETCORE
					else if (objProperty.PropertyType.GetTypeInfo().IsSubclassOf(typeof(GxUserType)))
#else
					else if (objProperty.PropertyType.IsSubclassOf(typeof(GxUserType)))
#endif
					{
						objProperty.GetValue(this, null);
					}
				}
			}

		}
		private bool TryConvertValueToProperty(object Value, PropertyInfo property, out object convertedValue)
		{
			Type ptyType = property.PropertyType;
			Type valueType = Value.GetType();
			bool success = true;
			convertedValue = Value;
			bool valueTypeString = false;

			if (Value is JArray)
			{
				return false;
			}
			else if (ptyType == valueType || ptyType.IsAssignableFrom(valueType))
			{
				return true;
			}
			else
			{
				string sValue = Value as string;

				if (Value is Boolean)
					Value = bool.Parse(Value.ToString()) ? "1" : "0";
				else
					valueTypeString = (valueType == typeof(string));

				if (ptyType.Equals(typeof(string)))
				{
					convertedValue = Value.ToString();
				}
				else if (ptyType.Equals(typeof(Int16)))
				{
					if (valueTypeString)
					{
						short result;
						Int16.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
						convertedValue = result;
					}
					else
						convertedValue = Convert.ToInt16(Value);
				}
				else if (ptyType.Equals(typeof(Int32)))
				{
					if (valueTypeString)
					{
						int result;
						Int32.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
						convertedValue = result;
					}
					else
						convertedValue = Convert.ToInt32(Value);
				}
				else if (ptyType.Equals(typeof(Int64)))
				{
					if (valueTypeString)
					{
						long result;
						Int64.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
						convertedValue = result;
					}
					else
						convertedValue = Convert.ToInt64(Value);
				}
				else if (ptyType.Equals(typeof(DateTime)))
				{
					DateTime dtValue = DateTimeUtil.nullDate;
					DateTime.TryParse(Value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dtValue);
					convertedValue = dtValue;
				}
				else if (ptyType.Equals(typeof(bool)))
				{
					convertedValue = bool.Parse(Value.ToString());
				}
				else if (ptyType.Equals(typeof(decimal)))
				{
					if (valueTypeString)
					{
						Decimal result;
						Decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
						convertedValue = result;
					}
					else
						convertedValue = Convert.ToDecimal(Value);
				}
				else if (ptyType.Equals(typeof(double)))
				{
					if (valueTypeString)
					{
						Double result;
						Double.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
						convertedValue = result;
					}
					else
						convertedValue = Convert.ToDouble(Value);
				}
				else if (ptyType.Equals(typeof(float)))
				{
					if (valueTypeString)
					{
						float result;
						float.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
						convertedValue = result;
					}
					else
						convertedValue = float.Parse(Convert.ToString(Value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
				}
				else if (ptyType.Equals(typeof(Guid)))
				{
					convertedValue = new Guid(Value.ToString());
				}
				else if (ptyType.Equals(typeof(GeneXus.Utils.Geospatial)))
				{
					convertedValue = new Geospatial(Value.ToString());
				}
				else
				{
					success = false;
				}
			}
			return success;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		private Dictionary<string, bool> gxuploadAttrs = new Dictionary<string, bool>();
		private bool IsGxUploadAttribute(PropertyInfo property)
		{
			string key = property.Name;
			if (!gxuploadAttrs.ContainsKey(key))
			{
				bool hasAtt = property.IsDefined(typeof(GxUpload), false);
				gxuploadAttrs.Add(key, hasAtt);
			}
			return gxuploadAttrs[key];
		}

		private Hashtable props;

		private PropertyInfo GetTypeProperty(string propName)
		{
			if (props == null)
			{
				props = new Hashtable();
				foreach (PropertyInfo prop in this.GetType().GetProperties())
				{
					props.Add(prop.Name.ToLower(), prop);
				}
			}
			return (PropertyInfo)props[propName];
		}

		private Hashtable methods;

		private MethodInfo GetMethodInfo(string methodName)
		{
			if (methods == null)
			{
				methods = new Hashtable();
				foreach (MethodInfo method in this.GetType().GetMethods())
				{
					methods[method.Name.ToLower()] = method;
				}
			}
			return (MethodInfo)methods[methodName.ToLower()];
		}
		static public XmlElement[] XmlToElements(string s)
		{
			List<XmlElement> elements = new List<XmlElement>();
			if (!string.IsNullOrEmpty(s))
			{
				try
				{
					string validXml = "<rootNode>" + s + "</rootNode>";
					System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
					xmlDoc.InnerXml = validXml;
					foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
					{
						XmlElement elm = node as XmlElement;
						if (elm != null)
							elements.Add(elm);
					}
					elements.ToArray();
				}
				catch (Exception ex)
				{
					// Invalid xml: it sends it as text
					string validXml = "<content><![CDATA[" + ex.Message + "]]></content>";
					GXLogging.Warn(log, "InvalidXML content " + s, ex);
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.InnerXml = validXml;
					foreach (XmlNode node in xmlDoc.ChildNodes)
					{
						XmlElement elm = node as XmlElement;
						if (elm != null)
							elements.Add(elm);
					}
					elements.ToArray();
				}
			}
			return elements.ToArray();
		}
		public bool FromJSonFile(GxFile file)
		{
			return FromJSonFile(file, null);
		}

		public bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages = null)
		{
			if (GXUtil.CheckFile(file, Messages))
				return FromJSonString(file.ReadAllText(string.Empty), Messages);
			else
				return false;
		}

		public bool FromXmlFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages, string sName, string sNamespace)
		{
			if (GXUtil.CheckFile(file, Messages))
				return FromXml(file.ReadAllText(string.Empty), Messages, sName, sNamespace);
			else
				return false;
		}

	}

	public interface IGxJSONAble
	{
		void AddObjectProperty(string name, object prop);
		Object GetJSONObject();
		Object GetJSONObject(bool includeState);
		void FromJSONObject(dynamic obj);
		string ToJavascriptSource();
	}
	public interface IGxJSONSerializable
	{
		string ToJSonString();
		bool FromJSonString(string s);
		bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages);
		bool FromJSonFile(GxFile file);
		bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages);
	}
	public interface IGxXMLSerializable
	{
		short readxml(GXXMLReader oReader);
		short readxml(GXXMLReader oReader, string sName);
		void writexml(GXXMLWriter oWriter, string sName);
		void writexml(GXXMLWriter oWriter, string sName, string sNameSpace);
		void writexml(GXXMLWriter oWriter, string sName, string sNameSpace, bool includeState);
		string ToXml(string name);
		string ToXml(string name, string sNameSpace);
		string ToXml(bool includeHeader, bool includeState, string name, string sNameSpace);
		string ToXml(bool includeHeader, string name, string sNameSpace);
		bool FromXml(string s);
		bool FromXml(string s, string sName);
		bool FromXml(string s, GXBaseCollection<SdtMessages_Message> Messages, string sName, string sNameSpace);
		bool FromXmlFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages, string name, string sNameSpace);
	}

	[Serializable]
	public class GxArrayList
	{
		List<object[]> innerArray;
		public GxArrayList(int capacity)
		{
			innerArray = new List<object[]>(capacity);
		}
		public GxArrayList()
		{
			innerArray = new List<object[]>();
		}
		public List<object[]> InnerArray
		{
			get { return innerArray; }
			set { innerArray = value; }
		}
		public int Count
		{
			get { return innerArray.Count; }
		}

		public void Add(object[] item)
		{
			innerArray.Add(item);
		}
		public int Size
		{
			get
			{
				if (innerArray.Count > 0)
					return innerArray.Count * innerArray[0].Length;
				else return 0;
			}
		}
		public object Item(int index, int i)
		{
			return innerArray[index][i];
		}
	}

	[CollectionDataContract(Name = "GxUnknownObjectCollection")]
	[KnownType(typeof(GxSimpleCollection<object>))]
	[KnownType(typeof(GxStringCollection))]
	public class GxUnknownObjectCollection : GxSimpleCollection<object>
	{
		public override void writexmlcollection(GXXMLWriter oWriter, string sName, string sNameSpace, string itemName, string itemNamespace)
		{
			oWriter.WriteStartElement(sName);
			foreach (IGxXMLSerializable obj in this)
				((IGxXMLSerializable)obj).writexml(oWriter, "", "");
			oWriter.WriteEndElement();
		}
		public override short readxmlcollection(GXXMLReader oReader, string sName, string itemName)
		{
			return 0;
		}
	}
#if !NETCORE
	public class GXCData : Object, IXmlSerializable
#else
	public class GXCData : Object
#endif
	{
		public string content;

		public GXCData()
		{
			content = "";
		}
		public GXCData(string s)
		{
			content = s;
		}
#if !NETCORE
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return new System.Xml.Schema.XmlSchema();
		}
		public void ReadXml(System.Xml.XmlReader reader)
		{
			content = reader.ReadElementString();
		}
		public void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteCData(content);
		}
#endif
	}
#if !NETCORE
	public class GXXmlRaw : Object, IXmlSerializable
#else
	public class GXXmlRaw : Object
#endif
	{
		public string content;

		public GXXmlRaw()
		{
			content = "";
		}
		public GXXmlRaw(string s)
		{
			content = s;
		}
#if !NETCORE
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return new System.Xml.Schema.XmlSchema();
		}
		public void ReadXml(System.Xml.XmlReader reader)
		{
			content = reader.ReadOuterXml();
		}
		public void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteRaw(content);
		}
#endif
	}

	public class GXProperties : NameObjectCollectionBase, IGxJSONSerializable, IGxJSONAble
	{
		int current;
		bool eof;
		private object syncObj = new object();

		public GXProperties()
		{
		}
		public string this[int pos]
		{
			get
			{
				return (string)(this.BaseGet(pos));
			}
			set
			{
				lock (syncObj)
				{
					this.BaseSet(pos, value);
				}
			}
		}
		public string this[string key]
		{
			get
			{
				return (string)(this.BaseGet(key));
			}
			set
			{
				lock (syncObj)
				{
					this.BaseSet(key, value);
				}
			}
		}
		public string GetKey(int i)
		{
			return (string)(this.BaseGetKey(i));
		}
		public string Get(string key)
		{
			return (string)(this.BaseGet(key));
		}
		public void Add(string key, string value)
		{
			lock (syncObj)
			{
				this.BaseAdd(key, value);
			}
		}
		public void Set(string key, string value)
		{
			lock (syncObj)
			{
				this.BaseSet(key, value);
			}
		}
		public void Remove(string key)
		{
			lock (syncObj)
			{
				this.BaseRemove(key);
			}
		}
		public void Remove(int index)
		{
			lock (syncObj)
			{
				this.BaseRemoveAt(index);
			}
		}
		public void Clear()
		{
			lock (syncObj)
			{
				this.BaseClear();
			}
		}
		public GxKeyValuePair GetFirst()
		{
			current = 0;
			return getKeyValuePair(0);
		}
		public GxKeyValuePair GetNext()
		{
			current++;
			return getKeyValuePair(current);
		}
		public bool Eof()
		{
			return eof;
		}
		GxKeyValuePair getKeyValuePair(int i)
		{
			if (i < this.Count)
			{
				eof = false;
				return new GxKeyValuePair(this.BaseGetKey(i), (string)(this.BaseGet(i)));
			}
			else
			{
				eof = true;
				return null;
			}
		}
		public bool ContainsKey(string key)
		{
			if (this.BaseGet(key) == null)
				return false;
			return true;
		}

		public string ToJSonString()
		{
			JObject jObj = new JObject();
			foreach (object item in this)
			{

				jObj.Accumulate(item.ToString(), this.Get(item.ToString()));
			}
			return jObj.ToString();
		}
		public bool FromJSonString(string s)
		{
			return FromJSonString(s, null);
		}

		public bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages)
		{
			this.Clear();
			JObject jObj = JSONHelper.ReadJSON<JObject>(s, Messages);
			if (jObj != null)
			{
				foreach (string name in jObj.Names) //.Names keeps the original order
				{
					lock (syncObj)
					{
						this.Set(name, JSONHelper.WriteJSON<dynamic>(jObj[name]));
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool FromJSonFile(GxFile file)
		{
			return FromJSonFile(file, null);
		}

		public bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages)
		{
			if (GXUtil.CheckFile(file, Messages))
				return FromJSonString(file.ReadAllText(string.Empty), Messages);
			else
				return false;
		}

		public void AddObjectProperty(string name, object prop)
		{
			throw new NotImplementedException();
		}

		public object GetJSONObject()
		{
			JObject jObj = new JObject();
			foreach (object item in this)
			{

				jObj.Accumulate(item.ToString(), this.Get(item.ToString()));
			}
			return jObj;
		}

		public object GetJSONObject(bool includeState)
		{
			return GetJSONObject();
		}

		public void FromJSONObject(dynamic obj)
		{
			this.Clear();
			JObject jObj = obj as JObject;
			if (jObj != null)
			{
				foreach (DictionaryEntry item in jObj)
				{
					lock (syncObj)
					{
						this.Set(item.Key.ToString(), item.Value.ToString());
					}
				}
			}
		}
		public override string ToString()
		{
			StringBuilder values = new StringBuilder();
			foreach (object item in this)
			{
				values.Append(this[item.ToString()]);
			}
			return values.ToString();
		}
		public string ToJavascriptSource()
		{
			throw new NotImplementedException();
		}
	}

	public class GxKeyValuePair
	{
		string _key;
		string _value;

		public GxKeyValuePair()
		{
			_key = "";
			_value = "";
		}
		public GxKeyValuePair(string key, string value)
		{
			_key = key;
			_value = value;
		}
		public string Key
		{
			get { return _key; }
			set { _key = value; }
		}
		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}
	}
	public interface IGxCollection : IGxXMLSerializable
	{
		int Add(object value);
		void Add(object value, int idx);
		int Count { get; }
		void Clear();
		void ClearCollection();
		Object Clone();
		void RemoveItem(int idx);
		void RemoveElement(int idx);
		string GetString(int idx);
		DateTime GetDatetime(int idx);
		Geospatial GetGeospatial(int idx);
		object Item(int idx);
		void addNew(object s);
		int IndexOf(object value);
		IGxXMLSerializable CurrentItem { get; set; }
		void writexmlcollection(GXXMLWriter oWriter, string sName, string sNamespace, string itemName, string itemNamespace);
		short readxmlcollection(GXXMLReader oReader, string sName, string itemName);
		IList ExternalInstance { get; set; }
		string ToJSonString();
		string ToJSonString(bool includeState);
		bool FromJSonString(string s);
		bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages);
		bool FromJSonFile(GxFile file);
		bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages);
		void Sort(string order);
		void AddObjectProperty(string name, object prop);

	}
	public interface IGxCollection<T> : IGxCollection
	{
		T GetNumeric(int idx);
	}

	public interface IGxExternalObject
	{
		object ExternalInstance { get; set; }
	}
	public class GXExternalCollection<T> : IGxCollection where T : IGxExternalObject
	{
		IList instance;

		public string _containedType;
		public string _containedTypeNamespace;
		Type _containedObjType;
		public IGxContext context;

		public GXExternalCollection()
		{

		}
		public GXExternalCollection(IGxContext context, string containedType, string containedTypeNamespace)
		{
			this.context = context;
			_containedType = containedType;
			_containedTypeNamespace = containedTypeNamespace;

			if (_containedType.Length > 0)
			{
				_containedObjType = typeof(T);
			}
			instance = new ArrayList();
		}
		public IList ExternalInstance
		{
			get { return instance; }
			set { instance = value; }
		}
		public int Add(object value)
		{
			IGxExternalObject x = value as IGxExternalObject;
			if (x != null)
				return instance.Add(x.ExternalInstance);
			else
				return instance.Add(value);
		}
		public void Add(Object o, int idx)
		{
			object exoValue;
			IGxExternalObject x = o as IGxExternalObject;
			if (x != null)
				exoValue = x.ExternalInstance;
			else
				exoValue = o;
			if (idx == 0)
				instance.Add(exoValue);
			else
				instance.Insert(idx - 1, exoValue);
		}
		public int Count
		{
			get { return instance.Count; }
		}
		public object Clone()
		{
			if (_containedType != null)
				return new GXExternalCollection<T>(context, _containedType, _containedTypeNamespace);
			return new GXExternalCollection<T>();
		}
		public void Clear()
		{
			instance.Clear();
		}
		public void ClearCollection()
		{
			instance.Clear();
		}
		public void RemoveItem(int idx)
		{
			instance.RemoveAt(idx - 1);
		}
		public void RemoveElement(int idx)
		{
			instance.RemoveAt(idx - 1);
		}
		public string GetString(int i)
		{
			return Convert.ToString(instance[i - 1]);
		}
		public double GetNumeric(int i)
		{
			return Convert.ToDouble(instance[i - 1]);
		}
		public DateTime GetDatetime(int i)
		{
			return Convert.ToDateTime(instance[i - 1]);
		}
		public Geospatial GetGeospatial(int i)
		{
			return new Geospatial(instance[i - 1]);
		}
		public object Item(int idx)
		{
			if (_containedObjType != null)
			{

				IGxExternalObject o = (IGxExternalObject)Activator.CreateInstance(_containedObjType, new object[] { context });
				o.ExternalInstance = instance[idx - 1];
				return o;
			}
			else
			{

				return instance[idx - 1];
			}
		}
		public void addNew(object o)
		{
			Add(o);
		}
		public int IndexOf(object value)
		{
			IGxExternalObject x = value as IGxExternalObject;
			if (x != null)
				return instance.IndexOf(x.ExternalInstance) + 1;
			else
				return instance.IndexOf(value) + 1;
		}
		public string ToXml(string name)
		{
			return string.Empty;
		}
		public string ToXml(string name, string sNameSpace)
		{
			return string.Empty;
		}
		public string ToXml(bool includeHeader, bool includeState, string name, string sNameSpace)
		{
			return string.Empty;
		}
		public string ToXml(bool includeHeader, string name, string sNameSpace)
		{
			return string.Empty;
		}
		protected object currentItem;
		public IGxXMLSerializable CurrentItem
		{
			get
			{
				return (IGxXMLSerializable)currentItem;
			}
			set
			{
				currentItem = value;
			}
		}
		public bool FromXmlFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages, string sName, string sNameSpace)
		{
			return false;
		}
		public bool FromXml(string s)
		{
			return false;
		}
		public bool FromXml(string s, string sName)
		{
			return false;
		}
		public bool FromXml(string s, GXBaseCollection<SdtMessages_Message> Messages, string sName, string sNamespace)
		{
			return false;
		}
		public void Sort(string order)
		{
		}
		public void AddObjectProperty(string name, object prop)
		{
		}
		public string ToJSonString()
		{
			return "";
		}
		public string ToJSonString(bool includeState)
		{
			return "";
		}
		public bool FromJSonString(string s)
		{
			return false;
		}
		public bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages)
		{
			return false;
		}
		public bool FromJSonFile(GxFile file)
		{
			return false;
		}
		public bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages)
		{
			return false;
		}
		public void writexml(GXXMLWriter oWriter, string sName)
		{
		}
		public void writexml(GXXMLWriter oWriter, string sName, string sNamespace)
		{
		}
		public void writexml(GXXMLWriter oWriter, string sName, string sNamespace, bool includeState)
		{
		}
		public void writexml(GXXMLWriter oWriter)
		{
		}
		public void writexmlcollection(GXXMLWriter oWriter, string sName, string sNamespace, string itemName, string itemNamespace)
		{
		}
		public short readxml(GXXMLReader oReader, string sName)
		{
			return 0;
		}
		public short readxml(GXXMLReader oReader)
		{
			return 0;
		}
		public short readxmlcollection(GXXMLReader oReader, string sName, string itemName)
		{
			return 0;
		}
	}
	public class CollectionUtils
	{
		static ConcurrentDictionary<Type, IGxCollectionConverter> convertFuncts = new ConcurrentDictionary<Type, IGxCollectionConverter>();
		static public void AddConverter(Type t, IGxCollectionConverter c)
		{
			convertFuncts[t] = c;
		}

		static public object ConvertToInternal(Type to, Object i)
		{
			object o;
			Type ienumerableType = GetEnumerableType(to);
			if (convertFuncts.ContainsKey(to))
			{
				o = convertFuncts[to].ConvertToList(i);
			}
			else if (ienumerableType != null)
			{
				IList lst = (IList)Activator.CreateInstance((typeof(List<>).MakeGenericType(ienumerableType)));
				foreach (object item in i as IEnumerable)
					lst.Add(item);
				o = lst;
			}
			else if (to.IsInstanceOfType(i))
				o = i;
			else
			{
				IList l = (IList)Activator.CreateInstance(to);
				foreach (object obj in (IList)i)
					l.Add(obj);
				o = l;
			}
			return o;
		}

		static Type GetEnumerableType(Type type)
		{
#if !NETCORE
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				return type.GetGenericArguments()[0];
#endif
			return null;
		}

		static public object ConvertToExternal(Type to, Object i)
		{
			object o;
			if (convertFuncts.ContainsKey(to))
			{
				o = convertFuncts[to].ConvertFromList(i);
			}
			else if (to.IsInstanceOfType(i))
				o = i;
			else
			{
				IList l = (IList)Activator.CreateInstance(to);
				Type itemType;
				IList inputList = (IList)i;
				if (to.IsGenericType && to.GetGenericArguments() != null)
				{
					itemType = to.GetGenericArguments()[0];
					foreach (object obj in inputList)
						l.Add(Convert.ChangeType(obj, itemType));
				}
				else
				{
					foreach (object obj in inputList)
						l.Add(obj);
				}
				o = l;
			}
			return o;
		}
	}
	public interface IGxCollectionConverter
	{
		object ConvertToList(object i);
		object ConvertFromList(object i);
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class ObjectCollectionAttribute : System.Attribute
	{
		private Type itemType;

		public Type ItemType
		{
			get
			{
				return itemType;
			}
			set
			{

				itemType = value;
			}
		}
	}
	public class GxDatetimeString
	{
		static public string NullValue = "0000-00-00T00:00:00";
		static public string NullValueMs = "0000-00-00T00:00:00.000";

		string m_value;
		protected virtual string formatDate(DateTime value, bool useMillis)
		{
			return (useMillis) ? value.ToString(DateTimeUtil.JsonDateFormatMillis) : value.ToString(DateTimeUtil.JsonDateFormat);
		}

		public GxDatetimeString(DateTime value)
			: this(value, null, false)
		{
		}
		public GxDatetimeString(DateTime value, int? timeOffset, bool useMillis)
		{
			this.value = formatDate(value, useMillis);
			if (timeOffset.HasValue)
				this.value += getStringOffset(timeOffset.Value);
		}
		string getStringOffset(int offset)
		{
			int sgn = Math.Sign(offset);
			int offset1 = Math.Abs(offset);
			int hr = (int)Math.Floor((double)offset1 / 60);
			int min = offset1 - hr * 60;
			return (sgn < 0 ? "-" : "+") + StringUtil.PadL(StringUtil.Str(hr, 2, 0), 2, '0') + ":" + StringUtil.PadL(StringUtil.Str(min, 2, 0), 2, '0');
		}
		public string value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
			}
		}
	}
	public class GxDateString : GxDatetimeString
	{
		new static public string NullValue = "0000-00-00";
		static public string GregorianDate = "0001-01-01";

		protected override string formatDate(DateTime value, bool useMillis)
		{
			return value.ToString("yyyy-MM-dd");
		}
		public GxDateString(DateTime value)
			: base(value)
		{
		}
	}

	[Serializable]
	public class GxJsonArray : CollectionBase
	{

		public virtual void Add(object value)
		{
			List.Add(value);
		}
		public override string ToString()
		{
			return JSONHelper.Serialize<GxJsonArray>(this, Encoding.UTF8);
		}

	}

	public class GxDictionary : Dictionary<string, Object>
	{
		public bool HasKey(string key)
		{
			return ContainsKey(key);
		}
		new public GxStringCollection Keys()
		{
			GxStringCollection result = new GxStringCollection();
			foreach (string key in base.Keys)
			{
				result.Add(key);
			}
			return result;
		}
		public string Value(string key)
		{
			if (ContainsKey(key))
				return (string)this[key];
			else
				return string.Empty;
		}
		public void Set(string key, string value)
		{
			this[key] = value;
		}
		public GxDictionary Difference(GxDictionary value)
		{
			GxDictionary diffDictionary = new GxDictionary();
			foreach (string key in base.Keys)
			{
				if (!value.ContainsKey(key))
					diffDictionary.Add(key, base[key]);
			}
			return diffDictionary;
		}

	}

}