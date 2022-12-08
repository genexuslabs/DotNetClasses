using System;
using GeneXus.Application;
using System.Data;
using GeneXus.Configuration;
using GeneXus.Data.NTier.ADO;
using GeneXus.Reorg;
using GeneXus.Performance;
using log4net;
using System.Collections.Generic;
using GeneXus.Data.ADO;
using GeneXus.Utils;
using System.Text;

namespace GeneXus.Data.NTier
{
	public interface IDataStoreProvider
	{
		void execute( int cursor);
		void execute( int cursor, Object[] parms);
		void readNext(int cursor);
		int getStatus(int cursor);
		void commit(String auditObjectName);
		void rollback(String auditObjectName);
		void commitDataStores(String auditObjectName);
		void rollbackDataStores(String auditObjectName);
		int close(int cursor);
		void dynParam(int cursorId, object [] dynConstraints);
		void setErrorHandler( GxErrorHandler errorHandler);
        void executeBatch(int cursor);
        void addRecord( int cursor, Object[] recordValues);
        void initializeBatch(int cursor, int batchSize, object instance, string method);
        int getBatchSize(int cursor);
        int readNextErrorRecord(int cursor);
        void setErrorBuffers(int cursor, Object[] errorBuffers);
        void setDynamicOrder(int cursor, String[] parameters);
        int recordCount(int cursor);
        DateTime serverNow();
		DateTime serverNowMs();
		string userId();
		GxSmartCacheProvider SmartCacheProvider { get; }
    }

	public interface IRemoteDataStoreProvider
	{
		byte[] execute( byte[] parms);
		byte[] readNext(int cursor);
		int close(int cursor);
		void commitDataStores(String auditObjectName);
		void rollbackDataStores(String auditObjectName);
		void commit();
		void rollback();
	}

	public interface ICursor
	{
		void createCursor( IGxDataStore ds, GxErrorHandler errorHandler );
		void execute();
        short[] preExecute(int cursorNum, IDataStoreProviderBase connectionProvider, IGxDataStore ds);
        void readNext();
		int getStatus();
        void setDynamicOrder( string[] parameters);
		int close();
		IFieldGetter getFieldGetter();
        IFieldGetter getBufferFieldGetter();
        IFieldSetter getFieldSetter();
		string SQLStatement{get;}
		string Id{get;}
        void addRecord(Object[] parms);
        int BatchSize { get; set;}
        int RecordCount { get;}
        void OnCommitEvent(object instance, string method);
        int readNextErrorRecord();
		List<ParDef> DynamicParameters { get; }
    }
	public interface IFieldGetter
	{
		IDataReader DataReader { get; set;}
		bool wasNull(int id);
		short getShort(int id);
		int getInt(int id);
		long getLong(int id);
		double getDouble(int id);
		Decimal getDecimal(int id);
		string getString(int id, int size);
		DateTime getDateTime(int id);
        DateTime getDateTime(int id, Boolean precision);
        string getLongVarchar(int id);
		DateTime getGXDateTime(int id);
        DateTime getGXDateTime(int id, Boolean precision);
        DateTime getGXDate(int id);
		string getBLOBFile(int id);
		string getBLOBFile(int id, string extension, string name);
		string getMultimediaFile(int id, string name);
		string getMultimediaUri(int id);
		string getMultimediaUri(int id, bool absUrl);
		Utils.IGeographicNative getGeospatial(int id);
		string getVarchar(int id);
		decimal getBigDecimal(int id, int dec);
        bool getBool(int id);
        Guid getGuid(int id);
    }
	public interface IFieldSetter
	{
        void SetParameter(int id, Utils.IGeographicNative parm);
		void SetParameter(int id, Utils.IGeographicNative parm, GXType type);
		void SetParameterObj(int id, object parm);
        void SetParameter(int id, Guid parm);
        void SetParameter(int id, bool parm);
        void SetParameter(int id, short parm);
		void SetParameter( int id, int parm);
		void SetParameter( int id, long parm);
		void SetParameter( int id, double parm);
		void SetParameter( int id, Decimal parm);
		void SetParameter( int id, string parm);
		void SetParameter( int id, DateTime parm);
        void SetParameterDatetime(int id, DateTime value);
        void SetParameterDatetime(int id, DateTime value, Boolean precision);
        void RegisterOutParameter(int id, Object type);
		void RegisterInOutParameter(int id, Object type);
		void setNull(int index, Object sqlType);
		void SetParameterBlob(int id, string parm, bool dbBlob);
		void SetParameterLVChar( int id, string parm);
		void SetParameterChar( int id, string parm);
		void SetParameterVChar(int id, string parm);
		void SetParameterMultimedia(int id, string parm, string imgParm);
		void SetParameterMultimedia(int id, string parm, string imgParm, string tableName, string fieldName);
		void SetParameterRT(string name, string value);
		void RestoreParametersRT();
		List<ParDef> ParameterDefinition { get; }
	}
	public interface IDataStoreHelper
	{
		ICursor[] getCursors();
		void getResults(int cursor, IFieldGetter rslt, Object[] buf);
        void getErrorResults(int cursor, IFieldGetter rslt, Object[] buf);
        void setParameters(int cursor, IFieldSetter rslt, Object[] parms);
        string getDataStoreName();
        Object[] getDynamicStatement(int cursor, IGxContext context, object[] dynConstraints);

	}

