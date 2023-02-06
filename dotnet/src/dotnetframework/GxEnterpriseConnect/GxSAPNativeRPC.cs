using System;

using GeneXus.Application;
using GeneXus.Utils;
using SAP.Middleware.Connector;
using Jayrock.Json;
using Artech.GeneXus.Inspectors;


namespace GeneXus.SAP
{
    class GxSAPNativeRPC
    {
        const string DEFAULTCONNNAME = "MyConnection";

        private SAPConnectionManager connMgrSAP;
        String _connName = DEFAULTCONNNAME;
		String _serverName = "GX_FILE_SERVER";
        IRfcFunction executeFunction = null;
		RfcServer currentServer = null;	
        public GxSAPNativeRPC(IGxContext context)
        {
            _connName = context.GetContextProperty("SessionName") as String;
			_serverName = context.GetContextProperty("ServerName") as String;
			connMgrSAP = SAPConnectionManager.EnterpriseInstance();
		}

        public GxSAPNativeRPC(GXECSessionManager manager)
        {
            SAPConnectionManager sapManager = SAPConnectionManager.EnterpriseInstance();            
            sapManager.AppHost = manager.AppServer;
            sapManager.ClientId = manager.ClientNumber;
            sapManager.SystemID = manager.SystemId;
            sapManager.SystemNumber = manager.InstanceNumber;
            sapManager.UserName = manager.UserName;
            sapManager.Password = manager.Password;
            sapManager.RouterString = manager.RouterString;
            sapManager.SAPGUI = manager.SAPGUI;
            sapManager.ConnectionName = manager.SessionName;
            sapManager.Language = manager.Language;
            sapManager.MessageHost = manager.MessageHost;
            sapManager.MessageSrv = manager.MessageSrv;
            sapManager.Port = manager.Port;            
            sapManager.SAPRouter = manager.SAPRouter;
            sapManager.GatewayHost = manager.GatewayHost;
            sapManager.GatewaySrv = manager.GatewaySrv;
			sapManager.ProgramID = manager.ProgramID;
			sapManager.ServerName = manager.ServerName;
            connMgrSAP = sapManager;
            _connName = manager.SessionName;
			_serverName = manager.ServerName;
            sapManager.initConnection();
        }

		public void setValueNull(String parameterName)
		{
			executeFunction.SetValue(parameterName, (object)null);
			executeFunction.SetParameterActive(parameterName, true);
		}

		public void setValueEmpty(String parameterName)
		{
			executeFunction.SetValue(parameterName, "");
			executeFunction.SetParameterActive(parameterName, true);			
		}

		public void setValue(String parameterName, String val)
        {
            Boolean setValues = false;
            if (val != null && !String.IsNullOrEmpty(val.Trim()))
            {
                setValues = true;
                executeFunction.SetValue(parameterName, val);

            }
            if (setValues)
            {
                executeFunction.SetParameterActive(parameterName, true);
            }
            else
            {
                executeFunction.SetParameterActive(parameterName, false);
            }
        }

        public void setValue(String parameterName, int val)
        {
            executeFunction.SetValue(parameterName, val);
            executeFunction.SetParameterActive(parameterName, true);        
        }

        public void setValue(String parameterName, long val)
        {
            executeFunction.SetValue(parameterName, val);
            executeFunction.SetParameterActive(parameterName, true);
        }

        public void setValue(String parameterName, decimal val)
        {
            executeFunction.SetValue(parameterName, val);
            executeFunction.SetParameterActive(parameterName, true);
        }

        public void setValue(String parameterName, DateTime val)
        {
            if (val != null && val.CompareTo(new DateTime(100, 1, 1, 0, 0, 0)) >= 0)
            {
                executeFunction.SetValue(parameterName, val);
                executeFunction.SetParameterActive(parameterName, true);
            }
            else
            {
                executeFunction.SetParameterActive(parameterName, false);
            }
        }

        public void setFieldValues(String parameterName, GxUserType sdt)
        {
            Boolean setValues = false;
            Object table = sdt.GetJSONObject(false);
            System.Collections.DictionaryBase values;
            values = table as System.Collections.DictionaryBase;
            
            if (values != null)
            {
                setValues = true;
                RfcElementMetadata metaData = executeFunction.GetElementMetadata(parameterName);
                if (metaData.DataType == RfcDataType.STRUCTURE)
                {
					IRfcStructure valueStruct = executeFunction.GetStructure(parameterName);
                    setStructureFieldValues(executeFunction, valueStruct, values);
                }
                else if (metaData.DataType == RfcDataType.INT4 || metaData.DataType == RfcDataType.INT1
                       || metaData.DataType == RfcDataType.INT2 || metaData.DataType == RfcDataType.INT8)
                {
                    setNumericValue(executeFunction, parameterName, values);
                }
                else if (metaData.DataType == RfcDataType.CHAR || metaData.DataType == RfcDataType.STRING)
                {
                    setStringValue(executeFunction, parameterName, values);
                }

            }

            if (setValues)
            {
                executeFunction.SetParameterActive(parameterName, true);
            }
            else
            {
                executeFunction.SetParameterActive(parameterName, false);
            }
        }

