using System;
using System.Collections.Generic;
using System.Text;

using Lucene.Net.Documents;
using Lucene.Net.Search;

using GeneXus.Utils;

using Jayrock.Json;
using System.Globalization;
using System.Security;

namespace GeneXus.Search
{
	[SecuritySafeCritical]
	public class LuceneSearchResult : SearchResult
	{
		internal LuceneSearchResult(TopDocs docs, int itemsPerPage, int pageNumber, double elapsedTime) 
		{
			m_items = new LuceneSearchResultCollection(docs, itemsPerPage, pageNumber);
			m_itemsPerPage = itemsPerPage;
			m_elapsedTime = (decimal)elapsedTime;
			if (pageNumber == 0 || pageNumber == 1)
			{
				m_offset = 0;
			}
			else
			{
				m_offset = itemsPerPage * (pageNumber - 1);
			}
			m_maxresults = (docs != null) ? docs.TotalHits : 0; 
		}
	}

	[SecuritySafeCritical]
	public class LuceneSearchResultCollection : SearchResultCollection
	{
		#region Internal Data
		private TopDocs m_docs;
		#endregion

		public LuceneSearchResultCollection()
		{ }

		internal LuceneSearchResultCollection(TopDocs docs, int itemsPerPage, int pageNumber) 
		{
			m_docs = docs;
			m_itemsPerPage = itemsPerPage;
			if (pageNumber == 0 || pageNumber == 1)
			{
				m_offset = 0;
			}
			else
			{
				m_offset = itemsPerPage * (pageNumber - 1);
			}
			m_maxresults = (m_docs != null) ? m_docs.TotalHits : 0;
		}
		[SecuritySafeCritical]
        protected override int GetCount()
        {
            int count = 0;
            if (m_itemsPerPage == -1)
            {
                count = (m_docs != null) ? m_docs.TotalHits : 0; 
			}
            else
            {
                if (m_docs != null)
                {
                    count = m_docs.TotalHits - m_offset;
                    if (count < 0)
                    {
                        count = 0;
                    }
                    else if (count > m_itemsPerPage)
                    {
                        count = m_itemsPerPage;
                    }
                }
                else
                {
                    count = 0;
                }
            }
            return count;
        }
		[SecuritySafeCritical]
		protected override SearchResultItem GetItem(int index)
        {
            index = m_offset + index;
			if (index < m_maxresults && index >= 0)
            {
				int foundDoc = m_docs.ScoreDocs[index].Doc;
				IndexSearcher searcher = Searcher.Instance.GetSearcher();
				currentItem = new LuceneSearchResultItem(searcher.Doc(foundDoc), m_docs.ScoreDocs[index].Score);
                return currentItem as SearchResultItem;
            }
            else
            {
                throw new SearchException(SearchException.INDEXERROR);
            }
        }

    }
	[SecuritySafeCritical]
	public sealed class LuceneSearchResultItem : SearchResultItem
	{
		public LuceneSearchResultItem()
		{ }

		#region Internal Data

		private Document m_document;

		#endregion

		internal LuceneSearchResultItem(Document document, float score)
		{
			_Properties = new JObject();
			m_document = document;
			m_score = score;
		}

		#region ISearchResultItem Members

		public override string Id
		{
			get { return m_document.GetField(IndexRecord.URIFIELD).StringValue; }
		}

		public override string Viewer
		{
			get { return m_document.GetField(IndexRecord.VIEWERFIELD).StringValue; }
		}

		public override string Title
		{
			get { return m_document.GetField(IndexRecord.TITLEFIELD).StringValue; }
		}

		public override string Entity
		{
			get { return m_document.GetField(IndexRecord.ENTITYFIELD).StringValue; }
		}

		public override DateTime TimeStamp
		{
			get { return
#pragma warning disable CS0618 // Type or member is obsolete
				  DateField.StringToDate(m_document.GetField(IndexRecord.TIMESTAMPFIELD).StringValue);
#pragma warning restore CS0618 // Type or member is obsolete
			}
			
		}

		public override GxStringCollection Keys
		{
			get
			{
				GxStringCollection keys = new GxStringCollection();
				foreach (Field field in m_document.GetFields())
				{
					if (field.Name.StartsWith(IndexRecord.KEYFIELDPREFIX))
						keys.Add(field.StringValue);
				}
				return keys;
			}
		}

#endregion
	}
}
