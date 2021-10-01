using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using static GeneXus.Data.NTier.ServiceCursorDef;

namespace GeneXus.Data.NTier
{
	public class Query
	{
		private object mDataStoreHelper;

		public string TableName { get; set; } = String.Empty;
		public string[] Projection { get; set; } = Array.Empty<string>();
		public string[] OrderBys { get; set; } = Array.Empty<string>();
		public string[] Filters { get; set; } = Array.Empty<string>();
		public string[] AssignAtts { get; set; } = Array.Empty<string>();
		public IODataMap2[] SelectList { get; set; } = Array.Empty<IODataMap2>();

		private List<VarValue> mVarValues = new List<VarValue>();
		public IEnumerable<VarValue> Vars { get { return mVarValues; } }
		public CursorType CursorType { get; set; } = CursorType.Select;

		public Query(object dataStoreHelper)
		{
			mDataStoreHelper = dataStoreHelper;
		}
		public Query For(string v)
		{
			TableName = v;
			return this;
		}

		public Query Select(string[] columns)
		{
			Projection = columns;
			return this;
		}
		public Query OrderBy(string[] orders)
		{
			OrderBys = orders;
			return this;
		}

		public Query Filter(string[] filters)
		{
			Filters = filters;
			return this;
		}

		public Query Set(string[] assignAtts)
		{
			AssignAtts = assignAtts;			
			return this;
		}

		public Query SetMaps(IODataMap2[] iODataMap)
		{
			SelectList = iODataMap;
			return this;
		}

//		public Query SetVars(VarValue[] vars)
//		{
//			Vars = vars;
//			return this;
//		}

		public Query SetType(CursorType cType)
		{
			CursorType = cType;
			return this;
		}

		public Query AddParm(GXType gxType, object parm)
		{
			mVarValues.Add(new VarValue($":parm{ mVarValues.Count + 1 }", gxType, parm));
			return this;
		}
	}

	public class VarValue
	{
		public string Name { get; set; }
		public object Value { get; set; }
		public GXType Type { get; set; }

		public VarValue(string name, GXType type, object value)
		{
			Name = name;
			Type = type;
			Value = value;
		}

	}

	public class QueryExpression
	{
		public string For { get; set; }
		internal string[] Select { get; set; }
	}
				
	

	public interface IODataMap2
	{
		object GetValue(IOServiceContext serviceContext, RecordEntryRow currentEntry);
		string GetName(IOServiceContext serviceContext);
		void SetValue(RecordEntryRow currentEntry, object value);
	}


	public abstract class RecordEntryRow
	{

	}

	public class Map : IODataMap2
	{
		internal string Name { get; }

		public Map(string name)
		{
			Name = name;
		}

		public virtual object GetValue(IOServiceContext context, RecordEntryRow currentEntry)
		{
			throw new NotImplementedException();
		}

		public virtual void SetValue(RecordEntryRow currentEntry, object value)
		{
			throw new NotImplementedException();
		}

		public string GetName(IOServiceContext context)
		{
			return Name;
		}

	}

	public abstract class Filter
	{
	}

	public interface IOServiceContext
	{
		object Entity(string entity);
	}

	public interface IODataMap
	{
		object GetValue(IOServiceContext serviceContext, IDictionary<string, object> currentEntry);
		string GetName(IOServiceContext serviceContext);
		void SetValue(IDictionary<string, object> currentEntry, object value);
	}

	public abstract class DynServiceDataStoreHelper : DataStoreHelperBase
	{
		public abstract Guid GetParmGuid(IDataParameterCollection parms, string parm);
		public abstract string GetParmStr(IDataParameterCollection parms, string parm);
		public abstract int GetParmInt(IDataParameterCollection parms, string parm);
		public abstract decimal GetParmFP(IDataParameterCollection parms, string parm);
		public abstract DateTime GetParmDate(IDataParameterCollection parms, string parm);
		public abstract TimeSpan GetParmTime(IDataParameterCollection parms, string parm);
		public abstract object GetParmObj(IDataParameterCollection parms, string parm);
		public virtual DateTime GetParmDateTime(IDataParameterCollection parms, string parm)
		{
			return GetParmDate(parms, parm);
		}

		public virtual Guid? GetParmUGuid(IDataParameterCollection parms, string parm)
		{
			return GetParmGuid(parms, parm);
		}

		public virtual string GetParmUStr(IDataParameterCollection parms, string parm)
		{
			return GetParmStr(parms, parm);
		}

		public virtual int? GetParmUInt(IDataParameterCollection parms, string parm)
		{
			return GetParmInt(parms, parm);
		}

		public virtual decimal? GetParmUFP(IDataParameterCollection parms, string parm)
		{
			return GetParmFP(parms, parm);
		}

		public virtual DateTime? GetParmUDate(IDataParameterCollection parms, string parm)
		{
			return GetParmDate(parms, parm);
		}

		public virtual TimeSpan? GetParmUTime(IDataParameterCollection parms, string parm)
		{
			return GetParmTime(parms, parm);
		}

		public virtual object GetParmUObj(IDataParameterCollection parms, string parm)
		{
			return GetParmObj(parms, parm);
		}

		public virtual DateTime? GetParmUDateTime(IDataParameterCollection parms, string parm)
		{
			return GetParmDateTime(parms, parm);
		}
	}

	public static class DictionaryExtensions
	{
		public static IDictionary<string, object> Set(this IDictionary<string, object> entry, string key, object parmValue)
		{
			if (entry.ContainsKey(key))
			{
				if (entry[key] is IList<object> entryItemList)
				{
					if (!entryItemList.Contains(parmValue))
					{
						entryItemList.Add(parmValue);
						return entry;
					}
				}
				throw new ServiceException(ServiceError.RecordAlreadyExists);
			}
			else
			{
				entry.Add(key, parmValue);
			}
			return entry;
		}

		public static IDictionary<string, object> Remove(this IDictionary<string, object> entry, string key, object parmValue)
		{
			if (entry.ContainsKey(key))
			{
				if (entry[key] is IList<object> entryItemList)
				{
					if (entryItemList.Contains(parmValue))
					{
						entryItemList.Remove(parmValue);
						return entry;
					}
				}
				else
				{
					entry.Remove(key);
					return entry;
				}
			}
			ArgumentNullException nullExc = new ArgumentNullException(key, "Key not found");
			throw new AggregateException(new Exception[] { nullExc });
		}

		public static IDictionary<string, object> Set(this IDictionary<string, object> entry, IODataMap key, object parmValue)
		{
			key.SetValue(entry, parmValue);
			return entry;
		}

	}

	public class ServiceMapName : IODataMap
	{
		string name;
		public ServiceMapName(string name)
		{
			this.name = name;
		}

		public virtual object GetValue(IOServiceContext context, IDictionary<string, object> currentEntry)
		{
			if (currentEntry.ContainsKey(name))
				return currentEntry[name];
			else return null;
		}

		public virtual void SetValue(IDictionary<string, object> currentEntry, object value)
		{
			if (currentEntry.ContainsKey(name))
				currentEntry.Remove(name);
			currentEntry.Add(name, value);
		}

		public string GetName(IOServiceContext context)
		{
			return name;
		}
	}

	#region Convertible
	public class Convertible : IConvertible
	{
		object obj;
		public Convertible(object obj)
		{
			this.obj = obj;
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public long ToInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			if (conversionType.IsAssignableFrom(obj.GetType()))
				return obj;
			return null;
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ulong ToUInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}
	}
	#endregion


}
