using System;
using System.IO;
using System.Security;
using GeneXus.Configuration;

using log4net;

namespace GeneXus.Search
{

	[SecuritySafeCritical]	
	public class Settings
	{
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Search.Settings));
        private static Settings m_instance = new Settings();
		private Lucene.Net.Store.Directory m_storeFolder;
		private string m_indexFolder;
		private string m_lockFolder;
        private FileInfo m_stopWords;
		private int m_optimizeThreshold = 500;
		private int m_maxQueueSize = 200;
        private string m_analyzer = string.Empty;

		private Settings()
		{
			string dir;
			if (Config.GetValueOf("LUCENE_INDEX_DIRECTORY", out dir))
			{
				if (!Path.IsPathRooted(dir))
				{
#if NETCORE
					m_indexFolder = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, dir);
#else
					m_indexFolder = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName, dir);
#endif

				}
				else
				{
					m_indexFolder = dir;
				}
			}
			else
			{
				m_indexFolder = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName).FullName + Path.DirectorySeparatorChar + "LuceneIndex";
			}

			m_lockFolder = Path.Combine(m_indexFolder, "Lock");

			if (!Directory.Exists(m_indexFolder))
			{
				Directory.CreateDirectory(m_indexFolder);
			}
			if (!Directory.Exists(m_lockFolder))
			{
				Directory.CreateDirectory(m_lockFolder);
			}
            string stopWords = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stopwords.txt");
            if (File.Exists(stopWords))
            {
                GXLogging.Debug(log,"STOP_WORDS: " + stopWords);
                m_stopWords = new FileInfo(stopWords);
            }

			string maxQueueSize;
			try
			{
				if (Config.GetValueOf("INDEX_QUEUE_MAX_SIZE", out maxQueueSize))
				{
					m_maxQueueSize = Convert.ToInt32(maxQueueSize);
					GXLogging.Debug(log,"INDEX_QUEUE_MAX_SIZE: " + m_maxQueueSize);
				}
			}catch
			{}
            string analyzerName;
            if (Config.GetValueOf("LUCENE_ANALYZER", out analyzerName))
            {
                m_analyzer = analyzerName;
            }
			m_storeFolder = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(m_indexFolder));
		}

		public int OptimizeThreshold
		{
			get { return m_optimizeThreshold; }
			set { m_optimizeThreshold = value; }
		}

		public static Settings Instance
		{
			get { return m_instance; }
		}

		public string LockFolder
		{
			get { return m_lockFolder; }
			set { m_lockFolder = value; }
		}
		public Lucene.Net.Store.Directory StoreFolder
		{
			get { return m_storeFolder; }
			set { m_storeFolder = value; }
		}
		public string IndexFolder
		{
			get { return m_indexFolder; }
			set { m_indexFolder = value; }
		}
		public FileInfo StopWords
		{
			get { return m_stopWords; }
		}

		public int MaxQueueSize
		{
			get { return m_maxQueueSize; }
		}
        public string Analyzer
        {
            get { return m_analyzer; }
        }

        public const string WhitespaceAnalyzer = "WhitespaceAnalyzer";
        public const string SimpleAnalyzer = "SimpleAnalyzer";
        public const string StopAnalyzer = "StopAnalyzer";
        public const string StandardAnalyzer = "StandardAnalyzer";

	}
}
