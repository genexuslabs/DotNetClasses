using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

using log4net;

using GeneXus.Utils;
using Lucene.Net.Analysis;
using System.Globalization;
using System.Security;

namespace GeneXus.Search
{
	[SecuritySafeCritical]
	public sealed class Indexer
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Search.Indexer));

		public static Lucene.Net.Util.Version LUCENE_VERSION = Lucene.Net.Util.Version.LUCENE_24;
		#region Singleton

		private static Indexer m_instance;
        private static Object syncObj = new Object();
        private Thread worker;
		private Indexer()
		{
			var folder = Settings.Instance.IndexFolder;
            GXLogging.Debug(log,"LUCENE_INDEX_DIRECTORY: " + folder);
			worker = new Thread(new ThreadStart(BackgroundIndexer));
			worker.Start();
		}

		public static Indexer Instance {
            get
            {
                if (m_instance == null)
                    m_instance = new Indexer(); 
                return m_instance;
            }
        }
        public static Analyzer CreateAnalyzer()
        {
            if (string.IsNullOrEmpty(Settings.Instance.Analyzer) || string.Compare(Settings.Instance.Analyzer, Settings.StandardAnalyzer) == 0)
            {
				if (Settings.Instance.StopWords == null)
                    return new StandardAnalyzer(LUCENE_VERSION);
                else
                    return new StandardAnalyzer(LUCENE_VERSION, Settings.Instance.StopWords);

			}
			else if (string.Compare(Settings.Instance.Analyzer, Settings.WhitespaceAnalyzer) == 0)
            {
                return new WhitespaceAnalyzer();
            }
            else if ((string.Compare(Settings.Instance.Analyzer, Settings.StopAnalyzer) == 0))
            {
				if (Settings.Instance.StopWords == null)
					return new StopAnalyzer(LUCENE_VERSION);
				else
					return new StopAnalyzer(LUCENE_VERSION, Settings.Instance.StopWords);

			}
			else if ((string.Compare(Settings.Instance.Analyzer, Settings.SimpleAnalyzer) == 0))
            {
                return new SimpleAnalyzer();
            }
            else
				return new StandardAnalyzer(LUCENE_VERSION);

        }
		#endregion

		#region Internal Data

		private Queue m_queue = Queue.Synchronized(new Queue());
        private Analyzer m_analyzer = CreateAnalyzer();
		private int m_counter = 0;

		#endregion

		#region Public Methods

		public bool UpdateContent(object obj, GxContentInfo contentInfo)
		{
			IndexRecord ir = GetIndexRecord(obj, contentInfo);
			if (ir != null)
			{
				Enqueue(new IndexAction(Action.Update, ir));
			}
			return true;
		}

		public bool InsertContent(Object obj, GxContentInfo contentInfo)
		{
			IndexRecord ir = GetIndexRecord(obj, contentInfo);
			if (ir != null)
			{
				Enqueue(new IndexAction(Action.Insert, ir));
			}
			return true;
		}

		private IndexRecord GetIndexRecord(object obj, GxContentInfo contentInfo)
		{
			IndexRecord ir=null;
			GxFile file = obj as GxFile;
			GxSilentTrnSdt silent = obj as GxSilentTrnSdt;
			string str = obj as string;
			if (file != null && contentInfo!=null)
			{
				ir = new IndexRecord();
				ir.Uri = file.GetAbsoluteName();
				ir.Content = DocumentHandler.GetText(file.GetAbsoluteName(), Path.GetExtension(file.GetAbsoluteName()));
				ir.Entity = contentInfo.Entity == null ? file.GetType().ToString() : contentInfo.Entity;
				ir.Title = contentInfo.Title == null ? file.GetName() : contentInfo.Title;
				ir.Viewer = contentInfo.Viewer == null ? file.GetName() : contentInfo.Viewer;
				ir.Keys = contentInfo.Keys == null || contentInfo.Keys.Count == 0 ? new List<string>() : contentInfo.Keys;
			}
			else if (silent !=null)
			{
				IGxSilentTrn bc = (silent).getTransaction();
				GxContentInfo info = bc.GetContentInfo();
				if (info != null)
				{
					ir = new IndexRecord();
					ir.Uri = info.Id;
					ir.Content = bc.ToString();
					ir.Entity = contentInfo.Entity == null ? info.Entity : contentInfo.Entity;
					ir.Title = contentInfo.Title == null ? info.Title : contentInfo.Title;
					ir.Viewer = contentInfo.Viewer == null ? info.Viewer : contentInfo.Viewer;
					ir.Keys = contentInfo.Keys == null || contentInfo.Keys.Count == 0 ? info.Keys : contentInfo.Keys;
				}
			}
			else if (str != null && contentInfo != null)
			{
				ir = new IndexRecord();
				ir.Uri = contentInfo.Id == null ? string.Empty : contentInfo.Id;
				ir.Content = str;
				ir.Entity = contentInfo.Entity == null ? String.Empty : contentInfo.Entity;
				ir.Title = contentInfo.Title == null ? String.Empty : contentInfo.Title;
				ir.Viewer = contentInfo.Viewer == null ? String.Empty : contentInfo.Viewer;
				ir.Keys = contentInfo.Keys == null || contentInfo.Keys.Count == 0 ? new List<string>() : contentInfo.Keys;
			}
			return ir;
		}

		public bool RemoveContent(object obj)
		{
			string str = obj as string; 
			if (str!=null)
				Enqueue(new IndexAction(Action.Delete, new IndexRecord(str)));
			else
				Enqueue(new IndexAction(Action.Delete, GetIndexRecord(obj, new GxContentInfo())));
			return true;
		}

		public bool RemoveEntity(string entityType)
		{
			Enqueue(new IndexAction(Action.Delete, new IndexRecord(null, entityType, null, null, null, null)));
			return true;
		}

		#endregion

		#region Internal Operations

		private void Enqueue(IndexAction action)
		{
			lock (syncObj)
			{
				if (!worker.IsAlive)
				{
					worker = new Thread(new ThreadStart(BackgroundIndexer));
					worker.Start();
				}
			}
			if ((m_queue.Count > Settings.Instance.MaxQueueSize))
			{
				GXLogging.Debug(log, "Indexer Enqueue waiting...");
			}
			while (m_queue.Count > Settings.Instance.MaxQueueSize)
			{
				Thread.Sleep(50);
			}
			m_queue.Enqueue(action);
		}

		private void BackgroundIndexer()
		{
			GXLogging.Debug(log,"BackgroundIndexer starting");
			int totsec = 0;
			while (totsec<3)
			{
				while (m_queue.Count > 0)
				{
					Index(m_queue.Dequeue() as IndexAction);
				}

				GXLogging.Debug(log,"BackgroundIndexer sleeping...");
				Thread.Sleep(1000);
                totsec++;
			}
		}

		private void Index(IndexAction action)
		{
			switch (action.Action)
			{
				case Action.Insert:
					Insert(action.Record);
					break;
				case Action.Update:
					Delete(action.Record);
					Insert(action.Record);
					break;
				case Action.Delete:
					Delete(action.Record);
					break;
			}
		}

		private void Delete(IndexRecord indexRecord)
		{
			IndexReader reader = Reader;
			if (reader == null)
				return;

			try
			{
				if (indexRecord.Uri != null)
				{
					reader.DeleteDocuments(new Term(IndexRecord.URIFIELD, indexRecord.Uri));
				}
				else
				{
					reader.DeleteDocuments(new Term(IndexRecord.ENTITYFIELD, indexRecord.Entity));
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log,"Delete error", e);
			}
			finally
			{
				reader.Dispose();

				Searcher.Instance.Close();
				
			}
		}

		private void Insert(IndexRecord indexRecord)
		{
			IndexWriter writer = Writer;
			if (writer == null)
				return;

			try
			{
				Document doc = new Document();
				doc.Add(new Field(IndexRecord.URIFIELD, indexRecord.Uri, Field.Store.YES, Field.Index.NOT_ANALYZED));
				doc.Add(new Field(IndexRecord.ENTITYFIELD, indexRecord.Entity, Field.Store.YES, Field.Index.NOT_ANALYZED));
				doc.Add(new Field(IndexRecord.CONTENTFIELD, new StringReader(IndexRecord.ProcessContent(indexRecord.Content))));
#pragma warning disable CS0618 // Type or member is obsolete
				doc.Add(new Field(IndexRecord.TIMESTAMPFIELD,DateField.DateToString(DateTime.Now), Field.Store.YES, Field.Index.NO));
#pragma warning restore CS0618 // Type or member is obsolete
				
				doc.Add(new Field(IndexRecord.VIEWERFIELD, indexRecord.Viewer, Field.Store.YES, Field.Index.NOT_ANALYZED));
				doc.Add(new Field(IndexRecord.TITLEFIELD, indexRecord.Title, Field.Store.YES, Field.Index.NOT_ANALYZED));

				int i = 1;
				foreach (string key in indexRecord.Keys)
					doc.Add(new Field(string.Format("{0}{1}", IndexRecord.KEYFIELDPREFIX, (i++).ToString()), key, Field.Store.YES, Field.Index.NO));

				GXLogging.Debug(log,"AddDocument:" + indexRecord.Uri + " content:" + indexRecord.Content);
				writer.AddDocument(doc, m_analyzer);
				if (m_counter++ > Settings.Instance.OptimizeThreshold)
				{
					m_counter = 0;
					GXLogging.Warn(log,"Optimizing index");
					writer.Optimize();
				}
			}
			catch (Exception e)
			{
				GXLogging.Error(log,"Insert error", e);
			}
			finally
			{
				try {
					writer.Dispose();

					Searcher.Instance.Close();
				}
				catch (Exception ex)
				{
					GXLogging.Error(log,"Close writer error", ex);
				}
			}
		}

		public static IndexReader Reader
		{
			get
			{
				if (IndexReader.IndexExists(Settings.Instance.StoreFolder))
				{
					ManageIndexLock(true);
					return IndexReader.Open(Settings.Instance.StoreFolder, false);
				}
				return null;
			}
		}

		private IndexWriter Writer
		{
            get
            {
                bool create = true;
				if (IndexReader.IndexExists(Settings.Instance.StoreFolder))
					create = false;
				ManageIndexLock(!create);
				return new IndexWriter(Settings.Instance.StoreFolder, m_analyzer, create, IndexWriter.MaxFieldLength.UNLIMITED);
			}
		}

        private static void ManageIndexLock(bool indexExists)
        {
            try
            {
				if (indexExists && IndexWriter.IsLocked(Settings.Instance.StoreFolder))
				{
					GXLogging.Warn(log,"There is more than one process trying to write to the index folder");
					int indexChecks = 0;
					while (IndexWriter.IsLocked(Settings.Instance.StoreFolder) && indexChecks < 20)
					{
						indexChecks++;
						Thread.Sleep(3000);
						GXLogging.Warn(log,"Waiting for the lock to release.");
					}
					if (IndexWriter.IsLocked(Settings.Instance.StoreFolder))
					{
						GXLogging.Warn(log,"Forcefully releasing lock");
						IndexWriter.Unlock(Settings.Instance.StoreFolder);
					}
				}
			}
			catch (IOException e1)
            {
                GXLogging.Error(log,"There was a problem waiting for the lock to release.", e1);
            }
        }
#endregion
	}
}