        public void setFieldValues(String parameterName, IGxCollection sdtColl)
        {
            Boolean setValues = false;
            for (int i = 1; i <= sdtColl.Count; i++)
            {

                GxUserType item = sdtColl.Item(i) as GxUserType;
                if (item != null)
                {
                    Object table = item.GetJSONObject(false);
                    System.Collections.DictionaryBase values;
                    values = table as System.Collections.DictionaryBase;
                    IRfcTable valueTable = null;
                    if (values != null)
                    {
                        setValues = true;
                        valueTable = executeFunction.GetTable(parameterName);
                        IRfcStructure tableRow = valueTable.Metadata.LineType.CreateStructure();
                        valueTable.Insert(tableRow);
                        setCollectionFieldValues(executeFunction, tableRow, values);
                    }
                }
            }
            if (setValues)
            {
                executeFunction.SetParameterActive(parameterName, true);
            }
            else
            {
                executeFunction.SetParameterActive(parameterName, false);
            }
        }

        public void setCollectionFieldValues(IRfcFunction function, IRfcStructure tableRow, System.Collections.DictionaryBase dictvalues)
        {
            setStructureFieldValues(function, tableRow, dictvalues);
        }

        public void setStructureFieldValues(IRfcFunction function, IRfcStructure tableRow, System.Collections.DictionaryBase dictvalues)
        {
            foreach (System.Collections.DictionaryEntry p in dictvalues)
            {
                RfcElementMetadata elementMetaData =  tableRow.GetElementMetadata(p.Key.ToString());
                String svalue = p.Value.ToString().Trim();
                  
                if (elementMetaData.DataType == RfcDataType.TIME)
                {
                    svalue = svalue.Substring(svalue.Length - 8);
                }
               
                tableRow.SetValue(p.Key.ToString(), svalue);
            }
        }

        public void setNumericValue(IRfcFunction function, String parameterName, System.Collections.DictionaryBase dictvalues)
        {
            foreach (System.Collections.DictionaryEntry p in dictvalues)
            {
                if (p.Value != null && !string.IsNullOrEmpty(p.Value.ToString()))
                {
                    function.SetValue(parameterName, Int16.Parse(p.Value.ToString()));
                }
            }
        }

        public void setStringValue(IRfcFunction function, String parameterName, System.Collections.DictionaryBase dictvalues)
        {
            foreach (System.Collections.DictionaryEntry p in dictvalues)
            {
                if (p.Value != null && !string.IsNullOrEmpty(p.Value.ToString()))
                {
                    function.SetValue(parameterName, p.Value.ToString());
                }
            }
        }

        public void getFieldValues(String parameterName, out String result)
        {
            result = executeFunction.GetString(parameterName);           
        }

        public void getFieldValues(String parameterName, out DateTime result)
        {
            try
            {
                String dateStr = executeFunction.GetString(parameterName);
                if (dateStr.Equals("0000-00-00"))
                    result = System.DateTime.MinValue;
                else
                    result = System.DateTime.Parse(dateStr);
            }
            catch (System.FormatException)
            {
                result = System.DateTime.MinValue;            
            }
        }

        public void getFieldValues(String parameterName, out int result)
        {            
            result = executeFunction.GetInt(parameterName);
        }

        public void getFieldValues(String parameterName, out decimal result)
        {
            result = executeFunction.GetDecimal(parameterName);
        }

        public void getFieldValues(String parameterName, out short result)
        {
            try
            {
                result = executeFunction.GetShort(parameterName);
            }
            catch (RfcTypeConversionException)
            {
                result = (short) executeFunction.GetDecimal(parameterName);            
            }

        }

        public void getFieldValues(String parameterName, out long result)
        {
            try
            {
                result = executeFunction.GetLong(parameterName);
            }
            catch (RfcTypeConversionException)
            {
                result = (long) executeFunction.GetDecimal(parameterName);            
            }
        }

        public void getFieldValues(String parameterName, out float result)
        {
            result = executeFunction.GetFloat(parameterName);
        }

        public void getFieldValues(String parameterName, IGxJSONAble sdtObject)
        {
			GxUserType sdt = sdtObject as GxUserType;

			if (sdt!=null)
            {
                IRfcStructure valueStruct = executeFunction.GetStructure(parameterName);
                copyFromStruct(sdt, valueStruct);
            }
            else
            {
                IRfcTable valueTable = executeFunction.GetTable(parameterName);
                copyFromTable(sdtObject, valueTable);
            }

        }

