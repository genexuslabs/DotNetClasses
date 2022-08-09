using System;
using GeneXus.Office;
using System.Collections;
using System.Reflection;
using System.IO;
using log4net;
using GeneXus.Application;
using GeneXus.Office.Excel;

namespace GeneXus.Office.ExcelLite
{
	public class ExcelCells : IExcelCells
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ExcelCells(IGxError errAccess,  object ef, int row, int col, int height, int width)
		{
			this.errAccess = errAccess;
			object cells = GxExcelUtils.GetPropValue(GxExcelUtils.GetPropValue(GxExcelUtils.GetPropValue(ef, "Worksheets"), "ActiveWorksheet"), "Cells");
			cr = GxExcelUtils.Invoke(cells, "GetSubrangeAbsolute", new Object[]{row, col, row + height, col + width});
			GxExcelUtils.SetPropValue(cr, "Merged", true);
		}

		public double Number
		{
			get
			{
				try
				{
					object cell = GxExcelUtils.Invoke(cr, "get_Item", new object[]{0,0});
					object result = GxExcelUtils.GetPropValue(cell, "Value");
					return Convert.ToDouble(result);
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Number error", e);
					errAccess.setErrDes("Invalid cell value");
					errAccess.setErrCod((short)7);
					return -1;
				}
			}
			set
			{
				try
				{
					GxExcelUtils.SetPropValue(cr, "Value", value);
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Number error", e);
					errAccess.setErrDes("Invalid cell value");
					errAccess.setErrCod((short)7);
				}
			}
		}

