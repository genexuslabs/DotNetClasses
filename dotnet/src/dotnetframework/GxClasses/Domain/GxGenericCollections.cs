using GeneXus.Application;
using GeneXus.XML;
using Jayrock.Json;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace GeneXus.Utils
{

	[Serializable]
	public class GXBaseCollection<T> : List<T>, IGxXMLSerializable, IGxJSONAble, IGxCollection<T>, IGxJSONSerializable where T : GxUserType, IGxXMLSerializable, IGxJSONAble, new()
	{

		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GXBaseCollection<T>));
		public string _containedName;
		public string _containedXmlNamespace;
		public IGxContext context;

		protected CollectionBase _jsonArr;

		public GXBaseCollection() : base() { }
		public GXBaseCollection(IGxContext context,
								 string containedName,
								 string containedXmlNamespace)
		{
			this.context = context;
			_containedName = containedName;
			_containedXmlNamespace = containedXmlNamespace;
		}
		public GXBaseCollection(GXBaseCollection<T> value)
		{
			foreach (T item in value)
			{
				Add(item);
			}
		}

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

		public Object Item(int i)
		{
			if (i > 0 && i - 1 < this.Count)
				return this[i - 1];
			else
				return null;
		}
		public virtual IGxXMLSerializable CurrentItem
		{
			get
			{
				if (currentItem == null)
				{
					GXLogging.Warn(log, "CurrentItem of type " + typeof(T) + " is null");
					try
					{
						currentItem = (IGxXMLSerializable)Activator.CreateInstance(typeof(T), new object[] { context });
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
		public void addNew(object o)
		{
			Add(o as T);
		}
		public void Add(object o, int idx)
		{
			if (idx == 0)
				this.Add(o); //GxBaseCollection<T>.Add
			else
				Insert(idx - 1, o as T);
		}
		public virtual int Add(object o)
		{
			base.Add(o as T);
			return 0;
		}
		//In SDT level collections it does not delete the item but marks it with DLT mode
		public virtual void RemoveItem(int idx)
		{
			RemoveElement(idx);
		}
		//Remove an element from the collection
		public void RemoveElement(int idx)
		{
			if (idx <= 0 || idx > Count)
				return;
			base.RemoveAt(idx - 1);
		}
		public virtual void ClearCollection()
		{
			if (_jsonArr!=null)
				_jsonArr.Clear();
			base.Clear();
		}
		public virtual object Clone()
		{
			GXBaseCollection<T> collection = new GXBaseCollection<T>();
			foreach (T item in this)
			{
				collection.Add(item);
			}
			return collection;
		}
		public int IndexOf(object value)
		{
			return base.IndexOf((T)value) + 1;
		}
		public virtual void writexml(GXXMLWriter oWriter, string sName)
		{
			writexml(oWriter, sName, "");
		}
		public virtual void writexml(GXXMLWriter oWriter, string sName, string sNamespace)
		{
			writexmlcollection(oWriter, sName, sNamespace, this._containedName, sNamespace);
		}
		public virtual void writexml(GXXMLWriter oWriter, string sName, string sNamespace, bool includeState)
		{
			writexmlcollection(oWriter, sName, sNamespace, this._containedName, this._containedXmlNamespace, includeState);
		}
		public virtual void writexmlcollection(GXXMLWriter oWriter, string sName, string sNamespace, string itemName, string itemNamespace)
		{
			writexmlcollection(oWriter, sName, sNamespace, itemName, itemNamespace, true);
		}

		public virtual void writexmlcollection(GXXMLWriter oWriter, string sName, string sNameSpace, string itemName, string itemNamespace, bool includeState)
		{
			if (StringUtil.StrCmp(StringUtil.Left(sNameSpace, 10), "[*:nosend]") == 0)
			{
				sNameSpace = StringUtil.Right(sNameSpace, (short)(StringUtil.Len(sNameSpace) - 10));
			}

			if (string.IsNullOrEmpty(sName))
			{
				writexmlcollectionUnwrapped(oWriter, itemName, itemNamespace, includeState);
			}
			else
			{
				writexmlcollectionWrapped(oWriter, sName, sNameSpace, itemName, itemNamespace, includeState);
			}

		}
		public virtual void writexmlcollectionWrapped(GXXMLWriter oWriter, string sName, string sNameSpace,  string itemName, string itemNamespace, bool includeState)
		{
			oWriter.WriteStartElement(sName);
			oWriter.WriteAttribute("xmlns", sNameSpace);
			writexmlcollectionUnwrapped(oWriter, itemName, itemNamespace, includeState);
			oWriter.WriteEndElement();
		}
		public virtual void writexmlcollectionUnwrapped(GXXMLWriter oWriter, string itemName, string itemNamespace, bool includeState)
		{

			foreach (T obj in this)
			{
				string xml = obj.ToXml(false, includeState, itemName, itemNamespace);
				xml = GxUserType.UpdateNodeDefaultNamespace(xml, "", true, null);
				oWriter.WriteRawText(xml);
			}
		}

		public virtual short readxml(GXXMLReader oReader)
		{
			return readxml(oReader, "");
		}
		public virtual short readxml(GXXMLReader oReader, string sName)
		{
			return readxmlcollection(oReader, sName, GetContainedName());
		}
		public virtual short readxmlcollection(GXXMLReader oReader, string sName, string itemName)
		{
			if (string.IsNullOrEmpty(sName))
			{
				return readxmlcollectionUnwrapped(oReader, "", itemName);
			}
			else
			{
				try
				{
					if (oReader.LocalName == sName)
					{
						ClearCollection();
						oReader.Read();
						readxmlcollectionUnwrapped(oReader, "", itemName);

					}
					return 1;
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "readxmlcollection error", ex);
					return -1;
				}
			}
		}
		public short readxmlcollectionUnwrapped(GXXMLReader oReader, string sName, string itemName)
		{
			short currError = 1;
			while (oReader.LocalName == itemName && currError > 0)
			{
				T currObject = new T();
				currObject.context = this.context;
				string xml = GxUserType.UpdateNodeDefaultNamespace(oReader.ReadRawXML(), null, false, this.GetPrefixesInContext());
				currObject.FromXml(xml, itemName, oReader.NamespaceURI);
				Add(currObject);
				oReader.Read();
			}
			return currError;
		}
		public bool FromXmlFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages, string sName, string sNameSpace)
		{
			if (GXUtil.CheckFile(file, Messages))
				return FromXml(file.ReadAllText(string.Empty), Messages, sName, sNameSpace);
			else
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
			try
			{
				if (!string.IsNullOrEmpty(s))
				{
					base.Clear();
					GXBaseCollection<T> deserialized = GXXmlSerializer.Deserialize<GXBaseCollection<T>>(this.GetType(), s, sName, sNamespace, out List<string> serializationErrors);
					GXXmlSerializer.SetSoapError(context, serializationErrors);
					if (deserialized != null)
					{
						foreach (T item in deserialized)
						{
							item.context = context;
							Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "FromXml error", ex);
				
				context.nSOAPErr = -20006;
				GXXmlSerializer.SetSoapError(context, string.Format( "Error reading {0}", this.GetType()));
				GXXmlSerializer.SetSoapError(context, ex.Message);
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
					GXXmlSerializer.SetSoapError(context, ex.Message);
				}
				GXUtil.ErrorToMessages("FromXML Error", ex, Messages);
				return false;
			}
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
		public virtual string ToXml(bool includeHeader, bool includeState, string rootName, string sNameSpace)
		{
			XmlAttributeOverrides ov = null;
			if (!includeState)
			{
				if (Count > 0)
				{
					ov = GXXmlSerializer.IgnoredAttributes(GxUserType.StateAttributesTypeMap(typeof(T)));
				}
			}
			return GXXmlSerializer.Serialize(rootName, sNameSpace, ov, includeHeader, this);

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
				return this;
			}
			set
			{
				if (this != value) { 
					base.Clear();
					foreach (T item in value)
						this.Add(item);
				}
			}
		}
		public virtual void Sort(string order)
		{
			base.Sort(new GxSDTComparer<T>(order));
		}
		public String ToJavascriptSource(bool includeState)
		{
			return JSONHelper.WriteJSON<dynamic>(GetJSONObject(includeState)); 
		}
		public String ToJavascriptSource()
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

		public string GetContainedName()
		{
			if (_containedName == null)
				return "item";
			return _containedName;
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
			if (file != null && file.Exists())
			{
				string s = file.ReadAllText(string.Empty);
				return FromJSonString(s, Messages);
			}
			else {
				GXUtil.ErrorToMessages("FromJSon Error", "File does not exist", Messages);
				return false;
			}
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
			for (int i = 0; i < jobj.Length; i++)
			{
				T obj1 = (T)Activator.CreateInstance(typeof(T), new object[] { context });
				obj1.FromJSONObject(jobj[i]);
				Add(obj1);
			}
		}
	}

	public interface IGXBCCollection : IGxCollection
	{
		void BaseAdd(Object item);
	}
	[Serializable]
	public class GXBCLevelCollection<T> : GXBCCollection<T>, IGXBCCollection where T : GxSilentTrnSdt, IGxSilentTrnGridItem, new()
	{
		public GXBCLevelCollection() : base() { }
		public GXBCLevelCollection(IGxContext context,
								 string containedName,
								 string containedXmlNamespace)
		{
			this.context = context;
			_containedName = containedName;
			_containedXmlNamespace = containedXmlNamespace;
		}
		public override int Add(object item)
		{
			SetModeNewSilentItem(item as T);
			base.Add(item);
			return 0;
		}
		public new void Add(T item)
		{
			base.Add(item);
		}
		public new void Insert(int index, T item)
		{
			SetModeNewSilentItem(item);
			base.Insert(index, item);
		}
		private void SetModeNewSilentItem(T gridItem)
		{
			if (gridItem != null)
			{
				gridItem.gxTpr_Mode = TRANSACTION_MODE.INSERT;
				gridItem.gxTpr_Modified = 1;
			}
		}
		public override void RemoveItem(int idx)
		{
			if (idx <= 0 || idx > Count)
				return;

			IGxSilentTrnGridItem item = (IGxSilentTrnGridItem)(this[idx - 1]);
			if (StringUtil.StrCmp(item.gxTpr_Mode, TRANSACTION_MODE.INSERT) == 0)
			{
				base.RemoveAt(idx - 1);
			}
			else
			{
				item.gxTpr_Mode = "DLT";
			}
			return;
		}
		
		public new void Clear()
		{
			int idx = Count;
			while (idx >= 1)
			{
				RemoveItem(idx);
				idx = idx - 1;
			}
			return;
		}
	}
	[Serializable]
	public class GXBCCollection<T> : GXBaseCollection<T>, IGXBCCollection where T : GxSilentTrnSdt, new()
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GXBCCollection<T>));

		public GXBCCollection() : base() { }
		public GXBCCollection(IGxContext context,
								 string containedName,
								 string containedXmlNamespace)
		{
			this.context = context;
			_containedName = containedName;
			_containedXmlNamespace = containedXmlNamespace;
		}
		public void BaseAdd(object item)
		{
			base.Add(item);
		}

		public bool Insert()
		{
			bool result = true;
			foreach (T Item in this)
			{
				result = Item.Insert() & result; //Try to do insert in all items regardless result value
			}
			return result;
		}
		public bool Update()
		{
			bool result = true;
			foreach (T Item in this)
			{
				result = Item.Update() & result; //Try to do Update in all items regardless result value
			}
			return result;
		}
		public bool InsertOrUpdate()
		{
			bool result = true;
			foreach (T Item in this)
			{
				result = Item.InsertOrUpdate() & result; //Try to do InsertOrUpdate in all items regardless result value
			}
			return result;
		}
		public bool Delete()
		{
			bool result = true;
			foreach (T Item in this)
			{
				Item.Delete();
				result = Item.Success() & result; //Try to do Delete in all items regardless result value
			}
			return result;
		}

		public bool RemoveByKey(params object[] key)
		{
			for(int i=0; i<this.Count; i++)
			{
				T item = this[i];
				if (IsEqualComparedByKey(item, key))
				{
					this.RemoveItem(i + 1);
					return true;
				}
			}
			return false;
		}

		public T GetByKey(params object[] key)
		{
			foreach (T item in this)
				if (IsEqualComparedByKey(item, key))
					return item;
			return new T();
		}
		private bool IsEqualComparedByKey(T item, params object[] key)
		{
			try
			{
				Object[][] itemKey = item.GetBCKey();
				Object[] returnedKey = new Object[itemKey.Length];
				Object[] parsedKey = new Object[key.Length];
				for (int i = 0; i < itemKey.Length; i++)
				{
					string property = "gxTpr_" + itemKey[i][0].ToString().Substring(0, 1).ToUpper() + itemKey[i][0].ToString().Substring(1).ToLower();
					Type keyType = (Type)itemKey[i][1];
					returnedKey[i] = item.GetType().GetProperty(property).GetValue(item, null);
					parsedKey[i] = Convert.ChangeType(key[i], keyType);
					if (keyType == typeof(string))
					{
						string rString = (string)returnedKey[i];
						returnedKey[i] = rString.TrimEnd();
						string pString = (string)parsedKey[i];
						parsedKey[i] = pString.TrimEnd();
					}
				}
				return Enumerable.SequenceEqual(returnedKey, parsedKey);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error in IsEqualComparedByKey", ex);
				return false;
			}
		}
	}
}
