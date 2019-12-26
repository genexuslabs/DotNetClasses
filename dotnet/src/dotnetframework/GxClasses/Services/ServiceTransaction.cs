using System;
using System.Data;

namespace GeneXus.Data.NTier
{
	public class ServiceTransaction : IDbTransaction
	{
		private ServiceConnection mConnection;

		public ServiceTransaction(ServiceConnection oDataConnection, IsolationLevel il)
		{
			mConnection = oDataConnection;
			IsolationLevel = il;
		}

		public IDbConnection Connection
		{
			get
			{
				return mConnection;
			}
		}

		public IsolationLevel IsolationLevel { get; set; }

		public virtual void Commit()
		{
		}

		public virtual void Rollback()
		{
		}

		public void Dispose()
		{
			mConnection.Dispose();
		}

	
	}

}
