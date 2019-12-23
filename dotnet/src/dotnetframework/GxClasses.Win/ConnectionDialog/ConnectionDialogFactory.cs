using GeneXus.Utils;


namespace ConnectionBuilder
{

	/// <summary>
	/// Summary description for ConnectionDialogFactory.
	/// </summary>
	public class ConnectionDialogFactory : IConnectionDialogFactory
	{
		static ConnectionDialogFactory()
		{
			Dialogs.DialogFactory = new ConnectionDialogFactory();
		}
		public ConnectionDialogFactory()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public IConnectionDialog GetConnectionDialog(int iType)
		{
			if (iType == 1)
			   return new OracleConnectionDialog();
			else
				return new SQLConnectionDialog();
		}
	}
}
