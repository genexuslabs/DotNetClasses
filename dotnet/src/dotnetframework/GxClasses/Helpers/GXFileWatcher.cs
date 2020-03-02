namespace GeneXus.Application
{
	using System;
	using System.Collections;
	using System.IO;
	using GeneXus.Configuration;
	using log4net;
	using System.Collections.Generic;
	using System.Web;
	using System.Threading;
#if NETCORE
	using Microsoft.AspNetCore.Http;
#else
	using System.Web.Configuration;
#endif
	using System.Xml;

	public class GXFileWatcher : IDisposable
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXFileWatcher));
		private static volatile GXFileWatcher m_Instance;
		private static object m_SyncRoot = new Object();
		TimeSpan TIMEOUT;
		private static bool DISABLED;
		long TIMEOUT_TICKS;
		List<GxFile> tmpFiles;
		Dictionary<string,List<GxFile>> webappTmpFiles;
#if !NETCORE
		Thread t;
#endif
		public static GXFileWatcher Instance
		{
			get
			{
				if (m_Instance == null)
				{
					lock (m_SyncRoot)
					{
						if (m_Instance == null)
							m_Instance = new GXFileWatcher(); 
					}
				}
				return m_Instance;
			}
		}
		private GXFileWatcher()
		{
			webappTmpFiles = new Dictionary<string, List<GxFile>>();
			string fwTimeout = string.Empty;
			long result = 0;
			string delete;
			if (Config.GetValueOf("DELETE_ALL_TEMP_FILES", out delete) && (delete.Equals("0") || delete.StartsWith("N", StringComparison.OrdinalIgnoreCase)))
			{
				DISABLED = true;
			}

			if (Config.GetValueOf("FILEWATCHER_TIMEOUT", out fwTimeout) && Int64.TryParse(fwTimeout, out result))
			{
				TIMEOUT = TimeSpan.FromSeconds(result);
				GXLogging.Debug(log, "TIMEOUT (from FILEWATCHER_TIMEOUT)", () => TIMEOUT.TotalSeconds + " seconds");
			}
			else
			{
				var webconfig = Path.Combine(GxContext.StaticPhysicalPath(), "web.config");
				if (File.Exists(webconfig))
				{
					XmlDocument inventory = new XmlDocument();
					inventory.Load(webconfig);

					double secondsTime;
					XmlNode element = inventory.SelectSingleNode("//configuration/system.web/httpRuntime/@executionTimeout");
					if (element != null)
					{
						if (!string.IsNullOrEmpty(element.Value) && double.TryParse(element.Value, out secondsTime))
						{
							TIMEOUT = TimeSpan.FromSeconds(secondsTime);
							GXLogging.Debug(log, "TIMEOUT (from system.web/httpRuntime ExecutionTimeout)", () => TIMEOUT.TotalSeconds + " seconds");
						}
					}
				}
			}
			if (TIMEOUT.TotalSeconds == 0)
				TIMEOUT = TimeSpan.FromSeconds(110);

			TIMEOUT_TICKS = TIMEOUT.Ticks;
		}
		public void AsyncDeleteFiles(GxDirectory directory)
		{
			GXLogging.Debug(log, "DeleteFiles ", directory.GetName());
			try
			{
				if (!DISABLED && directory.Exists())
				{

					Thread t = new Thread(new ParameterizedThreadStart(DeleteFiles));
					t.Priority = ThreadPriority.BelowNormal;
					t.IsBackground = true;
					t.Start(directory);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "DeleteFiles error", ex);
			}
		}
		private void DeleteFiles(object odirectory)
		{
			try
			{
				GxDirectory directory = (GxDirectory)odirectory;
				long now = DateTime.Now.Ticks;
				foreach (GxFile file in directory.GetFiles("*.*"))
				{
					if (ExpiredFile(file.GetAbsoluteName(), now))
					{
						file.Delete();
					}
					GXLogging.Debug(log, "DeleteFiles ", file.GetName());
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "DeleteFiles error", ex);
			}
		}
		public void AddTemporaryFile(GxFile FileUploaded)
		{
			if (!DISABLED)
			{
				GXLogging.Debug(log, "AddTemporaryFile ", FileUploaded.Source);
#if !NETCORE
				if (HttpContext.Current != null)
				{
					try
					{
						string sessionId;
						if (HttpContext.Current.Session != null)
							sessionId = HttpContext.Current.Session.SessionID;
						else
							sessionId = "nullsession";
						List<GxFile> sessionTmpFiles;
						lock (m_SyncRoot)
						{
							if (webappTmpFiles.ContainsKey(sessionId))
								sessionTmpFiles = (List<GxFile>)webappTmpFiles[sessionId];
							else
								sessionTmpFiles = new List<GxFile>();
							sessionTmpFiles.Add(FileUploaded);
							webappTmpFiles[sessionId] = sessionTmpFiles;
						}
					}
					catch (Exception exc)
					{
						GXLogging.Error(log, "AddTemporaryFile Error", exc);
					}
					if (t == null)
					{
						GXLogging.Debug(log, "ThreadStart GXFileWatcher.Instance.Run");
						t = new Thread(new ThreadStart(GXFileWatcher.Instance.Run));
						t.IsBackground = true;
						t.Start();
					}
				}
				else
#endif
				{
					if (tmpFiles == null)
						tmpFiles = new List<GxFile>();
					lock (tmpFiles)
					{
						tmpFiles.Add(FileUploaded);
					}
				}
			}
		}

		private void Run()
		{
			if (!DISABLED)
			{
				while (true)
				{
					Thread.Sleep(TIMEOUT);
					GXLogging.Debug(log, "loop start ");
					DeleteWebAppTemporaryFiles(false);
					GXLogging.Debug(log, "loop end ");
				}
			}
		}
		private bool ExpiredFile(string filename, long since)
		{
			return (File.Exists(filename) && since - File.GetLastAccessTime(filename).Ticks > TIMEOUT_TICKS);
		}
		private void DeleteWebAppTemporaryFiles(bool disposing)
		{
			
			if (webappTmpFiles != null && webappTmpFiles.Count > 0)
			{
				long now = DateTime.Now.Ticks;
				ArrayList toRemove = new ArrayList();
				try
				{
					foreach (string sessionId in webappTmpFiles.Keys)
					{
						List<GxFile> files = new List<GxFile>();
						webappTmpFiles.TryGetValue(sessionId, out files); 
						if (files != null && files.Count > 0)
						{
							var lastFileName = files[files.Count - 1].GetURI();
							if (disposing || ExpiredFile(lastFileName, now))
							{
								try
								{
									lock (m_SyncRoot)
									{
										foreach (GxFile f in files)
										{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
											f.Delete();
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
											GXLogging.Debug(log, "File.Delete ", f.GetURI());
										}
									}
								}
								catch (Exception ex)
								{
									GXLogging.Error(log, "File.Delete error sessionid ", sessionId, ex);
								}
								toRemove.Add(sessionId);
							}
						}
					}
				}
				catch (Exception ex1)
				{
					GXLogging.Error(log, "MoveNext webappTmpFiles error ", ex1);
				}
				try
				{
					foreach (string sessionId in toRemove)
					{
						webappTmpFiles.Remove(sessionId);
					}
				}
				catch (Exception ex2)
				{
					GXLogging.Error(log, "Remove webappTmpFiles error", ex2);
				}
			}
		}
		public void DeleteTemporaryFiles()
		{
			if (!DISABLED)
			{
				if (tmpFiles != null)
				{
					lock (tmpFiles)
					{
						foreach (GxFile s in tmpFiles)
						{
							s.Delete();
						}
						tmpFiles.Clear();
					}
				}
			}
		}
#region IDisposable Members

		public void Dispose()
		{
			if (!DISABLED)
			{
				GXLogging.Debug(log, "GXFileWatcher Dispose");
#if !NETCORE
				if (t != null && t.IsAlive) t.Abort();
#endif
				DeleteWebAppTemporaryFiles(true);
			}
		}
#endregion
	}

}
