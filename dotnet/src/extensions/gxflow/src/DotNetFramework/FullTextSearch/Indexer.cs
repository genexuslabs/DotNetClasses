using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NPOI.OpenXml4Net.OPC;
using NPOI.XSSF.Extractor;
using NPOI.XWPF.Extractor;
using System;
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace com.genexus.CA.search
{
  public class Indexer
  {
    private string indexDirectory = ".";

    internal Indexer(string directory)
    {
      this.indexDirectory = directory;
      if (this.IndexExists(directory))
        return;
      try
      {
        this.indexDirectory = directory;
        new IndexWriter((Lucene.Net.Store.Directory) FSDirectory.Open(this.indexDirectory), (Analyzer) new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), true, IndexWriter.MaxFieldLength.UNLIMITED).Close();
      }
      catch (Exception ex)
      {
        Logger.Print(ex.ToString());
      }
    }

    internal void AddContent(
      string uri,
      string lang,
      string title,
      string summary,
      short fromFile,
      string body,
      string filePath)
    {
      string str = "";
      Document doc = new Document();
      if (fromFile == (short) 1)
      {
        if (filePath.EndsWith(".pdf"))
        {
          foreach (Page page in PdfDocument.Open(filePath, (ParsingOptions) null).GetPages())
          {
            foreach (Word word in page.GetWords())
              str = str + word.Text + " ";
          }
        }
        else if (filePath.EndsWith(".doc") || filePath.EndsWith(".docx"))
        {
          XWPFWordExtractor xwpfWordExtractor = new XWPFWordExtractor(OPCPackage.Open(filePath));
          str += xwpfWordExtractor.Text;
        }
        else if (filePath.EndsWith(".xsl") || filePath.EndsWith(".xslx"))
        {
          XSSFExcelExtractor xssfExcelExtractor = new XSSFExcelExtractor(OPCPackage.Open(filePath));
          str += xssfExcelExtractor.Text;
        }
        else if (filePath.EndsWith(".txt") || filePath.EndsWith(".html"))
          str += File.ReadAllText(filePath);
      }
      if (doc == null)
        return;
      if (this.DocumentExists(uri, lang))
        this.IndexOperation(2, lang, (Document) null, uri.ToLower());
      doc.Add((IFieldable) new Field(nameof (uri), uri, Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add((IFieldable) new Field("language", lang, Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add((IFieldable) new Field(nameof (title), title, Field.Store.YES, Field.Index.ANALYZED));
      doc.Add((IFieldable) new Field("path", filePath, Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add((IFieldable) new Field("content", str, Field.Store.YES, Field.Index.ANALYZED));
      doc.Add((IFieldable) new Field(nameof (summary), summary, Field.Store.NO, Field.Index.ANALYZED));
      doc.Add((IFieldable) new Field(nameof (body), body, Field.Store.YES, Field.Index.ANALYZED));
      try
      {
        this.IndexOperation(1, lang, doc, (string) null);
      }
      catch (Exception ex)
      {
        Logger.Print(ex.ToString());
      }
    }

    internal void DeleteContent(string uri)
    {
      try
      {
        this.IndexOperation(2, (string) null, (Document) null, uri.ToLower());
      }
      catch (Exception ex)
      {
        Logger.Print(ex.ToString());
      }
    }

    protected void IndexOperation(int op, string lang, Document doc, string uri)
    {
      lock (this)
      {
        switch (op)
        {
          case 1:
            try
            {
              IndexWriter indexWriter = new IndexWriter((Lucene.Net.Store.Directory) FSDirectory.Open(this.GetIndexDirectory()), AnalyzerManager.GetAnalyzer(lang), false, IndexWriter.MaxFieldLength.UNLIMITED);
              indexWriter.AddDocument(doc);
              indexWriter.Optimize();
              indexWriter.Close();
              break;
            }
            catch (Exception ex)
            {
              Logger.Print(ex.ToString());
              break;
            }
          case 2:
            try
            {
              Term term = (Term) null;
              int docNum = 0;
              if (lang == null)
                term = new Term(nameof (uri), uri);
              else
                docNum = this.GetDocumentId(uri, lang);
              IndexReader indexReader = IndexReader.Open((Lucene.Net.Store.Directory) FSDirectory.Open(this.GetIndexDirectory()), false);
              if (lang == null)
                indexReader.DeleteDocuments(term);
              else if (docNum != -1)
                indexReader.DeleteDocument(docNum);
              indexReader.Close();
              break;
            }
            catch (Exception ex)
            {
              Logger.Print(ex.ToString());
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
        IndexSearcher indexSearcher = new IndexSearcher((Lucene.Net.Store.Directory) FSDirectory.Open(dir));
        return true;
      }
      catch (IOException ex)
      {
        Logger.Print(ex.ToString());
        return false;
      }
    }

    private bool DocumentExists(string uri, string lang)
    {
      bool flag = false;
      try
      {
        IndexSearcher indexSearcher = new IndexSearcher((Lucene.Net.Store.Directory) FSDirectory.Open(this.indexDirectory));
        TopDocs topDocs = indexSearcher.Search((Query) new BooleanQuery()
        {
          {
            (Query) new TermQuery(new Term(nameof (uri), uri)),
            Occur.MUST
          },
          {
            (Query) new TermQuery(new Term("language", lang)),
            Occur.MUST
          }
        }, (Filter) null, 1);
        indexSearcher.Close();
        if (topDocs.TotalHits > 0)
          flag = true;
      }
      catch (IOException ex)
      {
        Logger.Print(ex.ToString());
      }
      return flag;
    }

    private int GetDocumentId(string uri, string lang)
    {
      int documentId = -1;
      try
      {
        IndexSearcher indexSearcher = new IndexSearcher((Lucene.Net.Store.Directory) FSDirectory.Open(this.indexDirectory));
        TopDocs topDocs = indexSearcher.Search((Query) new BooleanQuery()
        {
          {
            (Query) new TermQuery(new Term(nameof (uri), uri)),
            Occur.MUST
          },
          {
            (Query) new TermQuery(new Term("language", lang)),
            Occur.MUST
          }
        }, 1);
        if (topDocs.TotalHits > 0)
          documentId = topDocs.ScoreDocs[0].Doc;
        indexSearcher.Close();
      }
      catch (IOException ex)
      {
        Logger.Print(ex.ToString());
      }
      return documentId;
    }
  }
}
