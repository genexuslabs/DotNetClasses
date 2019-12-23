using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

using log4net;

using GeneXus.Application;
using Lucene.Net.Analysis;
using System.Security;

namespace GeneXus.Search
{
	[SecuritySafeCritical]
	public sealed class Searcher
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Search.Searcher));
		#region Singleton

		private static Searcher m_instance = new Searcher();
		private IndexSearcher m_searcher;
		private Searcher() { }
		public static Searcher Instance { get { return m_instance; } }

		#endregion

		#region Internal Data

        private Analyzer m_analyzer = Indexer.CreateAnalyzer();

		#endregion

		public SearchResult Search(string query, int maxresults)
		{
			return Search(query, maxresults, null);
		}

		public SearchResult Search(string query, int maxresults, IGxContext context)
		{
			return Search(query, maxresults, 0, null);
		}

		public SearchResult Search(string query, int itemsPerPage, int pageNumber, IGxContext context)
		{
			if (string.IsNullOrEmpty(query))
				return SearchResult.Empty;
			if (context != null)
			{
                query = TranslateQuery(query, context);
			}

			IndexSearcher searcher = GetSearcher();
			if (searcher == null)
			{
				return new EmptySearchResult();
			}

			try
			{
				QueryParser qp = new QueryParser(Indexer.LUCENE_VERSION, IndexRecord.CONTENTFIELD, m_analyzer);

				qp.AllowLeadingWildcard = true;                
				qp.DefaultOperator = QueryParser.Operator.AND;
				Query q = qp.Parse(IndexRecord.ProcessContent(query));
				DateTime t1 = DateTime.Now;

				TopDocs results = searcher.Search(q, int.MaxValue);
				DateTime t2 = DateTime.Now;

				return new LuceneSearchResult(results, itemsPerPage, pageNumber, TimeSpan.FromTicks(t2.Ticks - t1.Ticks).TotalMilliseconds);
			}
			catch (ParseException)
			{
				GXLogging.Error(log,"Search error", new SearchException(SearchException.PARSEERROR));
				return SearchResult.Empty;
				
			}
			catch (Exception ex)
			{
				GXLogging.Debug(log, "Search error", ex);
				throw new SearchException(SearchException.IOEXCEPTION);
			}
		}

		private string TranslateQuery(string query, IGxContext context)
		{
			
			return query;
		}
		public void Close()
		{
			if (m_searcher != null)
			{
				m_searcher.Dispose();

				m_searcher = null;
			}
		}
#region Internal Operations

		internal IndexSearcher GetSearcher()
		{
			try
			{
				if (m_searcher==null)
					m_searcher = new IndexSearcher(Settings.Instance.StoreFolder);
				return m_searcher;
			}
			catch (Exception)
			{
				GXLogging.Error(log,"Search error, index directory:" + Settings.Instance.IndexFolder, new SearchException(SearchException.COULDNOTCONNECT));
				return null;
			}
		}

#endregion
	}
}
