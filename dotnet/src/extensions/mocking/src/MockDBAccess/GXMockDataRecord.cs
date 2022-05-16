using System;
using System.Data;
using System.Data.Common;
using GeneXus.Cache;
using GeneXus.Data;
using GeneXus.Data.NTier.ADO;
using GeneXus.Utils;

namespace GeneXus.Data
{
	public class GXMockDataRecord : GxDataRecord
	{
		private GxDataRecord realDataRecord;

		public GXMockDataRecord(GxDataRecord innerInstance)
		{
			realDataRecord = innerInstance;
		}

		public override bool AllowsDuplicateParameters { get => realDataRecord.AllowsDuplicateParameters; }

		public override bool SupportUpdateBatchSize { get => realDataRecord.SupportUpdateBatchSize; }

		public override string DataSource { get => realDataRecord.DataSource; set => realDataRecord.DataSource = value; }

		public override string DataBaseName { get => realDataRecord.DataBaseName; set => realDataRecord.DataBaseName = value; }

		public override string ConnectionString { get => realDataRecord.ConnectionString; set => realDataRecord.ConnectionString = value; }

		public override bool MultiThreadSafe => realDataRecord.MultiThreadSafe;

		public override IsolationLevel IsolationLevelTrn { get => realDataRecord.IsolationLevelTrn; set => realDataRecord.IsolationLevelTrn = value; }

		public override int LockRetryCount { get => base.LockRetryCount; set => base.LockRetryCount = value; }

		public override int LockTimeout { get => base.LockTimeout; set => base.LockTimeout = value; }

		public override IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters, bool isCursor, bool forFirst, bool isRpc)
		{
			return new GXMockDbCommand(realDataRecord.GetCommand(con, stmt, parameters, isCursor, forFirst, isRpc), parameters);
		}

		public override DbDataAdapter CreateDataAdapeter()
		{
			return realDataRecord.CreateDataAdapeter();
		}

		public override IDbDataParameter CreateParameter()
		{
			return realDataRecord.CreateParameter();
		}

