using System;
using System.Collections.Generic;
using SAP.Middleware.Connector;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

namespace Artech.GeneXus.Inspectors
{

	[DataContract]
	public class SAPConnectionManager : SAP.Middleware.Connector.IDestinationConfiguration, IServerConfiguration
	{
		private static SAPConnectionManager _instance;
		private bool enterpriseInstance;
		public static SAPConnectionManager Instance()
		{
			if (_instance == null)
			{
				_instance = new SAPConnectionManager();
			}
			return _instance;
		}
		public static SAPConnectionManager EnterpriseInstance()
		{
			if (_instance == null)
			{
				_instance = new SAPConnectionManager(true);
			}
			return _instance;
		}

		public static string FILENAME = "SAPConnectionCatalog.json";
		ConcurrentDictionary<String, RfcConfigParameters> _connectionList = new ConcurrentDictionary<String, RfcConfigParameters>();
		ConcurrentDictionary<String, RfcConfigParameters> _serverList = new ConcurrentDictionary<String, RfcConfigParameters>();

		public const string ENCKEY = "7E9D0683C1584297BDD4848E07746EEA";

		string sLang = "";
		[DataMember]
		public string Language
		{
			get { return sLang; }
			set { sLang = value; }
		}

		string sUserName = "";
		[DataMember]
		public string UserName
		{
			get { return sUserName; }
			set { sUserName = value; }
		}

		string sPassword = "";
		[DataMember]
		public string Password
		{
			get
			{
				return sPassword;
			}
			set
			{
				sPassword = value;
			}
		}

		string ePassword = "";
		[DataMember]
		public string EPassword
		{
			get { return ePassword; }
			set { ePassword = value; }
		}

		string sSystemNumber = "";

		[DataMember]
		public string SystemNumber
		{
			get { return sSystemNumber; }
			set { sSystemNumber = value; }
		}

		string sAppHost = "";

		[DataMember]
		public string AppHost
		{
			get { return sAppHost; }
			set { sAppHost = value; }
		}

		string sRouterString = "";

		[DataMember]
		public string RouterString
		{
			get { return sRouterString; }
			set { sRouterString = value; }
		}

		string sClientId = "";

		[DataMember]
		public string ClientId
		{
			get { return sClientId; }
			set { sClientId = value; }
		}
		string sConnName = "";

		[DataMember]
		public string ConnectionName
		{
			get { return sConnName; }
			set { sConnName = value; }
		}

		string sSystemID = "";

		[DataMember]
		public string SystemID
		{
			get { return sSystemID; }
			set { sSystemID = value; }
		}

		string sSAPGUI = "";
		[DataMember]
		public string SAPGUI
		{
			get { return sSAPGUI; }
			set { sSAPGUI = value; }
		}

		// Connection data for load Balancing /server .

		string sMSHost = "";
		string sMSServ = "";
		string sSapRouter = "";
		string sGroup = "";
		string sPort = "";
		string sGatewayHost = "";
		string sGatewaySrv = "";
		string sProgramID = "";
		string sRegistrationCount = "";
		string sServerName = "";

		[DataMember]
		public string MessageHost
		{
			get { return sMSHost; }
			set { sMSHost = value; }
		}

		[DataMember]
		public string MessageSrv
		{
			get { return sMSServ; }
			set { sMSServ = value; }
		}

		[DataMember]
		public string SAPRouter
		{
			get { return sSapRouter; }
			set { sSapRouter = value; }
		}

		[DataMember]
		public string Group
		{
			get { return sGroup; }
			set { sGroup = value; }
		}

		[DataMember]
		public string Port
		{
			get { return sPort; }
			set { sPort = value; }
		}

		[DataMember]
		public string GatewayHost
		{
			get { return sGatewayHost; }
			set { sGatewayHost = value; }
		}

		[DataMember]
		public string GatewaySrv
		{
			get { return sGatewaySrv; }
			set { sGatewaySrv = value; }
		}

		[DataMember]
		public string ProgramID
		{
			get { return sProgramID; }
			set { sProgramID = value; }
		}

		[DataMember]
		public string RegistrationCount
		{
			get { return sRegistrationCount; }
			set { sRegistrationCount = value; }
		}

		[DataMember]
		public string ServerName
		{
			get { return sServerName; }
			set { sServerName = value; }
		}

		String errorCode;

		public String ErrorCode
		{
			get { return errorCode; }
			set { errorCode = value; }
		}

		String errorMessage;

		public String ErrorMessage
		{
			get { return errorMessage; }
			set { errorMessage = value; }
		}