	public class DataStoreHelperBase
	{
		/*DO NOT ADD INSTANCE VARIABLES IN THIS CLASS, THIS IS REFERENCED BY THE STATIC CURSORDEF ARRAY IN THE XX___DEFAULT, ALL THE VARIABLES HERE LIVE FOREVER*/
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(DataStoreHelperBase));
		private const string AND = " and ";
		private const string WHERE = " WHERE ";

		public virtual string getDataStoreName()
		{
			return Preferences.DefaultDatastore;
		}
		public void setParameters(int cursor,
							   IFieldSetter stmt,
							   Object[] parms)
		{
			List<ParDef> parmdefs = stmt.ParameterDefinition;
			int idx = 0;
			int idxParmCollection = 1;
			object[] parmsValues = new object[parmdefs.Count];
			foreach (ParDef pdef in parmdefs)
			{
				bool valueIsNull = false;
				try
				{
					if (pdef.InOut)
					{
						stmt.RegisterInOutParameter(idxParmCollection, null);
					}
					else if (pdef.Out)
					{
						stmt.RegisterOutParameter(idxParmCollection, null);
						goto Increment;
					}

					if (pdef.Nullable)
					{
						valueIsNull = (bool)parms[idx];
						if (valueIsNull)
						{
							stmt.setNull(idxParmCollection, DBNull.Value);
						}
						idx += 1;
					}
					parmsValues[idxParmCollection - 1] = parms[idx];
					if (!valueIsNull)
					{
						if (pdef.Return)
						{
							stmt.SetParameterRT(pdef.Name, (string)parms[idx]);
						}
						else
						{
							switch (pdef.GxType)
							{
								case GXType.Char:
								case GXType.NChar:
								case GXType.VarChar:
									if (pdef.AddAtt && !pdef.Preload)
									{
										if (!string.IsNullOrEmpty(pdef.Tbl) && !string.IsNullOrEmpty(pdef.Fld))
											stmt.SetParameterMultimedia(idxParmCollection, (string)parms[idx], (string)parmsValues[pdef.ImgIdx], pdef.Tbl, pdef.Fld);
										else
											stmt.SetParameterMultimedia(idxParmCollection, (string)parms[idx], (string)parmsValues[pdef.ImgIdx]);
									}
									else
									{
										if (pdef.GxType == GXType.VarChar)
										{
											if (pdef.ChkEmpty)
												stmt.SetParameterVChar(idxParmCollection, (string)parms[idx]);
											else
												stmt.SetParameterObj(idxParmCollection, parms[idx]);
										}
										else
										{
											if (pdef.ChkEmpty)
												stmt.SetParameterChar(idxParmCollection, (string)parms[idx]);
											else
												stmt.SetParameter(idxParmCollection, (string)parms[idx]);
										}
									}
									break;
								case GXType.NVarChar:
									if (pdef.ChkEmpty)
										stmt.SetParameterVChar(idxParmCollection, (string)parms[idx]);
									else
										stmt.SetParameter(idxParmCollection, (string)parms[idx]);
									break;
								case GXType.NClob:
								case GXType.Clob:
								case GXType.LongVarChar:
									if (pdef.ChkEmpty)
										stmt.SetParameterLVChar(idxParmCollection, (string)parms[idx]);
									else
										stmt.SetParameter(idxParmCollection, (string)parms[idx]);
									break;
								case GXType.DateAsChar:
								case GXType.Date:
									stmt.SetParameter(idxParmCollection, (DateTime)parms[idx]);
									break;
								case GXType.DateTime:
									stmt.SetParameterDatetime(idxParmCollection, (DateTime)parms[idx]);
									break;
								case GXType.DateTime2:
									stmt.SetParameterDatetime(idxParmCollection, (DateTime)parms[idx], true);
									break;
								case GXType.Blob:
									stmt.SetParameterBlob(idxParmCollection, (string)parms[idx], pdef.InDB);
									break;
								case GXType.UniqueIdentifier:
									stmt.SetParameter(idxParmCollection, (Guid)parms[idx]);
									break;
								case GXType.Geography:
								case GXType.Geopoint:
								case GXType.Geoline:
								case GXType.Geopolygon:
									stmt.SetParameter(idxParmCollection, (Geospatial)parms[idx], pdef.GxType);
									break;
								default:
									stmt.SetParameterObj(idxParmCollection, parms[idx]);
									break;
							}
						}
					}
				Increment:
					idx += 1;
					if (!pdef.Return)
					{
						idxParmCollection += 1;
					}

				}
				catch (InvalidCastException ex)
				{
					string msg = this.GetType() + ".setParameters error  parameterName:" + pdef.Name + " parameterType:" + pdef.GxType;
					GXLogging.Error(log, ex, msg + " value:" + parms[idx]);
					throw new Exception("Invalid parameter conversion at " + msg, ex);
				}
			}
		}
		[Obsolete("getDynamicStatement with 2 arguments is deprecated", false)]
        public virtual Object[] getDynamicStatement(int cursor, object[] dynConstraints)
        {
            return null;
        }
        public virtual Object[] getDynamicStatement(int cursor, IGxContext context, object[] dynConstraints)
        {
            return null;
        }
        public virtual void getErrorResults(int cursor, IFieldGetter rslt, Object[] buf)
        { 
        }
		public void AddWhere(StringBuilder currentWhere, string condition)
		{
			if (currentWhere.Length > 0)
				currentWhere.Append(AND);
			else
				currentWhere.Append(WHERE);
			currentWhere.Append(condition);
		}
	}

	public class DataStoreUtil
	{
		public static void LoadDataStores(IGxContext context)
		{
			string strcount;
			string id;
			if ( Config.GetValueOf( "DataStore-Count", out strcount) )
			{
				int count = Convert.ToInt32(strcount);
                bool error=false;
				for (int i=0; i<count; i++)
				{
					if (Config.GetValueOf("DataStore"+(i+1), out id))
					{
						string dbms;
						Config.GetValueOf(Config.DATASTORE_SECTION + id, "Connection-" + id + "-DBMS",out dbms);

                        if (dbms != null && !error)
						{
                            if (dbms.IndexOf(',')>0)
                            {
                                error = true;
                                dbms = dbms.Split(',')[0];
                            }
							context.AddDataStore(new GeneXus.Data.ADO.GxDataStore(id,context));
						}
					}
				}
			}
		}
	
	}
	public interface IDataStoreProviderBase
	{
		void dynParam(int cursorId, Object [] dynConstraints);
		Object [] getDynConstraints();
        IGxContext context{get;}
	}

	public class DataStoreProvider : IDataStoreProviderBase,IDataStoreProvider
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.NTier.DataStoreProvider));
		ICursor[] _cursor;
		Object[][] results;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		IDictionary<int, Object[]> errorBuffers;
		IGxDataStore _ds;
		IDataStoreHelper _dataStoreHelper;
		object [] _dynConstraints;
		GxErrorHandler _errorHandler;
        IGxContext _context;
		WMIDataStoreProvider wmiDataStoreProvider;
		private static int dataStoreRequestCount;

        public IGxContext context { get{return _context;} }

		public DataStoreProvider( IGxContext context, IDataStoreHelper dataStoreHelper, Object[][] cursorParms)
		{
			GXLogging.Debug(log, "Start DataStoreProvider.Ctr, Parameters: handle '"+ context.handle + "', dataStoreHelper:" + dataStoreHelper.GetType());
			_dataStoreHelper = dataStoreHelper;
            _context = context;
			_ds = context.GetDataStore( dataStoreHelper.getDataStoreName());
			if (_ds == null)
			{
				
                _ds = new GxDataStore(new GxSqlServer(),dataStoreHelper.getDataStoreName(), context);
                context.AddDataStore(_ds);
                GXLogging.Error(log, dataStoreHelper.GetType() + " Datastore " + dataStoreHelper.getDataStoreName() + " not found in app config");
			}
			_ds.Handle=context.handle; 
			_cursor = dataStoreHelper.getCursors();
			results = cursorParms;
            errorBuffers = new Dictionary<int, Object[]>();
			if (Preferences.Instrumented)
			{
				wmiDataStoreProvider = WMIDataStoreProviders.Instance().AddDataStoreProvider(dataStoreHelper.GetType().ToString());
			}
			dataStoreRequestCount++;

		}
		public GxSmartCacheProvider SmartCacheProvider
		{
			get
			{
				return _ds.SmartCacheProvider;
			}
		}
		ICursor getCursor( int cursor)
		{
			return _cursor[cursor];
		}

        public void setDynamicOrder(int cursor, String[] parameters)
        {
            _cursor[cursor].setDynamicOrder((String[])parameters);
        }

        public void execute( int cursor)
		{
			execute( cursor, null);
		}
        public void executeBatch(int cursor)
        {
            execute(cursor, null, true);
        }
        public void execute(int cursor, Object[] parms)
        {
            execute(cursor, parms, false);
        }
        private void execute(int cursor, Object[] parms, bool batch)
        {
            ICursor oCur = getCursor(cursor);

            if (GxContext.isReorganization && oCur is ForEachCursor && GXReorganization.ExecutedBefore(oCur.Id))
            {
                return;
            }			
            if (!batch)
            {
                oCur.createCursor(_ds, _errorHandler);
            }
			short[] parmHasValue = oCur.preExecute(cursor, this, _ds);
			if (Preferences.Instrumented && wmiDataStoreProvider != null)
			{
				wmiDataStoreProvider.IncSentencesCount(oCur);
				wmiDataStoreProvider.BeginExecute(oCur, _ds.Connection);
			}
			try
			{
				if (!batch)
				{
					try
					{

						if (parmHasValue != null)
						{
							List<ParDef> pdefList = oCur.DynamicParameters;
							if (pdefList.Count == 0) //Backward compatibility
							{
								Object[] parmsNew = new Object[parms.Length + parmHasValue.Length];
								parmHasValue.CopyTo(parmsNew, 0);
								parms.CopyTo(parmsNew, parmHasValue.Length);
								_dataStoreHelper.setParameters(cursor, oCur.getFieldSetter(), parmsNew);
							}
							else
							{
								List<object> parmsNew = new List<object>();
								int idx = 0;
								for (int i=0; i< pdefList.Count; i++)
								{
									ParDef pdef = pdefList[i];
									if (pdef.Nullable)
									{
										if (parmHasValue[i] == 0)
										{
											parmsNew.Add(parms[idx]);
										}
										idx += 1;
									}
									if (parmHasValue[i] == 0)
									{
										parmsNew.Add(parms[idx]);
									}
									idx += 1;
								}
								_dataStoreHelper.setParameters(cursor, oCur.getFieldSetter(), parmsNew.ToArray());
							}
						}
						else
						{
							_dataStoreHelper.setParameters(cursor, oCur.getFieldSetter(), parms);
						}
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, "Execute error", ex);
						_ds.CloseConnections();
						throw ex;
					}
				}

				GXLogging.Debug(log, "gxObject:" + _dataStoreHelper.GetType() + ", handle '" + _ds.Handle + "' cursorName:" + oCur.Id);
				oCur.execute();
			}finally
			{
				oCur.getFieldSetter().RestoreParametersRT();
			}

			_dataStoreHelper.getResults(cursor, oCur.getFieldGetter(), results[cursor]);
			
            _dynConstraints = null;

            if (Preferences.Instrumented)
            {
                wmiDataStoreProvider.EndExecute(oCur, _ds.Connection);
            }
            dataStoreRequestCount++;
        }


		public void readNext(int cursor)
		{
			Cursor oCur = getCursor(cursor) as Cursor;
			try
			{
				if (oCur != null)
				{
					oCur.readNext();
					_dataStoreHelper.getResults(cursor, oCur.getFieldGetter(), results[cursor]);
					dataStoreRequestCount++;
				}
			}
			catch (GxADODataException e)
			{
				bool retry = false;
				int retryCount = 0;
				bool pe = oCur.Command.ProcessException(e, ref retry, retryCount, "FETCH");
				GXLogging.Error(log, "readNext Error", e);
				if (!pe)
				{
					throw;
				}
			}

		}
		public int getStatus(int cursorIdx)
		{
            ICursor cursor = getCursor(cursorIdx);
            if (GxContext.isReorganization && cursor is ForEachCursor && GXReorganization.ExecutedBefore(cursor.Id))
            {
                return Cursor.EOF;
            }
            else
            {
                return cursor.getStatus();
            }
		}
		public void commit(String auditObjectName)
		{
			commitDataStore(_ds, auditObjectName);
		}
		public void commitDataStores(String auditObjectName)
		{
			foreach (IGxDataStore ds in context.DataStores)
			{
				commitDataStore(ds, auditObjectName);
			}
		}
		public void rollback(String auditObjectName)
		{
			rollbackDataStore(_ds, auditObjectName);
        }
		public void rollbackDataStores(String auditObjectName)
		{
			foreach (IGxDataStore ds in context.DataStores)
			{
				rollbackDataStore(ds, auditObjectName);
			}
		}
		private void commitDataStore(IGxDataStore ds, String auditObjectName)
		{

			GXLogging.Debug(log, "DataStoreProvider commit auditObjectName:" + auditObjectName);
			GxCommand cmd = new GxCommand(ds.Db, "commit", ds, 0, false, true, _errorHandler);
			cmd.ErrorMask = GxErrorMask.GX_NOMASK;
			try
			{
				ds.Commit();
			}
			catch (Exception dbEx)
			{
				//If commit fails it should not retry, it makes no sense because it will no longer be possible. just close the existing connection.
				GxADODataException e = new GxADODataException(dbEx);
				bool retry = false;
				int retryCount = 0;
				GXLogging.Error(log, "Commit Transaction Error", e);
				bool pe = cmd.ProcessException(e, ref retry, retryCount, "FETCH");
				if (!pe)
				{
					try
					{
						if (retry)
							ds.Connection.Open();
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, "beginTransaction in commit transaction failed", ex);
						throw (new GxADODataException(e.ToString(), e));
					}
				}
			}
			cmd.Dispose();

		}
		private void rollbackDataStore(IGxDataStore ds, String auditObjectName)
		{
			GxCommand cmd = new GxCommand(ds.Db, "rollback", ds, 0, false, true, _errorHandler);
			cmd.ErrorMask = GxErrorMask.GX_NOMASK;
			try
			{
				ds.Rollback();
			}
			catch (Exception dbEx)
			{
				GxADODataException e = new GxADODataException(dbEx);
				bool retry = false;
				int retryCount = 0;
				bool pe = cmd.ProcessException(e, ref retry, retryCount, "FETCH");
				GXLogging.Error(log, "Rollback Transaction Error", e);
				if (!pe)
				{
					try
					{
						if (retry)
							ds.Connection.Open();
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, "beginTransaction in Rollback transaction failed", ex);
						throw (new GxADODataException(e.ToString(), e));
					}
				}
			}
			cmd.Dispose();
		}
		public int close(int cursor)
		{
            int result = 0;
            ICursor oCur = getCursor(cursor);
            ForEachCursor fc = oCur as ForEachCursor;
            if (GxContext.isReorganization && fc!=null && GXReorganization.ExecutedBefore(oCur.Id))
            {
                return result;
            }
            else
            {
                dataStoreRequestCount++;
                result = oCur.close();
                if (GxContext.isReorganization && fc!=null)
                {
                    GXReorganization.AddExecutedStatement(oCur.Id);
                }
                return result;
            }
		}
		public void dynParam(int cursorId, object [] dynConstraints)
		{
			this._dynConstraints = dynConstraints;
		}
	
		public object [] getDynConstraints()
		{
			return (object[])_dynConstraints[0];
		}
		public void setErrorHandler( GxErrorHandler errorHandler)
		{
			_errorHandler = errorHandler;
		}
        public void addRecord(int cursor, Object[] parms) {
            ICursor oCur = getCursor(cursor);
			_dataStoreHelper.setParameters(cursor, oCur.getFieldSetter(), parms);
			oCur.addRecord(parms);
        }
        public int recordCount(int cursor)
        {
            ICursor oCur = getCursor(cursor);
            return oCur.RecordCount;
        }

        public void initializeBatch(int cursor, int batchSize, object instance, string method)
        {
            ICursor oCur = getCursor(cursor);
            if (oCur.BatchSize == 0)
            {
                oCur.BatchSize = batchSize;
                oCur.createCursor(_ds, _errorHandler);
                oCur.OnCommitEvent(instance, method);
            }
        }
        public int getBatchSize(int cursor)
        {
            ICursor oCur = getCursor(cursor);
            return oCur.BatchSize;
        }
        public int readNextErrorRecord(int cursor)
        {
            ICursor oCur = getCursor(cursor);
            int res = oCur.readNextErrorRecord();
            if (res == 1)
            {
                _dataStoreHelper.getErrorResults(cursor, oCur.getBufferFieldGetter(), (Object[])errorBuffers[cursor]);
            }
            dataStoreRequestCount++;
            return res;
        }
        public void setErrorBuffers(int cursor, Object[] errorBuffers)
        {
            this.errorBuffers[cursor] = errorBuffers;

        }
        public string userId()
        {
            GxConnection gxconn = _ds.Connection as GxConnection;
            if (gxconn != null && !String.IsNullOrEmpty(gxconn.InternalUserId))
            {
                return gxconn.InternalUserId;
            }
            else
            {
                GXLogging.Debug(log, "UserId method");
                string stmt = ((GxDataRecord)_ds.Db).GetServerUserIdStmt();
                if (string.IsNullOrEmpty(stmt))
                    return string.Empty;
                else
                {
                    GxCommand cmd = new GxCommand(_ds.Db, stmt, _ds, 0, false, true, _errorHandler);
                    cmd.ErrorMask = GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK;
                    IDataReader reader;
                    cmd.FetchData(out reader);
                    string s = string.Empty;
                    if (reader != null)
                    {
                        s = reader.GetString(0);
                        reader.Close();
                    }
                    return s;
                }
            }
        }

		public DateTime serverNowMs()
		{
			return serverNowIn(true);
		}

		public DateTime serverNow()
		{
			return serverNowIn(false);
		}
		public DateTime serverNowIn(bool hasMilliseconds )
        {
			string stmt = "";
			if (hasMilliseconds)
				stmt = ((GxDataRecord)_ds.Db).GetServerDateTimeStmtMs(_ds.Connection); 
			else
				stmt = ((GxDataRecord)_ds.Db).GetServerDateTimeStmt(_ds.Connection);

            if (string.IsNullOrEmpty(stmt))
            {
				if (hasMilliseconds)
					return DateTimeUtil.ResetMicroseconds(DateTime.Now);
				else
					return DateTimeUtil.ResetMilliseconds(DateTime.Now);
			}
            else
            {
                GxCommand cmd = new GxCommand(_ds.Db, stmt, _ds, 0, false, true, _errorHandler);
                cmd.ErrorMask = GxErrorMask.GX_NOMASK | GxErrorMask.GX_MASKLOOPLOCK;
                IDataReader reader;
                cmd.FetchData(out reader);
                DateTime d = DateTimeUtil.NullDate();
                if (reader != null)
                {
                    d = reader.GetDateTime(0);
					if (hasMilliseconds)
						d = DateTimeUtil.ResetMicroseconds(d);
					else
					    d = DateTimeUtil.ResetMilliseconds(d);
                    reader.Close();
                }
                return d;
            }

        }
    }

}