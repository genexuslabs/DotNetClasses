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
		private readonly RequestWrapper mRequest;
		private ResponseWrapper mResponse;
		private readonly IODataMap2[] selectList;
		private FeedIterator feedIterator;
		private CosmosDBRecordEntry currentEntry;
		private int mCurrentPosition;

		private int ItemCount;
		private List<Dictionary<string, object>> Items = null;

		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(CosmosDBDataReader));
		private void CheckCurrentPosition()
		{
			if (currentEntry == null)
				throw new ServiceException(ServiceError.RecordNotFound);
		}
		public CosmosDBDataReader(ServiceCursorDef cursorDef, RequestWrapper request)
		{
			Query query = cursorDef.Query as Query;
			selectList = query.SelectList.ToArray();
			mRequest = request;
			mResponse = mRequest.Read();
			feedIterator = mResponse.feedIterator;
			//mCurrentPosition = -1;
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
			return Convert.ToInt64(GetAttValue(i));
		}

		private object GetAttValue(int i)
		{
			return (selectList[i].GetValue(CosmosDBConnection.NewServiceContext(), currentEntry));
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
			return selectList[i].GetValue(CosmosDBConnection.NewServiceContext(), currentEntry).GetType();
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
			return GetAttValue(i).ToString();
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
			return GetAttValue(i) == null;
		}

		public bool NextResult()
		{
			mCurrentPosition++;
			currentEntry = (mCurrentPosition < ItemCount) ? new CosmosDBRecordEntry(Items[mCurrentPosition]) : null;
			return currentEntry != null;
		}

		private async Task<bool> GetPage()
		{
			//Config.LoadConfiguration();
			while (feedIterator.HasMoreResults)
			{
				try
				{
					using (ResponseMessage response = await feedIterator.ReadNextAsync().ConfigureAwait(false))
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
								ItemCount = Items.Count;
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
				task = Task.Run<bool>(async () => await GetPage().ConfigureAwait(false));
				if (task.Result)
					{
						mCurrentPosition = -1;
						return NextResult();
					}
			return false;
		}

		public object GetValue(int i)
		{
			return GetAttValue(i);
		}
	}

}
