using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Text;

namespace com.genexus.CA.search
{
  public class Searcher
  {
    public static string Search(string dir, string lang, string query, int maxResults, int from)
    {
      StringBuilder stringBuilder = new StringBuilder();
      try
      {
        IndexSearcher indexSearcher = new IndexSearcher((Directory) FSDirectory.Open(dir));
        string[] fields = new string[2]
        {
          "title",
          "content"
        };
        Occur[] flags = new Occur[2]
        {
          Occur.SHOULD,
          Occur.SHOULD
        };
        Query query1 = MultiFieldQueryParser.Parse(Lucene.Net.Util.Version.LUCENE_30, query, fields, flags, AnalyzerManager.GetAnalyzer(lang));
        if (!lang.Equals("IND"))
        {
          Query query2 = (Query) new TermQuery(new Term("language", lang));
          query1 = (Query) new BooleanQuery()
          {
            {
              query1,
              Occur.MUST
            },
            {
              query2,
              Occur.MUST
            }
          };
        }
        TopDocs topDocs = indexSearcher.Search(query1, maxResults);
        string str = "";
        int totalHits = topDocs.TotalHits;
        stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        stringBuilder.Append("<Results hits = '" + totalHits.ToString() + "' time = '" + str + "'>");
        for (int index = 0; index < totalHits; ++index)
        {
          stringBuilder.Append("<Result >");
          Document document = indexSearcher.Doc(topDocs.ScoreDocs[index].Doc);
          stringBuilder.Append("<URI>" + document.GetField("uri").StringValue + "</URI>");
          stringBuilder.Append("</Result>");
        }
      }
      catch (Exception ex)
      {
        Logger.Print(ex.ToString());
      }
      stringBuilder.Append("</Results>");
      return stringBuilder.ToString();
    }
  }
}
