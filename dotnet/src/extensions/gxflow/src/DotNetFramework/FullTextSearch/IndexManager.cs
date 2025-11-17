using System.Collections;

namespace com.genexus.CA.search
{
  public class IndexManager
  {
    private static Hashtable hash = new Hashtable();

    public static void AddContent(
      string dir,
      string uri,
      string lang,
      string title,
      string summary,
      short fromFile,
      string body,
      string filePath)
    {
      IndexManager.GetIndexer(dir).AddContent(uri, lang, title, summary, fromFile, body, filePath);
    }

    public static void DeleteContent(string dir, string uri)
    {
      IndexManager.GetIndexer(dir).DeleteContent(uri);
    }

    private static Indexer GetIndexer(string dir)
    {
      Indexer indexer = (Indexer) null;
      if (IndexManager.hash.ContainsKey((object) dir))
      {
        indexer = (Indexer) IndexManager.hash[(object) dir];
      }
      else
      {
        lock (IndexManager.hash)
        {
          indexer = new Indexer(dir);
          IndexManager.hash.Add((object) dir, (object) indexer);
        }
      }
      return indexer;
    }
  }
}
