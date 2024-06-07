using System;
using System.ServiceModel;
using System.Xml;
using System.Runtime.Serialization;
using GeneXus.Application;
using SapNwRfc;
using SapNwRfc.Pooling;

namespace GeneXus.SAP
{
                 
    public class GXECSessionManager
    {

        ISapConnection connection = null;
		//GxEnterpriseConnect GxConnect = null;		
		IGxContext _context;
		string _connectionString = null;
		static SapConnectionPool _pool;

        public GXECSessionManager(IGxContext context)
        {
            _context = context;
			this.SessionName = _context.GetContextProperty("SAP-Session") as string;
			_connectionString = _context.GetContextProperty("SAP-ConnStr") as string;

        }

		String userName;
		public String UserName
        {
            get { return userName; }
            set { userName = value; }
        }
        String password;

        public String Password
        {
            get { return password; }
            set { password = value; }
        }
        String appServer;

        public String AppServer
        {
            get { return appServer; }
            set { appServer = value; }
        }
        String instanceNumber;

        public String InstanceNumber
        {
            get { return instanceNumber; }
            set { instanceNumber = value; }
        }
        String clientNumber;

        public String ClientNumber
        {
            get { return clientNumber; }
            set { clientNumber = value; }
        }
        String routerString;

        public String RouterString
        {
            get { return routerString; }
            set { routerString = value; }
        }
        String systemId;

        public String SystemId
        {
            get { return systemId; }
            set { systemId = value; }
        }
        String sessionName;

        public String SessionName
        {
            get { return sessionName; }
            set {
                sessionName = value; 
            }
        }
        String sAPGUI;

        public String SAPGUI
        {
            get { return sAPGUI; }
            set { sAPGUI = value; }
        }

        String lang;
        public String Language
        {
            get { return lang; }
            set { lang = value; }
        }

        String msHost;
        public string MessageHost
        {
            get { return msHost; }
            set { msHost = value; }
        }

        String msServ;
        public string MessageSrv
        {
            get { return msServ; }
            set { msServ = value; }
        }
       
        String group;
        public String Group
        {
            get { return group; }
            set { group = value; }
        }
        String sPort;
        public String Port
        {
            get { return sPort; }
            set {   sPort = value;}
        }

        String sapRouter;
        public String SAPRouter
        {
            get { return sapRouter; }
            set { sapRouter = value; }
        }

        String sGatewayHost;
        public String GatewayHost
        {
            get { return sGatewayHost; }
            set { sGatewayHost = value; }
        }

        String sGatewaySrv;
        public String GatewaySrv
        {
            get { return sGatewaySrv; }
            set { sGatewaySrv = value; }
        }

		String sProgramID;
		public String ProgramID
		{
			get { return sProgramID; }
			set { sProgramID = value; }
		}

		String sRegistrationCount;
		public String RegistrationCount
		{
			get { return sRegistrationCount; }
			set { sRegistrationCount = value; }
		}

		String sServerName;
		public String ServerName
		{
			get { return sServerName; }
			set { sServerName = value; }
		}

		int errorCode;

		public string ConnectionString
		{
			get {
				if (String.IsNullOrEmpty(_connectionString))
				{
					return "AppServerHost=" + this.RouterString + this.AppServer + ";"
							+ "SystemNumber=" + this.instanceNumber + ";"
						   + "User=" + this.UserName + ";"
						   + "Password=" + this.Password + ";"
						   + "Client=" + this.ClientNumber + ";"
						   + "PoolSize=" + "5" + ";"
						   + "Language=" + this.Language;
				}
				else
				{
					return _connectionString;
				}
			}
		}
        public int ErrorCode
        {
            get 
			{
				return errorCode;				
			}
            set
			{
				errorCode = value;
			}
        }
        String errorMessage;

        public string ErrorMessage
        {
			get
			{
				return errorMessage;				
			}
            set
			{
				errorMessage = value;
			}
        }

        public void Save()
        {
        }

        public void Load()
        {

        }

		public void DocumentReceiverStart()
		{
			//connection.StartReceiverServer();
		}

		public void DocumentSenderStart()
		{
			//connection.StartSenderServer();
		}

		public void DocumentSenderStop()
		{
			//connection.StopSender();
		}

		public void DocumentReceiverStop()
		{
		//	connection.StopReceiver();
		}

		public void TransactionBegin()
        {
          //  connection.TransactionBegin();
        }

        public void TransactionCommit()
        {
           // connection.TransactionCommit();
        }

		private ISapConnection FindConnection()
		{
			/*
			if (connection != null)
			{
				return connection;
			}
			else
			{
				string _session = "";
				if (!String.IsNullOrEmpty(this.sessionName))
				{
					_session = this.sessionName;
				}
				else
				{
					Object objectSession = _context.GetContextProperty("SessionName");
					if (objectSession != null && !String.IsNullOrEmpty((String)objectSession))
					{
						this.sessionName = (String)objectSession;
						_session = this.sessionName;
					}
				}
				this.sessionName = _session;
				connection = new GxEnterpriseConnect(this);
				return connection;
			}
			*/
			return null;

		}
		public void Disconnect()
        {
			//connection = FindConnection();
			if (connection != null)
			{
				connection.Disconnect();
			}
        }

		public bool IsConnected()
		{
			//connection = FindConnection();
			if(connection != null)
				return connection.IsValid;
			else
				return false;

		}

		public void ConnectSession(string SessionName, string Scope)
		{
			Connect();
		}

		public ISapConnection GetCurrentConnection()
		{
			if (errorCode == 0)
				return connection;
			else
				return null;
		}


		public void Connect()
        {
			try
			{
				if (connection == null)
				{
					if (_pool==null)
						_pool = new SapConnectionPool(ConnectionString);
					_context.SetContextProperty("SAP-Session", this.SessionName);
					_context.SetContextProperty("SAP-ConnStr", ConnectionString);
					connection = _pool.GetConnection();
					//GxConnect = new GxEnterpriseConnect(this);
					bool result = connection.Ping();
					if (result)
					{
						errorCode = 0;
						errorMessage = "";
					}
					else
					{
						errorCode = 1;
					}
				}
			}
			catch (Exception e)
			{
				errorCode = 2;
				errorMessage = e.Message;
			}
        }
    }
}