		public override IDbDataParameter CreateParameter(string name, object dbtype, int gxlength, int gxdec)
		{
			return realDataRecord.CreateParameter(name, dbtype, gxlength, gxdec);
		}

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId, string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			return realDataRecord.GetConnection(showPrompt, datasourceName, userId, userPassword, databaseName, port, schema, extra, connectionCache);
		}

		public override IDataReader GetDataReader(IGxConnectionManager connManager, IGxConnection connection, GxParameterCollection parameters, string stmt, ushort fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool hasNested, bool dynStmt)
		{
			return realDataRecord.GetDataReader(connManager, connection, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, hasNested, dynStmt);
		}

		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return realDataRecord.GetServerDateTimeStmt(connection);
		}

		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return realDataRecord.GetServerDateTimeStmtMs(connection);
		}

		public override string GetServerUserIdStmt()
		{
			return realDataRecord.GetServerUserIdStmt();
		}

		public override string GetServerVersionStmt()
		{
			return realDataRecord.GetServerVersionStmt();
		}

		protected override string BuildConnectionString(string datasourceName, string userId, string userPassword, string databaseName, string port, string schema, string extra)
		{
			return realDataRecord.BuildConnectionStringImpl(datasourceName, userId, userPassword, databaseName, port, schema, extra);
		}

		public override void AddParameters(IDbCommand cmd, GxParameterCollection parameters)
		{
			realDataRecord.AddParameters(cmd, parameters);
		}

		public override string AfterCreateCommand(string stmt, GxParameterCollection parmBinds)
		{
			return realDataRecord.AfterCreateCommand(stmt, parmBinds);
		}

		public override int BatchUpdate(DbDataAdapterElem da)
		{
			return realDataRecord.BatchUpdate(da);
		}

		public override string ConcatOp(int pos)
		{
			return realDataRecord.ConcatOp(pos);
		}

		public override void CreateDataBase(string dbname, IGxConnection con)
		{
			realDataRecord.CreateDataBase(dbname, con);
		}

		public override DateTime Dbms2NetDate(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.Dbms2NetDate(cmd, DR, i);
		}

		public override DateTime Dbms2NetDateTime(DateTime dt, bool precision)
		{
			return realDataRecord.Dbms2NetDateTime(dt, precision);
		}

		public override IGeographicNative Dbms2NetGeo(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.Dbms2NetGeo(cmd, DR, i);
		}

		public override string DbmsTToC(DateTime dt)
		{
			return realDataRecord.DbmsTToC(dt);
		}

		public override void DisposeCommand(IDbCommand command)
		{
			realDataRecord.DisposeCommand(command);
		}

		public override DateTime DTFromString(string s)
		{
			return realDataRecord.DTFromString(s);
		}

		public override object[] ExecuteStoredProcedure(IDbCommand cmd)
		{
			return realDataRecord.ExecuteStoredProcedure(cmd);
		}

		public override string FalseCondition()
		{
			return realDataRecord.FalseCondition();
		}

		public override string GetBinary(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetBinary(cmd, DR, i);
		}

		public override bool GetBoolean(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetBoolean(cmd, DR, i);
		}

		public override byte GetByte(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetByte(cmd, DR, i);
		}

		public override long GetBytes(IGxDbCommand cmd, IDataRecord DR, int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			return realDataRecord.GetBytes(cmd, DR, i, fieldOffset, buffer, bufferOffset, length);
		}

		public override IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return realDataRecord.GetCacheDataReader(item, computeSize, keyCache);
		}

		public override IDbCommand GetCachedCommand(IGxConnection con, string stmt)
		{
			return realDataRecord.GetCachedCommand(con, stmt);
		}

		public override DbDataAdapterElem GetCachedDataAdapter(IGxConnection con, string stmt)
		{
			return realDataRecord.GetCachedDataAdapter(con, stmt);
		}

		public override IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters)
		{
			return realDataRecord.GetCommand(con, stmt, parameters);
		}

		public override int GetCommandTimeout()
		{
			return realDataRecord.GetCommandTimeout();
		}

		public override DbDataAdapterElem GetDataAdapter(IGxConnection con, string stmt, GxParameterCollection parameters)
		{
			return realDataRecord.GetDataAdapter(con, stmt, parameters);
		}

		public override DbDataAdapterElem GetDataAdapter(IGxConnection con, string stmt, int batchSize, string stmtId)
		{
			return realDataRecord.GetDataAdapter(con, stmt, batchSize, stmtId);
		}

		public override void SetParameterDir(GxParameterCollection parameters, int num, ParameterDirection dir)
		{
			realDataRecord.SetParameterDir(parameters, num, dir);
		}

		public override void SetParameter(IDbDataParameter parameter, object value)
		{
			realDataRecord.SetParameter(parameter, value);
		}

		public override void SetParameterBlob(IDbDataParameter parameter, string value, bool dbBlob)
		{
			realDataRecord.SetParameterBlob(parameter, value, dbBlob);
		}

		public override void SetBinary(IDbDataParameter parameter, byte[] binary)
		{
			realDataRecord.SetBinary(parameter, binary);
		}

		public override object SetParameterValue(IDbDataParameter parameter, object value)
		{
			return realDataRecord.SetParameterValue(parameter, value);
		}

		public override void SetParameterChar(IDbDataParameter parameter, string value)
		{
			realDataRecord.SetParameterChar(parameter, value);
		}

		public override void SetParameterLVChar(IDbDataParameter parameter, string value, IGxDataStore datastore)
		{
			realDataRecord.SetParameterLVChar(parameter, value, datastore);
		}

		public override void SetParameterVChar(IDbDataParameter parameter, string value)
		{
			realDataRecord.SetParameterVChar(parameter, value);
		}

		public override void SetTimeout(IGxConnectionManager connManager, IGxConnection connection, int handle)
		{
			realDataRecord.SetTimeout(connManager, connection, handle);
		}

		public override string SetTimeoutSentence(long milliseconds)
		{
			return realDataRecord.SetTimeoutSentence(milliseconds);
		}

		public override short GetShort(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetShort(cmd, DR, i);
		}

		public override int GetInt(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetInt(cmd, DR, i);
		}

		public override long GetLong(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetLong(cmd, DR, i);
		}

		public override double GetDouble(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetDouble(cmd, DR, i);
		}

		public override string GetString(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetString(cmd, DR, i);
		}

		public override DateTime GetDateTimeMs(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetDateTimeMs(cmd, DR, i);
		}

		public override DateTime GetDateTime(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetDateTime(cmd, DR, i);
		}

		public override DateTime GetDate(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetDate(cmd, DR, i);
		}

		public override Guid GetGuid(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetGuid(cmd, DR, i);
		}

		public override IGeographicNative GetGeospatial(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetGeospatial(cmd, DR, i);
		}

		public override decimal GetDecimal(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.GetDecimal(cmd, DR, i);
		}

		public override object Net2DbmsDateTime(IDbDataParameter parm, DateTime dt)
		{
			return realDataRecord.Net2DbmsDateTime(parm, dt);
		}

		public override object Net2DbmsGeo(GXType type, IGeographicNative geo)
		{
			return realDataRecord.Net2DbmsGeo(type, geo);
		}

		public override bool ProcessError(int dbmsErrorCode, string emsg, GxErrorMask errMask, IGxConnection con, ref int status, ref bool retry, int retryCount)
		{
			return realDataRecord.ProcessError(dbmsErrorCode, emsg, errMask, con, ref status, ref retry, retryCount);
		}

		public override bool IsDBNull(IGxDbCommand cmd, IDataRecord DR, int i)
		{
			return realDataRecord.IsDBNull(cmd, DR, i);
		}

		public override string ToDbmsConstant(short Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override string ToDbmsConstant(int Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override string ToDbmsConstant(long Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override string ToDbmsConstant(decimal Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override string ToDbmsConstant(float Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override string ToDbmsConstant(double Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override string ToDbmsConstant(string Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override string ToDbmsConstant(DateTime Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override string ToDbmsConstant(bool Value)
		{
			return realDataRecord.ToDbmsConstant(Value);
		}

		public override void SetAdapterInsertCommand(DbDataAdapterElem da, IGxConnection con, string stmt, GxParameterCollection parameters)
		{
			realDataRecord.SetAdapterInsertCommand(da, con, stmt, parameters);
		}

		public override bool IsBlobType(IDbDataParameter idbparameter)
		{
			return realDataRecord.IsBlobType(idbparameter);
		}

		public override void PrepareCommand(IDbCommand cmd)
		{
			realDataRecord.PrepareCommand(cmd);
		}

		public override bool hasKey(string data, string key)
		{
			return realDataRecord.hasKey(data, key);
		}

		public override string ParseAdditionalData(string data, string extractWord)
		{
			return realDataRecord.ParseAdditionalData(data, extractWord);
		}

		public override string ReplaceKeyword(string data, string keyword, string newKeyword)
		{
			return realDataRecord.ReplaceKeyword(data, keyword, newKeyword);
		}

		public override string RemoveDuplicates(string data, string extractWord)
		{
			return realDataRecord.RemoveDuplicates(data, extractWord);
		}

		public override void GetValues(IDataReader reader, ref object[] values)
		{
			realDataRecord.GetValues(reader, ref values);
		}

		public override void SetCursorDef(CursorDef cursorDef)
		{
			realDataRecord.SetCursorDef(cursorDef);
		}

		public override string GetString(IGxDbCommand cmd, IDataRecord DR, int i, int size)
		{
			return realDataRecord.GetString(cmd, DR, i, size);
		}
	}
	public class GXMockDbCommand: IDbCommand
	{
		IDbCommand dbCommandImpl;
		GxParameterCollection parameters;
		public GXMockDbCommand(IDbCommand innerInstance, GxParameterCollection parms)
		{
			dbCommandImpl=innerInstance;
			parameters=parms;
		}

		public IDbConnection Connection { get => dbCommandImpl.Connection; set => dbCommandImpl.Connection=value; }
		public IDbTransaction Transaction { get => dbCommandImpl.Transaction; set => dbCommandImpl.Transaction=value; }
		public string CommandText { get => dbCommandImpl.CommandText; set => dbCommandImpl.CommandText=value; }
		public int CommandTimeout { get => dbCommandImpl.CommandTimeout; set => dbCommandImpl.CommandTimeout=value; }
		public CommandType CommandType { get => dbCommandImpl.CommandType; set => dbCommandImpl.CommandType=value; }

		public IDataParameterCollection Parameters => dbCommandImpl.Parameters;

		public UpdateRowSource UpdatedRowSource { get => dbCommandImpl.UpdatedRowSource; set => dbCommandImpl.UpdatedRowSource=value; }

		public void Cancel()
		{
			dbCommandImpl.Cancel();
		}

		public IDbDataParameter CreateParameter()
		{
			return dbCommandImpl.CreateParameter();
		}

		public void Dispose()
		{
			dbCommandImpl.Dispose();
		}

		public int ExecuteNonQuery()
		{
			return dbCommandImpl.ExecuteNonQuery();
		}

		public IDataReader ExecuteReader()
		{
			return dbCommandImpl.ExecuteReader();
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return dbCommandImpl.ExecuteReader(behavior);
		}

		public object ExecuteScalar()
		{
			return dbCommandImpl.ExecuteScalar();
		}

		public void Prepare()
		{
			dbCommandImpl.Prepare();
		}
	}
}
