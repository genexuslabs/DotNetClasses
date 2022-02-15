using System.Data;
using System.Data.Common;
using GeneXus.Cache;
using GeneXus.Data;
using GeneXus.Utils;

namespace GeneXus.Data
{
	public class GXMockDataRecord : GxDataRecord
	{
		GxDataRecord dataRecordImpl;
		public GXMockDataRecord(GxDataRecord innerInstance)
		{
			dataRecordImpl=innerInstance; 
		}
		public override string DataSource { get => dataRecordImpl.DataSource; set => dataRecordImpl.DataSource=value; }
		public override string DataBaseName { get => dataRecordImpl.DataBaseName; set => dataRecordImpl.DataBaseName=value; }
		public override string ConnectionString { get => dataRecordImpl.ConnectionString; set => dataRecordImpl.ConnectionString=value; }
		public override bool MultiThreadSafe => dataRecordImpl.MultiThreadSafe;

		public override bool AllowsDuplicateParameters => dataRecordImpl.AllowsDuplicateParameters;
		public override IsolationLevel IsolationLevelTrn { get => dataRecordImpl.IsolationLevelTrn; set => dataRecordImpl.IsolationLevelTrn=value; }
		public override int LockRetryCount { get => base.LockRetryCount; set => base.LockRetryCount=value; }
		public override int LockTimeout { get => base.LockTimeout; set => base.LockTimeout=value; }
		public override IDbCommand GetCommand(IGxConnection con, string stmt, GxParameterCollection parameters, bool isCursor, bool forFirst, bool isRpc)
		{
			return new GXMockDbCommand(dataRecordImpl.GetCommand(con, stmt, parameters, isCursor, forFirst, isRpc));
		}
		public override DbDataAdapter CreateDataAdapeter()
		{
			return dataRecordImpl.CreateDataAdapeter();
		}

		public override IDbDataParameter CreateParameter()
		{
			return dataRecordImpl.CreateParameter();
		}

		public override IDbDataParameter CreateParameter(string name, object dbtype, int gxlength, int gxdec)
		{
			return dataRecordImpl.CreateParameter(name, dbtype, gxlength, gxdec);
		}

		public override GxAbstractConnectionWrapper GetConnection(bool showPrompt, string datasourceName, string userId, string userPassword, string databaseName, string port, string schema, string extra, GxConnectionCache connectionCache)
		{
			return dataRecordImpl.GetConnection(showPrompt, datasourceName, userId, userPassword, databaseName, port, schema, extra, connectionCache);	
		}

		public override IDataReader GetDataReader(IGxConnectionManager connManager, IGxConnection connection, GxParameterCollection parameters, string stmt, ushort fetchSize, bool forFirst, int handle, bool cached, SlidingTime expiration, bool hasNested, bool dynStmt)
		{
			return dataRecordImpl.GetDataReader(connManager, connection, parameters, stmt, fetchSize, forFirst, handle, cached, expiration, hasNested, dynStmt);
		}

		public override string GetServerDateTimeStmt(IGxConnection connection)
		{
			return dataRecordImpl.GetServerDateTimeStmt(connection);
		}

		public override string GetServerDateTimeStmtMs(IGxConnection connection)
		{
			return dataRecordImpl.GetServerDateTimeStmtMs(connection);
		}

		public override string GetServerUserIdStmt()
		{
			return dataRecordImpl.GetServerUserIdStmt();
		}

		public override string GetServerVersionStmt()
		{
			return dataRecordImpl.GetServerVersionStmt();
		}

		protected override string BuildConnectionString(string datasourceName, string userId, string userPassword, string databaseName, string port, string schema, string extra)
		{
			return dataRecordImpl.BuildConnectionStringImpl(datasourceName, userId, userPassword, databaseName, port, schema, extra);	
		}
	}
	public class GXMockDbCommand: IDbCommand
	{
		IDbCommand dbCommandImpl;
		public GXMockDbCommand(IDbCommand innerInstance)
		{
			dbCommandImpl=innerInstance;
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
