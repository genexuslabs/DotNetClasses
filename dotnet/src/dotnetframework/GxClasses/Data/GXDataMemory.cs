using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GeneXus.Cache;
using GeneXus.Utils;

namespace GeneXus.Data
{
	public class MemoryDataRecord : IDataRecord, IDisposable
	{
        private const string AlreadyClosed = "MemoryDataReader is already closed";
        #region Fields

        private int _depth;

		private bool _isDisposed = false;

		private object[] _values;

		private string[] _names;

		#endregion


		public int Depth
		{
			get
			{
				return this._depth;
			}
		}


		#region Constructors

		public MemoryDataRecord(IList list)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));

			this._values = new object[list.Count];
			this._names = Array.Empty<string>();

			for (int fi = 0; fi < this._values.Length; fi++)
			{
				this._values[fi] = MemoryDataHelper.GetCopyFrom(list[fi]);
			}
		}

		public MemoryDataRecord(IList list, int depth)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));

			this._values = new object[list.Count];
			this._names = Array.Empty<string>();

			for (int fi = 0; fi < this._values.Length; fi++)
			{
				this._values[fi] = MemoryDataHelper.GetCopyFrom(list[fi]);
			}

			this._depth = depth;
		}

		public MemoryDataRecord(object[] list)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));

			this._values = new object[list.Length];
			this._names = Array.Empty<string>();

			for (int fi = 0; fi < this._values.Length; fi++)
			{
				this._values[fi] = MemoryDataHelper.GetCopyFrom(list[fi]);
			}
		}

		public MemoryDataRecord(object[] list, int depth)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));

			this._values = new object[list.Length];
			this._names = Array.Empty<string>();

			for (int fi = 0; fi < this._values.Length; fi++)
			{
				this._values[fi] = MemoryDataHelper.GetCopyFrom(list[fi]);
			}

			this._depth = depth;
		}

		public MemoryDataRecord(IDictionary dict)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));

			this._values = new object[dict.Count];
			this._names = new string[dict.Count];

			int index = 0;
			foreach (object fKey in dict.Keys)
			{
				this._values[index] = MemoryDataHelper.GetCopyFrom(dict[fKey]);
				this._names[index] = fKey.ToString();

				index++;
			}
		}

		public MemoryDataRecord(IDictionary dict, int depth)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));

			this._values = new object[dict.Count];
			this._names = new string[dict.Count];

			int index = 0;
			foreach (object fKey in dict.Keys)
			{
				this._values[index] = MemoryDataHelper.GetCopyFrom(dict[fKey]);
				this._names[index] = fKey.ToString();

				index++;
			}

			this._depth = depth;
		}

		public MemoryDataRecord(IDataRecord record)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));

			this._values = new object[record.FieldCount];

			for (int fi = 0; fi < this._values.Length; fi++)
			{
				this._values[fi] = MemoryDataHelper.GetCopyFrom(record[fi]);
			}

			this._names = Array.Empty<string>();
		}

		public MemoryDataRecord(IDataRecord record, int depth)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));

			this._values = new object[record.FieldCount];

			for (int fi = 0; fi < this._values.Length; fi++)
			{
				this._values[fi] = MemoryDataHelper.GetCopyFrom(record[fi]);
			}

			this._names = Array.Empty<string>();
			this._depth = depth;
		}

		public MemoryDataRecord(IDataRecord record, string[] colnames)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));

			this._values = new object[record.FieldCount];

			for (int fi = 0; fi < this._values.Length; fi++)
			{
				this._values[fi] = MemoryDataHelper.GetCopyFrom(record[fi]);
			}

			if (colnames == null)
				this._names = Array.Empty<string>();
			else
				this._names = colnames;
		}

		public MemoryDataRecord(IDataRecord record, string[] colnames, int depth)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));

			this._values = new object[record.FieldCount];

			for (int fi = 0; fi < this._values.Length; fi++)
			{
				this._values[fi] = MemoryDataHelper.GetCopyFrom(record[fi]);
			}

			if (colnames == null)
				this._names = Array.Empty<string>();
			else
				this._names = colnames;

			this._depth = depth;
		}

		#endregion


		#region IDataRecord Member

		public int FieldCount
		{
			get
			{
				if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

				return this._values.Length;
			}
		}

		public bool GetBoolean(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

            return (bool)this._values[i];
		}

		public byte GetByte(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (byte)this._values[i];
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
            byte[] bytes = (byte[])this._values[i];

            long longLength = Math.Min(bytes.Length - fieldOffset, length);
            Array.Copy(bytes, fieldOffset, buffer, (long)bufferoffset, longLength);

			return longLength;
		}

		public char GetChar(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (char)this._values[i];
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			Array.Copy((char[])this._values[i], fieldoffset, buffer, (long)bufferoffset, (long)length);

			return length;
		}

		public IDataReader GetData(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (IDataReader)this._values[i];
		}

		public string GetDataTypeName(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._values[i].GetType().Name;
		}

		public DateTime GetDateTime(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (DateTime)this._values[i];
		}

		public decimal GetDecimal(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (decimal)this._values[i];
		}

		public double GetDouble(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (double)this._values[i];
		}

		public Type GetFieldType(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._values[i].GetType();
		}

		public float GetFloat(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (float)this._values[i];
		}

		public Guid GetGuid(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (Guid)this._values[i];
		}

		public short GetInt16(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (short)this._values[i];
		}

		public int GetInt32(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (int)this._values[i];
		}

		public long GetInt64(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (long)this._values[i];
		}

		public string GetName(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			if (i < this._names.Length)
				return this._names[i];
			else
				return "";
		}

		public int GetOrdinal(string name)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			for (int fi = 0; fi < this._names.Length; fi++)
			{
				if (this._names[fi].ToLower() == name.ToLower()) return fi;
			}

			return -1;
		}

		public string GetString(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return (string)this._values[i];
		}

		public object GetValue(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._values[i];
		}

		public int GetValues(object[] values)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			Array.Copy(this._values, values, this._values.Length);

			return this._values.Length;
		}

		public bool IsDBNull(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._values[i] == DBNull.Value;
		}

		public object this[string name]
		{
			get
			{
				if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

				int index = this.GetOrdinal(name);

				if (index == -1)
					throw new ArgumentOutOfRangeException(string.Format("The column \"{0}\" was not found.", name));
				else
					return this.GetValue(index);
			}
		}

		public object this[int i]
		{
			get
			{
				if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

				return this.GetValue(i);
			}
		}

		#endregion

		#region IDisposable Member

		public bool IsDisposed
		{
			get
			{
				return this._isDisposed;
			}
		}

		public void Dispose()
		{
			if (!this._isDisposed)
			{
				this._isDisposed = true;

				for (int fi = 0; fi < this._values.Length; fi++)
				{
					try
					{
						if (this._values[fi] is IDisposable)
						{
							((IDisposable)this._values[fi]).Dispose();
						}
					}
					catch (Exception )
					{

					}

					this._values[fi] = null;
				}

				this._values = null;
				this._names = null;
			}
		}

		#endregion
	}

	public class MemoryDataReader : IDataReader
	{
        private const string AlreadyClosed = "MemoryDataReader is already closed";

        #region Fields

        private bool _isDisposed = false;

		private List<MemoryDataRecord> _records = new List<MemoryDataRecord>();

		private DataTable _schemaDataTable = null;
		private IGxConnection con;
		private int _currentIndex = -1;
		private SlidingTime expiration;
		private bool cached;
		private string key;
		private GxArrayList block;
		private long readBytes;

		#endregion


		public bool HasRows
		{
			get
			{
				return this._records.Count > 0;
			}
		}


		#region Constructors

		public MemoryDataReader()
		{

		}

		public MemoryDataReader(IDataReader reader, IGxConnection connection, GxParameterCollection parameters,
			string stmt, ushort fetchSize, bool isForFirst, bool withCached, SlidingTime expiration)
			: this()
		{
			if (reader == null) throw new ArgumentNullException(nameof(reader));
			if (reader.IsClosed) throw new ArgumentException("Reader is closed");

			DataTable schemaTab = reader.GetSchemaTable();
			if (schemaTab != null) this._schemaDataTable = schemaTab.Clone();


			string[] colnames = null;

			while (reader.Read())
			{
				if (colnames == null)
				{
					colnames = new string[reader.FieldCount];
					for (int fi = 0; fi < colnames.Length; fi++)
					{
						colnames[fi] = reader.GetName(fi);
					}
				}

				this.AddRecord(reader, colnames);
			}
			this.cached = withCached;
			this.con = connection;
			block = new GxArrayList(fetchSize);
			if (cached)
			{
				this.key = SqlUtil.GetKeyStmtValues(parameters, stmt, isForFirst);
				this.expiration = expiration;
			}
		}

		~MemoryDataReader()
		{
			if (!this._isDisposed) this.Dispose();
		}

		#endregion

		#region Features

		public void AddRecord(MemoryDataRecord record)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));
			if (record.IsDisposed) throw new ArgumentException("Reader is closed");

			this._records.Add(record);
		}

		public void AddRecord(IDataRecord record)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));

			this.AddRecord(new MemoryDataRecord(record));
		}

		public void AddRecord(IDataRecord record, string[] colnames)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));

			this.AddRecord(new MemoryDataRecord(record, colnames));
		}

		protected void RemoveCurrentRecord()
		{
			this._records[0].Dispose();
			this._records.RemoveAt(0);
		}

		#endregion

		#region IDataReader Member

		public void Close()
		{
			this.Dispose();
		}

		public int Depth
		{
			get
			{
				if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

				return this._records[0].Depth;
			}
		}

		public DataTable GetSchemaTable()
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._schemaDataTable;
		}

		public bool IsClosed
		{
			get { return this._isDisposed; }
		}

		public bool NextResult()
		{
			return this.Read();
		}

		public bool Read()
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			if (this._currentIndex == 0)
				this.RemoveCurrentRecord();
			else
				this._currentIndex = 0;

			if (cached)
			{
				AddToCache(this._records.Count > 0);
			}

			return this._records.Count > 0;
		}
		public void AddToCache(bool hasNext)
		{
			if (hasNext)
			{
				object[] values = new object[FieldCount];
				MemoryDataRecord record = this._records[0];
				record.GetValues(values);
				block.Add(values);
			}
			else
			{
				SqlUtil.AddBlockToCache(key, new CacheItem(block, false, block.Count, readBytes), con, expiration != null ? (int)expiration.ItemSlidingExpiration.TotalMinutes : 0);
			}
		}
		public int RecordsAffected
		{
			get
			{
				throw new GxNotImplementedException();
			}
		}

		#endregion

		#region IDisposable Member

		public void Dispose()
		{
            if (!this._isDisposed)
            {
                this._isDisposed = true;

                this._currentIndex = -1;

                if (this._schemaDataTable != null)
                {
                    this._schemaDataTable.Dispose();
                    this._schemaDataTable = null;
                }

                while (this._records.Count > 0)
                {
                    this.RemoveCurrentRecord();
                }

                this._records = null;
            }
		}

		#endregion

		#region IDataRecord Member

		public int FieldCount
		{
			get
			{
				if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

				return this._records[0].FieldCount;
			}
		}

		public bool GetBoolean(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			readBytes += 1;
			return this._records[0].GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetByte(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			long byteCount = this._records[0].GetBytes(i, fieldOffset, buffer, bufferoffset, length);
			readBytes += byteCount;
			return byteCount;
		}

		public char GetChar(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetChar(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		public IDataReader GetData(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetData(i);
		}

		public string GetDataTypeName(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetDataTypeName(i);
		}

		public DateTime GetDateTime(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			readBytes += 8;
			return this._records[0].GetDateTime(i);
		}

		public decimal GetDecimal(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			readBytes += 12;
			return this._records[0].GetDecimal(i);
		}

		public double GetDouble(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			readBytes += 8;
			return this._records[0].GetDouble(i);
		}

		public Type GetFieldType(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetFieldType(i);
		}

		public float GetFloat(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetFloat(i);
		}

		public Guid GetGuid(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			readBytes += 16;
			return this._records[0].GetGuid(i);
		}
		public short GetInt16(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			readBytes += 2;
			return this._records[0].GetInt16(i);
		}

		public int GetInt32(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			readBytes += 4;
			return this._records[0].GetInt32(i);
		}

		public long GetInt64(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			readBytes += 8;
			return this._records[0].GetInt64(i);
		}

		public string GetName(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetName(i);
		}

		public int GetOrdinal(string name)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetOrdinal(name);
		}

		public string GetString(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);
			string data = this._records[0].GetString(i);
			readBytes += 10 + (2 * data.Length);
			return data;
		}

		public object GetValue(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetValue(i);
		}

		public int GetValues(object[] values)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].GetValues(values);
		}

		public bool IsDBNull(int i)
		{
			if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

			return this._records[0].IsDBNull(i);
		}

		public object this[string name]
		{
			get
			{
				if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

				return this._records[0][name];
			}
		}

		public object this[int i]
		{
			get
			{
				if (this._isDisposed) throw new ObjectDisposedException(AlreadyClosed);

				return this._records[0][i];
			}
		}

		#endregion
	}

	class MemoryDataHelper
	{
		#region Copy objects

		public static object GetCopyFrom(object value)
		{
			if (value == null || value == DBNull.Value)
				return DBNull.Value;
			else
			{
				Type type = value.GetType();

				if (type.IsSubclassOf(typeof(ValueType)))
					return value;
				else if (type == typeof(string))
					return value;
				else if (type == typeof(Stream))
				{
					MemoryStream memStream = new MemoryStream();
					Stream oStream = (Stream)value;
					oStream.CopyTo(memStream);
					return memStream;
				}
                else
				{
                    byte[] valueByte = value as byte[];
                    if (valueByte != null)
                    {
						byte[] newValue = new byte[valueByte.Length];
						Array.Copy(valueByte, newValue, valueByte.Length);
						return newValue;
                    }
                    else
                    {
#pragma warning disable SYSLIB0011 // BinaryFormatter serialization is obsolete and should not be used
						using (MemoryStream memStream = new MemoryStream())
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(memStream, value);

                            object newValue = formatter.Deserialize(memStream);

                            return newValue;
                        }
#pragma warning restore SYSLIB0011 //BinaryFormatter serialization is obsolete and should not be used
					}
				}
			}
		}

		#endregion
	}

}
