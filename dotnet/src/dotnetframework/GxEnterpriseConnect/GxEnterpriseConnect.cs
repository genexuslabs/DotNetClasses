using System;
using GeneXus.Utils;
using GeneXus.Application;

namespace GeneXus.SAP
{
    public class GxEnterpriseConnect
    {

        GxSAPNativeRPC rpchelper = null;

        public GxEnterpriseConnect( GXECSessionManager manager)
        {
            if (manager != null)
            {
                rpchelper = new GxSAPNativeRPC(manager);
            }
        }
        
        public GxEnterpriseConnect(IGxContext context)
        {
            rpchelper = new GxSAPNativeRPC(context);
        }

        public void Disconnect()
        {
            if (rpchelper != null)
            {
                rpchelper.Disconnect();
            }
        }       

        public void GetValue(String name, out String result)
        {
            rpchelper.getFieldValues(name, out result);
        }

        public void GetValue(String name, out DateTime result)
        {
            rpchelper.getFieldValues(name, out result);
        }

        public void GetValue(String name, out int result)
        {
            rpchelper.getFieldValues(name, out result);
        }

        public void GetValue(String name, out short result)
        {
            rpchelper.getFieldValues(name, out result);
        }

        public void GetValue(String name, out long result)
        {
            rpchelper.getFieldValues(name, out result);
        }

        public void GetValue(String name, out float result)
        {
            rpchelper.getFieldValues(name, out result);
        }

        public void GetValue(String name, IGxJSONAble sdtcoll)
        {
            rpchelper.getFieldValues(name, sdtcoll);
        }

        public void GetValue(String name, out decimal result)
        {
            rpchelper.getFieldValues(name, out result);
        }

        public void SetValue(String name, String val)
        {
            rpchelper.setValue(name, val);
        }

        public void SetValue(String name, int val)
        {
            rpchelper.setValue(name, val);
        }

        public void SetValue(String name, short val)
        {
            rpchelper.setValue(name, val);
        }

        public void SetValue(String name, decimal val)
        {
            rpchelper.setValue(name, val);
        }

        public void SetValue(String name, long val)
        {
            rpchelper.setValue(name, val);
        }

        public void SetValue(String name, DateTime val)
        {
            rpchelper.setValue(name, val);
        }

        public void SetValue(String name, GxUserType sdt)
        {
            rpchelper.setFieldValues(name, sdt);
        }

        public void SetValue(String name, IGxCollection sdtColl)
        {
            rpchelper.setFieldValues(name, sdtColl);
        }

		public void SetValueEmpty(String name)
		{
			rpchelper.setValueEmpty(name);
		}

		public void SetValueNull(String name)
		{
			rpchelper.setValueNull(name);
		}

		public void TransactionBegin()
        {
            rpchelper.Begin();
        }

        public void TransactionCommit()
        {
            rpchelper.Commit();
        }

		public void StartReceiverServer()
		{
			rpchelper.StartReceiverServer();
		}

		public void StartSenderServer()
		{
			rpchelper.StartSenderServer();
		}

		public void StopReceiver()
		{
			rpchelper.StopServer();
		}
		public void StopSender()
		{
			rpchelper.StopServer();
		}

		public int InvokeFunction(String name)
        {
            rpchelper.ExecuteFunction(false);
            return 0;
        }

        public void StartFunction(String name)
        {
            rpchelper.ExecuteStart(name, false);
        }

		public bool IsConnected(string sesssionName)
		{
			return rpchelper.IsConnected(sesssionName);
		}
		public bool TestConnection(out String emessage)
        {
            return rpchelper.TestConnection(out emessage);
        }

		public int GetErrorCode()
		{
			return rpchelper.ErrorCode;
		}
		public string GetErrorMessage()
		{
			return rpchelper.ErrorMessage;
		}
    }
}
