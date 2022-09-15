using System;
using System.IO;
using log4net;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using GeneXus.Office.Excel;

namespace GeneXus.Office.ExcelGXEPPlus
{

	public class ExcelDocument : IGxError, IExcelDocument
    {
        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ExcelPackage p;
        public string dateFormat = "m/d/yy h:mm";
		public bool OpenFromTemplate { get; set;}

        public short Open(String fileName)
        {
			OpenFromTemplate = false;
			try
            {
                if (!string.IsNullOrWhiteSpace(template))
                {
                    GxFile temp = new GxFile(Path.GetDirectoryName(template), template);
                    if (temp.Exists())
                    {
						Stream stream = temp.GetStream();
						if (stream != null)
						{
							GXLogging.Debug(log, "Opening Template " + template);
							p = new ExcelPackage(stream);
							OpenFromTemplate = true;
						}
						else
						{
							errCod = 4;
							errDescription = "Invalid template.";
							return errCod;
						}

					}
                    else
                    {
                        errCod = 4; 
                        errDescription = "Invalid template.";
                        return errCod;
                    }
                }
                else
                {
                    GxFile file = new GxFile(fileName, fileName, GxFileType.Private);

                    if (string.IsNullOrEmpty(Path.GetExtension(fileName)) && !file.Exists())
                    {
                        fileName += Constants.EXCEL2007Extension;
                    }
					if (file.IsExternalFile)
					{
						Stream stream = file.GetStream();
						if (stream != null)
						{
							p = new ExcelPackage(file.GetStream());
						}
						else
						{
							errCod = 4;
							errDescription = "Invalid file.";
							return errCod;
						}
					}
					else
						p = new ExcelPackage(new FileInfo(fileName));
				}
                
                workBook = (ExcelWorkbook)p.Workbook;
                workBook.CalcMode = ExcelCalcMode.Automatic;                

                this.selectFirstSheet();
                xlsFileName = fileName.ToString();
            }
            catch (Exception e)
            {
                GXLogging.Error(log, "Error opening " + fileName, e);

                errCod = 10; 
                errDescription = "Could not open file." + e.Message + (e.InnerException != null ? e.InnerException.Message : "");

                return errCod;
            }
            return 0;
        }

        public short Save()
        {
            try
            {
                if (IsReadOnly())
                {
                    errCod = 13;
                    errDescription = "Can not modify a readonly document";
					p.Dispose();
                }
                else
                {
                    using (var stream = new MemoryStream())
                    {
                        p.SaveAs(stream);
						p.Dispose();
                        GxFile file = new GxFile(Path.GetDirectoryName(xlsFileName), xlsFileName, GxFileType.Private);
                        stream.Position = 0;
                        file.Create(stream);
                    }
                }
            }
            catch (Exception e)
            {
                GXLogging.Error(log, "Error saving file " + xlsFileName, e);
                errCod = 12; 
                errDescription = "Could not save file." + e.Message;
                return -1;
            }
            return 0;
        }

