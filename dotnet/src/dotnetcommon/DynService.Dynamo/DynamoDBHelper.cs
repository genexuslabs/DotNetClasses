using Amazon.DynamoDBv2.Model;
using GeneXus.Data.NTier;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneXus.Data.Dynamo
{
	internal class DynamoDBHelper
	{
#if NETCORE
		private static readonly TaskFactory mTaskFactory = new
			  TaskFactory(CancellationToken.None,
						  TaskCreationOptions.None,
						  TaskContinuationOptions.None,
						  TaskScheduler.Default);
		internal static TResult RunSync<TResult>(Func<Task<TResult>> func) => mTaskFactory.StartNew<Task<TResult>>(func).Unwrap<TResult>().GetAwaiter().GetResult();
#endif

		internal static AttributeValue ToAttributeValue(VarValue var)
		{
			return ToAttributeValue(GxService.GXTypeToDbType(var.Type), var.Value, false);			
		}

		internal static void GXToDynamoQueryParameter(string prefix, Dictionary<string, AttributeValue> dynParm, ServiceParameter parm) => AddAttributeValue($"{prefix}{parm.ParameterName}", dynParm, parm);

		internal static bool AddAttributeValue(string parmName, Dictionary<string, AttributeValue> dynParm, ServiceParameter parm)
		{
			if (parm == null)
				return false;
			AttributeValue value = ToAttributeValue(parm.DbType, parm.Value, true);
			if (value != null)
			{
				dynParm[parmName] = value;
				return true;
			}
			return false;
		}

		private static AttributeValue ToAttributeValue(DbType dbType, Object value, bool skipEmpty)
		{
			AttributeValue attValue = null;
			switch (dbType)
			{
				case System.Data.DbType.Binary:
					throw new NotImplementedException("Binary column not implemented yet");
				case System.Data.DbType.Boolean:
				case System.Data.DbType.Byte:
					attValue = new AttributeValue
					{
						BOOL = (bool)value
					};
					break;
				case System.Data.DbType.Time:
				case System.Data.DbType.Date:
				case System.Data.DbType.DateTime2:
				case System.Data.DbType.DateTime:
					attValue = new AttributeValue
					{
						S = value.ToString()
					};
					break;

				
				case System.Data.DbType.UInt16:
				case System.Data.DbType.UInt32:
				case System.Data.DbType.UInt64:
				case System.Data.DbType.VarNumeric:
				case System.Data.DbType.Decimal:
				case System.Data.DbType.Double:
				case System.Data.DbType.Int16:
				case System.Data.DbType.Int32:
				case System.Data.DbType.Int64:
					attValue = new AttributeValue
					{
						N = value.ToString()
					};
					break;
				default:
					string valueS = value.ToString().Replace("%", string.Empty);
					attValue = new AttributeValue
					{
						S = valueS
					};
					break;
			}
			return attValue;
		}

		public static string GetString(AttributeValue attValue)
		{
			string value = attValue.S;
			if (value != null)
				return value;
			else if (attValue.NS.Count > 0)
				return SetToString(attValue.NS);
			else if (attValue.SS.Count > 0)
				return SetToString(attValue.SS);
			else if (attValue.IsMSet)
				return JSONHelper.Serialize(ConvertToDictionary(attValue.M), Encoding.UTF8);
			else if (attValue.IsLSet)
				return JSONHelper.Serialize<List<string>>(attValue.L.Select(item => GetString(item)).ToList());
			return null;
		}

		private static Dictionary<string, string> ConvertToDictionary(Dictionary<string, AttributeValue> m)
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();
			foreach (KeyValuePair<string, AttributeValue> keyValues in m)
			{
				dict.Add(keyValues.Key, GetString(keyValues.Value));
			}
			return dict;
		}

		private static string SetToString(List<string> nS) => $"[ { string.Join(", ", nS) } ]";
	}
}
