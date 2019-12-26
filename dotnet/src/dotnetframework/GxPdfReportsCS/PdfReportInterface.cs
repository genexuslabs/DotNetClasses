using System;
using System.IO;
using log4net;

namespace GeneXus.Printer
{

	public class GxReportBuilderPdf : IReportHandler
	{
		static ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		string _appPath;
		IReportHandler _pdfReport;

		public GxReportBuilderPdf(string appPath, Stream outputStream)
		{
			_appPath = appPath;
		{
			_pdfReport = new com.genexus.reports.PDFReportItextSharp(appPath);
		}
			if (outputStream != null)
			{
				
				_pdfReport.setOutputStream( outputStream);
				GXLogging.Debug(log,"GxReportBuilderPdf outputStream: binaryWriter");
			}
		} 
		public GxReportBuilderPdf() : this( "", null)
		{
		}
		public bool GxPrintInit(string output, ref int gxXPage, ref int gxYPage, string iniFile, string form, string printer, int mode, int orientation, int pageSize, int pageLength, int pageWidth, int scale, int copies, int defSrc, int quality, int color, int duplex) 
		{
			
			bool ret = _pdfReport.GxPrintInit( output, ref gxXPage, ref gxYPage, iniFile, form, printer, mode==2 ? 3 : 0, orientation, pageSize, pageLength, pageWidth, scale, copies, defSrc, quality, color, duplex) ;
			return ret;
		}
		public virtual bool GxPrTextInit(string output, ref int gxXPage, ref int gxYPage, string psIniFile, string psForm, string sPrinter, int nMode, int nPaperLength, int nPaperWidth, int nGridX, int nGridY, int nPageLines)
		{
			bool ret = _pdfReport.GxPrTextInit(output, ref gxXPage, ref gxYPage, psIniFile, psForm, sPrinter, nMode, nPaperLength, nPaperWidth, nGridX, nGridY, nPageLines) ;
			return ret;
		}
		public virtual void GxStartDoc()
		{
			_pdfReport.GxStartDoc();
		}
		public void GxStartPage()
		{
			_pdfReport.GxStartPage();
		} 
		public void GxEndPage()
		{
			_pdfReport.GxEndPage();
		}
		public void GxEndDocument()
		{
			_pdfReport.GxEndDocument();
			
		}
		public virtual void GxEndPrinter()
		{
			_pdfReport.GxEndPrinter();
		}
        public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat, int border, int valign)
        {
            _pdfReport.GxDrawText(text, left, top, right, bottom, align, htmlformat, border, valign);
        }
		public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat, int border)
		{
			GxDrawText(text, left, top, right, bottom, align, htmlformat, border, 0);
		}
		public void GxDrawText(string text, int left, int top, int right, int bottom, int align, int htmlformat)
		{
			GxDrawText(text, left, top, right, bottom, align, htmlformat, 0);
		}
        public void GxDrawText(string text, int left, int top, int right, int bottom, int align)
        {
            GxDrawText(text, left, top, right, bottom, align, 0);
        }
        public void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue)
		{
			GxDrawLine( left, top, right, bottom, width, foreRed, foreGreen, foreBlue, 0);
		}
		public void GxDrawLine(int left, int top, int right, int bottom, int width, int foreRed, int foreGreen, int foreBlue, int style)
		{
			_pdfReport.GxDrawLine( left, top, right, bottom, width, foreRed, foreGreen, foreBlue, style);
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen,int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			GxDrawRect( left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, 0, 0);
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen,int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, int style, int cornerRadius)
		{
			GxDrawRect( left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, style, style, style, style, cornerRadius, cornerRadius, cornerRadius, cornerRadius);
		}
		public void GxDrawRect(int left, int top, int right, int bottom, int pen,int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue, 
			int styleTop, int styleBottom, int styleRight, int styleLeft, int cornerRadioTL, int cornerRadioTR, int cornerRadioBL, int cornerRadioBR)
		{
			_pdfReport.GxDrawRect( left, top, right, bottom, pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue, styleTop, styleBottom, styleRight, styleLeft, cornerRadioTL, cornerRadioTR, cornerRadioBL, cornerRadioBR);
		}
		public void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom)
		{
			GxDrawBitMap(bitmap, left, top, right, bottom, 0);
		}
		public void GxDrawBitMap(string bitmap, int left, int top, int right, int bottom, int aspectRatio)
		{
			bitmap = ReportUtils.AddPath(bitmap, _appPath);
			_pdfReport.GxDrawBitMap(bitmap, left, top, right, bottom, aspectRatio);
		}
		public void GxAttris(string fontName, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikethru, int Pen, int foreRed, int foreGreen, int foreBlue, int backMode, int backRed, int backGreen, int backBlue)
		{
			_pdfReport.GxAttris(fontName, fontSize, fontBold, fontItalic, fontUnderline, fontStrikethru, Pen, foreRed, foreGreen, foreBlue, backMode, backRed, backGreen, backBlue);
		}
		public virtual void GxClearAttris()
		{
			_pdfReport.GxClearAttris();
		}
		public virtual void GxRVSetLanguage(string lang)
		{
			_pdfReport.GxRVSetLanguage(lang);
		}
		public virtual void GxSetDocName(string docName)
		{
			docName = ReportUtils.AddPath( docName, _appPath );
			_pdfReport.GxSetDocName(docName);
		}
		public void GxSetDocFormat(string docFormat)
		{
		}
		public virtual void setModal(bool modal)
		{
			_pdfReport.setModal( modal);
		}
		public virtual bool getModal()
		{
			return _pdfReport.getModal();
		}
		public virtual void setPageLines(int pageLines) 
		{
			_pdfReport.setPageLines(pageLines);
		}
		public virtual int getPageLines() 
		{
			return _pdfReport.getPageLines();
		}
		public virtual void setLineHeight(int lineHeight) 
		{
			_pdfReport.setLineHeight(lineHeight);
		}
		public virtual int getLineHeight() 
		{
			return _pdfReport.getLineHeight();
		}
		public virtual void setPage(int page) 
		{
			_pdfReport.setPage( page) ;
		}
		public virtual int getPage() 
		{
			return _pdfReport.getPage();
		}
		public virtual void GxPrintMax() 
		{
			_pdfReport.GxPrintMax();
		}
		public virtual void GxPrintNormal() 
		{
			_pdfReport.GxPrintNormal();
		}
		public virtual void GxPrintOnTop() 
		{
			_pdfReport.GxPrintOnTop();
		}
		public virtual void GxPrnCmd(string scmd)
		{
			_pdfReport.GxPrnCmd(scmd);
		}
		public virtual void GxPrnCmd()
		{
		}
		public void GxShutdown()
		{
		}
		public virtual bool GxIsAlive() 
		{
			return _pdfReport.GxIsAlive();
		}
		public virtual bool GxIsAliveDoc()
		{
			return _pdfReport.GxIsAliveDoc();
		}
		public virtual bool GxPrnCfg( string ini )
		{
			return _pdfReport.GxPrnCfg( ini );
		}
		public virtual void setMetrics(string fontName, bool bold, bool italic, int ascent, int descent, int height, int maxAdvance, int[] sizes)
		{
			_pdfReport.setMetrics(fontName, bold, italic, ascent, descent, height, maxAdvance, sizes);
		}
		public bool GxOpenDoc(string fileName)
		{
			return false;
		}
		public bool GxRptSilentMode()
		{
			return false;
		}
		public void GxSetTextMode(int nHandle, int nGridX, int nGridY, int nPageLength)
		{
		}

		public int getM_top()
		{
			return _pdfReport.getM_top();
		}

		public int getM_bot()
		{
			return _pdfReport.getM_bot();
		}

		public void setM_top(int top)
		{
			_pdfReport.setM_top(top);
		}

		public void setM_bot(int bot)
		{
			_pdfReport.setM_bot(bot);
		}
		public void setOutputStream(object stream)
		{
		}

	}

}
