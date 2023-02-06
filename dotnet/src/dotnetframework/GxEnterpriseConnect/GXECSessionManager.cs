using System;
using GeneXus.Application;
using GeneXus.Http;

namespace GeneXus.SAP
{
                 
    public class GXECSessionManager
    {

        GxEnterpriseConnect connection = null;
        String userName;

        IGxContext _context;

        public GXECSessionManager(IGxContext context)
        {
            _context = context;
        }

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

        public int ErrorCode
        {
            get 
			{ 
				if (errorCode == 0)
				{
					if (connection != null)
						return connection.GetErrorCode();
				}
				return errorCode; 
			}
            set { errorCode = value; }
        }
        String errorMessage;

        public string ErrorMessage
        {
			get
			{
				if (string.IsNullOrEmpty(errorMessage))
				{
					if (connection != null)
						return connection.GetErrorMessage();
				}
				return errorMessage;
			}
            set { errorMessage = value; }
        }

        public void Save()
        {
        }

        public void Load()
        {

        }

		public void DocumentReceiverStart()
		{
			connection.StartReceiverServer();
		}

		public void DocumentSenderStart()
		{
			connection.StartSenderServer();
		}

		public void DocumentSenderStop()
		{
			connection.StopSender();
		}

		public void DocumentReceiverStop()
		{
			connection.StopReceiver();
		}

		public void TransactionBegin()
        {
            connection.TransactionBegin();
        }

        public void TransactionCommit()
        {
            connection.TransactionCommit();
        }

		private GxEnterpriseConnect FindConnection()
		{
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

		}
		public void Disconnect()
        {
			connection = FindConnection();
			connection.Disconnect();
        }

		public bool IsConnected()
		{
			connection = FindConnection();
			return  connection.IsConnected(this.sessionName);
		}

        public void Connect()
        {
            _context.SetContextProperty("SessionName", this.SessionName);
            connection =  new GxEnterpriseConnect(this);                
            bool result = connection.TestConnection(out errorMessage);
            if (result)
            {
                errorCode = 0;
                errorMessage = "";
            }
            else
            { 
                errorCode  = 1;                
            }
        }
    }
}
