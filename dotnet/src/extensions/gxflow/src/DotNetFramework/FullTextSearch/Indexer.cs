using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using GeneXus;
using NPOI.OpenXml4Net.OPC;
using NPOI.XSSF.Extractor;
using NPOI.XWPF.Extractor;
using System;
using System.IO;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace com.genexus.CA.search
{
  public class Indexer
  {
    private static readonly IGXLogger logger = GXLoggerFactory.GetLogger<Indexer>();
    private const int OPERATION_INDEX = 1;
    private const int OPERATION_DELETE = 2;
    private string indexDirectory = ".";

    internal Indexer(string directory)
    {
      this.indexDirectory = NormalizeIndexDirectory(directory);
      if (this.IndexExists(this.indexDirectory))
        return;
      try
      {
        new IndexWriter((Lucene.Net.Store.Directory) FSDirectory.Open(this.indexDirectory), (Analyzer) new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), true, IndexWriter.MaxFieldLength.UNLIMITED).Close();
      }
      catch (Exception ex)
      {
        GXLogging.Error(logger, "Error creating index.", ex);
      }
    }

    internal void AddContent(string uri, string lang, string title, string summary, short fromFile, string body, string filePath)
    {
      if (string.IsNullOrWhiteSpace(uri))
      {
        GXLogging.Error(logger, "AddContent called with empty uri.");
        return;
      }

      string normalizedLang = string.IsNullOrWhiteSpace(lang) ? string.Empty : lang.Trim().ToLowerInvariant();
      string normalizedUri = uri.Trim().ToLowerInvariant();
      StringBuilder contentBuilder = new StringBuilder();
      bool fileContentRead = false;
      Document doc = new Document();

      if (fromFile == (short) 1)
      {
        if (string.IsNullOrWhiteSpace(filePath))
        {
          GXLogging.Error(logger, "AddContent called with fromFile=1 but empty filePath.");
        }
        else if (!File.Exists(filePath))
        {
          GXLogging.Error(logger, "File not found: " + filePath);
        }
        else
        {
          string extractedContent;
          if (TryReadFileContent(filePath, out extractedContent))
          {
            contentBuilder.Append(extractedContent);
            fileContentRead = true;
          }
        }
      }

      if (!fileContentRead && !string.IsNullOrWhiteSpace(body))
      {
        if (contentBuilder.Length > 0)
          contentBuilder.Append(' ');
        contentBuilder.Append(body);
      }

      doc.Add((IFieldable) new Field("uri", normalizedUri, Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add((IFieldable) new Field("language", normalizedLang, Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add((IFieldable) new Field("title", SafeValue(title), Field.Store.YES, Field.Index.ANALYZED));
      doc.Add((IFieldable) new Field("path", SafeValue(filePath), Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add((IFieldable) new Field("content", contentBuilder.ToString(), Field.Store.YES, Field.Index.ANALYZED));
      doc.Add((IFieldable) new Field("summary", SafeValue(summary), Field.Store.YES, Field.Index.ANALYZED));
      doc.Add((IFieldable) new Field("body", SafeValue(body), Field.Store.YES, Field.Index.ANALYZED));

      try
      {
        this.IndexOperation(OPERATION_INDEX, normalizedLang, doc, normalizedUri);
      }
      catch (Exception ex)
      {
        GXLogging.Error(logger, "Error indexing content.", ex);
      }
    }

    internal void DeleteContent(string uri)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(uri))
          return;

        this.IndexOperation(OPERATION_DELETE, (string) null, (Document) null, uri.Trim().ToLowerInvariant());
      }
      catch (Exception ex)
      {
        GXLogging.Error(logger, "Error deleting content.", ex);
      }
    }

    protected void IndexOperation(int op, string lang, Document doc, string uri)
    {
      lock (this)
      {
        switch (op)
        {
          case OPERATION_INDEX:
            try
            {
              IndexWriter indexWriter = null;
              try
              {
                indexWriter = new IndexWriter((Lucene.Net.Store.Directory) FSDirectory.Open(this.GetIndexDirectory()), AnalyzerManager.GetAnalyzer(lang), false, IndexWriter.MaxFieldLength.UNLIMITED);
                if (!string.IsNullOrWhiteSpace(uri))
                {
                  Query deleteQuery = null;

                  if (string.IsNullOrWhiteSpace(lang))
                  {
                    deleteQuery = (Query) new TermQuery(new Term("uri", uri));
                  }
                  else
                  {
                    deleteQuery = (Query) new BooleanQuery()
                    {
                      {
                        (Query) new TermQuery(new Term("uri", uri)),
                        Occur.MUST
                      },
                      {
                        (Query) new TermQuery(new Term("language", lang)),
                        Occur.MUST
                      }
                    };
                  }
                  indexWriter.DeleteDocuments(deleteQuery);
                }

                indexWriter.AddDocument(doc);
              }
              finally
              {
                if (indexWriter != null)
                  indexWriter.Close();
              }
              break;
            }
            catch (Exception ex)
            {
              GXLogging.Error(logger, "Error writing to index.", ex);
              break;
            }
          case OPERATION_DELETE:
            try
            {
              Term term = (Term) null;
              Query deleteQuery = null;

              if (lang == null)
              {
                term = new Term("uri", uri);
              }
              else
              {
                deleteQuery = (Query) new BooleanQuery()
                {
                  {
                    (Query) new TermQuery(new Term("uri", uri)),
                    Occur.MUST
                  },
                  {
                    (Query) new TermQuery(new Term("language", lang)),
                    Occur.MUST
                  }
                };
              }

              IndexReader indexReader = null;
              IndexSearcher indexSearcher = null;
              try
              {
                indexReader = IndexReader.Open((Lucene.Net.Store.Directory) FSDirectory.Open(this.GetIndexDirectory()), false);

                if (lang == null)
                {
                  indexReader.DeleteDocuments(term);
                }
                else
                {
                  indexSearcher = new IndexSearcher(indexReader);
                  TopDocs topDocs = indexSearcher.Search(deleteQuery, int.MaxValue);
                  foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                    indexReader.DeleteDocument(scoreDoc.Doc);
                }
              }
              finally
              {
                if (indexSearcher != null)
                  indexSearcher.Close();
                if (indexReader != null)
                  indexReader.Close();
              }
              break;
            }
            catch (Exception ex)
            {
              GXLogging.Error(logger, "Error deleting from index.", ex);
              break;
            }
        }
      }
    }

    public string GetIndexDirectory() => this.indexDirectory;

    private bool IndexExists(string dir)
    {
      try
      {
        return IndexReader.IndexExists((Lucene.Net.Store.Directory) FSDirectory.Open(dir));
      }
      catch (IOException ex)
      {
        GXLogging.Error(logger, "Error checking index.", ex);
        return false;
      }
    }

    internal static string NormalizeIndexDirectory(string directory)
    {
      if (string.IsNullOrWhiteSpace(directory))
        return ".";
      return Path.GetFullPath(directory);
    }

    private static bool TryReadFileContent(string filePath, out string content)
    {
      content = string.Empty;
      string extension = Path.GetExtension(filePath)?.ToLowerInvariant();

      if (string.IsNullOrWhiteSpace(extension))
      {
        GXLogging.Debug(logger, "Unsupported file extension for indexing: " + extension);
        return false;
      }

      try
      {
        switch (extension)
        {
          case ".pdf":
            content = ExtractPdfContent(filePath);
            return true;
          case ".docx":
            content = ExtractDocxContent(filePath);
            return true;
          case ".xlsx":
            content = ExtractXlsxContent(filePath);
            return true;
          case ".txt":
          case ".html":
            content = File.ReadAllText(filePath);
            return true;
          default:
            GXLogging.Debug(logger, "Unsupported file extension for indexing: " + extension);
            return false;
        }
      }
      catch (Exception ex)
      {
        GXLogging.Error(logger, "Error extracting content from file: " + filePath, ex);
        return false;
      }
    }

    private static string ExtractPdfContent(string filePath)
    {
      StringBuilder builder = new StringBuilder();
      using (PdfDocument pdfDocument = PdfDocument.Open(filePath, (ParsingOptions) null))
      {
        foreach (Page page in pdfDocument.GetPages())
        {
          foreach (Word word in page.GetWords())
          {
            builder.Append(word.Text).Append(' ');
          }
        }
      }
      return builder.ToString();
    }

    private static string ExtractDocxContent(string filePath)
    {
      OPCPackage package = null;
      XWPFWordExtractor xwpfWordExtractor = null;
      try
      {
        package = OPCPackage.Open(filePath);
        xwpfWordExtractor = new XWPFWordExtractor(package);
        return xwpfWordExtractor.Text;
      }
      finally
      {
        (xwpfWordExtractor as IDisposable)?.Dispose();
        (package as IDisposable)?.Dispose();
      }
    }

    private static string ExtractXlsxContent(string filePath)
    {
      OPCPackage package = null;
      XSSFExcelExtractor xssfExcelExtractor = null;
      try
      {
        package = OPCPackage.Open(filePath);
        xssfExcelExtractor = new XSSFExcelExtractor(package);
        return xssfExcelExtractor.Text;
      }
      finally
      {
        (xssfExcelExtractor as IDisposable)?.Dispose();
        (package as IDisposable)?.Dispose();
      }
    }

    private static string SafeValue(string value)
    {
      return value ?? string.Empty;
    }
  }
}
