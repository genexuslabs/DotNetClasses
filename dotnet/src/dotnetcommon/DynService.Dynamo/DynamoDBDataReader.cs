using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using GeneXus.Cache;
using GeneXus.Data.NTier;
using GeneXus.Data.NTier.DynamoDB;

namespace GeneXus.Data.Dynamo
{
	public class DynamoDBDataReader : IDataReader
	{
		private readonly RequestWrapper mRequest;
		private ResponseWrapper mResponse;
		private int mCurrentPosition;
		private readonly IODataMap2[] selectList;
		private DynamoDBRecordEntry currentEntry;

		private int ItemCount
		{
			get
			{
				return mResponse.ItemCount;
			}
		}

		private List<Dictionary<string, AttributeValue>> Items
		{
			get
			{
				return mResponse.Items;
			}
		}

		private void CheckCurrentPosition()
		{
			if (currentEntry == null)
				throw new ServiceException(ServiceError.RecordNotFound);
		}

		public DynamoDBDataReader(ServiceCursorDef cursorDef, RequestWrapper request)
		{
			Query query = cursorDef.Query as Query;
			if (query != null)
				selectList = query.SelectList.ToArray();
			mRequest = request;
			mResponse = mRequest.Read();
			mCurrentPosition = -1;
		}

		public object this[string name]
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public object this[int i]
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Depth
		{
			get
			{
				return 0;
			}
		}

		public int FieldCount
		{
			get
			{				
				return selectList.Length;				
			}
		}

		public bool IsClosed
		{
			get
			{
				return false;
			}
		}

		public int RecordsAffected
		{
			get
			{
				return -1;
			}
		}

		public void Close()
		{
			if (mRequest != null)
			{
				mRequest.Close();
			}
		}

		public void Dispose()
		{
		}

		public long getLong(int i)
		{
			return Convert.ToInt64(GetAttValueN(i));			
		}

		public bool GetBoolean(int i)
		{
			AttributeValue value = GetAttValue(i);
			return value.IsBOOLSet && value.BOOL;
		}

		public byte GetByte(int i)
		{
			return Convert.ToByte(GetAttValue(i).BOOL);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			MemoryStream ms = GetAttValue(i).B;
			if (ms == null)
				return 0;
			ms.Seek(fieldOffset, SeekOrigin.Begin);
			return ms.Read(buffer, bufferoffset, length);
		}

		public char GetChar(int i)
		{
			return GetAttValue(i).S.ToCharArray()[0];
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public DateTime GetDateTime(int i)
		{
			DateTime.TryParse(GetAttValue(i).S, null, DateTimeStyles.AdjustToUniversal, out DateTime dt);
			return dt;
		}

		public decimal GetDecimal(int i)
		{
			return decimal.Parse(GetAttValueN(i));
		}

		public double GetDouble(int i)
		{
			return double.Parse(GetAttValueN(i));
		}

		public Type GetFieldType(int i)
		{
			return selectList[i].GetValue(DynamoDBConnection.NewServiceContext(), currentEntry).GetType();
		}

		public float GetFloat(int i)
		{
			return float.Parse(GetAttValueN(i));
		}

		public Guid GetGuid(int i)
		{
			return new Guid(GetAttValue(i).S);
		}

		public short GetInt16(int i)
		{
			return short.Parse(GetAttValueN(i));
		}

		public int GetInt32(int i)
		{
			return int.Parse(GetAttValueN(i));

		}

		public long GetInt64(int i)
		{
			return long.Parse(GetAttValueN(i));
		}

		public string GetName(int i)
		{
			return selectList[i].GetName(null);			
		}

		public int GetOrdinal(string name)
		{
			CheckCurrentPosition();
			int ordinal = currentEntry.CurrentRow.ToList().FindIndex(col => col.Key.ToLower() == name.ToLower());
			if (ordinal == -1)
				throw new ArgumentOutOfRangeException(nameof(name));
			else return ordinal;
		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			return DynamoDBHelper.GetString(GetAttValue(i));
		}

		public object GetValue(int i)
		{
			object value = null;
			AttributeValue attValue = GetAttValue(i);
			
			if (attValue.IsBOOLSet)
			{
				value = attValue.BOOL;
			}else if (attValue.S != null || attValue.IsLSet || attValue.IsMSet)
			{
				value = DynamoDBHelper.GetString(attValue);
			}else if (attValue.N != null)
			{
				value = attValue.N;
			}else if (attValue.B != null)
			{
				value = attValue.B;
			}
			return value;
		}

		private AttributeValue GetAttValue(int i)
		{
			return (AttributeValue)selectList[i].GetValue(DynamoDBConnection.NewServiceContext(), currentEntry);
		}

		private string GetAttValueN(int i) => GetAttValue(i)?.N ?? "0";

		public int GetValues(object[] values)
		{			
			System.Diagnostics.Debug.Assert(selectList.Length == values.Length, "Values mismatch");
			for (int i = 0; i < selectList.Length && i < values.Length; i++)
			{
				values[i] = GetAttValue(i);
			}			
			return selectList.Length;
		}

		public bool IsDBNull(int i)
		{			
			return GetAttValue(i) == null || GetAttValue(i).NULL;
		}

		public bool NextResult()
		{
			mCurrentPosition++;
			currentEntry = (mCurrentPosition < ItemCount) ? new DynamoDBRecordEntry(Items[mCurrentPosition]) : null;
			return currentEntry != null;
		}

		public bool Read()
		{
			if (NextResult())
				return true;
			else if (mCurrentPosition > 0 && mResponse.LastEvaluatedKey?.Count > 0)
			{
				mResponse = mRequest.Read(mResponse.LastEvaluatedKey);
				/*
				 *
				 * A query and scan operation returns a maximum 1 MB of data in a single operation.
				 * The result set contains the last_evaluated_key field. If more data is available for the operation,
				 * this key contains information about the last evaluated key. Otherwise, the key remains empty.
				 * */
				mCurrentPosition = -1;
				return NextResult();
			}
			return false;
		}
	}

	public class GxDynamoDBCacheDataReader : GxCacheDataReader
	{
		public GxDynamoDBCacheDataReader(CacheItem cacheItem, bool computeSize, string keyCache)
			: base(cacheItem, computeSize, keyCache)
		{

		}

		public override DateTime GetDateTime(int i)
		{
			DateTime.TryParse(GetAttValue(i).S, null, DateTimeStyles.AdjustToUniversal, out DateTime dt);
			return dt;
		}

		private AttributeValue GetAttValue(int i)
		{
			return (AttributeValue)block.Item(pos, i);
		}
		private string GetAttValueN(int i) => GetAttValue(i)?.N ?? "0";

		public override decimal GetDecimal(int i)
		{
			return decimal.Parse(GetAttValueN(i));
		}

		public override short GetInt16(int i)
		{
			return short.Parse(GetAttValueN(i));
		}

		public override int GetInt32(int i)
		{
			return int.Parse(GetAttValueN(i));
		}

		public override long GetInt64(int i)
		{
			return long.Parse(GetAttValueN(i));
		}

		public override string GetString(int i)
		{
			return DynamoDBHelper.GetString(GetAttValue(i));
		}

		public override bool GetBoolean(int i)
		{
			AttributeValue value = GetAttValue(i);
			return value.IsBOOLSet && value.BOOL;
		}

		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			MemoryStream ms = GetAttValue(i).B;
			if (ms == null)
				return 0;
			ms.Seek(fieldOffset, SeekOrigin.Begin);
			return ms.Read(buffer, bufferoffset, length);
		}
	}
}
