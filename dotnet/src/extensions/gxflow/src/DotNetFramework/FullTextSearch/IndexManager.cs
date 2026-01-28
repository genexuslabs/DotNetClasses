using System.Collections.Concurrent;
using System.IO;

namespace com.genexus.CA.search
{
  public class IndexManager
  {
    private static readonly ConcurrentDictionary<string, Indexer> indexers = new ConcurrentDictionary<string, Indexer>();

    public static void AddContent(string dir, string uri, string lang, string title, string summary, short fromFile, string body, string filePath)
    {
      GetIndexer(dir).AddContent(uri, lang, title, summary, fromFile, body, filePath);
    }

    public static void DeleteContent(string dir, string uri)
    {
      GetIndexer(dir).DeleteContent(uri);
    }

    private static Indexer GetIndexer(string dir)
    {
      string normalizedDir = Indexer.NormalizeIndexDirectory(dir);
      return indexers.GetOrAdd(normalizedDir, key => new Indexer(key));
    }
  }
}
