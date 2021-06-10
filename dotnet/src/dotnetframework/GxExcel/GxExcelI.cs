using System;
using System.Reflection;
using log4net;
using System.Diagnostics;
using System.IO;
using GeneXus.Application;
using GeneXus.Utils;
using GeneXus.Services;
using GeneXus.Office.Excel;

namespace GeneXus.Office
{
    public class ExcelDocumentI
    {
        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public short Index = -1;
		
        private string fileName;
        [Obsolete("It is here for backward compatibility", false)]
        public void setDefaultUseAutomation(short useAutomation) { }

        [Obsolete("It is here for backward compatibility", false)]
        public static short getDefaultUseAutomation() { return 0; }

        [Obsolete("It is here for backward compatibility", false)]
        public short Useautomation
        {
            get { return 0; }
            set { }
        }

        bool readOnly = false;

        public short ReadOnly
        {
            get { return (short)(readOnly ? 1 : 0); }
            set { this.readOnly = (value != 0) ? true : false; }
        }

        IExcelDocument document;

        public object Document
        {
            get
            {
                return document;
            }

        }
        public bool checkExcelDocument()
        {
            if (Document == null)
            {
#if !NETCORE
                if (this.fileName.EndsWith(Constants.EXCEL2003Extension) || this.template.EndsWith(Constants.EXCEL2003ExtensionTemplate))
                {
                    if (document == null || document.ErrCode == 99)
                    {
                        GXLogging.Debug(log,"GeneXus.Office.ExcelLite.ExcelDocument");
                        string initErrDesc = (document != null && document.ErrCode == 99) ? document.ErrDescription : "";
                        document = new GeneXus.Office.ExcelLite.ExcelDocument();
                        document.ReadOnly = ReadOnly;
                        document.Init(initErrDesc);
                    }
                    if (document == null || document.ErrCode != 0) //Automation
                    {
                        GXLogging.Debug(log,"Interop.GXOFFICE2Lib.ExcelDocumentClass");
                        document = new IExcelDocumentWrapper(new Interop.GXOFFICE2Lib.ExcelDocumentClass());
                        document.ReadOnly = ReadOnly;
                        document.Init("");
                    }
			}
			else
#endif
				{
					document = new GeneXus.Office.ExcelGXEPPlus.ExcelDocument();
                    document.ReadOnly = ReadOnly;
                }

                if (!String.IsNullOrEmpty(defPath))
                    setDefaultPath(defPath);
                if (!String.IsNullOrEmpty(template))
                    Template = template;
                ErrDisplay = errDisplay;
                if (!String.IsNullOrEmpty(delimiter))
                    Delimiter = delimiter;
                AutoFit = autoFit;
            }

            return (document != null && document.ErrCode != 99);
        }

        String defPath = "";
        public void setDefaultPath(String p1)
        {
            defPath = p1;
            if (document != null)
            {
                document.DefaultPath = p1;
            }
        }

        public String getDefaultPath()
        {
            if (document != null) return document.DefaultPath;
            else return defPath;

        }

        public void SetDateFormat(IGxContext ctx, int lenDate, int lenTime, int ampmFmt, int dateFmt,
            String dSep, String tSep, String dtSep)
        {
            string date = ctx.localUtil.DTUtil.DateFormatFromSize(lenDate, dateFmt, dSep);
            string time = ctx.localUtil.DTUtil.TimeFormatFromSize(lenTime, ampmFmt, tSep);
            string sep = (lenDate > 0 && lenTime > 0 ? dtSep : "");
            String dateFormat = date + sep + time.Replace("tt", "AM/PM");
            document.SetDateFormat(dateFormat);
        }

        String template = "";

        public String Template
        {
            get
            {
                if (document != null)
                    return document.Template;
                else
                    return template;
            }
            set
            {
                bool storageEnabled = GXServices.Instance != null && GXServices.Instance.Get(GXServices.STORAGE_SERVICE) != null;
                template = value;
                if (storageEnabled)
                {                    
                    if (Path.IsPathRooted(value))
                        template = Path.GetFileName(value);
                    GxFile templatefile = new GxFile("", template);
                    if (!templatefile.Exists())
                    {
                        GXLogging.Warn(log, "Uploading template to storage as " + template);
                        string localTemplate = value;
                        if (!Path.IsPathRooted(localTemplate))
                            localTemplate = Path.Combine(GxContext.StaticPhysicalPath(), localTemplate);
                        ServiceFactory.GetExternalProvider().Upload(localTemplate, template, GxFileType.PublicRead);
                    }
                }
                else if (!Path.IsPathRooted(value))
                {
                    try
                    {
                        template = Path.Combine(GxContext.StaticPhysicalPath(), value);
                    }
                    catch (Exception e)
                    {
                        GXLogging.Warn(log, "Setting Rooted Path on " + value, e);
                    }
                }
                if (document != null)
                    document.Template = template;
            }
        }

