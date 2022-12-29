using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Data.NTier;
using GeneXus.Data.NTier.CosmosDB;
using log4net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace GeneXus.Data.Cosmos
{
	public class CosmosDBDataReader : IDataReader
	{
		private readonly RequestWrapper m_request;
		private ResponseWrapper m_response;
		private readonly IODataMap2[] m_selectList;
		private FeedIterator m_feedIterator;
		private CosmosDBRecordEntry m_currentEntry;
		private int m_currentPosition;

		private int ItemCount;
		private List<Dictionary<string, object>> Items = null;

		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(CosmosDBDataReader));
		private void CheckCurrentPosition()
		{
			if (m_currentEntry == null)
				throw new ServiceException(ServiceError.RecordNotFound);
		}
		private void ProcessPKStream(Stream stream)
		{
			//Query by PK -> only one record

			if (stream != null)
			{ 
				using (StreamReader sr = new StreamReader(stream))
				using (JsonTextReader jtr = new JsonTextReader(sr))
				{
					Newtonsoft.Json.JsonSerializer jsonSerializer = new Newtonsoft.Json.JsonSerializer();
					object array = jsonSerializer.Deserialize<object>(jtr);

					Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(array.ToString());

					//remove metadata
					result.Remove("_rid");
					result.Remove("_self");
					result.Remove("_etag");
					result.Remove("_attachments");
					result.Remove("_ts");

					Items = new List<Dictionary<string, object>>();
					Items.Add(result);

					if (Items != null)
						ItemCount = Items.Count;
					else
						ItemCount = 0;
				}
			}
		}
		public CosmosDBDataReader(ServiceCursorDef cursorDef, RequestWrapper request)
		{
			Query query = cursorDef.Query as Query;
			if (query != null)
				m_selectList = query.SelectList.ToArray();
			m_request = request;
			m_response = m_request.Read();
			m_feedIterator = m_response.feedIterator;
			if (m_feedIterator == null)
			{
				if (m_response != null)
				{ 
					Items = m_response.Items;
					ItemCount = Items.Count;
					m_currentPosition = -1;
				}
			}
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
				return m_selectList.Length;
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
			if (m_request != null)
			{
				m_request.Close();
			}
		}
		public void Dispose()
		{
		}
		public long getLong(int i)
		{
			return Convert.ToInt64(GetAttValue(i));
		}

		private object GetAttValue(int i)
		{
			return (m_selectList[i].GetValue(CosmosDBConnection.NewServiceContext(), m_currentEntry));
		}
		public bool GetBoolean(int i)
		{
			if (GetAttValue(i) is bool value)
			{
				return value;
			}
			return false;
		}

		public byte GetByte(int i)
		{
			return Convert.ToByte(GetAttValue(i));
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			MemoryStream ms = (MemoryStream)GetAttValue(i);
			if (ms == null)
				return 0;
			ms.Seek(fieldOffset, SeekOrigin.Begin);
			return ms.Read(buffer, bufferoffset, length);
		}
		public char GetChar(int i)
		{
			return Convert.ToChar(GetAttValue(i));
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
		   if (GetAttValue(i) is DateTime value)
				return value;
			return default(DateTime);
		//	DateTime.TryParse(GetAttValue(i).ToString(), null, DateTimeStyles.AdjustToUniversal, out DateTime dt);
		//	return dt;
		}
		public decimal GetDecimal(int i)
		{
			return Convert.ToDecimal(GetAttValue(i));
		}
		public double GetDouble(int i)
		{
			return Convert.ToDouble(GetAttValue(i));
		}
		public Type GetFieldType(int i)
		{
			return m_selectList[i].GetValue(CosmosDBConnection.NewServiceContext(), m_currentEntry).GetType();
		}

		public float GetFloat(int i)
		{
			return Convert.ToSingle(GetAttValue(i));
		}
		public Guid GetGuid(int i)
		{
			return new Guid(GetAttValue(i).ToString());
		}

		public short GetInt16(int i)
		{
			return Convert.ToInt16(GetAttValue(i));
		}

		public int GetInt32(int i)
		{
			return Convert.ToInt32(GetAttValue(i));
		}

		public long GetInt64(int i)
		{
			return Convert.ToInt64(GetAttValue(i)); ;
		}

		public string GetName(int i)
		{
			return m_selectList[i].GetName(null);
		}

		public int GetOrdinal(string name)
		{
			CheckCurrentPosition();
			int ordinal = m_currentEntry.CurrentRow.ToList().FindIndex(col => col.Key.ToLower() == name.ToLower());
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
			return GetAttValue(i).ToString();
		}
		public int GetValues(object[] values)
		{
			System.Diagnostics.Debug.Assert(m_selectList.Length == values.Length, "Values mismatch");
			for (int i = 0; i < m_selectList.Length && i < values.Length; i++)
			{
				values[i] = GetAttValue(i);
			}
			return m_selectList.Length;
		}
		public bool IsDBNull(int i)
		{
			return GetAttValue(i) == null;
		}

		public bool NextResult()
		{
			m_currentPosition++;
			m_currentEntry = (m_currentPosition < ItemCount) ? new CosmosDBRecordEntry(Items[m_currentPosition]) : null;
			return m_currentEntry != null;
		}

		private async Task<bool> GetPage()
		{
			while (m_feedIterator.HasMoreResults)
			{
				try
				{
					using (ResponseMessage response = await m_feedIterator.ReadNextAsync().ConfigureAwait(false))
					{
						
						if (!response.IsSuccessStatusCode)
						{
							if (response.Diagnostics != null)
								GXLogging.Debug(logger, $"Read ItemStreamFeed Diagnostics: {response.Diagnostics.ToString()}");
								throw new Exception(GeneXus.Data.Cosmos.CosmosDBHelper.FormatExceptionMessage(response.StatusCode.ToString(),response.ErrorMessage));
						}
						else
						{ 
							using (StreamReader sr = new StreamReader(response.Content))
							using (JsonTextReader jtr = new JsonTextReader(sr))
							{
								Newtonsoft.Json.JsonSerializer jsonSerializer = new Newtonsoft.Json.JsonSerializer();
								object array = jsonSerializer.Deserialize<object>(jtr);

								string json = ((Newtonsoft.Json.Linq.JToken)array).Root.ToString();
								var jsonDocument = JsonDocument.Parse(json);
								var jsonDoc = jsonDocument.RootElement;
								foreach (var jsonProperty in jsonDoc.EnumerateObject())
								{
									if (jsonProperty.Name == "Documents")
									{
										Items = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonProperty.Value.ToString());
										break;
									}
								}
								if (Items != null)
									ItemCount = Items.Count;
								else
									ItemCount = 0;
							}
						}
					}
					return true;
				}
				catch (CosmosException ex)
				{
					GXLogging.Error(logger, ex);
					throw ex;
				}
			}
			return false;
		}
		public bool Read()
		{
			Task<bool> task;
			if (NextResult())
				return true;
			else
				if (m_feedIterator != null)
				{ 
					task = Task.Run<bool>(async () => await GetPage().ConfigureAwait(false));
					if (task.Result)
					{
						m_currentPosition = -1;
						return NextResult();
					}
				}
			return false;
		}

		public object GetValue(int i)
		{
			return GetAttValue(i);
		}
	}

}
