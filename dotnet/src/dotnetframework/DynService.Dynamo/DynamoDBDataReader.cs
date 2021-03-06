using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using GeneXus.Cache;
using GeneXus.Data.Dynamo;
using GeneXus.Data.NTier;
using GeneXus.Data.NTier.DynamoDB;
using log4net;

namespace GeneXus.Data.Dynamo
{
	public class DynamoDBDataReader : IDataReader
	{
		private RequestWrapper mRequest;
		private ResponseWrapper mResponse;
		private int mCurrentPosition = -1;
		private IODataMap2[] selectList;

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

		public DynamoDBDataReader(ServiceCursorDef cursorDef, RequestWrapper request, IDataParameterCollection parameters)
		{
			Query query = cursorDef.Query as Query;
			selectList = query.SelectList;
			mRequest = request;
			mResponse = mRequest.Read();
		}

		public object this[string name]
		{
			get
			{
				if (mCurrentPosition >= 0 && mCurrentPosition < ItemCount)
				{
					return Items[mCurrentPosition][name].S;
				}
				throw new ArgumentOutOfRangeException(nameof(name));

			}
		}

		public object this[int i]
		{
			get
			{
				if (mCurrentPosition >= 0 && mCurrentPosition < ItemCount)
				{
					int j = 0;
					foreach (var col in Items[mCurrentPosition])
					{
						if (j == i)
							return col.Value.S;
					}
				}
				throw new ArgumentOutOfRangeException(nameof(i));
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
			return Convert.ToInt64(GetAttValue(i).N);			
		}

		public bool GetBoolean(int i)
		{
			return GetAttValue(i).BOOL;
		}

		public byte GetByte(int i)
		{
			return Convert.ToByte(GetAttValue(i).BOOL);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			return GetAttValue(i).S.ToCharArray()[0];
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			buffer = GetAttValue(i).S.ToCharArray();
			return buffer.Length;
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
			DateTime dt = new DateTime();
			DateTime.TryParse(GetAttValue(i).S, out dt);
			return dt;
		}

		public decimal GetDecimal(int i)
		{
			return decimal.Parse(GetAttValue(i).N);
		}

		public double GetDouble(int i)
		{
			return double.Parse(GetAttValue(i).N);
		}

		public Type GetFieldType(int i)
		{
			throw new NotImplementedException();
		}

		public float GetFloat(int i)
		{
			return float.Parse(GetAttValue(i).N);
		}

		public Guid GetGuid(int i)
		{
			return new Guid(GetAttValue(i).S);
		}

		public short GetInt16(int i)
		{
			return short.Parse(GetAttValue(i).N);
		}

		public int GetInt32(int i)
		{
			return int.Parse(GetAttValue(i).N);

		}

		public long GetInt64(int i)
		{
			return long.Parse(GetAttValue(i).N);
		}

		public string GetName(int i)
		{
			return selectList[i].GetName(null);			
		}

		public int GetOrdinal(string name)
		{
			int j = 0;
			foreach (var col in Items[mCurrentPosition])
			{
				if (col.Key.ToLower() == name.ToLower())
					return j;
				j++;
			}
			throw new ArgumentOutOfRangeException(nameof(name));
		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			return GetAttValue(i).S;
		}

		public object GetValue(int i)
		{
			object value = null;
			AttributeValue attValue = GetAttValue(i);
			
			if (attValue.IsBOOLSet)
			{
				value = attValue.BOOL;
			}
			if (attValue.S != null)
			{
				value = attValue.S;
			}
			if (attValue.N != null)
			{
				value = attValue.N;
			}
			if (attValue.B != null)
			{
				value = attValue.B;
			}
			return value;
		}

		private AttributeValue GetAttValue(int i)
		{
			return (AttributeValue)selectList[i].GetValue(NewServiceContext(), new DynamoDBRecordEntry(Items[mCurrentPosition]));
		}

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
			return (mCurrentPosition < ItemCount);
		}

		public bool Read()
		{			
			if (mCurrentPosition == ItemCount && mCurrentPosition > 0)
			{
				mResponse = mRequest.Read(mResponse.LastEvaluatedKey);
				/*
				 *
				 * A query and scan operation returns a maximum 1 MB of data in a single operation.
				 * The result set contains the last_evaluated_key field. If more data is available for the operation,
				 * this key contains information about the last evaluated key. Otherwise, the key remains empty.
				 * */
			}
			mCurrentPosition++;
			return (mCurrentPosition < ItemCount);
		}
		internal IOServiceContext NewServiceContext()
		{
			return null;
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
			DateTime dt = new DateTime();
			DateTime.TryParse(GetAttValue(i).S, out dt);
			return dt;
		}

		private AttributeValue GetAttValue(int i)
		{
			return (AttributeValue)block.Item(pos, i);
		}


		public override decimal GetDecimal(int i)
		{
			return decimal.Parse(GetAttValue(i).N);
		}


		public override short GetInt16(int i)
		{
			return short.Parse(GetAttValue(i).N);
		}

		public override int GetInt32(int i)
		{
			return int.Parse(GetAttValue(i).N);
		}

		public override long GetInt64(int i)
		{
			return long.Parse(GetAttValue(i).N);
		}

		public override string GetString(int i)
		{
			return GetAttValue(i).S;
		}

		public override bool GetBoolean(int i)
		{
			return GetAttValue(i).IsBOOLSet;
		}

		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}
	}
}