        short errDisplay = 0;
        public short ErrDisplay
        {
            get
            {
                if (document != null)
                    return document.ErrDisplay;
                else return errDisplay;
            }
            set
            {
                errDisplay = value;
                if (document != null)
                    document.ErrDisplay = value;
            }
        }

        String delimiter = "";

        public String Delimiter
        {
            get
            {
                if (document != null)
                    return document.Delimiter;
                else return delimiter;
            }

            set
            {
                delimiter = value;
                if (document != null)
                    document.Delimiter = value;

            }
        }

        short autoFit = 0;
        public short AutoFit
        {

            get
            {

                if (document != null)
                    return document.AutoFit;
                else return autoFit;
            }
            set
            {
                autoFit = value;
                if (document != null)
                    document.AutoFit = value;
            }
        }

        public short Open(String xlName)
        {
            this.fileName = xlName;
            if (document != null && closed)
            {                				
                document = null;
            }
            if (!string.IsNullOrEmpty(xlName) && checkExcelDocument())
            {
                closed = false;
                try
                {
                    if (GXServices.Instance == null || GXServices.Instance.Get(GXServices.STORAGE_SERVICE) == null)
                    {
                        if (!Path.IsPathRooted(this.fileName))
                            this.fileName = Path.Combine(GxContext.StaticPhysicalPath(), this.fileName);
                    }
                    else
                    {
                        this.fileName = this.fileName.Replace("\\", "/");
                    }
                }
                catch (Exception e)
                {
                    GXLogging.Warn(log, "Setting Rooted Path on " + this.fileName, e);
                }
                return document.Open(this.fileName);
            }
            else
            {
                return document.ErrCode;
            }
        }

        public short Show()
        {
            return document != null ? (short)document.Show() : (short)-1;
        }

        public bool closed = false;
        public short Close()
        {
            if (document != null && !closed)
            {
                short ret = document.Close();
                if (ret == 0)
                {
                    closed = true;
                }
                return ret;
            }
            return 0;
        }

        public void CalculateFormulas()
        {
            document.CalculateFormulas();
        }

        public short Unbind()
        {
            if (document != null)
                return (short)document.UnBind();
            else return (short)-1;
        }

        public short Save()
        {
            if (document != null)
            {
                short result = (short)document.Save();
                if (result == 0)
                    closed = true;
                return result;
            }
            else return (short)-1;

        }
        public short Hide()
        {
            if (document != null)
                return (short)document.Hide();
            else return (short)-1;
        }
        public short Clear()
        {
            if (document != null)
                return (short)document.Clear();
            else return (short)-1;
        }
        public IExcelCells get_Cells(int Row, int Col, int Height, int Width)
        {
            checkExcelDocument();
            return document.get_Cells(Row, Col, Height, Width);
        }

        public IExcelCells get_Cells(int Row, int Col)
        {
            return get_Cells(Row, Col, 1, 1);
        }

        public short PrintOut()
        {
            return PrintOut((short)0);
        }

        public short PrintOut(short Preview)
        {
            if (document != null)
                return (short)document.PrintOut(Preview);
            else return (short)-1;
        }
        public short SelectSheet(String Sheet)
        {
            if (document != null)
                return (short)document.SelectSheet(Sheet);
            else return (short)-1;
        }
        public short RenameSheet(String SheetName)
        {
            if (document != null)
                return (short)document.RenameSheet(SheetName);
            else return (short)-1;
        }

        public short AddRows(int rowIdx, int rows)
        {
            if (document != null)
                return (short)document.AddRows(rowIdx, rows);
            else return (short)-1;
        }

        public short ErrCode
        {
            get
            {
                if (document != null)
                    return (short)document.ErrCode;
                else return (short)-1;
            }
        }
        public String ErrDescription
        {
            get
            {
                if (document != null)
                    return document.ErrDescription;
                else return "";
            }
        }

