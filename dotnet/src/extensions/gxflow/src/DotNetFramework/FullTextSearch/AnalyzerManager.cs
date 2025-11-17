using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System.Collections;

namespace com.genexus.CA.search
{
  public class AnalyzerManager
  {
    private static Hashtable hash = new Hashtable();

    public static Analyzer GetAnalyzer(string lang)
    {
      Analyzer analyzer;
      if (AnalyzerManager.hash.ContainsKey((object) lang))
      {
        analyzer = (Analyzer) AnalyzerManager.hash[(object) lang];
      }
      else
      {
        analyzer = !lang.Equals("spa") ? (Analyzer) new StandardAnalyzer(Version.LUCENE_30) : (Analyzer) new StandardAnalyzer(Version.LUCENE_30);
        AnalyzerManager.hash.Add((object) lang, (object) analyzer);
      }
      return analyzer;
    }
  }
}
