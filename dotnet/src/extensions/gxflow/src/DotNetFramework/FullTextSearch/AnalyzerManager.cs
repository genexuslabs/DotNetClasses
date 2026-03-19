using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System.Collections.Concurrent;

namespace com.genexus.CA.search
{
  public class AnalyzerManager
  {
    private static readonly ConcurrentDictionary<string, Analyzer> analyzers = new ConcurrentDictionary<string, Analyzer>()
    {
      // Default analyzer. Add language-specific analyzers here if needed.
      ["default"] = new StandardAnalyzer(Version.LUCENE_30)
    };

    public static Analyzer GetAnalyzer(string lang)
    {
      string key = string.IsNullOrWhiteSpace(lang) ? "default" : lang.Trim().ToLowerInvariant();
      Analyzer analyzer;
      if (AnalyzerManager.analyzers.TryGetValue(key, out analyzer))
        return analyzer;
      return AnalyzerManager.analyzers["default"];
    }
  }
}
