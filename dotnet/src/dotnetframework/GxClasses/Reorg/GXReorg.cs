namespace GeneXus.Reorg 
{
	using System;
	using System.IO;
	using System.Collections;
	using GeneXus.Application;
	using log4net;
	using System.Collections.Specialized;
    using System.Runtime.CompilerServices;
    using GeneXus.Metadata;
	using System.Threading;
	using GeneXus.Configuration;
	using GeneXus.Data.ADO;
	using GeneXus.Data;
	using GeneXus.Resources;
	using GeneXus.Utils;
    using GeneXus.Data.NTier;
	using System.Reflection;

	public interface IReorgReader
	{

		void NotifyMessage(string idMsg, Object[] args);
		void NotifyMessage(int id, string idMsg, Object[] args);
		void NotifyStatus(int id, ReorgBlockStatusInfo status);
		void NotifyEnd(string id);
        void NotifyError();
        bool GuiDialog{get;}
	}


    public class GXReorganization
    {
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Reorg.GXReorganization));
        public virtual void initialize() { }
		public virtual void cleanup() { }
		static ArrayList lstMsgs;
        public static IReorgReader _ReorgReader;
        protected IGxContext _Context;
        public static bool Error;
        bool _isMain;
        public static bool printOnlyRecordCount;
        public static bool ignoreResume;
        public static bool noPreCheck;
		const string AFTER_REOR_SCRIPT = "afterReorganizationScript.txt";
		const string BEFORE_REOR_SCRIPT = "beforeReorganizationScript.txt";
		const string RESUME_REOR_FILE = "resumereorg.txt";


		public GXReorganization()
        {
            DataStoreUtil.LoadDataStores(new GxContext()); //force to load dbms library (p.e. libmysql.dll 32x)
            _isMain = true;
            GxContext.isReorganization = true;
           GXLogging.Debug(log, "GXReorganization.Ctr()");
        }
        public IGxContext context
        {
            get { return _Context; }
            set { _Context = value; }
        }
        public bool IsMain
        {
            set { _isMain = value; }
            get { return _isMain; }
        }
        public static void AddMsg(string msg, Object[] args)
        {
            if (_ReorgReader != null)
            {
                if (msg != null)
                    _ReorgReader.NotifyMessage(msg, args);
            }
            else
            {
                if (lstMsgs == null)
                    lstMsgs = new ArrayList();
                lstMsgs.Add(msg);
            }
        }

        public static void NotifyEnd()
        {
            if (_ReorgReader != null)
                _ReorgReader.NotifyEnd("");
        }

        public static void NotifyError()
        {
            Error = true;
        }

        public virtual void ExecForm()
        {
        }

        public virtual bool GetCreateDataBase()
        {
            return false;
        }

        public virtual bool PrintOnlyRecordCount()
        {
            return printOnlyRecordCount;

        }
        public virtual void PrintRecordCount(string table, int recordCount)
        {
            if (!executingResume)
            {
				AddMsg(GXResourceManager.GetMessage("GXM_table_recordcount", new object[] { table, recordCount.ToString() }), null);
            }
        }
        public static ArrayList ReorgLog
        {
            get
            {
                
                return lstMsgs;
            }
            set
            {
                lstMsgs = value;
            }
        }
        public void RegisterForSubmit(string blockName, string msg, int id, object[] parms)
        {
            ReorgExecute.RegisterBlockForSubmit(id, blockName, parms);
        }
        public static void SetMsg(int id, string msg)
        {
            if (_ReorgReader != null)
            {
                if (msg != null)
                    _ReorgReader.NotifyMessage(id, msg, null);
            }
            else
            {
                if (lstMsgs == null)
                    lstMsgs = new ArrayList();
                lstMsgs.Add(id.ToString() + " - " + msg);
            }
        }
        public static void SetStatus(int id, ReorgBlockStatusInfo rs)
        {
            if (_ReorgReader != null)
                _ReorgReader.NotifyStatus(id, rs);
        }
        public void ExecBeforeReorg()
        {
            ArrayList stmts = ParseStmtFile(ReorgScriptType.Before);
            IGxDataStore dsDefault = context.GetDataStore("Default");
            foreach (string stmt in stmts)
            {
				AddMsg(GXResourceManager.GetMessage("GXM_executing", new object[] { stmt}), null);
				GxCommand RGZ = new GxCommand(dsDefault.Db, stmt, dsDefault, 0, true, false, null);
                RGZ.ExecuteStmt();
                RGZ.Drop();
            }
        }
		public virtual void ExecDataInitialization()
		{
		}

        public void ExecAfterReorg()
        {
			ExecDataInitialization();
            DeleteResumeFile();
			ArrayList stmts = ParseStmtFile(ReorgScriptType.After);
            IGxDataStore dsDefault = context.GetDataStore("Default");
            foreach (string stmt in stmts)
            {
				AddMsg(GXResourceManager.GetMessage("GXM_executing", new object[] { stmt }), null);
				GxCommand RGZ = new GxCommand(dsDefault.Db, stmt, dsDefault, 0, true, false, null);
                RGZ.ExecuteStmt();
                RGZ.Drop();
            }
		}
		public ArrayList ParseStmtFile(ReorgScriptType time)
		{
			string stmtString;
			string fileName = time== ReorgScriptType.After ? AFTER_REOR_SCRIPT : BEFORE_REOR_SCRIPT;

			if (!File.Exists(fileName))
				return new ArrayList();
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					stmtString = sr.ReadToEnd();
				}
			}
			ArrayList stmts = new ArrayList();
            System.Text.StringBuilder sb = new System.Text.StringBuilder(stmtString);
            System.Text.StringBuilder stmt = new System.Text.StringBuilder();
            bool inLiteral = false;
            for (int i = 0; i < sb.Length; i++)
            {
                if (inLiteral)
                {
                    if (sb[i] == '"')
                        inLiteral = false;
                }
                else
                {
                    if (sb[i] == '"')
                        inLiteral = true;
                    else if (sb[i] == ';')
                    {
                        stmts.Add(stmt.ToString().Trim());
                        stmt = new System.Text.StringBuilder();
                        continue;
                    }
                }
                stmt.Append(sb[i]);
            }
            if (stmt.ToString().Trim().Length > 1)
                stmts.Add(stmt.ToString().Trim());
            return stmts;
        }

        private static Hashtable executedStatements = new Hashtable();
        private static bool executingResume;
        private static bool createDataBase;

        public bool BeginResume()
        {
			StreamReader input = null;
				
            try
            {
                if (createDataBase || ignoreResume)
                {
                    File.Delete(RESUME_REOR_FILE);
                }
                else if (File.Exists(RESUME_REOR_FILE))
                {
					input = File.OpenText(RESUME_REOR_FILE);
                    String statement = input.ReadLine();
					if (!string.IsNullOrEmpty(statement))
                    {
                        string timeStamp;
                        Config.GetValueOf("VER_STAMP", out timeStamp);
                        if (statement!=timeStamp)
                        {
							AddMsg(GXResourceManager.GetMessage("GXM_lastreorg_failed1"), null);
							AddMsg(GXResourceManager.GetMessage("GXM_lastreorg_failed2"), null);
							AddMsg(GXResourceManager.GetMessage("GXM_lastreorg_failed3"), null);
							GXReorganization.Error = true;
                            return false;
                        }
                    }
                    while (statement != null)
                    {
                        executedStatements[statement] = null;
                        statement = input.ReadLine();
                    }
					executingResume = true;
                }
				return true;
            }
            catch (Exception ex)
            {
                GXLogging.Error(log, "Beginresume error", ex);
				GXReorganization.Error = true;
				return false;
			}
            finally
            {
#if !NETCORE
				if (input != null)
					input.Close();
                SerializeExecutedStatements();
#endif
			}
		}

        public static bool ExecutedBefore(String statement)
        {
            if (executingResume)
            {
                return executedStatements.Contains(statement);
            }
            return false;
        }

