using System;
using System.Collections.Generic;
using System.Text;
using GeneXus.Utils;
using Jayrock.Json;
using GeneXus.Application;
using System.Reflection;
using GeneXus.Metadata;
using System.Runtime.CompilerServices;
using System.IO;
#if NETCORE
using GxClasses.Helpers;
#endif

namespace GeneXus.Search
{
	public class GxSearchUtils
	{
#if NETCORE
		public static Assembly m_GxSearchAssembly = AssemblyLoader.LoadAssembly(new AssemblyName("GxSearch"));
#else
		public static Assembly m_GxSearchAssembly = Assembly.Load(new AssemblyName("GxSearch"));
#endif

		public static object m_IndexerInstance = m_GxSearchAssembly.GetType("GeneXus.Search.Indexer").GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty).GetValue(null, null);
        public static object m_SearcherInstance = m_GxSearchAssembly.GetType("GeneXus.Search.Searcher").GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty).GetValue(null, null);
		public static object m_SpellingInstance = m_GxSearchAssembly.GetType("GeneXus.Search.Spelling").GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty).GetValue(null, null);
		public static bool InsertContent(Object obj, GxContentInfo contentInfo)
		{
			return (bool)ClassLoader.Invoke(m_IndexerInstance, "InsertContent", new object[] { obj, contentInfo });
		}
		public static bool UpdateContent(object obj, GxContentInfo contentInfo)
		{
			return (bool)ClassLoader.Invoke(m_IndexerInstance, "UpdateContent", new object[] { obj, contentInfo });
		}
		public static bool RemoveContent(object obj)
		{
			return (bool)ClassLoader.Invoke(m_IndexerInstance, "RemoveContent", new object[] { obj });
		}
		public static bool RemoveEntity(string entityType)
		{
			return (bool)ClassLoader.Invoke(m_IndexerInstance, "RemoveEntity", new object[] { entityType });
		}
		public static SearchResult Search(string query, int maxresults)
		{
			return Search(query, maxresults, null);
		}
		public static SearchResult Search(string query, int maxresults, IGxContext context)
		{
			return Search(query, maxresults, 0, null);
		}
		public static SearchResult Search(string query, int itemsPerPage, int pageNumber, IGxContext context)
		{
			return (SearchResult)ClassLoader.Invoke(m_SearcherInstance, "Search", new object[] { query, itemsPerPage, pageNumber, context });
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static string HtmlPreview(Object obj, string query, string textType, string preTag, string postTag, int fragmentSize, int maxNumFragments)
		{
			return (string)ClassLoader.InvokeStatic(m_GxSearchAssembly, "GeneXus.Utils.DocumentHandler", "HtmlPreview",
				new object[] { obj, query, textType, preTag, postTag, fragmentSize, maxNumFragments });
		}
		public static string GetText(string filename, string extension)
		{
			return (string)ClassLoader.InvokeStatic(m_GxSearchAssembly, "GeneXus.Utils.DocumentHandler", "GetText",
				new object[] { filename, extension });
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static string HTMLClean(string text)
		{
			return (string)ClassLoader.InvokeStatic(m_GxSearchAssembly, "GeneXus.Utils.DocumentHandler", "HTMLClean",
				new object[] { text });
		}
		public static bool BuildDictionary()
		{
			return (bool)ClassLoader.Invoke(m_SpellingInstance, "BuildDictionary", null);

		}
		public static string Suggest(string phrase)
		{
			return (string)ClassLoader.Invoke(m_SpellingInstance, "Suggest", new object[] { phrase });

		}

	}
	public interface ISearchResultCollection
	{
		int Count { get;}
		ISearchResultItem Item(int index);
	}

	public interface ISearchResultItem
	{
		string Id { get;}
		string Entity { get;}
		DateTime TimeStamp { get;}
		float Score { get;}
		GxStringCollection Keys { get;}
	}
	public class SearchResult
	{

		protected static SearchResultCollection m_items;
		protected static SearchResult m_empty;
		protected int m_maxresults;
		protected decimal m_elapsedTime;
		protected int m_itemsPerPage;
		protected int m_offset;

		public SearchResult() { }
		public int MaxItems
		{
			get { return m_maxresults; }
		}
		public decimal ElapsedTime
		{
			get { return m_elapsedTime; }
		}
		public SearchResultCollection Items()
		{
			return m_items;
		}
		public static SearchResult Empty
		{
			get
			{
				if (m_empty == null)
					m_empty = new EmptySearchResult();
				return m_empty;
			}
		}
	}
	public class SearchResultCollection : GxSimpleCollection<SearchResultItem>
	{
#region Internal Data

		protected int m_maxresults;
		protected int m_itemsPerPage;
		protected int m_offset;

		#endregion
		public SearchResultCollection()
		{
		}
		public override void FromJSONObject(dynamic obj)
		{
		}

	}

#region EmptySearchResult class

	public sealed class EmptySearchResult : SearchResult
	{
		public EmptySearchResult()
		{
			m_items = new SearchResultCollection();
		}

		public int Count
		{
			get { return 0; }
		}

	}

#endregion

	public class SearchResultItem : ISearchResultItem, IGxJSONAble
	{
#region Internal Data

		protected JObject _Properties;
		protected float m_score;

#endregion

#region ISearchResultItem Members

		public virtual string Id { get { return ""; } }
		public virtual string Viewer { get { return ""; } }
		public virtual string Title { get { return ""; } }
		public virtual string Entity { get { return ""; } }
		public virtual DateTime TimeStamp { get { return DateTime.Now; } }
		public float Score
		{
			get { return m_score; }
		}

		public virtual GxStringCollection Keys { get { return new GxStringCollection(); } }

#endregion

#region IGxJSONAble Members

		public void AddObjectProperty(string name, object prop)
		{
			_Properties.Put(name, prop);
		}

		public object GetJSONObject()
		{
			return GetJSONObject(true);
		}
		public object GetJSONObject(bool includeState)
		{
			ToJSON();
			return _Properties;
		}

		public void ToJSON()
		{
			AddObjectProperty("Id", Id);
			AddObjectProperty("Viewer", Viewer);
			AddObjectProperty("Title", Title);
			AddObjectProperty("Type", Entity);
			AddObjectProperty("Score", Score);
			AddObjectProperty("Timestamp", TimeStamp);
		}
		public void FromJSONObject(dynamic obj)
		{
		}

		public string ToJavascriptSource()
		{
			return GetJSONObject().ToString();
		}

#endregion
	}


}
