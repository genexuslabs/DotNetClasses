using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json.Nodes;
using GeneXus.Data.NTier;

namespace GeneXus.Data.Cosmos
{
	internal class CosmosDBHelper
	{
		internal static bool AddItemValue(string parmName, string fromName, Dictionary<string, object> values, IDataParameterCollection parms, IEnumerable<VarValue> queryVars, ref JsonObject jsonObject)
		{		
			if (!AddItemValue(parmName, values, parms[fromName] as ServiceParameter, ref jsonObject))
			{
				VarValue varValue = queryVars.FirstOrDefault(v => v.Name == $":{fromName}");
				if (varValue != null)
				{
					values[parmName] = varValue.Value;
					jsonObject.Add(parmName, JsonValue.Create(varValue.Value));
				}
				return varValue != null;
			}
			return true;
		}
		public static bool FormattedAsStringGXType(GXType gXType)
		{
			return (gXType == GXType.Date || gXType == GXType.DateTime || gXType == GXType.DateTime2 || gXType == GXType.VarChar || gXType == GXType.DateAsChar || gXType == GXType.NVarChar || gXType == GXType.LongVarChar || gXType == GXType.NChar ||  gXType == GXType.Char || gXType == GXType.Text || gXType == GXType.NText);
		}
		internal static bool FormattedAsStringDbType(DbType dbType)
		{
			return (dbType == DbType.String || dbType == DbType.Date || dbType == DbType.DateTime || dbType == DbType.DateTime2 || dbType == DbType.DateTimeOffset || dbType == DbType.StringFixedLength || dbType == DbType.AnsiString || dbType == DbType.AnsiStringFixedLength || dbType == DbType.Guid || dbType == DbType.Time);
		}
		internal static string FormatExceptionMessage(string statusCode, string message)
		{
			return ($"CosmosDB Execution failed. Status code: {statusCode}. Message: {message}");
		}
		internal static bool AddItemValue(string parmName, Dictionary<string, object> dynParm, ServiceParameter parm, ref JsonObject jsonObject)
		{
			if (parm == null)
				return false;
			if (parm.Value != null)
			{
				dynParm[parmName] = parm.Value;
				jsonObject.Add(parmName, JsonValue.Create(parm.Value));
				return true;
			}
			return false;
		}
	}
}
