using GeneXus;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace com.genexus.CA.search
{
  public class Searcher
  {
    private static readonly IGXLogger logger = GXLoggerFactory.GetLogger<Searcher>();

    public static string Search(string dir, string lang, string query, int maxResults, int from)
    {
      StringBuilder stringBuilder = new StringBuilder();
      List<string> uris = new List<string>();
      int totalHits = 0;
      Stopwatch stopwatch = Stopwatch.StartNew();
      string normalizedLang = string.IsNullOrWhiteSpace(lang) ? string.Empty : lang.Trim().ToLowerInvariant();

      try
      {
        if (from < 0)
          from = 0;
        if (maxResults < 0)
          maxResults = 0;

        if (!IndexExists(dir))
        {
          stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Results hits=\"0\" time=\"0ms\"></Results>");
          return stringBuilder.ToString();
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
          int start = Math.Max(0, from);
          int size = maxResults > 0 ? maxResults : 10;
          IndexSearcher indexSearcher = null;

          try
          {
            indexSearcher = new IndexSearcher((Directory) FSDirectory.Open(dir));

            string[] fields = new string[3]
            {
              "title",
              "content",
              "summary"
            };
            Occur[] flags = new Occur[3]
            {
              Occur.SHOULD,
              Occur.SHOULD,
              Occur.SHOULD
            };

            Query query1 = null;
            try
            {
              query1 = MultiFieldQueryParser.Parse(Lucene.Net.Util.Version.LUCENE_30, query, fields, flags, AnalyzerManager.GetAnalyzer(normalizedLang));
            }
            catch (ParseException)
            {
              try
              {
                string escapedQuery = QueryParser.Escape(query);
                query1 = MultiFieldQueryParser.Parse(Lucene.Net.Util.Version.LUCENE_30, escapedQuery, fields, flags, AnalyzerManager.GetAnalyzer(normalizedLang));
              }
              catch (ParseException)
              {
                query1 = (Query) new TermQuery(new Term("content", query));
              }
            }

            if (!string.IsNullOrWhiteSpace(normalizedLang) && !string.Equals(normalizedLang, "ind", StringComparison.OrdinalIgnoreCase))
            {
              Query query2 = (Query) new TermQuery(new Term("language", normalizedLang));
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

            TopDocs topDocs = indexSearcher.Search(query1, start + size);
            totalHits = topDocs.TotalHits;

            int end = Math.Min(totalHits, start + size);
            for (int index = start; index < end; ++index)
            {
              Document document = indexSearcher.Doc(topDocs.ScoreDocs[index].Doc);
              string uriValue = document.GetField("uri")?.StringValue ?? string.Empty;
              uris.Add(uriValue);
            }
          }
          finally
          {
            if (indexSearcher != null)
              indexSearcher.Close();
          }
        }
      }
      catch (Exception ex)
      {
        GXLogging.Error(logger, "Error executing search.", ex);
        stringBuilder.Clear();
        stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Results hits=\"0\" time=\"0ms\"></Results>");
        return stringBuilder.ToString();
      }
      finally
      {
        stopwatch.Stop();
      }

      stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
      stringBuilder.Append("<Results hits=\"").Append(totalHits.ToString()).Append("\" time=\"").Append(stopwatch.ElapsedMilliseconds.ToString()).Append("ms\">");
      foreach (string uriValue in uris)
      {
        stringBuilder.Append("<Result>");
        stringBuilder.Append("<URI>").Append(EscapeXml(uriValue)).Append("</URI>");
        stringBuilder.Append("</Result>");
      }
      stringBuilder.Append("</Results>");
      return stringBuilder.ToString();
    }

    private static string EscapeXml(string value)
    {
      return SecurityElement.Escape(value ?? string.Empty) ?? string.Empty;
    }

    private static bool IndexExists(string dir)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(dir))
          return false;

        return IndexReader.IndexExists((Directory) FSDirectory.Open(dir));
      }
      catch (Exception ex)
      {
        GXLogging.Error(logger, "Error checking index existence.", ex);
        return false;
      }
    }

  }
}
