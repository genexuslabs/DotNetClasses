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

		public virtual string getDataStoreName()
		{
			return "Default";
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
							Object[] parmsNew = new Object[parms.Length + parmHasValue.Length];
							parmHasValue.CopyTo(parmsNew, 0);
							parms.CopyTo(parmsNew, parmHasValue.Length);
							_dataStoreHelper.setParameters(cursor, oCur.getFieldSetter(), parmsNew);
						}
						else
						{
							_dataStoreHelper.setParameters(cursor, oCur.getFieldSetter(), parms);
						}
					}
					catch (Exception ex)
					{
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
			ICursor oCur = getCursor(cursor);
			oCur.readNext();
			_dataStoreHelper.getResults( cursor, oCur.getFieldGetter(), results[cursor]);
			dataStoreRequestCount++;

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
				int status = 0;
				GxADODataException e = new GxADODataException(dbEx);
				bool retry = false;
				int retryCount = 0;
				bool pe = ds.Connection.DataRecord.ProcessError(e.DBMSErrorCode, e.ErrorInfo, cmd.ErrorMask, ds.Connection, ref status, ref retry, retryCount);
				GXLogging.Error(log, "Commit Transaction Error", e);
				retryCount++;
				cmd.processErrorHandler(status, e.DBMSErrorCode, e.SqlState, e.ErrorInfo, cmd.ErrorMask, "FETCH", ref pe, ref retry);
				if (!pe)
				{
					try
					{
						ds.Connection.Close();
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
				int status = 0;
				GxADODataException e = new GxADODataException(dbEx);
				bool retry = false;
				int retryCount = 0;
				bool pe = ds.Connection.DataRecord.ProcessError(e.DBMSErrorCode, e.ErrorInfo, cmd.ErrorMask, ds.Connection, ref status, ref retry, retryCount);
				GXLogging.Error(log, "Rollback Transaction Error", e);
				retryCount++;
				cmd.processErrorHandler(status, e.DBMSErrorCode, e.SqlState, e.ErrorInfo, cmd.ErrorMask, "FETCH", ref pe, ref retry);
				if (!pe)
				{
					try
					{
						ds.Connection.Close();
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