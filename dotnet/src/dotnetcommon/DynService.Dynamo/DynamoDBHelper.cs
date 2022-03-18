using Amazon.DynamoDBv2.Model;
using GeneXus.Data.NTier;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
#if NETCORE
using System.Threading;
using System.Threading.Tasks;
#endif

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
			return ToAttributeValue(GxService.GXTypeToDbType(var.Type), var.Value);			
		}

		internal static void GXToDynamoQueryParameter(string prefix, Dictionary<string, AttributeValue> dynParm, ServiceParameter parm) => AddAttributeValue($"{prefix}{parm.ParameterName}", dynParm, parm);

		internal static bool AddAttributeValue(string parmName, Dictionary<string, AttributeValue> dynParm, ServiceParameter parm)
		{
			if (parm == null)
				return false;
			AttributeValue value = ToAttributeValue(parm.DbType, parm.Value);
			if (value != null)
			{
				dynParm[parmName] = value;
				return true;
			}
			return false;
		}

		private static AttributeValue ToAttributeValue(DbType dbType, Object value)
		{
			AttributeValue attValue;
			switch (dbType)
			{
				case DbType.Binary:
					throw new NotImplementedException("Binary column not implemented yet");
				case DbType.Boolean:
				case DbType.Byte:
					attValue = new AttributeValue
					{
						BOOL = (bool)value
					};
					break;
				case DbType.Time:
				case DbType.Date:
				case DbType.DateTime2:
				case DbType.DateTime:
					attValue = new AttributeValue
					{
						S = value.ToString()
					};
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
