using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using GeneXus.Data.NTier;

namespace GeneXus.Data.DynService.Fabric
{
	public class FabricDataReader : IDataReader
	{
		List<IDictionary<String,object>> mResponse;
		private int mCurrentPosition = -1;
		private String[] mProjection = null;
		
		private IDictionary<string, object> currentEntry = new Dictionary<string, object>();
		private IDictionary<string, object> currentRecord = new Dictionary<string, object>();

		internal IDictionary<string, object> CurrentOf { get { return currentRecord; } }
		internal IDictionary<string, object> CurrentOfEntry { get { return currentEntry; } }

		public FabricDataReader(List<IDictionary<String,object>> response, IDataParameterCollection parameters, String[] projection)
		{
			mResponse = response;
			mProjection = projection;
		}
		public object this[string name]
		{
			get
			{
				if (mCurrentPosition >= 0 && mCurrentPosition < mResponse.Count)
				{
					
					return mResponse[mCurrentPosition];
				}
				throw new ArgumentOutOfRangeException(nameof(name));

			}
		}

		public object this[int i]
		{
			get
			{
				if (mCurrentPosition >= 0 && mCurrentPosition < mResponse.Count)
				{
					int j = 0;
					foreach (var col in mResponse)
					{
						if (j == i)
							return col.ToString();
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
				if (mCurrentPosition >= 0 && mCurrentPosition < mResponse.Count)
					return mResponse[mCurrentPosition].Count;
				return 0;
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
		}

		public void Dispose()
		{
		}

		public bool GetBoolean(int i)
		{
			return (mResponse[mCurrentPosition])[mProjection[i]].ToString() == "1";
		}

		public byte GetByte(int i)
		{
			throw new NotImplementedException();
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			return mResponse[mCurrentPosition].ToString().ToCharArray()[0];
				
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			buffer = mResponse[mCurrentPosition].ToString().ToCharArray();
				
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
			
			return DateTime.Parse((mResponse[mCurrentPosition])[mProjection[i]].ToString());
		}

		public decimal GetDecimal(int i)
		{
			
			try
			{
				return decimal.Parse((mResponse[mCurrentPosition])[mProjection[i]].ToString());
			}
			catch (Exception) {
				return 0;
			}

		}

		public double GetDouble(int i)
		{
			return double.Parse((mResponse[mCurrentPosition])[mProjection[i]].ToString());
		}

		public Type GetFieldType(int i)
		{
			
			return typeof(String);
		}

		public float GetFloat(int i)
		{
			return float.Parse((mResponse[mCurrentPosition])[mProjection[i]].ToString());
		}

		public Guid GetGuid(int i)
		{
			return new Guid(mResponse[mCurrentPosition].ToString());
		}

		public short GetInt16(int i)
		{
			return short.Parse((mResponse[mCurrentPosition])[mProjection[i]].ToString());
		}

		public int GetInt32(int i)
		{
			
				return int.Parse((mResponse[mCurrentPosition])[mProjection[i]].ToString());

		}

		public long GetInt64(int i)
		{
			return long.Parse((mResponse[mCurrentPosition])[mProjection[i]].ToString());

		}

		public string GetName(int i)
		{
			
			return "";
			throw new ArgumentOutOfRangeException(nameof(i));
		}

		public int GetOrdinal(string name)
		{
			return 0;
			
		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			return (mResponse[mCurrentPosition])[mProjection[i]].ToString();
		}

		public object GetValue(int i)
		{
			if (mResponse[mCurrentPosition].ContainsKey(mProjection[i]))
			{
				return (mResponse[mCurrentPosition])[mProjection[i]].ToString();
			}
			else
				return null;
		}

		public int GetValues(object[] values)
		{
			System.Diagnostics.Debug.Assert(mResponse[mCurrentPosition].Values.Count == values.Length, "Values mismatch");
			int i = 0;			
			foreach (string attName in mProjection)
			{
				values[i] = mResponse[mCurrentPosition][attName];
				i++;
			}
			return i;
		}

		public bool IsDBNull(int i)
		{
			if (mResponse[mCurrentPosition].ContainsKey(mProjection[i]))
			{
				return (mResponse[mCurrentPosition])[mProjection[i]] == null;
			}
			else
				return false;
		}

		public bool NextResult()
		{
			mCurrentPosition++;
			return (mCurrentPosition < mResponse.Count);
		}

		public bool Read()
		{
			mCurrentPosition++;
			return (mCurrentPosition < mResponse.Count);
		}
	}
}