		public void Copy(SAPConnectionManager source)
		{
			errorCode = source.ErrorCode;
			errorMessage = source.ErrorCode;
			sUserName = source.UserName;
			sPassword = source.Password;
			sSystemNumber = source.SystemNumber;
			sAppHost = source.AppHost;
			sRouterString = source.RouterString;
			sClientId = source.ClientId;
			sConnName = source.ConnectionName;
			sSystemID = source.SystemID;
			sSAPGUI = source.SAPGUI;
			sLang = source.Language;
			sMSHost = source.MessageHost;
			sMSServ = source.MessageSrv;
			sGroup = source.Group;
			sSapRouter = source.SAPRouter;
			sPort = source.Port;
			sGatewayHost = source.GatewayHost;
			sGatewaySrv = source.GatewaySrv;
			sProgramID = source.ProgramID;
			sRegistrationCount = source.RegistrationCount;
			sServerName = source.ServerName;
		}

		protected SAPConnectionManager(bool enterpriseConnectInstance = false)
		{
			errorCode = "";
			errorMessage = "";
			sUserName = "";
			sPassword = "";
			sSystemNumber = "";
			sAppHost = "";
			sRouterString = "";
			sClientId = "";
			sConnName = "";
			sSystemID = "";
			sMSHost = "";
			sMSServ = "";
			sGroup = "";
			sSapRouter = "";
			sPort = "";
			sGatewayHost = "";
			sGatewaySrv = "";
			sSAPGUI = "";
			sProgramID = "";
			sRegistrationCount = "";
			sServerName = "";
			enterpriseInstance = enterpriseConnectInstance;
			if (!enterpriseInstance)
				sLang = "EN";
			else
				sLang = "";
		}

		public void disconnect()
		{
			if (RfcDestinationManager.IsDestinationConfigurationRegistered())
			{
				RfcDestinationManager.UnregisterDestinationConfiguration(this);
			}
			RfcConfigParameters oldparms;
			RfcConfigParameters value;
			if (!String.IsNullOrEmpty(this.ServerName))
			{
				if (_serverList.TryGetValue(this.ServerName, out oldparms))
					_serverList.TryRemove(this.ServerName, out value);
			}
			if (!String.IsNullOrEmpty(this.ConnectionName))
			{
				if (_connectionList.TryGetValue(this.ConnectionName, out oldparms))
					_connectionList.TryRemove(this.ConnectionName, out value);
			}
		}

		public void initConnection()
		{
			if (_connectionList == null)
			{
				_connectionList = new ConcurrentDictionary<String, RfcConfigParameters>();
			}

			if (_serverList == null)
			{
				_serverList = new ConcurrentDictionary<String, RfcConfigParameters>();
			}

			if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
			{
				RfcDestinationManager.RegisterDestinationConfiguration(this);
			}
			this.SetParameters();
			if (!(String.IsNullOrEmpty(this.ServerName) || String.IsNullOrEmpty(this.ProgramID) || String.IsNullOrEmpty(this.sGatewayHost)))
			{
				this.SetServerParameters();
			}
		}

		public bool TestConnection(string connName, out String emessage)
		{
			initConnection();
			try
			{
				RfcDestination destination = RfcDestinationManager.GetDestination(connName);
				destination.Ping();
				emessage = "";
				return true;
			}
			catch (RfcLogonException ex)
			{
				System.Console.Out.WriteLine(ex.ToString());
				emessage = ex.Message;
				return false;
			}
			catch (RfcBaseException ex)
			{
				System.Console.Out.WriteLine(ex.ToString());
				emessage = ex.Message;
				return false;
			}
		}

		public bool saveConnectionParams(string connName, string appHost, string systemNum, string systemId, string clientId,
											string routerStr, string sapGui, string lang,
											string messageHost, string messageService,
											string loginGroup, string sapRouter, string portString, string gatewayHost, string gatewayService, string programID, string registrationCount, string serverName)
		{
			if (connName.Trim().CompareTo("") == 0)
			{
				return false;
			}
			else
			{
				sConnName = connName;
				sAppHost = appHost;
				sSystemNumber = systemNum;
				sSystemID = systemId;
				sClientId = clientId;
				sRouterString = routerStr;
				sMSHost = messageHost;
				sMSServ = messageService;
				sGroup = loginGroup;
				sSapRouter = sapRouter;
				sPort = portString;
				sGatewayHost = gatewayHost;
				sGatewaySrv = gatewayService;
				sProgramID = programID;
				sRegistrationCount = registrationCount;
				sServerName = ServerName;
				sSAPGUI = sapGui;
				sLang = lang;
				return true;
			}

		}

		public bool saveCredentials(string user, string pass)
		{
			sUserName = user;
			sPassword = pass;
			return true;
		}

		public String ToStringValues()
		{
			return sConnName + ":" + sAppHost + ":" + sSystemNumber + ":" + sClientId + ":" + sRouterString + ":" + sLang + ":" + sUserName;
		}

