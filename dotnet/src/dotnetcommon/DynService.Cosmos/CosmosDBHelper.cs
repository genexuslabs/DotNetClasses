using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Data.NTier;

namespace GeneXus.Data.Cosmos
{
	internal class CosmosDBHelper
	{
		internal static bool AddItemValue(string parmName, string fromName, Dictionary<string, object> values, IDataParameterCollection parms, IEnumerable<VarValue> queryVars, ref string jsonData)
		{
			
			if (!AddItemValue(parmName, values, parms[fromName] as ServiceParameter, out string data))
			{
				VarValue varValue = queryVars.FirstOrDefault(v => v.Name == $":{fromName}");
				if (varValue != null)
				{
					values[parmName] = varValue;
					jsonData = AddToJsonStream(varValue.Type, parmName, varValue);
				}
				return varValue != null;
			}
			StringBuilder stringBuilder = new StringBuilder(jsonData);
			string concatData;
			if (!string.IsNullOrEmpty(jsonData))
			{
				concatData = $",{data}";
				stringBuilder.Append(concatData);
				jsonData = stringBuilder.ToString();
			}
			else
				jsonData = data;

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
		internal static string AddToJsonStream(GXType gXType, string parmName, object value)
		{
			string valueItem;
			if (FormattedAsStringGXType(gXType))
			{
				valueItem = string.Format("\"{0}\"", value);
				return string.Format("\"{0}\": {1}", parmName, string.Join(",", valueItem));
			}
			else
				return string.Format("\"{0}\": {1}", parmName, string.Join(",", value));
		}
		internal static string AddToJsonStream(DbType dbType, string parmName, object value)
		{
			string valueItem;
			string data;
			if (FormattedAsStringDbType(dbType))
			{
				valueItem = string.Format("\"{0}\"", value);
				return string.Format("\"{0}\": {1}", parmName, valueItem);	
			}
			else
			{
				data = string.Format("\"{0}\": {1}", parmName, value);
				if (dbType == DbType.Boolean || dbType == DbType.Byte)
				{ 
					data = data.Replace("True", "true");
					data = data.Replace("False", "false");
				}
				return data;

			}
		}
		internal static string FormatExceptionMessage(string statusCode, string message)
		{
			return ($"CosmosDB Execution failed. Status code: {statusCode}. Message: {message}");
		}
		internal static bool AddItemValue(string parmName, Dictionary<string, object> dynParm, ServiceParameter parm, out string jsonData)
		{
			jsonData = string.Empty;
			if (parm == null)
				return false;
			object value = ToItemValue(parm.DbType, parm.Value);
			if (value != null)
			{
				dynParm[parmName] = value;
				jsonData = AddToJsonStream(parm.DbType, parmName, value);
				return true;
			}
			return false;
		}

		internal static object ToItemValue(DbType dbType, Object value)
		{
			object attValue;
			switch (dbType)
			{
				case DbType.Binary:
					if (value is byte[] valueArr)
					{
						attValue = new MemoryStream(valueArr);
						break;
					}
					else throw new ArgumentException("Required value not found");
				case DbType.Boolean:
				case DbType.Byte:
					attValue = Convert.ToByte(value) == 1 ? true : false;
					break;
				case DbType.Time:
				case DbType.Date:
					attValue = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc).ToString("yyyy-MM-dd");
					break;
				case DbType.DateTime2:
				case DbType.DateTime:
					attValue = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ssK");
					break;
				
				case DbType.UInt16:
				case DbType.UInt32:
				case DbType.UInt64:
				case DbType.VarNumeric:
				case DbType.Decimal:
				case DbType.Double:
				case DbType.Int16:
				case DbType.Int32:
				case DbType.Int64:
					attValue = value.ToString();
					break;
				default:
					string valueDefault = value.ToString().Replace("%", string.Empty);
					attValue = valueDefault;
					break;
			}
			return attValue;
		}
	}
}
