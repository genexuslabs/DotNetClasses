using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using GeneXus.Data.NTier;

namespace GeneXus.Data.NTier
{
	public interface IServiceConnection
	{
		string DataSource { get; set; }
	}

	public abstract class ServiceConnection : IDbConnection, IServiceConnection
	{
		public virtual string ConnectionString
		{
			get;
			set;
		}

		public virtual string DataSource
		{
			get;
			set;
		}

		public ConnectionState State { get; set; }

		public int ConnectionTimeout
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public string Database
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual IDbTransaction BeginTransaction()
		{
			return new ServiceTransaction(this, IsolationLevel.ReadCommitted);
		}

		public virtual IDbTransaction BeginTransaction(IsolationLevel il)
		{
			return new ServiceTransaction(this, il);
		}

		public virtual void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}

		public virtual void Close()
		{
			State = ConnectionState.Closed;
		}

		public virtual IDbCommand CreateCommand()
		{
			return new ServiceCommand(this); ;
		}

		public void Dispose()
		{
			if (State != ConnectionState.Closed)
				Close();
		}

		public virtual void Open()
		{
			State = ConnectionState.Open;
		}

		public abstract IDataReader ExecuteReader(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior);
		public abstract int ExecuteNonQuery(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior);


	}
}
