using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using GeneXus.Data.NTier.ADO;
using GeneXus.Utils;

namespace GeneXus.Data.NTier
{
	public class ServiceCommand : IDbCommand, IServiceCommand
	{
		public ServiceCommand(IDbConnection conn)
		{
			Connection = conn;
			Parameters = new GxParameterCollection();
		}

		public string CommandText { get; set; }
		public int CommandTimeout { get; set; }
		public CommandType CommandType { get; set; }
		public IDbConnection Connection { get; set; }
		public CursorDef CursorDef { get; set; }
		public IDataParameterCollection Parameters { get; internal set; }
		public IDbTransaction Transaction { get; set; }
		public UpdateRowSource UpdatedRowSource { get; set; }

		public void Cancel()
		{
			throw new NotImplementedException();
		}
		public IDbDataParameter CreateParameter()
		{
			throw new NotImplementedException();
		}
		public void Dispose()
		{
			Connection = null;
			Parameters = null;
		}

		public int ExecuteNonQuery()
		{
			return (Connection as ServiceConnection).ExecuteNonQuery((CursorDef as ServiceCursorDef), Parameters, CommandBehavior.Default);
		}

		public IDataReader ExecuteReader()
		{
			return (Connection as ServiceConnection).ExecuteReader((CursorDef as ServiceCursorDef), Parameters, CommandBehavior.Default);
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return (Connection as ServiceConnection).ExecuteReader((CursorDef as ServiceCursorDef), Parameters, behavior);
		}

		public object ExecuteScalar()
		{
			throw new NotImplementedException();
		}

		public void Prepare()
		{
			throw new NotImplementedException();
		}

	}
}