		private void SetServerParameters()
		{
			if (!String.IsNullOrEmpty(this.ServerName))
			{
				RfcConfigParameters parms = new RfcConfigParameters();
				RfcConfigParameters oldparms;
				parms.Add(RfcConfigParameters.GatewayHost, sGatewayHost);
				parms.Add(RfcConfigParameters.GatewayService, sGatewaySrv);
				parms.Add(RfcConfigParameters.ProgramID, sProgramID);
				parms.Add(RfcConfigParameters.RegistrationCount, sRegistrationCount);
				if (_serverList.TryGetValue(this.ServerName, out oldparms))
				{
					RfcConfigParameters value;
					_serverList.TryRemove(this.ServerName, out value);
				}
				_serverList.TryAdd(this.ServerName, parms);
				errorCode = "";
				errorMessage = "";
				if (!RfcServerManager.IsServerConfigurationRegistered())
				{
					RfcServerManager.RegisterServerConfiguration(this);
				}
			}
		}

		private void SetParameters()
		{
			RfcConfigParameters parms = new RfcConfigParameters();
			RfcConfigParameters oldparms;
			if (String.IsNullOrEmpty(sMSHost))
			{
				parms.Add(RfcConfigParameters.AppServerHost, sRouterString + sAppHost);
			}
			else
			{
				if (String.IsNullOrEmpty(sPort))
				{
					parms.Add(RfcConfigParameters.MessageServerHost, sMSHost);
					parms.Add(RfcConfigParameters.MessageServerService, sMSServ);
				}
				else
				{
					parms.Add(RfcConfigParameters.MessageServerHost, sMSHost + ":" + sPort);
					if (!String.IsNullOrEmpty(sMSServ))
					{
						parms.Add(RfcConfigParameters.MessageServerService, sMSServ + ":" + sPort);
					}
				}
				parms.Add(RfcConfigParameters.GatewayHost, sGatewayHost);
				parms.Add(RfcConfigParameters.GatewayService, sGatewaySrv);
				parms.Add(RfcConfigParameters.SAPRouter, sSapRouter);
				parms.Add(RfcConfigParameters.LogonGroup, sGroup);
			}
			parms.Add(RfcConfigParameters.SystemNumber, sSystemNumber);
			parms.Add(RfcConfigParameters.User, sUserName);
			parms.Add(RfcConfigParameters.Password, sPassword);
			parms.Add(RfcConfigParameters.SystemID, sSystemID);
			parms.Add(RfcConfigParameters.Client, sClientId);
			parms.Add(RfcConfigParameters.Language, sLang);
			parms.Add(RfcConfigParameters.PoolSize, "5");
			parms.Add(RfcConfigParameters.PeakConnectionsLimit, "10");
			parms.Add(RfcConfigParameters.ConnectionIdleTimeout, "600");
			parms.Add(RfcConfigParameters.UseSAPGui, sSAPGUI);
			if (_connectionList.TryGetValue(this.ConnectionName, out oldparms))
			{
				RfcConfigParameters value;
				_connectionList.TryRemove(this.ConnectionName, out value);
			}
			_connectionList.TryAdd(this.ConnectionName, parms);
			errorCode = "";
			errorMessage = "";
		}


		/* change login via events not supported*/
		event RfcDestinationManager.ConfigurationChangeHandler IDestinationConfiguration.ConfigurationChanged
		{
			add { }
			remove { }
		}

		event RfcServerManager.ConfigurationChangeHandler IServerConfiguration.ConfigurationChanged
		{
			add { }
			remove { }
		}

		public bool ChangeEventsSupported()
		{
			return true;
		}

		RfcConfigParameters IDestinationConfiguration.GetParameters(String destinationName)
		{
			RfcConfigParameters parms;
			if (_connectionList.TryGetValue(destinationName, out parms))
			{
				return parms;
			}
			return null;
		}

		RfcConfigParameters IServerConfiguration.GetParameters(String destinationName)
		{
			RfcConfigParameters parms;
			if (_serverList.TryGetValue(destinationName, out parms))
			{
				return parms;
			}
			return null;
		}

		public static void SaveConnectionData(String fileName)
		{
			System.IO.TextWriter writer = new System.IO.StreamWriter(fileName, false);
			writer.WriteLine(EnterpriseInstance().ToStringValues());
			writer.Close();

		}

		public static string LoadConnectionData(String fileName)
		{
			System.IO.TextReader reader = new System.IO.StreamReader(fileName);
			String result = reader.ReadLine();
			reader.Close();
			return result;
		}

#pragma warning disable
		public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;
#pragma warning restore

	}
}