		public DateTime Date
		{
			get
			{
				try
				{
					object cell = GxExcelUtils.Invoke(cr, "get_Item", new object[]{0,0});
					return (DateTime)GxExcelUtils.GetPropValue(cell, "Value");
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Date error", e);
					errAccess.setErrDes("Invalid cell value");
					errAccess.setErrCod((short)7);
					return new DateTime(0);
				}
			}
			set
			{
				try
				{
					if (value!=DateTime.MinValue)
					{
						GxExcelUtils.SetPropValue(cr, "Value", value);
					}

				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Date error", e);
					errAccess.setErrDes("Invalid cell value");
					errAccess.setErrCod((short)7);
				}
			}
		}
		public string Text
		{
			get
			{
				try
				{
					object cell = GxExcelUtils.Invoke(cr, "get_Item", new object[]{0,0});
					object val= (object)GxExcelUtils.GetPropValue(cell, "Value");
					
					if (val is string)
						return (string)val;
					else
					{
						string valFrm = (string)GxExcelUtils.GetPropValue(cell, "Formula");
						if (valFrm!=null)
							return valFrm;
						else
							return Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture);
					}
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Text error", e);
					errAccess.setErrDes("Invalid cell value");
					errAccess.setErrCod((short)7);
					return "";
				}
			}
			set
			{
				try
				{
					if (value.Length>0 && value[0]=='=')
					{
						try
						{
							GxExcelUtils.SetPropValue(cr, "Formula", value);
						}
						catch(Exception ex)
						{
							GXLogging.Error(log,"Could not set formula " + value, ex);
							GxExcelUtils.SetPropValue(cr, "Value", value);
						}
					}
					else

					GxExcelUtils.SetPropValue(cr, "Value", value);
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Text error", e);
					errAccess.setErrDes("Invalid cell value");
					errAccess.setErrCod((short)7);
				}
			}
		}
		public string Value
		{
			get
			{
				try
				{
					object cell = GxExcelUtils.Invoke(cr, "get_Item", new object[] { 0, 0 });
					object val = (object)GxExcelUtils.GetPropValue(cell, "Value");

					if (val is string)
						return (string)val;
					else
					{
						return Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture);
					}
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Value error", e);
					errAccess.setErrDes("Invalid cell value");
					errAccess.setErrCod((short)7);
					return "";
				}
			}
		}

		public string Type
		{
			get
			{
				
				object cell = GxExcelUtils.Invoke(cr, "get_Item", new object[]{0,0});
				Type type = cell.GetType();

				if (type.Equals(typeof(DateTime)))
					return "D";
				if (type.Equals(typeof(Char)) || type.Equals(typeof(String)) || type.Equals(typeof(System.Text.StringBuilder)))
					return "C";
				if (type.Equals(typeof(Byte)) || type.Equals(typeof(Single)) || type.Equals(typeof(Double)) || type.Equals(typeof(Decimal)))
					return "N";
				return "U";
			}
		}

		public double Size
		{
			get
			{
				return (double)GxExcelUtils.GetPropValue(GetFont(cr), "Size");
			}
			set
			{

				try
				{
					GxExcelUtils.SetPropValue(GetFont(cr), "Size", (int)value * 20);
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Size error", e);
					errAccess.setErrDes("Invalid font properties");
					errAccess.setErrCod((short)6);
				}
			}
		}

		public string Font
		{
			get
			{
				return (string)GxExcelUtils.GetPropValue(GetFont(cr), "Name");
			}
			set
			{

				try
				{
					GxExcelUtils.SetPropValue(GetFont(cr), "Name", value);
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Font error", e);
					errAccess.setErrDes("Invalid font properties");
					errAccess.setErrCod((short)6);
				}
			}
		}
		private object GetFont(object crinstance)
		{
			object cell = GxExcelUtils.Invoke(crinstance, "get_Item", new object[]{0,0});
			return GxExcelUtils.GetPropValue(	GxExcelUtils.GetPropValue(cell, "Style"), "Font");
		}

		public short Bold
		{
			get
			{
				int w1 = (int)GxExcelUtils.GetPropValue(GetFont(cr), "Weight");
				int w2 = (int)GxExcelUtils.GetConstantValue(ExcelDocument.ass, ExcelDocument.nmspace + ".ExcelFont", "BoldWeight");

				if (w1 == w2)
					return 1;
				else
					return 0;
			}
			set
			{

				try
				{
					if (value == 1)
						GxExcelUtils.SetPropValue(GetFont(cr),"Weight", (int)GxExcelUtils.GetConstantValue(ExcelDocument.ass, ExcelDocument.nmspace + ".ExcelFont", "BoldWeight"));
					else
						GxExcelUtils.SetPropValue(GetFont(cr),"Weight", (int)GxExcelUtils.GetConstantValue(ExcelDocument.ass, ExcelDocument.nmspace + ".ExcelFont", "NormalWeight"));
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Bold error", e);
					errAccess.setErrDes("Invalid font properties");
					errAccess.setErrCod((short)6);
				}
			}
		}

		public short Italic
		{
			get
			{
				if ((bool)GxExcelUtils.GetPropValue(GetFont(cr),"Italic"))
					return 1;
				else
					return 0;
			}
			set
			{
				try
				{
					if (value == 1)
						GxExcelUtils.SetPropValue(GetFont(cr),"Italic", true);
					else
						GxExcelUtils.SetPropValue(GetFont(cr),"Italic", false);
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Italic error", e);
					errAccess.setErrDes("Invalid font properties");
					errAccess.setErrCod((short)6);
				}
			}
		}

		public short Underline
		{
			get
			{
				object ustyle = GxExcelUtils.GetPropValue(GetFont(cr),"UnderlineStyle");
				object none =  GxExcelUtils.GetEnumValue(ExcelDocument.ass, ExcelDocument.nmspace , "UnderlineStyle", "None");

				if (ustyle == none)
					return 0;
				else
					return 1;
			}
			set
			{

				try
				{
					if (value == 1)
						GxExcelUtils.SetPropValue(GetFont(cr),"UnderlineStyle", GxExcelUtils.GetEnumValue(ExcelDocument.ass, ExcelDocument.nmspace , "UnderlineStyle", "Single"));
					else
						GxExcelUtils.SetPropValue(GetFont(cr),"UnderlineStyle", GxExcelUtils.GetEnumValue(ExcelDocument.ass, ExcelDocument.nmspace , "UnderlineStyle", "None"));
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Underline error", e);
					errAccess.setErrDes("Invalid font properties");
					errAccess.setErrCod((short)6);
				}
			}
		}

        public int BackColor
        {
            get
            {
                return 0;
            }
            set
            {
				throw new NotImplementedException();               
            }
        }

		public int Color
		{
			get
			{
				return ((System.Drawing.Color)GxExcelUtils.GetPropValue(GetFont(cr),"Color")).ToArgb();
			}
			set
			{
				try
				{
					GxExcelUtils.SetPropValue(GetFont(cr),"Color",GXExcelHelper.ResolveColor(value));
				}
				catch (Exception e)
				{
					GXLogging.Error(log,"Color error", e);
					errAccess.setErrDes("Invalid font properties");
					errAccess.setErrCod((short)6);
				}
			}
		}

		private IGxError errAccess;
		private  object cr;
	}

	public class ExcelDocument : IGxError, IExcelDocument
	{
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static string nmspace;
		public static string license;
		public static Assembly ass;

		public short Init(string previousMsgError)
		{
			try
			{
				GXLogging.Debug(log,"Init");

				if (ass==null)
				{
					if (nmspace==null)
					{
						ass = loadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), "GemBox.Spreadsheet.dll"));
						if (ass==null)
						{
							ass = loadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), @"bin\GemBox.Spreadsheet.dll"));
						}
						if (ass!=null)
						{
							nmspace="GemBox.Spreadsheet";
						}
						else
						{
							if (ass==null)
							{
								ass = loadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), @"GemBox.ExcelLite.dll"));
							}
							if (ass==null)
							{
								ass = loadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), @"bin\GemBox.ExcelLite.dll"));
							}
							if (ass!=null)
							{
								nmspace="GemBox.ExcelLite";
							}
						}
							
					}
					else
					{
						ass = loadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), nmspace + ".dll"));
						if (ass==null)
						{
							ass = loadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), @"bin\" + nmspace + ".dll"));
						}
					}
					GXLogging.Debug(log, "nmspace:"  + nmspace);

					if (ass==null)
					{
						errCod = 99; 
						errDescription = previousMsgError + "Error Loading GemBox.ExcelLite.dll, GemBox.Spreadsheet.dll";
						return errCod;
					}
					else if (license!=null)
					{
						try
						{
							GxExcelUtils.InvokeStatic(ass, nmspace + ".SpreadsheetInfo", "SetLicense", new object[]{license});
						}
						catch(Exception e)
						{
							GXLogging.Error(log, @"Error setting license.", e);
						}
					}
				}
				return 0;
			}							
			catch (FileNotFoundException fe)
			{
				GXLogging.Error(log, "Error Loading " + fe.FileName, fe);
				errCod = 99; 
				errDescription = fe.Message;
				return errCod;
			}
		}

		private Assembly loadAssembly(string fileName)
		{
			if (File.Exists(fileName))
			{
				GXLogging.Debug(log, "Load Excel API: " + fileName);
				ass = Assembly.LoadFrom(fileName);
				return ass;				
			}
			else
			{
				return null;
			}

		}
		public short Open(String fileName)
		{
			try
			{
				GXLogging.Debug(log, "GetType "  +nmspace + ".ExcelFile");
				Type classType = ass.GetType( nmspace + ".ExcelFile", false, true);
				ef = Activator.CreateInstance(classType);

                GxFile file = new GxFile(Path.GetDirectoryName(fileName), fileName, GxFileType.Private);
				if (!String.IsNullOrEmpty(template))
				{
                    GxFile templateFile = new GxFile(Path.GetDirectoryName(template), template);
					if (templateFile.Exists())
					{
						GXLogging.Debug(log,"Opening Template " + template);
                        var stream = templateFile.GetStream();
						if (stream != null)
						{
							stream.Position = 0;
							GxExcelUtils.Invoke(ef, "LoadXls", new object[] { stream });
						}
                    }
					else
					{
						errCod = 4; 
						errDescription = "Invalid template.";
						return errCod;
					}
				}
				else if (file.Exists())
				{
                    var stream = file.GetStream();
					if (stream != null)
					{
						stream.Position = 0;
						GxExcelUtils.Invoke(ef, "LoadXls", new object[] { stream });
					}
				}
				else
				{
					object worksheets=  GxExcelUtils.GetPropValue(ef, "Worksheets");

					object ws = GxExcelUtils.Invoke(worksheets, "Add", new object[]{"Sheet1"});
					GxExcelUtils.SetPropValue(worksheets, "ActiveWorksheet", ws);
				}
				xlsFileName = fileName;
			}
			catch (Exception e)
			{
				GXLogging.Error(log,"Error opening " + fileName, e);
				errCod = 10; 
				errDescription = "Could not open file." + e.Message + (e.InnerException!=null ? e.InnerException.Message:"");
				return errCod;
			}
			return 0;
		}

		public short Save()
		{
			try
			{
                GxFile file = new GxFile(Path.GetDirectoryName(xlsFileName), xlsFileName, GxFileType.Private);
                MemoryStream content = new MemoryStream();
				GxExcelUtils.Invoke(ef, "SaveXls", new object[]{content});
                content.Position = 0;
                file.Create(content);
			}
			catch (Exception e)
			{
				GXLogging.Error(log,"Error saving " + xlsFileName, e);
				errCod = 12; 
				errDescription = "Could not save file." + e.Message;
				return -1;
			}
			return 0;
		}

		public short Close()
		{
			Save();
			return 0;
		}

        public void SetDateFormat(string dFormat)
        {

        }
		public void selectSheet(String sheetName) 
		{ 
			short err = SelectSheet(sheetName);
		}

		public short SelectSheet(String sheetName)
		{
			if (sheetName != null && sheetName.CompareTo("") != 0)
			{
				IEnumerable worksheets=  (IEnumerable)GxExcelUtils.GetPropValue(ef, "Worksheets");
				foreach (object ews in worksheets)
				{
					if (((string)GxExcelUtils.GetPropValue(ews, "Name")).Equals(sheetName))
					{
						GxExcelUtils.SetPropValue(worksheets, "ActiveWorksheet", ews);
						return 0;
					}
				}
			
				object ws = GxExcelUtils.Invoke(worksheets, "Add", new object[]{sheetName});
				GxExcelUtils.SetPropValue(worksheets, "ActiveWorksheet", ws);

				return 0;
			}
			else
			{
				this.errCod = 5;
				this.errDescription = "Invalid worksheet name";
				return errCod;
			}
		}

		public short RenameSheet(String sheetName)
		{
			GxExcelUtils.SetPropValue(GxExcelUtils.GetPropValue(GxExcelUtils.GetPropValue(ef, "Worksheets"), "ActiveWorksheet"), "Name", sheetName);
			return 0;
		}

        public void CalculateFormulas()
        {
            throw new NotImplementedException();
        }

		public short Clear()
		{
			IEnumerable rows=  (IEnumerable)GxExcelUtils.GetPropValue(GxExcelUtils.GetPropValue(GxExcelUtils.GetPropValue(ef, "Worksheets"),"ActiveWorksheet"), "Rows");

			ArrayList toDelete = new ArrayList();
			foreach(object er in rows)
			{
				toDelete.Add(er);
			}
			for(int i=toDelete.Count-1; i>=0; i--)
			{
				GxExcelUtils.Invoke(toDelete[i], "Delete", null);
			}

			return 0;
		}

		public IExcelCells Cells(int Row, int Col)
		{
			return new ExcelCells((IGxError)this, ef, Row -1, Col -1, 0, 0);
		}
		
		public IExcelCells Cells(int Row, int Col, int Height, int Width)
		{
			return new ExcelCells((IGxError)this, ef, Row -1, Col -1, Height, Width);
		}

		public IExcelCells get_Cells(int Row, int Col, int Height, int Width)
		{
			return new ExcelCells((IGxError)this, ef, Row -1, Col -1, Height -1, Width -1);
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

		public void setErrCod(short EerrCod)
		{
			this.errCod = EerrCod;
		}
		public void setErrDes(String EerrDes)
		{
			this.errDescription = EerrDes;
		}

		public short ReadOnly
		{
			get { return readOnly; }
			set { readOnly = value; }
		}
		public short Show() 
		{ 
			try
			{
				ExcelUtils.Show(xlsFileName);
				return 0;
			}
			catch(Exception e)
			{
				GXLogging.Error(log,"Show Error" ,e);
				errCod = 10; 
                errDescription = "Could not open file." + e.Message + (e.InnerException != null ? e.InnerException.Message : "");
				return errCod;
			}

		}

        public short AddRows(int rowIdx, int rows)
        {
            throw new NotImplementedException();
        }

		public String Template 
		{ 
			get { return template; } 
			set {template=value; } 
		}

		public short AutoFit 
		{ 
			get { return autoFit?(short)1:(short)0; } 
			set {autoFit=(value==1); } 
		}

		private bool autoFit=false;

		public void cleanup() {}
		public short UnBind() { return -1; }
		public short Hide() { return -1; }
		public short PrintOut(short preview) { return -1; }
		public short ErrDisplay { get { return -1; } set { } }
		public String DefaultPath { get { return ""; } set { } }
		public String Delimiter { get { return ""; } set { } }

		public short RunMacro(string Macro, object Arg1, object Arg2, object Arg3, object Arg4, object Arg5, object Arg6, object Arg7, object Arg8, object Arg9, object Arg10, object Arg11, object Arg12, object Arg13, object Arg14, object Arg15, object Arg16, object Arg17, object Arg18, object Arg19, object Arg20, object Arg21, object Arg22, object Arg23, object Arg24, object Arg25, object Arg26, object Arg27, object Arg28, object Arg29, object Arg30) { return 0; }
		public object MacroReturnValue { get {return null;} }
		public object Workbook { get{ return null;} }
		public string MacroReturnText { get{return null;} }
		public double MacroReturnNumber { get { return 0; } }
		public DateTime MacroReturnDate { get { return DateTime.MinValue; } }

		private short errCod = 0;
		private String errDescription = "OK";
		private object ef;
		private String xlsFileName;
		private short readOnly;
		private String template="";

	}

}
