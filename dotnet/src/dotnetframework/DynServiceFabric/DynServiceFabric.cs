using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using GeneXus.Data.NTier;

namespace GeneXus.Data.DynService.Fabric
{


	public class CurrentOfManager
	{
		private IDictionary<string, FabricDataReader> m_CurrentOfManager = new Dictionary<string, FabricDataReader>();
		internal void AddQuery(string CursorId, FabricDataReader DataReader)
		{
			m_CurrentOfManager.Remove(CursorId);
			m_CurrentOfManager.Add(CursorId, DataReader);
		}

		public void RemoveQuery(string CursorId)
		{
			m_CurrentOfManager.Remove(CursorId);
		}

		internal FabricDataReader GetQuery(string CursorId)
		{
			return m_CurrentOfManager[CursorId];
		}
	}

	public class FabricDBService : GxService
	{
		public FabricDBService(string id, string providerId) : base(id, providerId, typeof(FabricConnection))
		{
		}
	}

	public class CurrentOf
	{
		string CursorName;
		IDictionary<string, object> CurrentEntry;
		CurrentOfManager CurrentOfManager;
		public CurrentOf(CurrentOfManager CurrentOfManager, string CursorName)
		{
			this.CurrentOfManager = CurrentOfManager;
			this.CursorName = CursorName;
			CurrentEntry = CurrentOfManager.GetQuery(CursorName).CurrentOfEntry;
		}


		public IDictionary<string, object> End()
		{
			return CurrentOfManager.GetQuery(CursorName).CurrentOf;
		}

	}

	public class QueryExpression
	{
		public string For { get; set; }
		public string[] Select { get; set; }
	}

	
	public class FabricException : Exception
	{
		public FabricException(string mes) : base(mes)
		{
			m_ErrorMsg = mes;
		}
		public FabricException(int code, string mes) : base(mes)
		{
			m_ErrorMsg = mes;
			m_ErrorCode = code;
		}
		string m_ErrorMsg;
		int m_ErrorCode;

		public string ErrorMsg { get => m_ErrorMsg; set => m_ErrorMsg = value; }
		public int ErrorCode { get => m_ErrorCode; set => m_ErrorCode = value; }
	}

		public class DataStoreHelperFabric : DynServiceDataStoreHelper
	{
		public char[] likeChars = new char[] { '%' };
		
		public CurrentOfManager CurrentOfManager { get; internal set; } = new CurrentOfManager();


		public override Guid GetParmGuid(IDataParameterCollection parms, string parm)
		{
			Guid.TryParse(GetParmObj(parms, parm) as string, out Guid guidValue);
			return guidValue;
		}

		public override string GetParmStr(IDataParameterCollection parms, string parm)
		{
			return GetParmObj(parms, parm) as string;
		}

		public override int GetParmInt(IDataParameterCollection parms, string parm)
		{
			return Int32.TryParse(GetParmObj(parms, parm) as string, out int res) ? res : 0;
		}

		public override decimal GetParmFP(IDataParameterCollection parms, string parm)
		{
			return Decimal.TryParse(GetParmObj(parms, parm) as string, out decimal res) ? res : 0M;
		}

		public override DateTime GetParmDate(IDataParameterCollection parms, string parm)
		{
			return DateTime.TryParse(GetParmObj(parms, parm) as string, out DateTime res) ? res : DateTime.MinValue;
		}

		public override TimeSpan GetParmTime(IDataParameterCollection parms, string parm)
		{
			return TimeSpan.TryParse(GetParmObj(parms, parm) as string, out TimeSpan res) ? res : TimeSpan.Zero;
		}

		public override object GetParmObj(IDataParameterCollection parms, string parm)
		{
			for (int idx = 0; idx < parms.Count; idx++)
			{
				if (parms[idx] is IDataParameter sParm && sParm.ParameterName.Equals(parm))
				{
					
					string parmValue = Convert.ToString(sParm.Value);
					if (parm.StartsWith("l"))
					{// In Like
						parmValue = parmValue.TrimEnd(likeChars);
					}
					return parmValue;
				}
			}
			Debug.Assert(false, string.Format("Unknown parameter: {0}", parm));
			throw new GxADODataException(string.Format("Unknown parameter: {0}", parm));
		}


		public class Query
		{
			private object mDataStoreHelper;

			public string FunctionName { get; set; } = String.Empty;
			public string TableName { get; set; } = String.Empty;
			public string[] Projection { get; set; } = Array.Empty<string>();
			public string[] ColumnList { get; set; } = Array.Empty<string>();

			public string[] OrderBys { get; set; } = Array.Empty<string>();
			public string[] Filters { get; set; } = Array.Empty<string>();
			public IDictionary<String, String> Parms { get; set; } = new Dictionary<String, String>() { };
			public string Method { get; set; } = String.Empty;
			public ArrayList ExtTables = new ArrayList(); 
			private string[] _keyValues;

			public Query(object dataStoreHelper)
			{
				mDataStoreHelper = dataStoreHelper;
			}

			public Query SetMethod(string mth)
			{
				this.Method = mth;
				return this;
			}

			public Query For(string v)
			{
				TableName = v;
				return this;
			}

			public Query ForExt(string v)
			{
				ExtTables.Add(v);
				return this;
			}
			public Query Select(string[] columns)
			{
				Projection = columns;
				return this;
			}
			public Query SetKey(string[] keyValues)
			{
				_keyValues = keyValues;
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
			public Query SetParms(IDictionary<String,String> parms, String mode)
			{
				Method = mode;
				Parms = parms;
				return this;
			}

			public IDictionary<String, String> GetParms()
			{
				return this.Parms;
				
			}

			public String[] GetKeyVal()
			{
				return _keyValues;
			}

			public Query SetMaps(IODataMap[] iODataMap)
			{
				List<String> mapList = new List<String>();
				foreach (IODataMap map in iODataMap)
				{
					mapList.Add(map.GetName(null));
				}
				ColumnList = mapList.ToArray();
				return this;
			}
		};
		
		public IODataMap Map(string name)
		{
			return new ServiceMapName(name);
		}


	}
}
