using System;
#if !NETCORE
using System.Messaging;
#else
using Experimental.System.Messaging;
#endif
using System.Text;
using System.Text.RegularExpressions;
using GeneXus.Configuration;
using GeneXus.Application;
using log4net;
using System.Security;

namespace GeneXus.Utils
{
	[SecuritySafeCritical]
	public class GxQueueMessage
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GxQueueMessage));
		int priority;

		Message message;

		GXProperties properties;

		public GxQueueMessage()
		{
			message = new Message();
			properties = new GXProperties();
		}
		public string Text
		{
			get 
			{
				loadProperties( message);
				return message.Body.ToString();
			}
			set {message.Body = value;}
		}
		public int Priority
		{
			get 
			{
				if (priority == 0)
					priority = messagePriorityToPriority( message.Priority);
				return priority;
			}
			set 
			{
				message.Priority = priorityToMessagePriority( value);
               GXLogging.Debug(log, "Message priority:" + message.Priority + " (value:" + value + ")");
				priority = value;
			}
		}
		public string CorrelationId
		{
			get
			{
				return message.CorrelationId;
			}
			set
			{
				try
				{
					message.CorrelationId = value;
				}
				catch (InvalidOperationException ex)
				{
					GXLogging.Debug(log, "Error setting CorrelationId: " + ex.Message + " (value:" + value + ")");
				}
			}
		}
		public string MessageID
		{
			get
			{
				return message.Id;
			}
			set
			{
			}
		}
		int messagePriorityToPriority( MessagePriority p)
		{
			switch (p)
			{
				case MessagePriority.Highest:
					return 0;
				case MessagePriority.VeryHigh:
					return 2;
				case MessagePriority.High:
					return 3;
				case MessagePriority.AboveNormal:
					return 4;
				case MessagePriority.Normal:
					return 5;
				case MessagePriority.Low:
					return 6;
				case MessagePriority.VeryLow:
					return 7;
				case MessagePriority.Lowest:
					return 9;
			}
			return 5;
		}
		MessagePriority priorityToMessagePriority( int p)
		{
			switch (p)
			{
				case 0:
				case 1:
					return MessagePriority.Highest;
				case 2:
					return MessagePriority.VeryHigh;
				case 3:
					return MessagePriority.High;
				case 4:
					return MessagePriority.AboveNormal;
				case 5:
					return MessagePriority.Normal;
				case 6:
					return MessagePriority.Low;
				case 7:
					return MessagePriority.VeryLow;
				case 8:
				case 9:
					return MessagePriority.Lowest;
			}
			return MessagePriority.Normal;
		}
		public Message GetBaseMessage(MessagePropertyFilter mpf)
		{
			int i = 0;
			StringBuilder sb = new StringBuilder();
			if (properties.Count > 0)
			{
				
				bool hasUserProps = false;
				sb.Append("[GxPtys:]");
				for (i=0; i < properties.Count; i++)
				{
					if (! canAddMSMQPropertiesToGet( mpf, properties.GetKey(i), properties.Get(properties.GetKey(i))))
					{
						sb.Append( properties.GetKey(i)+"="+properties.Get(properties.GetKey(i)) +";");
						hasUserProps = true;
					}
				}
				if (hasUserProps)
					message.Body = Text+sb.ToString();
			}
			return message;
		}
		public void SetBaseMessage(Message m)
		{
			message = m;
			loadProperties( message);
		}
		public void loadProperties( Message msg)
		{
			string body, bodyProps;
			if (msg.Body == null)
				body = "";
			else 
				body = msg.Body.ToString();

			int ptysPos = body.IndexOf( "[GxPtys:]");
			if (ptysPos >= 0)
			{
				bodyProps = body.Substring( ptysPos+9);
				properties.Clear();
				string[] nameValue;
				string[] ptys = bodyProps.Split(';');
				foreach( string pty in ptys)
				{
					if (pty.Trim().Length > 0)
					{
						nameValue = pty.Split('=');
						properties.Add( nameValue[0].Trim(), nameValue[1]);
					}
				}
				msg.Body = removeProperties( body);
			}
		}
		string removeProperties( string body)
		{
			int ptysPos = body.IndexOf( "[GxPtys:]");
			if (ptysPos >= 0)
				body = body.Remove( ptysPos, body.Length - ptysPos);
			return body;
		}
		public GXProperties Properties
		{
			get 
			{
				loadProperties( message);
				return properties;
			}
			set {properties = value;}
		}
		public bool canAddMSMQPropertiesToGet( MessagePropertyFilter m, string key, string val )
		{
			bool propertyFound = false;
			
			return propertyFound;
		}
	}
	[SecuritySafeCritical]
	public class GxQueue
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxQueue));

		string provider;
		string user;
		string password;
		string queryString;
		bool browse;
		short errCode = 0;
		string errDescription = "";
		MessagePropertyFilter propertyFilter;
		
		string path;
		MessageQueue mq;
		
		MessageEnumerator mEnum;
		bool mEof;
		IGxContext context;

		public GxQueue( IGxContext context)
		{
			this.context = context;
			this.queryString = "";
			propertyFilter = new MessagePropertyFilter();
			propertyFilter.SetDefaults();
			propertyFilter.CorrelationId = true;
			propertyFilter.Priority = true;
		}

		public string Provider
		{
			get {return provider;}
			set 
			{
				setProvider( value);
				provider = value;
			}
		}
		public string User
		{
			get {return user;}
			set {user = value;}
		}
		public string Password
		{
			get {return password;}
			set{password = value;}
		}
		public string QueryString
		{
			get {return queryString;}
			set {queryString = value;}
		}
		public bool Browse
		{
			get {return browse;}
			set {browse = value;}
		}
		public short ErrCode
		{
			get { return errCode; }
		}

		public string ErrDescription
		{
			get { return errDescription; }
		}
		private void StartQueueService()
        {
        }
        private void CreateQueue(string path)
        {
            MessageQueue.Create(path);
        }
        private bool QueueExits(string path){
            if (!String.IsNullOrEmpty(path)){
                if (path.ToLower().StartsWith("formatname")){
                    //The Exists method does not support the FormatName prefix.
                    return true;
                }
                else if (path.IndexOf(".") < 0 && path.ToLower().IndexOf("private$") < 0)
                {
                    //Exists cannot be called to verify the existence of a remote private queue.
                    return true;
                }
                else
                {
                    return MessageQueue.Exists(path);
                }
            }
            return false;
        }
		public bool Connect()
		{
			try
			{
                StartQueueService();
                if (!QueueExits(path))//Exists cannot be called to verify the existence of a remote private queue
                {
                    string value = "0";
                    if (Config.GetValueOf("Queue-AutomaticCreate", out value) && value == "0")
                        return false;
                    else
                    {
                        try
                        {
                            CreateQueue(path);
                        }
                        catch(
                            MessageQueueException mex) {
							errCode = 1;
							errDescription = mex.Message;
							GXLogging.Error(log, "Create queue error code:" + mex.MessageQueueErrorCode + ", queue:" + path, mex);
                            return false; 
                        }
                    }
                }
				mq = new MessageQueue(path); 
				mq.Formatter = new XmlMessageFormatter( new Type[] {typeof(string)});
				mq.MessageReadPropertyFilter = propertyFilter;

				if (transactional(mq))
				{
					if (context.MQTransaction == null)
						context.MQTransaction = new MessageQueueTransaction();
				}
				return true;
			}
			catch(Exception ex)
			{
				errCode = 1;
				errDescription = ex.Message;
				GXLogging.Error(log, "Connect error path:" + path, ex);
				return false;
			}
		}
		private bool transactional(MessageQueue mq)
		{
			try
			{
				return mq.Transactional;
			}
			catch(MessageQueueException)
			{
				//Transactional cannot be called on remote queues.
				return false;
			}
		}
		public void Disconnect()
		{
			mq = null;
		}
		public void Commit()
		{
            if (transactional(mq) && context.MQTransaction != null)
				if (context.MQTransaction.Status == MessageQueueTransactionStatus.Pending)
					context.MQTransaction.Commit();
		}
		public void Rollback()
		{
            if (transactional(mq) && context.MQTransaction != null)
				if (context.MQTransaction.Status == MessageQueueTransactionStatus.Pending)
					context.MQTransaction.Abort();
		}
		public string Send( GxQueueMessage msg)
		{
            Message m = msg.GetBaseMessage(this.propertyFilter);
            try
			{
				if (mq == null)
				{
					errCode = 1;
					errDescription = "Send message error: connection to the queue is closed";
					GXLogging.Debug(log, errDescription);
					return "";
				}
                if (transactional(mq) && context.MQTransaction != null)
				{
					if (context.MQTransaction.Status != MessageQueueTransactionStatus.Pending)
						context.MQTransaction.Begin();
                   GXLogging.Debug(log, "Sending message to transactional queue:" + mq.Path);
					mq.Send( m, context.MQTransaction );
				}
                else{
                   GXLogging.Debug(log, "Sending message to non transactional queue:" + mq.Path);
					mq.Send( m);
                }
				msg.SetBaseMessage( m);
			}
			catch (MessageQueueException mex)
			{
				errCode = 1;
				errDescription = mex.Message;
				GXLogging.Debug(log, "Send message error code:" +  mex.MessageQueueErrorCode + ", msmq path:" + mq.Path, mex);
                return "";
			}
			catch (Exception ex)
			{
				errCode = 1;
				errDescription = ex.Message;
				GXLogging.Debug(log, "Send message error, msmq path:" + mq.Path, ex);
                return "";
			}
			return m.Id;
		}
		public GxQueueMessage GetFirst()
		{
			mEof = false;
			mEnum = mq.GetMessageEnumerator();
			
			return GetNext();
		}
		public GxQueueMessage GetNext()
		{
			GxQueueMessage gxm = new GxQueueMessage();
			bool messageExist = false;
			bool validMessage = false;
			while (! validMessage)
			{
				try
				{
					messageExist = mEnum.MoveNext();
					if (messageExist)
					{
						Message m = (Message) (mEnum.Current);
						gxm.SetBaseMessage( m);
						validMessage = testMessage( gxm);
						if (validMessage && !browse)
							mEnum.RemoveCurrent();
					}
					else 
						validMessage = true;
				}
				catch (MessageQueueException mex)
				{
					errCode = 1;
					errDescription = mex.Message;
					messageExist = false;
					validMessage = true;
				}
			}
			mEof = ! messageExist;
			return gxm;
		}
		public bool Eof()
		{
			return mEof;
		}
		bool testMessage( GxQueueMessage gxm)
		{
			if (QueryString.Trim().Length == 0)
				return true;
			Regex r = new Regex(@"\s*(?<ptyName>\S+)\s*(?<oper>[=><!][=>]{0,1})\s*(?<ptyVal>\S+);{0,1}");
			Match m = r.Match( QueryString);
			bool result = true;
			while ( m.Success )
			{
				result = false;   // have to match all the query conditions
				for (int i=0; i < gxm.Properties.Count; i++)
				{
					
					if ( m.Groups["ptyName"].Value.Trim() ==  gxm.Properties.GetKey(i).Trim())
					{
						
						if ( ! testCondition( gxm.Properties.Get(gxm.Properties.GetKey(i)).Trim(), m.Groups["oper"].Value.Trim(), m.Groups["ptyVal"].Value.Trim()))
							return false;    
						else 
							result = true;
						break;
					}
				}
				m = m.NextMatch();
			}
			return result;
		}
		bool testCondition( string left, string op, string right)
		{
			if (termType(right) == "N")
				return compareAsNumeric( left, op, right);
			else
				return compareAsString( left, op, right);
		}
		string termType( string term)
		{

			if (term.StartsWith( "'") && term.EndsWith("'"))
				return "C";
			if ( "0123456789".IndexOf( term[0] ) != -1)
				return "N";
			return "U";
		}
		string normalizeTerm( string txt)
		{
			string txtT = txt.Trim();
			if (txtT.StartsWith( "'") && txtT.EndsWith("'"))
				return txtT.Substring( 1, txtT.Length-2);
			return txtT;
		}
		bool compareAsNumeric( string sleft, string op, string sright)
		{
			decimal left = Convert.ToDecimal( sleft);
			decimal right = Convert.ToDecimal( sright);
			switch (op)
			{
				case "=":
				case "==":
					if (left == right)
						return true;
					break;
				case ">":
					if (left > right)
						return true;
					break;
				case "<":
					if (left < right)
						return true;
					break;
				case ">=":
					if (left >= right)
						return true;
					break;
				case "<=":
					if (left <= right)
						return true;
					break;
				case "<>":
				case "!=":
					if (left !=  right)
						return true;
					break;
			}
			return false;
		}
		bool compareAsString( string left, string op, string right)
		{
			left = normalizeTerm( left);
			right = normalizeTerm( right);
			switch (op)
			{
				case "=":
				case "==":
					if (left == right)
						return true;
					break;
				case ">":
					if (String.Compare( left , right) > 0)
						return true;
					break;
				case "<":
					if (String.Compare( left,  right) < 0)
						return true;
					break;
				case ">=":
					if (String.Compare( left ,  right) >= 0)
						return true;
					break;
				case "<=":
					if (String.Compare( left , right) <= 0)
						return true;
					break;
				case "<>":
				case "!=":
					if (String.Compare( left , right) != 0)
						return true;
					break;
			}
			return false;
		}
		void setProvider( string provName)
		{
			string queuePath;
			if ( Config.GetValueOf("Queue-"+provName.Trim(), out queuePath))
				path = queuePath;
			else
				path = provName;

		}
	}
}