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
				//TODO
				return false;
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
		internal static bool AddItemValue(string parmName, Dictionary<string, object> dynParm, ServiceParameter parm, out string jsonData)
		{
			jsonData = string.Empty;
			if (parm == null)
				return false;
			object value = ToItemValue(parm.DbType, parm.Value);
			if (value != null)
			{
				dynParm[parmName] = value;
				string valueItem;
				if (parm.DbType == DbType.String)
				{
					//if (!string.IsNullOrEmpty(jsonData))
				//	{ 
						valueItem = string.Format("\"{0}\"", value);
						jsonData = string.Format("\"{0}\": {1}", parmName, string.Join(",", valueItem));
					//}
				}
				else
					jsonData = string.Format("\"{0}\": {1}", parmName, string.Join(",", value));
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
					attValue = Convert.ToByte(value);
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