        public short Close()
        {
            Save();
            try
            {
                if (p != null)
                {
                    p.Dispose();
                    p = null;
                    workBook = null;
                    currentSheet = null;
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }

        public void SetDateFormat(string dFormat)
        {   
            dateFormat = dFormat;
        }
        public string GetDateFormat()
        {
            return dateFormat;
        }
      
        public short SelectSheet(String sheetName)
        {            
            GXLogging.Debug(log,"selectSheet " + sheetName);
            if (!string.IsNullOrEmpty(sheetName))
            {
                ExcelWorksheet pSheet = workBook.Worksheets[sheetName];
                if ((pSheet == null) && IsReadOnly())
                {
                    this.errCod = 5;
                    this.errDescription = "Invalid worksheet name";
                    return errCod;
                }

                if (pSheet == null)
                { 
                    GXLogging.Debug(log,"create Sheet " + sheetName);
                    pSheet = workBook.Worksheets.Add(sheetName);
                }
                currentSheet = pSheet;
                GXLogging.Debug(log,"currentSheet " + currentSheet);
                return 0;

            }
            else
            {
                this.errCod = 5;
                this.errDescription = "Invalid worksheet name";
                return errCod;
            }
        }

        private void selectFirstSheet()
        {
            if (workBook.Worksheets.Count == 0)
            {
                if (IsReadOnly())
                    return;
                else
                    workBook.Worksheets.Add("Sheet1");
            }
			int idx = 1;
#if NETCORE
			idx = 0;
#endif
			currentSheet = workBook.Worksheets[idx];
        }

        public short RenameSheet(String sheetName)
        {
            if (IsReadOnly())
            {
                errCod = 13;
                errDescription = "Can not modify a readonly document";
                return errCod;
            }
            currentSheet.Name = sheetName;
            return 0;
        }

        public short AddRows(int rowIdx, int rows)
        {
			if (IsReadOnly())
            {
                errCod = 13;
                errDescription = "Can not modify a readonly document";
                return errCod;
            }  
            currentSheet.InsertRow(rowIdx, rows);			
            return 0;
        }

        public short Clear()
        {
            if (IsReadOnly())
            {
                errCod = 13;
                errDescription = "Can not modify a readonly document";
                return errCod;
            }
            if (currentSheet != null)
                currentSheet.Cells.Clear();         
            return 0;
        }

        public IExcelCells Cells(int Row, int Col)
        {
            ExcelCells iCell = new ExcelCells((IGxError)this, this, workBook, currentSheet, Row - 1, Col - 1, 1, 1, IsReadOnly());
            return iCell;

        }
        public IExcelCells Cells(int Row, int Col, int Height, int Width)
        {
            ExcelCells iCell = new ExcelCells((IGxError)this, this, workBook, currentSheet, Row - 1, Col - 1, Height, Width, IsReadOnly());
            return iCell;
        }
        public IExcelCells get_Cells(int Row, int Col, int Height, int Width)
        {
            ExcelCells iCell = new ExcelCells(this, this, workBook, currentSheet, Row, Col, Height, Width, IsReadOnly());
            iCell.FitColumnWidth = autoFit;
            return iCell;
        }

        public short ErrCode
        {
            get
            {
                return this.errCod;
            }
        }

        public String ErrDescription
        {
            get { return this.errDescription; }
        }

        private bool IsReadOnly()
        {
            return (readOnly != 0);
        }

        public short ReadOnly
        {
            get { return readOnly; }
            set { readOnly = value; }
        }
        public void cleanup()
        {
        }
        public void setErrCod(short EerrCod)
        {
            this.errCod = EerrCod;
        }
        public void setErrDes(String EerrDes)
        {
            this.errDescription = EerrDes;
        }

        public short Show()
        {
            try
            {
                ExcelUtils.Show(xlsFileName);
                return 0;
            }
            catch (Exception e)
            {
                GXLogging.Error(log,"Show Error", e);
                errCod = 10; 
                errDescription = "Could not open file." + e.Message + (e.InnerException != null ? e.InnerException.Message : "");
                return errCod;
            }

        }
        public void CalculateFormulas()
        {            
            this.workBook.Calculate();
        }

        public String Template
        {
            get { return template; }
            set { template = value; }
        }

        public short AutoFit
        {
            get { return autoFit ? (short)1 : (short)0; }
            set { autoFit = (value == 1); }
        }

        private ExcelWorkbook workBook;
        private ExcelWorksheet currentSheet;
        private String xlsFileName;
        private String template = string.Empty;
        private short errCod = 0;
        private String errDescription = "OK";
        private short readOnly = 0;
        private bool autoFit = false;

        public short UnBind() { return -1; }
        public short Hide() { return -1; }
        public short PrintOut(short preview) { return -1; }
        public short ErrDisplay { get { return -1; } set { } }
        public String DefaultPath { get { return string.Empty; } set { } }
        public String Delimiter { get { return string.Empty; } set { } }

        public short RunMacro(string Macro, object Arg1, object Arg2, object Arg3, object Arg4, object Arg5, object Arg6, object Arg7, object Arg8, object Arg9, object Arg10, object Arg11, object Arg12, object Arg13, object Arg14, object Arg15, object Arg16, object Arg17, object Arg18, object Arg19, object Arg20, object Arg21, object Arg22, object Arg23, object Arg24, object Arg25, object Arg26, object Arg27, object Arg28, object Arg29, object Arg30) { return 0; }
        public object MacroReturnValue { get { return null; } }
        public ExcelWorkbook Workbook { get { return null; } }
        public string MacroReturnText { get { return null; } }
        public double MacroReturnNumber { get { return 0; } }
        public DateTime MacroReturnDate { get { return DateTime.MinValue; } }

        #region IExcelDocument Members

        public short Init(string previousMsgError)
        {
            return 0;
        }

        #endregion
    }

    public class ExcelCells : IExcelCells
    {

        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IGxError m_errAccess;
        private int pWidth, pHeight;
        private int pColPos;
        private ExcelWorksheet pSelectedSheet;
        private const int COLOR_OFFSET = 7;
        private bool readOnly;
        private ExcelDocument doc;

        public ExcelCells(IGxError errAccess, ExcelDocument doc,  object workBook, object selectedSheet, int rowPos, int colPos, int height, int width)
            : this(errAccess,doc, workBook, selectedSheet, rowPos, colPos, height, width, false)
        { }

        public ExcelCells(IGxError errAccess, ExcelDocument document, object workBook, object selectedSheet, int rowPos, int colPos, int height, int width, bool readOnly)
        {
            doc = document;
            m_errAccess = errAccess;
            pWidth = width;
            pHeight = height;
            cntCells = 0;
            pColPos = colPos;
            pSelectedSheet = (ExcelWorksheet)selectedSheet;
            this.readOnly = readOnly;

            try
            {
                pCellsRange = getExcelCell(rowPos, pColPos, pColPos + (pWidth - 1), rowPos + (pHeight - 1));
                pCells = new ExcelRange[width * height + 1];
                string address = pCellsRange.Address;
                int startRow = pCellsRange.Start.Row;
                int endRow = pCellsRange.End.Row;
                int startCol = pCellsRange.Start.Column;
                int endCol = pCellsRange.End.Column;
                for (int i = startRow; i <= endRow; i++)
                {
                    for (int j = startCol; j <= endCol; j++)
                    {
                        cntCells++;
                        pCells[cntCells] = pCellsRange[i,j];                        
                    }
                }
                pCellsRange.Address = address;
            }

            catch (Exception e)
            {
                GXLogging.Error(log,"ExcelCells error", e);
                m_errAccess.setErrDes("Invalid cell coordinates");
                m_errAccess.setErrCod((short)8);
            }
        }
        private ExcelRange getExcelCell(int startRow, int startColumn, int endColumn, int endRow)
        {
            ExcelRange cell = ((ExcelWorksheet)pSelectedSheet).Cells[startRow, startColumn, endRow, endColumn];

            if ((cell == null) && readOnly)
            {
                return null;
            }
            return cell;
        }

        public double Number
        {
            get
            {
                try
                {
                    return GetFirstCell().GetValue<Double>();

                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Number error", e);
                    m_errAccess.setErrDes("Invalid cell value");
                    m_errAccess.setErrCod((short)7);
                    return -1;
                }
            }
            set
            {
                try
                {
                    if (readOnly)
                    {
                        m_errAccess.setErrDes("Can not modify a readonly document");
                        m_errAccess.setErrCod((short)13);
                        return;
                    }

                    for (int i = 1; i <= cntCells; i++)
                    {
                        pCells[i].Value = value;
                    }
                    fitColumnWidth();

                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Number error", e);
                    m_errAccess.setErrDes("Invalid cell value");
                    m_errAccess.setErrCod((short)7);
                }

            }

        }

        private ExcelRange GetFirstCell()
        {
            return pSelectedSheet.Cells[pCellsRange.Start.Address];
        }

        public DateTime Date
        {
            get
            {
                try
                {

                    return GetFirstCell().GetValue<DateTime>();
                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Date error", e);
                    m_errAccess.setErrDes("Invalid cell value");
                    m_errAccess.setErrCod((short)7);
                    DateTime errDate = DateTime.MinValue;
                    return errDate;
                }
            }
            set
            {
                try
                {
                    if (readOnly)
                    {
                        m_errAccess.setErrDes("Can not modify a readonly document");
                        m_errAccess.setErrCod((short)13);
                        return;
                    }

                    if (value != DateTime.MinValue)
                    {
                        GXLogging.Debug(log,"SetDate value:" + value.ToString());
                        DateTime d = value;                        

                        string dformat = doc.GetDateFormat();

                        if (value.Minute == 0 && value.Hour == 0 && value.Second == 0 && value.Millisecond == 0 && dformat.IndexOf(' ') > 0)
                            dformat = dformat.Substring(0, dformat.IndexOf(' '));
                        
                        GXLogging.Debug(log,"dformat: " + dformat);

						for (int i = 1; i <= cntCells; i++)
                        {
							ExcelRange itemCell = pCells[i];
							string fmt = itemCell.Style.Numberformat.Format;
							if ( !((ExcelDocument)this.doc).OpenFromTemplate || itemCell.Style.Numberformat.NumFmtID == 0)
							{
								itemCell.Style.Numberformat.Format = dformat;
							}
							itemCell.Value = (DateTime)d;
                        }
                        fitColumnWidth();
                    }
                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Date error", e);
                    m_errAccess.setErrDes("Invalid cell value");
                    m_errAccess.setErrCod((short)7);
                }

            }
        }

        public bool FitColumnWidth
        {
            get { return _fitColumnWith; }
            set { _fitColumnWith = value; }
        }
        private bool _fitColumnWith;

        public string Text
        {
            get
            {
                string text = string.Empty;
                try
                {                    
                    ExcelRange cell = GetFirstCell();
                    if (string.IsNullOrEmpty(cell.Formula))
                    {
                        text = cell.GetValue<String>();
                    }
                    else
                    {
                        text = cell.Formula;
                    }                                                            
                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Text error", e);
                    m_errAccess.setErrDes("Invalid cell value");
                    m_errAccess.setErrCod((short)7);                    
                }
                return (text == null) ? String.Empty : text;
            }
            set
            {
                if (readOnly)
                {
                    m_errAccess.setErrDes("Can not modify a readonly document");
                    m_errAccess.setErrCod((short)13);
                    return;
                }

                try
                {
                    for (int i = 1; i <= cntCells; i++)
                    {
                        if (!String.IsNullOrEmpty(value) && value.StartsWith("=")) //It is a Formula
                        {
                            try
                            {
                                pCells[i].Formula = value.Substring(1, value.Length - 1);
                            }
                            catch
                            {
                                GXLogging.Warn(log, "Could not set formula " + value);
                                pCells[i].Value = value;
                            }
                        }
                        else
                            pCells[i].Value = value;

                        fitColumnWidth();
                    }
                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Text error", e);
                    m_errAccess.setErrDes("Invalid cell value");
                    m_errAccess.setErrCod((short)7);
                }

            }
        }

		public string Value
		{
			get
			{
				try
				{
                    return GetFirstCell().GetValue<String>();
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Value error", e);
					m_errAccess.setErrDes("Invalid cell value");
					m_errAccess.setErrCod((short)7);
					return null;
				}
			}
		}
		
		private void fitColumnWidth()
        {
            if (_fitColumnWith)
                pCellsRange.AutoFitColumns();
        }

        public string Type
        {
            get
            {
				ExcelRange e = GetFirstCell();
                if (e.Value != null)
                {
					ExcelNumberFormat format = e.Style.Numberformat;
					
					if (!format.BuildIn && format.NumFmtID > 0 && format.Format.IndexOf("MM",StringComparison.OrdinalIgnoreCase)>=0 && 
						format.Format.IndexOf("yy", StringComparison.OrdinalIgnoreCase)>=0 && 
						format.Format.IndexOf("dd", StringComparison.OrdinalIgnoreCase)>=0 && 
						(format.Format.IndexOf('-')>=0 || format.Format.IndexOf('/')>=0))
					{
							return "D";
					}
					else
					{
						Type type = GetFirstCell().Value.GetType();
						if (type.Equals(typeof(DateTime)))
							return "D";
						if (type.Equals(typeof(Char)) || type.Equals(typeof(String)) || type.Equals(typeof(System.Text.StringBuilder)))
							return "C";
						if (type.Equals(typeof(Byte)) || type.Equals(typeof(Single)) || type.Equals(typeof(Double)) || type.Equals(typeof(Decimal)))
							return "N";
						return "U";
					}
                }
                else
                    return "C";

            }
        }

        public double Size
        {
            get
            {
                return GetFont(1).Size;
            }
            set
            {
                if (readOnly)
                {
                    m_errAccess.setErrDes("Can not modify a readonly document");
                    m_errAccess.setErrCod((short)13);
                    return;
                }

                try
                {
                    for (int i = 1; i <= cntCells; i++)
                    {
                        ExcelFont fontCell = GetFont(i);
                        fontCell.Size = (float)value;

                    }
                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Size error", e);
                }
            }
        }

        private ExcelFont GetFont(int pCellsIndex)
        {
            return pCells[pCellsIndex].Style.Font;
        }

        public string Font
        {
            get
            {
                return GetFont(1).Name;
            }
            set
            {
                if (readOnly)
                {
                    m_errAccess.setErrDes("Can not modify a readonly document");
                    m_errAccess.setErrCod((short)13);
                    return;
                }

                try
                {
                    for (int i = 1; i <= cntCells; i++)
                    {
                        ExcelFont fontCell = GetFont(i);
                        fontCell.Name = value;
                    }

                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Font error", e);
                    m_errAccess.setErrDes("Invalid font properties");
                    m_errAccess.setErrCod((short)6);
                }
            }
        }

        public short Bold
        {
            get
            {

                return Convert.ToInt16(GetFont(1).Bold);
            }
            set
            {
                if (readOnly)
                {
                    m_errAccess.setErrDes("Can not modify a readonly document");
                    m_errAccess.setErrCod((short)13);
                    return;
                }

                try
                {

                    for (int i = 1; i <= cntCells; i++)
                    {
                        ExcelFont fontCell = GetFont(i);
                        fontCell.Bold = Convert.ToBoolean(value);
                    }
                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Bold error", e);
                    m_errAccess.setErrDes("Invalid font properties");
                    m_errAccess.setErrCod((short)6);
                }
            }
        }

        public short Italic
        {
            get
            {
                return Convert.ToInt16(GetFont(1).Italic);
            }
            set
            {
                if (readOnly)
                {
                    m_errAccess.setErrDes("Can not modify a readonly document");
                    m_errAccess.setErrCod((short)13);
                    return;
                }

                try
                {

                    for (int i = 1; i <= cntCells; i++)
                    {
                        ExcelFont fontCell = GetFont(i);
                        fontCell.Italic = Convert.ToBoolean(value);
                    }
                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Italic error", e);
                    m_errAccess.setErrDes("Invalid font properties");
                    m_errAccess.setErrCod((short)6);
                }
            }
        }
        public short Underline
        {
            get
            {
                return Convert.ToInt16(GetFont(1).UnderLine);
            }
            set
            {
                if (readOnly)
                {
                    m_errAccess.setErrDes("Can not modify a readonly document");
                    m_errAccess.setErrCod((short)13);
                    return;
                }

                try
                {
                    for (int i = 1; i <= cntCells; i++)
                    {
                        ExcelFont fontCell = GetFont(i);
                        fontCell.UnderLine = Convert.ToBoolean(value);
                    }
                }
                catch (Exception e)
                {
                    GXLogging.Error(log,"Underline error", e);
                    m_errAccess.setErrDes("Invalid font properties");
                    m_errAccess.setErrCod((short)6);
                }
            }
        }

        public int BackColor
        {
            get
            {
                int color = Convert.ToInt32(GetFont(1).Color.Tint);
                return color - COLOR_OFFSET;
            }
            set
            {

                for (int i = 1; i <= cntCells; i++)
                {
					if (pCells[i].Style.Fill.PatternType == ExcelFillStyle.None)
					{
						pCells[i].Style.Fill.PatternType = ExcelFillStyle.Solid;
					}
					pCells[i].Style.Fill.BackgroundColor.SetColor(GXExcelHelper.ResolveColor(value));
                }                
            }
        }

        public int Color
        {
            get
            {
                int color = Convert.ToInt32(GetFont(1).Color.Tint);
                return color - COLOR_OFFSET;
            }
            set
            {
                setColor((long)value);
            }
        }

        public void setColor(long value) 
        {
            try
            {
                if (readOnly)
                {
                    m_errAccess.setErrDes("Can not modify a readonly document");
                    m_errAccess.setErrCod((short)13);
                    return;
                }
                
                for (int i = 1; i <= cntCells; i++)
                {
                    ExcelFont fontCell = GetFont(i);
                    int val = (int)value;                    
					fontCell.Color.SetColor(GXExcelHelper.ResolveColor(val));
				}
            }
            catch (Exception e)
            {
                GXLogging.Error(log,"setcolor error", e);
                m_errAccess.setErrDes("Invalid font properties");
                m_errAccess.setErrCod((short)6);
            }
        }

        public short AutoFit { set { } }
        private int cntCells;
        private ExcelRange[] pCells;
        private ExcelRange pCellsRange;
    }


}