        public short RunMacro(string Macro, object Arg1, object Arg2, object Arg3, object Arg4, object Arg5, object Arg6, object Arg7, object Arg8, object Arg9, object Arg10, object Arg11, object Arg12, object Arg13, object Arg14, object Arg15, object Arg16, object Arg17, object Arg18, object Arg19, object Arg20, object Arg21, object Arg22, object Arg23, object Arg24, object Arg25, object Arg26, object Arg27, object Arg28, object Arg29, object Arg30)
        {
            return 0;
        }

        public object getMacroReturnValue() { return null; }

        public Object Workbook { get { return null; } }

        public void cleanup()
        {

            if (document != null)
            {
                document.Clear();
            }
        }
    }

    public class GxExcelUtils
    {
        public static object GetPropValue(object instante, string property)
        {
            PropertyInfo prop = instante.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            return prop.GetValue(instante, null);
        }
        public static object GetPropValue(object instante, string property, object[] index)
        {
            PropertyInfo prop = instante.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            return prop.GetValue(instante, index);
        }
        public static object GetConstantValue(Assembly ass, string className, string field)
        {
            Type prn1 = ass.GetType(className);
            FieldInfo f = prn1.GetField(field);
            return f.GetValue(null);
        }

        public static void SetPropValue(object instante, string property, object value)
        {
            PropertyInfo prop = instante.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instante, value, null);
        }

        public static object Invoke(object instance, string methodName, object[] parms)
        {
            return instance.GetType().InvokeMember(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, instance, parms);
        }
        public static object InvokeStatic(Assembly ass, string className, string methodName, object[] parms)
        {
            return ass.GetType(className, false, true).InvokeMember(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                null, null, parms);
        }
        public static object GetEnumValue(Assembly ass, string enumSuperClass, string enumName, string enumField)
        {
            Type prn1 = ass.GetType(enumSuperClass);
            Type e = prn1.GetNestedType(enumName, BindingFlags.Public);
            FieldInfo fi = e.GetField(enumField);
            return fi.GetValue(null);

        }

    }
    public interface IGxError
    {
        void setErrCod(short errCod);
        void setErrDes(String errDes);
    }
    public interface IExcelCells
    {
        DateTime Date
        {
            get;
            set;
        }
        double Number
        {
            get;
            set;
        }
        string Text
        {
            get;
            set;
        }
        string Font
        {
            get;
            set;
        }
        int Color
        {
            get;
            set;
        }

        int BackColor
        {
            get;
            set;
        }

        double Size
        {
            get;
            set;
        }

        string Type
        {
            get;
        }

        short Bold
        {
            get;
            set;
        }
        short Italic
        {
            get;
            set;
        }
        short Underline
        {
            get;
            set;
        }
        string Value
        {
            get;
        }
    }
    public interface IExcelDocument
    {

        short Init(string previousMsgError);

        short Open(String xlName);

        short Show();

        short Close();

        short UnBind();

        short Save();

        short Hide();

        short Clear();

        IExcelCells get_Cells(int Row, int Col, int Height, int Width);

        short PrintOut(short Preview);

        short SelectSheet(String Sheet);

        short RenameSheet(String SheetName);

        void SetDateFormat(string dFormat);

        short ErrCode
        {
            get;
        }
        string ErrDescription
        {
            get;
        }
        short ErrDisplay
        {
            get;
            set;
        }

        String DefaultPath { get; set; }

        String Template { get; set; }

        String Delimiter { get; set; }
        short ReadOnly
        {
            get;
            set;
        }

        short AutoFit
        {
            get;
            set;
        }

        void cleanup();

        short AddRows(int rowIdx, int rows);

        void CalculateFormulas();
    }