#if !NETCORE
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void AddExecutedStatement(String statement)
        {
            try
            {
                if (output != null)
                {
                    output.WriteLine(statement);
                    output.Flush();
                }
            }
            catch (IOException ex)
            {
                GXLogging.Error(log, "AddexecutedStatemet error", ex);
            }
        }
#else
		public static void AddExecutedStatement(String statement) { }
#endif
		public static void SetCreateDataBase()
        {
            createDataBase = true;
        }
#if !NETCORE
        private static StreamWriter output;

		private void SerializeExecutedStatements()
        {
            try
            {
                output = new StreamWriter(RESUME_REOR_FILE, true);
                if (!executingResume)
                {
                    string timeStamp;
                    Config.GetValueOf("VER_STAMP", out timeStamp);
                    output.WriteLine(timeStamp);
                    output.Flush();
                }
            }
            catch (IOException ex)
            {
                GXLogging.Error(log, "SerializeExecutedStatements error", ex);
            }
        }
#endif
		public static void CloseResumeFile()
        {
            try
            {
#if !NETCORE
				if (output != null)
					output.Close();
				output = null;
#endif
            }
            catch (Exception)
            {
            }
        }

        public static void DeleteResumeFile()
        {
            try
            {
#if !NETCORE
				if (output!=null)
					output.Close();
				output = null;
#endif
                File.Delete(RESUME_REOR_FILE);
            }
            catch (Exception)
            {
            }
        }

        public static bool checkError;

        public static void SetCheckError(String checkErrorMessage)
        {
            AddMsg(GXResourceManager.GetMessage("GXM_error_in_schema_verification"), null);
            AddMsg(checkErrorMessage, null);
            DeleteResumeFile();
            checkError = true;
            Error = true;
        }

        public virtual bool MustRunCheck()
        {
            return (!executingResume && !noPreCheck);
        }

        public virtual bool IsResumeMode()
        {
            return executingResume;
        }
		public static bool IsBadImageFormatException(Exception ex)
        {
#if !NETCORE
			return GXUtil.IsBadImageFormatException(ex);
#else
			return false;
#endif
		}
	}

		public delegate void BlockEndCallback(string blockName, int errorCode);

	public class ReorgExecute
	{
		static int threadsRunning;
		static int lastErrorCode;
		static ArrayList submitList = new ArrayList();
		static NameValueCollection precedenceList = new NameValueCollection();
        static bool onlyOneThread;

        public static void SetOnlyOneThread()
        {
            onlyOneThread = true;
        }

		public static void RegisterBlockForSubmit( int id, string blockName, object[] parms)
		{
			ReorgBlock execInfo = new ReorgBlock( id, blockName, parms, new BlockEndCallback(ReorgThreadEnd), Assembly.GetCallingAssembly());
			string sValue;
			if (Config.GetValueOf("AppMainNamespace", out sValue))
				execInfo.AppNamespace = sValue;

			if (! submitList.Contains( execInfo))
				lock( typeof( ReorgExecute))
				{
					submitList.Add( execInfo);
				}
		}
		public static void ReorgThreadEnd(string blockName, int errorCode)
		{
			Interlocked.Decrement( ref threadsRunning);
			if (errorCode > 0)	
				Interlocked.Exchange( ref lastErrorCode, errorCode);
		}
		public static void SubmitAll()
		{
			// Before reorg
			ReorgBlockStatusInfo rStat;
			rStat = executeBlock(new ReorgBlock(0, "ExecBeforeReorg", Array.Empty<object>(), null, Assembly.GetCallingAssembly()));
			if ( rStat.Status == ReorgBlockStatus.Error)
			{
				GXReorganization.SetStatus(0, rStat);
				return;
			}

			// Reorg
			bool retryExecute = true;
			int reorgMaxThreads;
			string sValue;
            if (onlyOneThread)
            {
                reorgMaxThreads = 1;
            }
            else
            {
                if (Config.GetValueOf("REORG_MAX_THREADS", out sValue))
                    reorgMaxThreads = int.Parse(sValue);
                else
                    reorgMaxThreads = 5;		
            }

			while ( threadsRunning > 0 || retryExecute) 
			{
				Thread.Sleep(500);
				
				retryExecute = false;
				foreach( ReorgBlock rt in submitList)
				{
					if (threadsRunning >= reorgMaxThreads)
						break;
					if (lastErrorCode == 0)	
						if( rt.StatusInfo.Status == ReorgBlockStatus.Pending)
						{
							retryExecute = true;
							submitBlock( rt);
						}
					GXReorganization.SetStatus( rt.Id, rt.StatusInfo);
				}
			}
			if (lastErrorCode != 0 || GXReorganization.checkError)
				throw new Exception("Reorganization error");
			else
			{
				// After reorg
				rStat = executeBlock(new ReorgBlock(0, "ExecAfterReorg", Array.Empty<object>(), null, Assembly.GetCallingAssembly()));
				if (rStat.Status == ReorgBlockStatus.Error)
				{
					GXReorganization.SetStatus(0, rStat);
					return;
				}
			}
		}
		static void submitBlock( ReorgBlock execInfo)
		{
			string canSubmitInfo;
			if( canSubmit( execInfo.BlockName, out canSubmitInfo))
			{
				Thread execThread;
				Interlocked.Increment( ref threadsRunning);
				execThread = new Thread(new ThreadStart( execInfo.ExecuteBlock));
				execThread.Start();
			}
			else 
				execInfo.StatusInfo.Status = ReorgBlockStatus.Pending;
			execInfo.StatusInfo.OtherStatusInfo = canSubmitInfo;
		}
		static ReorgBlockStatusInfo executeBlock(ReorgBlock execInfo)
		{
			execInfo.ExecuteBlock();
			return execInfo.StatusInfo;
		}
		static bool canSubmit(string name, out string submitInfo)
		{
			submitInfo = "";
			string[] blockPrecedences = precedenceList.GetValues( name);
			if (blockPrecedences == null)
				return true;

			foreach( string p in blockPrecedences)
				foreach( ReorgBlock rt in submitList)
					if( rt.BlockName == p && rt.StatusInfo.Status != ReorgBlockStatus.Ended)
					{
						submitInfo = "Waiting for "+p;
						return false;
					}
			return true;
		}
		public static void RegisterPrecedence( string blockName, string precedence)
		{
			precedenceList.Add( blockName, precedence);
		}
	}
	public class ReorgBlock
	{
		int id;
		string blockName;
		object[] blockParms;
		BlockEndCallback blockEnd;
		ReorgBlockStatusInfo statusInfo;
		Assembly assembly;
		string appNamespace;
		public ReorgBlock( int id, string blockName, object[] blockParms, BlockEndCallback blockEnd, Assembly reorgAssembly)
		{
			this.id = id;
			this.blockName = blockName;
			this.blockParms = blockParms;
			this.blockEnd = blockEnd;
			this.statusInfo = new ReorgBlockStatusInfo(ReorgBlockStatus.Pending, "");
			string appNS;
			if (Config.GetValueOf("AppMainNamespace", out appNS))
				this.appNamespace = appNS;
			else
				this.appNamespace = "GeneXus.Programs";
			assembly = reorgAssembly;
		}
		public void ExecuteBlock()
		{
			if ( statusInfo.Status != ReorgBlockStatus.Pending)
				return;
			statusInfo.Status = ReorgBlockStatus.Executing;
			string reorgAssembly = "Reorganization";
			GXReorganization o = (GXReorganization)(ClassLoader.FindInstance(reorgAssembly, appNamespace, "reorg", null, assembly));
			try
			{
				o.initialize();
				ClassLoader.Execute(o, blockName, blockParms);
				statusInfo.Status = ReorgBlockStatus.Ended;
				if (blockEnd != null)
					blockEnd( blockName, 0);
                o.context.CloseConnections();
			}
			catch (Exception e)
			{
				statusInfo.Status = ReorgBlockStatus.Error;
				if (blockEnd != null)
					blockEnd( blockName, 1);
				Exception innerE = e.InnerException;
				if (innerE != null)
				{
					statusInfo.OtherStatusInfo = innerE.Message;
					Console.WriteLine("ERROR in " + blockName + " : " + innerE.Message + StringUtil.NewLine() + innerE.StackTrace);
					
				}
				else
				{
					statusInfo.OtherStatusInfo = e.Message;
					Console.WriteLine("ERROR in " + blockName +" : "+ e.Message + StringUtil.NewLine() + e.StackTrace);
					
				}
			}
		}
		public string BlockName
		{
			get { return blockName;}
		}
		public int Id
		{
			get {return id;}
		}
		public ReorgBlockStatusInfo StatusInfo
		{
			get {return statusInfo;}
			set {statusInfo = value;}
		}
		public string AppNamespace
		{
			get { return appNamespace; }
			set { appNamespace = value; }
		}
	}
	public class ReorgBlockStatusInfo
	{
		ReorgBlockStatus status;
		string otherStatusInfo;
		public ReorgBlockStatusInfo( ReorgBlockStatus status, string otherInfo)
		{
			this.status = status;
			this.otherStatusInfo = otherInfo;
		}
		public ReorgBlockStatus Status
		{
			get {return status;}
			set {status = value;}
		}
		public string OtherStatusInfo
		{
			get { return otherStatusInfo;}
			set { otherStatusInfo = value;}
		}
	}
	public enum ReorgBlockStatus
	{
		Pending,
		Executing,
		Ended,
		Error
	}
	public enum ReorgScriptType
	{
		Before,
		After
	}
}