        Jayrock.Json.JObject copyToJson(IRfcDataContainer tbl) 
        {
            Jayrock.Json.JObject jRow = new Jayrock.Json.JObject();

            for (int liElement = 0; liElement < tbl.ElementCount; liElement++)
            {
                RfcElementMetadata metadata = tbl.GetElementMetadata(liElement);
                if (metadata.DataType == RfcDataType.NUM || metadata.DataType == RfcDataType.FLOAT ||
                    metadata.DataType == RfcDataType.BCD )
                {
                    jRow[metadata.Name] = tbl.GetDouble(metadata.Name);
                }
                else
                {
                    jRow[metadata.Name] = tbl.GetString(metadata.Name);
                }
            }
            return jRow;
        }

        public void copyFromStruct(GxUserType sdt , IRfcStructure str)
        {            
            if (str != null)
            {
                Jayrock.Json.JObject jRow = copyToJson(str);
                sdt.FromJSONObject(jRow);                
            }

        }

        public void copyFromTable(IGxJSONAble sdtColl, IRfcTable tbl)
        {           
            if (tbl != null && sdtColl != null)
            {
                JArray jCol = new Jayrock.Json.JArray();
                for (int i = 0; i < tbl.Count; ++i)
                {
                    tbl.CurrentIndex = i;
                    Jayrock.Json.JObject jRow =  copyToJson(tbl);
                    jCol.Add(jRow);
                }
                sdtColl.FromJSONObject(jCol);
            }

        }

		public static void rfcServerErrorOccured(Object server, RfcServerErrorEventArgs errorEventData)
		{
			// Technical problem in server connection (network, logon data, out-of-memory, etc.)
			Console.WriteLine(errorEventData.Error.ToString());
		}

		public void StartReceiverServer()
		{
			if (currentServer == null)
			{
				currentServer = GxDocumentReceiver.CreateDocumentServer(_serverName);
			}
			currentServer.Start();
		}

		public void StartSenderServer()
		{
			if (currentServer == null)
			{
				currentServer = GxDocumentSender.CreateDocumentServer(_serverName);
			}
			currentServer.Start();
		}

		public void StopServer()
		{
			if (currentServer != null) currentServer.Shutdown(true);	 
		}

		public void ExecuteStart(String functionName, bool isTransaction)
        {
            try
            {
                RfcDestination destination = RfcDestinationManager.GetDestination(_connName);

                if (isTransaction)
                {
                    RfcSessionManager.BeginContext(destination);

                }
                executeFunction = destination.Repository.CreateFunction(functionName);

            }
            catch (Exception ex)
            {
                System.Console.Out.WriteLine(ex.ToString());
                throw ex;

            }
        }

        public void Begin()
        {
            RfcDestination destination = RfcDestinationManager.GetDestination(_connName);
            RfcSessionManager.BeginContext(destination);
        }

        public void Commit()
        { 
            RfcDestination destination;
            try
            {
                destination = RfcDestinationManager.GetDestination(_connName);
                IRfcFunction commitFnc = destination.Repository.CreateFunction("BAPI_TRANSACTION_COMMIT");
                commitFnc.Invoke(destination);
                RfcSessionManager.EndContext(destination);
            }
            catch (Exception ex)
            {
                System.Console.Out.WriteLine(ex.ToString());
                connMgrSAP.ErrorCode = "-1";
                connMgrSAP.ErrorMessage = ex.Message;               
            }
        }

        public int ExecuteFunction(bool isTransaction)
        {
            RfcDestination destination;
            try
            {
                destination = RfcDestinationManager.GetDestination(_connName);                
                if (isTransaction)
                {
					IRfcFunction commitFnc = destination.Repository.CreateFunction("BAPI_TRANSACTION_COMMIT");
                    executeFunction.Invoke(destination);
                    commitFnc.Invoke(destination);
                    RfcSessionManager.EndContext(destination);
                }
                else
                {
                    executeFunction.Invoke(destination);
                }

            }
            catch (Exception ex)
            {
                System.Console.Out.WriteLine(ex.ToString());
                connMgrSAP.ErrorCode = "-1";
                connMgrSAP.ErrorMessage = ex.Message;
                return -1;
            }

            return 0;
        }
      
        public void Disconnect()
        {
            connMgrSAP.disconnect();
		}

		public bool IsConnected(string sessionName) {
			connMgrSAP.ConnectionName = sessionName;
			return TestConnection(out String emessage);
		}

		public bool TestConnection(out String emessage) 
        {
           return connMgrSAP.TestConnection(connMgrSAP.ConnectionName, out emessage);
        }

		public int ErrorCode
		{
			get
			{
				if (connMgrSAP != null && !string.IsNullOrEmpty(connMgrSAP.ErrorCode))
					return Convert.ToInt32(connMgrSAP.ErrorCode);
				else
					return 0;
			}
		}
		public string ErrorMessage
		{
			get
			{
				if (connMgrSAP != null && !string.IsNullOrEmpty(connMgrSAP.ErrorMessage))
					return connMgrSAP.ErrorMessage;
				else
					return "";
			}
		}
	}
}
