using System;
using System.Collections;
using System.IO;
using System.Text;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

using log4net;
using GeneXus.Search;
using System.Runtime.CompilerServices;
using System.Reflection;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Collections.Concurrent;

#if NETCORE
using NUglify;
using NUglify.Html;
using System.Web;
#endif

namespace GeneXus.Utils
{
	public class DocumentHandler
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.DocumentHandler));

		static IndexReader reader;
		static ConcurrentDictionary<string, Query> queries = new ConcurrentDictionary<string, Query>();
		static QueryParser qp;
        
        public static string GetText(string filename, string extension)
		{
			try
			{
				IDocumentHandler docHandler = null;
				if (extension.ToLower().StartsWith("htm") || extension.ToLower().StartsWith(".htm"))
				{
					docHandler = new NTidyHTMLHandler();
				}
				else if (extension.ToLower().StartsWith("txt") || extension.ToLower().StartsWith(".txt"))
				{
					docHandler = new TextHandler();
				}
                else if (extension.ToLower().StartsWith("pdf") || extension.ToLower().StartsWith(".pdf"))
                {
                    docHandler = new PdfHandler();
                }
				if (docHandler == null)
					return "";
				else
					return docHandler.GetText(filename);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log,"GetText error", ex);
				return "";
			}
		}
      [MethodImpl(MethodImplOptions.Synchronized)]
		public static string HtmlPreview(Object obj, string query, string textType, string preTag, string postTag, int fragmentSize, int maxNumFragments)
		{
			string text;
			GxSilentTrnSdt silent = obj as GxSilentTrnSdt;
			GxFile file = obj as GxFile;
			if (silent!=null)
			{
				text = (silent).Transaction.ToString();
			}
			else if (file!=null)
			{
				text = DocumentHandler.GetText(file.GetAbsoluteName(), System.IO.Path.GetExtension(file.GetAbsoluteName()));
			}
			else if (textType.ToLower().StartsWith("htm"))
			{
				text = new NTidyHTMLHandler().GetTextFromString(obj.ToString());
			}
			else
			{
				text = obj.ToString();
			}
			if (!string.IsNullOrEmpty(query) && !string.IsNullOrEmpty(text))
			{
				if (qp == null)
				{
					qp = new QueryParser(Lucene.Net.Util.Version.LUCENE_24, IndexRecord.CONTENTFIELD, Indexer.CreateAnalyzer());
					qp.DefaultOperator=QueryParser.Operator.AND;
					qp.MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE;
				}
				Query unReWrittenQuery = qp.Parse(query);
				Query q = unReWrittenQuery;
                try
                {
                    if (reader == null)
                    {
                        reader = Indexer.Reader;
                    }
                    if (!queries.TryGetValue(query, out q))
                    {
                        q = unReWrittenQuery.Rewrite(reader);//required to expand search terms (for the usage of highlighting with wildcards)

                        if (queries.Count == int.MaxValue)
                        {
                            queries.Clear();
                        }
                        queries[query] = q;
                    }

                }
                catch (Exception ex)
                {
                    GXLogging.Error(log,"HTMLPreview error", ex);
                }
				QueryScorer scorer = new QueryScorer(q);

				SimpleHTMLFormatter formatter = new SimpleHTMLFormatter(preTag, postTag);
				Highlighter highlighter = new Highlighter(formatter, scorer);
				IFragmenter fragmenter = new SimpleFragmenter(fragmentSize);
				
				highlighter.TextFragmenter = fragmenter;
				TokenStream tokenStream = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_24).TokenStream("Content", new StringReader(text));

				String result = highlighter.GetBestFragments(tokenStream, text, maxNumFragments, "...");
				return result;
			}
			else
			{
			return text;
			}
		}
        [MethodImpl(MethodImplOptions.Synchronized)]
		static public string HTMLClean(string text)
		{
			try
			{
				return new NTidyHTMLHandler().HTMLClean(text);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log,"HTMLClean error", ex);
				return "";
			}
		}
	}
	public interface IDocumentHandler
	{
		string GetText(string filename);
	}

	public class NTidyHTMLHandler : IDocumentHandler
	{
#if !NETCORE
        Assembly ntidy;
#endif
        public NTidyHTMLHandler()
        {
#if !NETCORE
			ntidy =  Assembly.LoadFrom(GXUtil.ProcessorDependantAssembly("NTidy.dll"));
#endif
        }
		public String GetText(string filename)
		{
#if NETCORE

			return Uglify.HtmlToText(File.ReadAllText(filename)).Code;
#else
			object rawDoc = ntidy.CreateInstance("NTidy.TidyDocument");
			LoadConfig(rawDoc);
			int status = (int)rawDoc.GetType().GetMethod("LoadFile").Invoke(rawDoc, new object[] { filename });
			if (status != 0)
			{
				rawDoc.GetType().GetMethod("CleanAndRepair").Invoke(rawDoc, null);
			}

			object tidyNode = rawDoc.GetType().GetProperty("Html").GetValue(rawDoc, null);
			return getText(tidyNode);
#endif
		}
		public String GetTextFromString(string text)
		{
#if NETCORE
			text = Uglify.HtmlToText(text).Code;
#else
			object rawDoc = ntidy.CreateInstance("NTidy.TidyDocument");
			LoadConfig(rawDoc);
			rawDoc.GetType().GetMethod("LoadString").Invoke(rawDoc, new object[] { text });
			if (rawDoc == null)
			{
				return text;
			}
			try
			{
				object tidyNode = rawDoc.GetType().GetProperty("Html").GetValue(rawDoc, null);
				return getText(tidyNode);
			}
			catch
#endif
			{
				return text;
			}
		}

		private String getText(object node)
		{
			StringBuilder sb = new StringBuilder();

			object childNodes = node.GetType().GetProperty("ChildNodes").GetValue(node, null);

			IEnumerator children = (IEnumerator)childNodes.GetType().GetMethod("GetEnumerator").Invoke(childNodes, null);
			while (children.MoveNext())
			{
				object child = children.Current;
				bool istext = (bool)child.GetType().GetProperty("IsText").GetValue(child, null);
				if (istext)
				{
					sb.Append(child.GetType().GetProperty("Value").GetValue(child, null));
				}
				else
				{
					sb.Append(getText(child));
					sb.Append(" ");
				}
			}
			return sb.ToString();
		}
		internal void LoadConfig(object rawDoc)
		{
			string tidyConf = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tidy.cfg");
			if (File.Exists(tidyConf))
			{
				rawDoc.GetType().GetMethod("LoadConfig").Invoke(rawDoc, new object[] { tidyConf });
			}
		}
		internal string HTMLClean(string text)
		{
#if !NETCORE
			object rawDoc = Assembly.LoadFrom(GXUtil.ProcessorDependantAssembly("NTidy.dll")).CreateInstance("NTidy.TidyDocument");

			LoadConfig(rawDoc);
			rawDoc.GetType().GetMethod("LoadString").Invoke(rawDoc, new object[] { text });
			rawDoc.GetType().GetMethod("CleanAndRepair").Invoke(rawDoc, null);
			return (string)rawDoc.GetType().GetMethod("ToString").Invoke(rawDoc, null);
#else
			HtmlSettings htmlSettings = new HtmlSettings { PrettyPrint = true };
			return Uglify.Html(text, htmlSettings).Code;
#endif
		}
	}

	public class TextHandler : IDocumentHandler
	{
#region IDocumentHandler Members

		public string GetText(string filename)
		{
			StreamReader sr = new StreamReader(filename);
			string text = sr.ReadToEnd();
			sr.Close();
			return text;
		}

#endregion
	}

    public class PdfHandler : IDocumentHandler
    {
#region IDocumentHandler Members

        public string GetText(string filename)
        {
            return ReadPdfFile(filename);
        }

#endregion

        private string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();

            if (File.Exists(fileName) && fileName.EndsWith(".pdf"))
            {                
                PdfReader pdfReader = new PdfReader(fileName);                
                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {                    
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }
    }

}