#if !NETCORE
    public class IExcelCellWrapper : IExcelCells
    {
        Interop.GXOFFICE2Lib.ExcelCells cell;
        public IExcelCellWrapper(Interop.GXOFFICE2Lib.ExcelCells cell)
        {
            this.cell = cell;
        }

        #region IExcelCells Members

        public DateTime Date
        {
            get
            {
                
                return cell.Date;
            }
            set
            {
                cell.Date = value;
            }
        }

        public double Number
        {
            get
            {
                return cell.Number;
            }
            set
            {
                cell.Number = value;
            }
        }

        public string Text
        {
            get
            {
                return cell.Text;
            }
            set
            {
                cell.Text = value;
            }
        }

        public string Value
        {
            get
            {
                return cell.Text;
            }
            set
            {
                cell.Text = value;
            }
        }

        public string Font
        {
            get
            {
                return cell.Font;
            }
            set
            {
                cell.Font = value;
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

            }
        }

        public int Color
        {
            get
            {
                return cell.Color;
            }
            set
            {
                cell.Color = value;
            }
        }

        public double Size
        {
            get
            {
                return cell.Size;
            }
            set
            {
                cell.Size = value;
            }
        }

        public string Type
        {
            get
            {
                return cell.Type;
            }
        }

        public short Bold
        {
            get
            {
                return cell.Bold;
            }
            set
            {
                cell.Bold = value;
            }
        }

        public short Italic
        {
            get
            {
                return cell.Italic;
            }
            set
            {
                cell.Italic = value;
            }
        }

        public short Underline
        {
            get
            {
                return cell.Underline;
            }
            set
            {
                cell.Underline = value;
            }
        }

        #endregion

    }

    public class IExcelDocumentWrapper : IExcelDocument
    {
        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Interop.GXOFFICE2Lib.IExcelDocument doc;

        public IExcelDocumentWrapper(Interop.GXOFFICE2Lib.IExcelDocument document)
        {
            doc = document;
        }
        #region IExcelDocument Members

        public short Init(string previousMsgError)
        {
            GXLogging.Debug(log,"Load Excel API: Interop.GXOFFICE2Lib.ExcelDocumentClass");
            return 0;
        }
        public short Open(String xlName)
        {
            
            return doc.Open(xlName);
        }

        public short Show()
        {
            
            return doc.Show();
        }

        public short Close()
        {
            
            return doc.Close();
        }
        public void SetDateFormat(string dFormat)
        {

        }

        public short UnBind()
        {
            
            return doc.UnBind();
        }

        public short Save()
        {
            
            return doc.Save();
        }

        public short Hide()
        {
            
            return doc.Hide();
        }

        public short Clear()
        {
            
            return doc.Clear();
        }

        public IExcelCells get_Cells(int Row, int Col, int Height, int Width)
        {
            
            return new IExcelCellWrapper(doc.get_Cells(Row, Col, Height, Width));
        }

        public short PrintOut(short Preview)
        {
            
            return doc.PrintOut(Preview);
        }

        public short SelectSheet(String Sheet)
        {
            
            return doc.SelectSheet(Sheet);
        }

        public short RenameSheet(String SheetName)
        {
            
            return doc.RenameSheet(SheetName);
        }

        public short ErrCode
        {
            get
            {
                
                return doc.ErrCode;
            }
        }

        public string ErrDescription
        {
            get
            {
                
                return doc.ErrDescription;
            }
        }

        public short ErrDisplay
        {
            get
            {
                
                return doc.ErrDisplay;
            }
            set
            {
                doc.ErrDisplay = value;
            }
        }

        public String DefaultPath
        {
            get
            {
                return doc.DefaultPath;
            }
            set
            {
                doc.DefaultPath = value;
            }
        }

        public String Template
        {
            get
            {
                return doc.Template;
            }
            set
            {
                doc.Template = value;
            }
        }

        public String Delimiter
        {
            get
            {
                return doc.Delimiter;
            }
            set
            {
                doc.Delimiter = value;
            }
        }

        public short ReadOnly
        {
            get
            {
                return doc.ReadOnly;
            }
            set
            {
                doc.ReadOnly = value;
            }
        }

        public short AutoFit
        {
            get
            {
                return doc.AutoFit;
            }
            set
            {
                doc.AutoFit = value;
            }
        }

        public void cleanup()
        {
        }

        public short AddRows(int rowIdx, int rows)
        {
            throw new NotImplementedException();
        }

        public void CalculateFormulas()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
#endif
	public class ExcelUtils
    {
        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Show(string xlsFileName)
        {
            Process p = new Process();
            p.StartInfo.FileName = "excel";
            p.StartInfo.Arguments = xlsFileName;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = true;

            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(xlsFileName);
            GXLogging.Debug(log,"Show FileName:'" + p.StartInfo.FileName + "',Arguments:'" + p.StartInfo.Arguments + "'");
            GXLogging.Debug(log,"Show Working directory:'" + p.StartInfo.WorkingDirectory + "'");
            bool res = p.Start();
        }

    }
}
