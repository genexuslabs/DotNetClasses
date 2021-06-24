using System;
using System.IO;
using System.Collections;
using GeneXus.Utils;
using GeneXus.Application;
using log4net;
using System.Runtime.CompilerServices;
using GeneXus.Services;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
#if !NETCORE
using System.Xml.Xsl;
using System.Xml.XPath;
#endif
using System.Reflection;
using GeneXus;
using GeneXus.Storage;
using GeneXus.Attributes;

public interface IGxDirectoryInfo
{
    string Name { get; }
    string FullName { get; }
    void Delete();
    bool Exists { get; }
    void Create();
    IGxFileInfo[] GetFiles(string searchPattern);
    IGxDirectoryInfo[] GetDirectories();
    void MoveTo(string desDirName);
}
public interface IGxFileInfo
{
    string Name { get; }
    string FullName { get; }
    string Path { get; }
	string AbsolutePath { get; }
	long Length { get; }
    DateTime LastWriteTime { get; }
    string DirectoryName { get; }
    string Source { get; set; }
    IGxDirectoryInfo Directory { get; }
    FileStream Create();
    string Create(Stream stream);
    IGxFileInfo CopyTo(string filename, bool overwrite);
    void MoveTo(string filename);
    bool Exist(string filename);
    void Delete();
    bool Exists { get; }
    bool PathIsRooted { get; }
    Stream GetStream();
	bool IsExternalFile { get; }
	string Separator { get; }

}
public class GxDirectoryInfo : IGxDirectoryInfo
{
    DirectoryInfo _directory;
    public GxDirectoryInfo()
    {
    }
    public GxDirectoryInfo(DirectoryInfo directory)
    {
        _directory = directory;
    }
    public GxDirectoryInfo(string directory)
    {
        _directory = new DirectoryInfo(directory);
    }

    public DirectoryInfo DirectoryInfo
    {
        get { return _directory; }
        set { _directory = value; }
    }
    public bool Exists
    {
        get
        {
            return _directory.Exists;
        }
    }

    public string FullName
    {
        get
        {
            return _directory.FullName;
        }
    }

    public string Name
    {
        get
        {
            return _directory.Name;
        }
    }

    public void Create()
    {
        _directory.Create();
    }

    public void Delete()
    {
        _directory.Delete(true);
    }

    public IGxDirectoryInfo[] GetDirectories()
    {
		return _directory.GetDirectories().Select(elem => new GxDirectoryInfo(elem)).ToArray();
    }

    public IGxFileInfo[] GetFiles(string searchPattern)
    {
		return _directory.GetFiles(searchPattern).Select(elem => new GxFileInfo(elem)).ToArray();
    }

    public void MoveTo(string desDirName)
    {
        _directory.MoveTo(desDirName);
    }
}
public class GxExternalDirectoryInfo : IGxDirectoryInfo
{
    string _name;
    string _path;
    ExternalProvider _provider;
    public GxExternalDirectoryInfo()
    {
    }
    public GxExternalDirectoryInfo(string storageObjectFullname, string path, ExternalProvider provider)
    {
        _name = NormalizeName(storageObjectFullname);
        _provider = provider;
        _path = path;
    }
    public GxExternalDirectoryInfo(string storageObjectFullname, ExternalProvider provider)
    {
        _name = NormalizeName(storageObjectFullname);
        _provider = provider;
        _path = "";
    }

    public string NormalizeName(string name)
    {
        string[] parts = name.Split('\\');
        name = "";
        foreach (string part in parts)
            if (!String.IsNullOrEmpty(part))
                name += part + "/";
        if (name.EndsWith("//"))
            name = name.Remove(name.Length - 1, 1);
		if (name.EndsWith("/"))
			name = name.Substring(0, name.Length - 1);
		return name;
    }

    public bool Exists
    {
        get
        {
            return _provider != null && _provider.ExistsDirectory(_name);
        }
    }

    public string FullName
    {
        get
        {
            if (String.IsNullOrEmpty(_path))
                _path = _provider.GetDirectory(_name);
            return _path;
        }
    }

    public string Name
    {
        get
        {
            return _name;
        }
    }

    public void Create()
    {
        if (_provider != null)
            _provider.CreateDirectory(_name);
    }

    public void Delete()
    {
        if (_provider != null)
            _provider.DeleteDirectory(_name);
    }

    public IGxDirectoryInfo[] GetDirectories()
    {
        List<string> dirs = _provider.GetSubDirectories(_name);
		
		GxExternalDirectoryInfo[] externalDirs = dirs.Select(elem => new GxExternalDirectoryInfo(elem, elem, _provider)).ToArray();
        return externalDirs;
    }

    public IGxFileInfo[] GetFiles(string searchPattern)
    {
        if (searchPattern.Equals("*.*"))
            searchPattern = "";
        List<string> files = _provider.GetFiles(_name, searchPattern);
		
		GxExternalFileInfo[] externalFiles = files.Select(elem => new GxExternalFileInfo(elem, _provider, GxFileType.Default)).ToArray();
        return externalFiles;
    }

    public void MoveTo(string desDirName)
    {
        if (_provider != null)
        {
            _provider.RenameDirectory(_name, desDirName);
        }
    }
}
public class GxFileInfo : IGxFileInfo
{
    FileInfo _file;
    string _baseDirectory;

    public GxFileInfo(FileInfo file)
    {
        _file = file;
    }
    public GxFileInfo(string fileName, string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        _file = new FileInfo(FileUtil.NormalizeSource(fileName, _baseDirectory));
    }
    public GxFileInfo(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }
    public string DirectoryName
    {
        get
        {
		return _file.DirectoryName;
        }
    }

    public string FullName
    {
        get
        {
		return _file.FullName;
        }
    }

    public DateTime LastWriteTime
    {
        get
        {
            return _file.LastWriteTime;
        }
    }

    public long Length
    {
        get
        {
            return _file.Length;
        }
    }

    public string Name
    {
        get
        {
            return _file.Name;
        }
    }

    public string Source
    {
        get
        {
            return _file.Name;
        }
        set
		{
			
			string _source = FileUtil.NormalizeSource(value, _baseDirectory);
			Uri uri = null;
			if (Uri.TryCreate(_source, UriKind.Absolute, out uri) && uri.IsFile)
			{
				_source = uri.LocalPath;
			}
			_file = new FileInfo(_source);
		}
	}

	public string Separator
	{
		get
		{
			return System.IO.Path.DirectorySeparatorChar.ToString();
		}
	}

    public IGxFileInfo CopyTo(string filename, bool overwrite)
    {
        filename = FileUtil.NormalizeSource(filename, _baseDirectory);

		FileInfo targetFile = new FileInfo(filename);
		if (!targetFile.Directory.Exists)
			targetFile.Directory.Create();

		return new GxFileInfo(_file.CopyTo(filename, overwrite));
    }

    public FileStream Create()
    {
        return _file.Create();
    }

    public string Create(Stream data)
    {
		using (data)
		{
			using (FileStream stream = _file.Create())
			{
				data.CopyTo(stream);
			}
		}
        return FileUtil.NormalizeSource(_file.Name, _baseDirectory);
    }

    public void Delete()
    {
        _file.Delete();
    }

    public bool Exists
    {
        get
        {
            return File.Exists(_file.FullName);
        }
    }

    public IGxDirectoryInfo Directory
    {
        get
        {
            return new GxDirectoryInfo(_file.Directory);
        }
    }

    public bool PathIsRooted
    {
        get
        {
            return System.IO.Path.IsPathRooted(_file.Name);
        }
    }
	public bool IsExternalFile
	{
		get
		{
			return false;
		}
	}

	public string Path
    {
        get
        {
            if (String.IsNullOrEmpty(_file.FullName))
                return "";
            else
                return System.IO.Path.GetDirectoryName(_file.FullName);
        }
    }
	public string AbsolutePath
	{
		get
		{
			return _file.FullName;
		}
	}

	public void MoveTo(string filename)
    {
        filename = FileUtil.NormalizeSource(filename, _baseDirectory);

        _file.MoveTo(filename);
    }

    public bool Exist(string name)
    {
        return File.Exists(name);
    }

    public Stream GetStream()
    {
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
        return File.OpenRead(FullName);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
    }
}
public class GxExternalFileInfo : IGxFileInfo
{
    private string _name;
	private ExternalProvider _provider;
    private string _url;	
	private GxFileType _fileTypeAtt = GxFileType.Private;

	public GxExternalFileInfo(ExternalProvider provider)
    {
        _provider = provider;
        _name = "";
        _url = "";
    }

    public GxExternalFileInfo(string storageObjectFullname, ExternalProvider provider, GxFileType fileType)
    {
		storageObjectFullname = storageObjectFullname!=null ? storageObjectFullname.Replace('\\', '/') : storageObjectFullname;
		_name = storageObjectFullname;
        _provider = provider;
		Uri result;
		if (Uri.TryCreate(storageObjectFullname, UriKind.Absolute, out result) && result.IsAbsoluteUri)
		{
			_url = storageObjectFullname;
		}
		_fileTypeAtt = fileType;
	}

    public GxExternalFileInfo(string storageObjectFullname, string url, ExternalProvider provider, GxFileType fileType = GxFileType.Private)
    {				
        _name = StorageFactory.GetProviderObjectAbsoluteUriSafe(provider, storageObjectFullname);
		_provider = provider;
        _url = url;
		_fileTypeAtt = fileType;
	}

	public GxFileType FileTypeAtt
	{
		get {
			return _fileTypeAtt;
		}
	}

	public string DirectoryName
    {
        get
        {
            return this.Directory.Name;
        }
    }

    public string FullName
    {
        get
        {
            return _name;
        }
    }

	public string Separator
	{
		get
		{
			return StorageUtils.DELIMITER;
		}
	}

    public DateTime LastWriteTime
    {
        get
        {
            if (_provider != null)
                return _provider.GetLastModified(_name, FileTypeAtt);
            else
                return DateTimeUtil.nullDate;
        }
    }

    public long Length
    {
        get
        {
            if (_provider != null)
                return _provider.GetLength(_name, FileTypeAtt);
            else
                return 0;
        }
    }

    public string Name
    {
        get
        {
            return System.IO.Path.GetFileName(_name);
        }
    }

    public string Source
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }


    public IGxDirectoryInfo Directory
    {
        get
        {
            if (_name.Contains("/"))
                return new GxExternalDirectoryInfo(_name.Substring(0, _name.LastIndexOf("/")), _provider);
            else
                return new GxExternalDirectoryInfo();
        }
    }

    public bool Exists
    {
        get
        {
            return _provider != null && _provider.Exists(_name, FileTypeAtt);
        }
    }

    public bool PathIsRooted
    {
        get
        {
            return true;
        }
    }
	public bool IsExternalFile
	{
		get
		{
			return true;
		}
	}
	public string Path
    {
        get
        {
			return System.IO.Path.GetDirectoryName(FullName);
		}
    }

	public string AbsolutePath
	{
		get
		{
			return URL;
		}
	}

	private string URL
	{
		get {
			if (string.IsNullOrEmpty(_url))
			{
				_url = _provider.Get(_name, _fileTypeAtt, 0);
			}
			return _url;
		}
	}

	public IGxFileInfo CopyTo(string filename, bool overwrite)
    {
        if (_provider != null)
        {
            string newFileName = _provider.Copy(_name, FileTypeAtt, filename, FileTypeAtt);
            return new GxExternalFileInfo(filename, newFileName, _provider);
        }
        else
            return new GxExternalFileInfo(string.Empty, string.Empty, _provider);
    }

    public FileStream Create()
    {
        throw new NotImplementedException();
    }

    public string Create(Stream data)
    {
        if (_provider != null)
        {			
            _url = _provider.Upload(_name, data, FileTypeAtt);
            return _name;
        }
        return string.Empty;
    }
    public void Delete()
    {
        if (_provider != null)
        {
            _provider.Delete(_name, FileTypeAtt);
        }
    }

    public void MoveTo(string filename)
    {
        if (_provider != null)
        {
            _provider.Rename(_name, filename, FileTypeAtt);
        }
    }

    public bool Exist(string name)
    {
        return _provider != null && _provider.Exists(name, FileTypeAtt);
    }

    public Stream GetStream()
    {
        return _provider.GetStream(_name, FileTypeAtt);
    }
}

[Flags]
public enum GxFileType
{
	Default = 0,
	PublicRead = 1,
	Private = 2
}

public class GxFile
{

    private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxFile));

    internal IGxFileInfo _file;
    string _baseDirectory;
    int _lastError;
    string _lastErrorDescription;
	string _source;
	string _uploadFileId;
    public GxFile()
        : this(GxContext.StaticPhysicalPath())
    {
    }

    public GxFile(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        if (_baseDirectory.EndsWith("\\", StringComparison.Ordinal))
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.Length - 1);
    }

    public GxFile(string baseDirectory, IGxFileInfo file, GxFileType fileType = GxFileType.Private)
      : this(baseDirectory)
    {
        _file = file;
    }

    public GxFile(string baseDirectory, string fileName, GxFileType fileType = GxFileType.Private)
      : this(baseDirectory)
    {
		if (GxUploadHelper.IsUpload(fileName))
		{
			_uploadFileId = fileName;
			fileName = GxUploadHelper.UploadPath(fileName);
		}
		fileName = fileName.Trim();
		if ((GXServices.Instance != null && GXServices.Instance.Get(GXServices.STORAGE_SERVICE) != null) && !Path.IsPathRooted(fileName))
            _file = new GxExternalFileInfo(fileName, ServiceFactory.GetExternalProvider(), fileType);
        else
            _file = new GxFileInfo(fileName, baseDirectory);
    }

	public IGxFileInfo FileInfo
    {
        get { return _file; }
        set { _file = value; }
    }
    public string Separator
    {
        get
        {
			if (_file != null)
			{
				return _file.Separator;
			}
			else
			{
				return Path.DirectorySeparatorChar.ToString();
			}
        }
    }
    public string Source
    {
        get
        {
            if (validSource())
                return _file.Name;
            else
                return string.Empty;
        }
        set
        {
            try
            {

				if (value != _source)
				{
					if (String.IsNullOrEmpty(value.Trim()))
					{
						_source = value;
						_file = null;
					}
					else
					{
						_file = null;
						if (GxUploadHelper.IsUpload(value))
						{
							_uploadFileId = value;
							value = GxUploadHelper.UploadPath(value);
							ExternalProvider provider = ServiceFactory.GetExternalProvider();
							_file = (provider != null)? new GxExternalFileInfo(value, String.Empty, provider): _file;
						}

						if (_file == null)
						{
							if (IsAbsoluteUrl(value))
							{
								_file =new GxExternalFileInfo(value, value, ServiceFactory.GetExternalProvider());
							}
							else
							{
								_file = new GxFileInfo(_baseDirectory);
								_file.Source = value;
							}
						}

						_lastError = 0;
						_lastErrorDescription = string.Empty;
						_source = value;
					}
				}

			}
            catch (Exception e)
            {
                GXLogging.Error(log, "Error setting File Source '" + value + "'", e);
                throw;
            }
        }
    }

    private bool IsAbsoluteUrl(string url)
    {
        Uri result;
#if NETCORE
		return Uri.TryCreate(url, UriKind.Absolute, out result) && (result.Scheme.Equals("http",StringComparison.OrdinalIgnoreCase) || result.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase));
#else
		return Uri.TryCreate(url, UriKind.Absolute, out result) && (result.Scheme == GXUri.UriSchemeHttp || result.Scheme == GXUri.UriSchemeHttps);
#endif
    }
	public bool IsExternalFile
	{
		get
		{
			if (_file == null)
				return false;
			else
				return _file.IsExternalFile;
		}
	}
	public int ErrCode
    {
        get
        {
            return _lastError;
        }
    }
    public string ErrDescription
    {
        get
        {
            if (_lastErrorDescription.Trim().Length > 0)
                return _lastErrorDescription;
            switch (_lastError)
            {
                case 0:
                    return "Ok";
                case 1:
                    return "Invalid file instance";
                case 2:
                    return "File does not exist";
                case 3:
                    return "File already exist";
                case 100:
                    return "Security error";
            }
            return "Unknown error";
        }
    }
    public void Create()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return;
        try
        {
            if (Exists())
            {
                _lastError = 1;
                return;
            }
			using (FileStream fs = _file.Create())
			{
				
			}
        }
        catch (Exception e)
        {
            setError(e);
        }
    }

    public string Create(Stream data)
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return "";
        try
        {
            return _file.Create(data);
        }
        catch (Exception e)
        {
            setError(e);
        }
        return "";
    }
    public void Delete()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return;
        try
        {
            if (!Exists())
            {
                _lastError = 2;
                return;
            }
            _file.Delete();
        }
        catch (Exception e)
        {
            setError(e);
        }
    }
    public bool Exists()
    {
        if (!validSource())
            return false;
        _lastError = 0;
        try
        {
            return _file.Exists;
        }
        catch (Exception e)
        {
            setError(e);
        }
        return false;
    }

    public void Copy(string name)
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return;
        try
        {
            if (!Exists())
            {
                _lastError = 2;
                return;
            }
            _file.CopyTo(name, true);
        }
        catch (Exception e)
        {
            setError(e);
        }
    }
    public void Rename(string name)
    {

        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return;
        try
        {
            if (!Exists())
            {
                _lastError = 2;
                return;
            }
            if (_file.Exist(name))
            {
                _lastError = 3;
                return;
            }
            _file.MoveTo(name);
        }
        catch (Exception e)
        {
            setError(e);
        }
    }
    public string GetName()
    {
        _lastError = 0;
        _lastErrorDescription = "";
		if (!string.IsNullOrEmpty(_uploadFileId))
		{
			return GxUploadHelper.UploadName(_uploadFileId);
		}
		if (!validSource())
            return "";
        if (!Exists())
        {
            _lastError = 2;
            return "";
        }
        return _file.Name;
    }
    public string GetAbsoluteName()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return "";
        string fullname = _file.FullName;
        if(String.IsNullOrEmpty(fullname))
            _lastError = 2;
        return fullname;
    }

	[GXApi]
	public string GetURI()
	{
		if (!validSource())
			return "";
		return _file.AbsolutePath;
	}

    public string GetExtension()
    {
		if (!string.IsNullOrEmpty(_uploadFileId))
		{
			return GxUploadHelper.UploadExtension(_uploadFileId);
		}
		else if (this.HasExtension())
            return System.IO.Path.GetExtension(this.GetAbsoluteName()).Replace(".", "");
        else
            return string.Empty;
    }

    public long GetLength()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return 0;
        if (!Exists())
        {
            _lastError = 2;
            return 0;
        }
        try
        {
            return _file.Length;
        }
        catch (Exception e)
        {
            setError(e);
        }
        return 0;
    }
    public DateTime GetLastModified()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return DateTime.MinValue;
        if (!Exists())
        {
            _lastError = 2;
            return DateTime.MinValue;
        }
        try
        {
            return DateTimeUtil.ResetMilliseconds(_file.LastWriteTime);
        }
        catch (Exception e)
        {
            setError(e);
        }
        return new DateTime();
    }
    public Stream GetStream()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return new MemoryStream();
        if (!Exists())
        {
            _lastError = 2;
            return new MemoryStream();
        }
        try
        {
            return _file.GetStream();
        }
        catch (Exception e)
        {
            setError(e);
        }
        return new MemoryStream();
    }
    public bool PathIsRooted()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return false;
        try
        {
            return _file.PathIsRooted;
        }
        catch (Exception e)
        {
            setError(e);
        }
        return false;
    }
    bool validSource()
    {
        if (_file != null)
            return true;
        else
        {
            _lastError = 1;
            return false;
        }
    }
    void setError(Exception e)
    {
        if (e is UnauthorizedAccessException)
            _lastError = 100;
        else
            _lastError = -1;
        _lastErrorDescription = e.Message;
        GXLogging.Error(log, "GXFile General Error", e);
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        return (this.FileInfo.Name == ((GxFile)obj).FileInfo.Name);
    }

    public override int GetHashCode()
    {
        return this.FileInfo.Name.GetHashCode();
    }
    public string XsltApplyOld(string xslFileName)
    {
#if !NETCORE
		XslTransform xsltE = new XslTransform();
        XPathDocument xpdXml = new XPathDocument(_file.FullName);
        XPathDocument xpdXslt = new XPathDocument(FileUtil.NormalizeSource(xslFileName, _baseDirectory));
        xsltE.Load(xpdXslt, new XmlUrlResolver(), System.Reflection.Assembly.GetCallingAssembly().Evidence);
        StringWriter result = new StringWriter();
        xsltE.Transform(xpdXml, null, result, null);
        return result.ToString();
#else
		return string.Empty;
#endif
	}
    public string XsltApply(string xslFileName)
    {
#if !NETCORE
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.CheckCharacters = false;
        try
        {
            return GxXsltImpl.Apply(FileUtil.NormalizeSource(xslFileName, _baseDirectory), _file.FullName);
        }
        catch (Exception ex) //ArgumentException invalid characters in xml, XslLoadException An item of type 'Attribute' cannot be constructed within a node of type 'Root'.
        {
            GXLogging.Warn(log, "XsltApply Error", ex);
            return GxXsltImpl.ApplyOld(FileUtil.NormalizeSource(xslFileName, _baseDirectory), _file.FullName);
        }
#else
		return string.Empty;
#endif
	}
#if !NETCORE
	[MethodImpl(MethodImplOptions.Synchronized)]
    public string HtmlClean()
    {
        try
        {
            object rawDoc = Assembly.LoadFrom(GXUtil.ProcessorDependantAssembly("NTidy.dll")).CreateInstance("NTidy.TidyDocument");
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tidy.cfg")))
            {
                rawDoc.GetType().GetMethod("LoadConfig").Invoke(rawDoc, new object[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tidy.cfg") });
            }
            rawDoc.GetType().GetMethod("LoadFile").Invoke(rawDoc, new object[] { _file.FullName });
            rawDoc.GetType().GetMethod("CleanAndRepair").Invoke(rawDoc, null);
            String htmlcleaned = (string)rawDoc.GetType().GetMethod("ToString").Invoke(rawDoc, null);
            return htmlcleaned;
        }
        catch (Exception ex)
        {
            GXLogging.Error(log, "HTMLClean error, file:" + _file.FullName, ex);
            return "";
        }
    }
#endif
	public bool HasExtension()
    {
        return System.IO.Path.HasExtension(this.GetAbsoluteName());
    }
    public void SetExtension(string ext)
    {
        string FileName = this.GetAbsoluteName();
        if (string.IsNullOrWhiteSpace(FileName))
            return;
        if (FileUtil.GetFileType(FileName).Equals(ext, StringComparison.OrdinalIgnoreCase) || ext.Trim().Length == 0)
            _lastError = 0;
        else
        {
            string sFilePath = (_file == null) ? "" : _file.DirectoryName;
            Rename(Path.Combine(sFilePath, FileUtil.GetFileName(FileName) + "." + ext));
        }
    }
    public string GetPath()
    {
        return _file.Path;
    }
    public GxDirectory GetDirectory()
    {
        return new GxDirectory(_baseDirectory, _file.Directory);
    }
	public string ToBase64()
	{
		try
		{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			using (FileStream fs = new FileStream(GetAbsoluteName(), FileMode.Open, FileAccess.Read))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			{
				using (BinaryReader br = new BinaryReader(fs))
				{
					byte[] bContent = br.ReadBytes((int)fs.Length);
					return Convert.ToBase64String(bContent);
				}
			}
		}
		catch
		{
			return string.Empty;
		}
	}
	public bool FromBase64(string base64)
	{
		if (!validSource())
			return false;
		bool ok = true;		
		try
		{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			using (FileStream fs = new FileStream(_file.FullName, FileMode.Create, FileAccess.Write))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			{
				using (BinaryWriter bw = new BinaryWriter(fs))
				{
					byte[] bContent = Convert.FromBase64String(base64);
					bw.Write(bContent, 0, bContent.Length);
				}				
			}
		}
		catch (Exception e)
		{
			setError(e);
			ok = false;
		}
		
		return ok;
	}
    public byte[] ToByteArray()
    {
        try
        {
			byte[] bContent;
			if (_file != null)
			{
				using (Stream s = _file.GetStream())
				{
					s.Seek(0, SeekOrigin.Begin);
					using (BinaryReader br = new BinaryReader(s))
					{
						bContent = br.ReadBytes((int)s.Length);
					}
				}
			}
			else
			{				
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				using (FileStream s = new FileStream(GetAbsoluteName(), FileMode.Open, FileAccess.Read))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				{
					using (BinaryReader br = new BinaryReader(s))
					{
						bContent = br.ReadBytes((int)s.Length);
					}
				}
			}
			return bContent;
        }
        catch
        {
            return Array.Empty<byte>();
        }

    }
    public void FromByteArray(byte[] bContent)
    {
        if (!validSource())
            return;

        try
        {

#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			using (FileStream fs = new FileStream(_file.FullName, FileMode.Create, FileAccess.Write))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			{
				using (BinaryWriter bw = new BinaryWriter(fs))
				{
					bw.Write(bContent, 0, bContent.Length);
				}
			}
		}
        catch
        {
        }
    }

    public String ReadAllText(String encoding)
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return String.Empty;
        try
        {
            if (!Exists())
            {
                _lastError = 2;
                _lastErrorDescription = "File does not exist";
                return String.Empty;
            }
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            return File.ReadAllText(_file.FullName, GXUtil.GxIanaToNetEncoding(encoding, false));
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
        }
        catch (Exception e)
        {
            setError(e);
            return string.Empty;
        }
    }

    public GxSimpleCollection<string> ReadAllLines()
    {
        return ReadAllLines(string.Empty);
    }

    public GxSimpleCollection<string> ReadAllLines(String encoding)
    {
        GxSimpleCollection<string> strColl = new GxSimpleCollection<string>();
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return strColl;
        try
        {
            if (!Exists())
            {
                _lastError = 2;
                _lastErrorDescription = "File does not exist";
                return strColl;
            }
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            strColl.AddRange(File.ReadAllLines(_file.FullName, GXUtil.GxIanaToNetEncoding(encoding, false)));
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            return strColl;
        }
        catch (Exception e)
        {
            setError(e);
            return strColl;
        }
    }

    public void WriteAllText(String value, String encoding)
    {
        WriteAllText(value, encoding, false);
    }

    public void WriteAllText(String value, String encoding, bool append)
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (validSource())
        {
            try
            {
                if (append)
                    File.AppendAllText(_file.FullName, value, GXUtil.GxIanaToNetEncoding(encoding, false));
                else
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
                    File.WriteAllText(_file.FullName, value, GXUtil.GxIanaToNetEncoding(encoding, false));
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            }
            catch (Exception e)
            {
                setError(e);
            }
        }
    }

    public void WriteAllLines(GxSimpleCollection<string> value, String encoding)
    {
        WriteAllLines(value, encoding, false);
    }

    public void WriteAllLines(GxSimpleCollection<string> svalue, String encoding, bool append)
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (validSource() && svalue != null)
        {
            try
            {
                if (append)
                {
                    foreach (string s in svalue)
                    {
                        File.AppendAllText(_file.FullName, s + StringUtil.NewLine(), GXUtil.GxIanaToNetEncoding(encoding, false));
                    }
                }
                else
                {
                    string[] strArray = new string[svalue.Count];
                    svalue.CopyTo(strArray, 0);
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
                    File.WriteAllLines(_file.FullName, strArray, GXUtil.GxIanaToNetEncoding(encoding, false));
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
                }
            }
            catch (Exception e)
            {
                setError(e);
            }
        }
    }

    public void AppendAllText(String value, String encoding)
    {
        WriteAllText(value, encoding, true);
    }

    public void AppendAllLines(GxSimpleCollection<string> value, String encoding)
    {
        WriteAllLines(value, encoding, true);
    }

    public void Open(String encoding)
    {
        OpenWrite(encoding);
        OpenRead(encoding);
    }
	private FileStream _fileStreamWriter;
    private StreamWriter _fileWriter;

	private FileStream _fileStreamReader;
	private StreamReader _fileReader;

    public void OpenWrite(String encoding)
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (validSource())
        {
            try
            {
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				_fileStreamWriter = new FileStream(_file.FullName, FileMode.Append | FileMode.OpenOrCreate, FileAccess.Write);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
                _fileWriter = new StreamWriter(_fileStreamWriter, GXUtil.GxIanaToNetEncoding(encoding, false));
            }
            catch (Exception e)
            {
                setError(e);
            }
        }
    }

    public void OpenRead(String encoding)
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (validSource())
        {
            try
            {
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				_fileStreamReader = new FileStream(_file.FullName, FileMode.Open, FileAccess.Read);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				_fileReader = new StreamReader(_fileStreamReader, GXUtil.GxIanaToNetEncoding(encoding, false));
            }
            catch (Exception e)
            {
                setError(e);
            }
        }
    }

    public void WriteLine(String value)
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (_fileWriter != null)
        {
            try
            {
                _fileWriter.WriteLine(value);
            }
            catch (Exception e)
            {
                setError(e);
            }
        }
        else
        {
            _lastError = 1;
            _lastErrorDescription = "Invalid File instance";
        }
    }

    public String ReadLine()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (_fileReader != null)
        {
            try
            {
                return _fileReader.ReadLine();
            }
            catch (Exception e)
            {
                setError(e);
            }
        }
        else
        {
            _lastError = 1;
            _lastErrorDescription = "Invalid File instance";
        }

        return string.Empty;
    }

    public bool Eof
    {
        get
        {
            if (_fileReader != null)
            {
                return _fileReader.EndOfStream;
            }
            return true;
        }
    }

    public void Close()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (_fileWriter != null)
        {
            try
            {
				_fileWriter.Close();
                _fileWriter = null;
				_fileStreamWriter.Close();
				_fileStreamWriter = null;
			}
			catch (Exception e)
            {
                setError(e);
            }
        }
        if (_fileReader != null)
        {
            try
            {
				_fileReader.Close();
                _fileReader = null;
				_fileStreamReader.Close();
				_fileStreamReader = null;
			}
			catch (Exception e)
            {
                setError(e);
            }
        }
    }
}

public class GxDirectory
{
    private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxDirectory));
    IGxDirectoryInfo _directory;
    string _baseDirectory;
    int _lastError;
    string _lastErrorDescription;
	bool _externalStorage = GXServices.Instance != null && GXServices.Instance.Get(GXServices.STORAGE_SERVICE) != null;

	public GxDirectory(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        if (_baseDirectory.EndsWith("\\", StringComparison.Ordinal))
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.Length - 1);
    }
    public GxDirectory(string baseDirectory, DirectoryInfo directory)
        : this(baseDirectory)
    {
        _directory = new GxDirectoryInfo(directory);
    }
    public GxDirectory(string baseDirectory, IGxDirectoryInfo directory)
    : this(baseDirectory)
    {
        _directory = directory;
    }
    public GxDirectory(string baseDirectory, string directory)
        : this(baseDirectory)
    {
        if (_externalStorage && !Path.IsPathRooted(directory))
        {
            _directory = new GxExternalDirectoryInfo(directory, ServiceFactory.GetExternalProvider());
            _baseDirectory = string.Empty;
        }
        else
            _directory = new GxDirectoryInfo(directory);
    }
    public IGxDirectoryInfo DirectoryInfo
    {
        get
        {
            return _directory;
        }
        set
        {
            _directory = value;
        }
    }
    public static string ApplicationDataPath
    {
        get
        {
            string userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            if (!string.IsNullOrEmpty(userProfile))
                return userProfile;
            else
                return string.Empty;
        }
    }
    public static string ExternalFilesPath
    {
        get { return ApplicationDataPath; }
    }
    public static string TemporaryFilesPath
    {
        get { return Path.GetTempPath(); }
    }
    public string Source
    {
        get
        {
            if (!validSource())
                return "";
            else
                return _directory.Name;
        }
        set
        {
            try
            {
                _directory = new GxDirectoryInfo(new DirectoryInfo(FileUtil.NormalizeSource(value, _baseDirectory)));
                _lastError = 0;
                _lastErrorDescription = "";
            }
            catch (Exception e)
            {
                GXLogging.Error(log, "Error setting File Source '" + value + "'", e);
                throw e;
            }

        }
    }
    public int ErrCode
    {
        get
        {
            return _lastError;
        }
    }
    public string ErrDescription
    {
        get
        {
            if (_lastErrorDescription.Trim().Length > 0)
                return _lastErrorDescription;
            switch (_lastError)
            {
                case 0:
                    return "Ok";
                case 1:
                    return "Invalid file instance";
                case 2:
                    return "Directory does not exist";
                case 3:
                    return "Directory already exist";
                case 4:
                    return "Directory not empty";
                case 100:
                    return "Security error";
            }
            return "Unknown error";
        }
    }
    public void Create()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return;
        try
        {
            if (_directory.Exists)
            {
                _lastError = 3;
                return;
            }
            _directory.Create();
            if (!_externalStorage)
                this.Source = _directory.FullName;
        }
        catch (IOException e)
        {
            setError(e);
        }
    }
    public void Delete()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return;
        try
        {
            if (!_directory.Exists)
            {
                _lastError = 2;
                return;
            }
            _directory.Delete();
			if (!_externalStorage)
				this.Source = _directory.FullName;
        }
        catch (Exception e)
        {
            setError(e);
        }
    }
    public bool Exists()
    {
        if (!validSource())
            return false;
        try
        {
            return _directory.Exists;
        }
        catch (Exception e)
        {
            setError(e);
        }
        return false;
    }
    public void Rename(string name)
    {
        name = FileUtil.NormalizeSource(name, _baseDirectory);

        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return;
        try
        {
            if (!_externalStorage && Directory.Exists(name))
            {
                _lastError = 3;
                return;
            }
            _directory.MoveTo(name);
        }
        catch (Exception e)
        {
            setError(e);
        }
    }
    public string GetName()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return "";
        return _directory.Name;
    }
    public string GetAbsoluteName()
    {
        _lastError = 0;
        _lastErrorDescription = "";
        if (!validSource())
            return "";
        return _directory.FullName;
	}
	string StandarizeSearchPattern(string filter)
	{
		if (string.IsNullOrEmpty(filter))
			return "*.*";
		else
		{
			// Valid extension are, for example: "*.txt", ".txt" and "txt". 
			char firstChar = filter.First();
			if (firstChar == '.' || (firstChar != '?' && firstChar != '*' && firstChar != '.'))
			{
				return $"*{filter}";
			}
			else return filter;
		}
	}
    public GxFileCollection GetFiles(string searchPattern)
    {
        GxFileCollection fc = new GxFileCollection();
        if (!validSource())
            return fc;
        if (!_directory.Exists)
            return fc;
		searchPattern = StandarizeSearchPattern(searchPattern);

        try
        {
            foreach (IGxFileInfo fi in _directory.GetFiles(searchPattern))
                fc.Add(new GxFile(_baseDirectory, fi));
        }
        catch (Exception e)
        {
            setError(e);
        }
        return fc;
    }
    public GxDirectoryCollection GetDirectories()
    {
        GxDirectoryCollection dc = new GxDirectoryCollection();
        if (!validSource())
            return dc;
        if (!_directory.Exists)
            return dc;
        try
        {
            foreach (IGxDirectoryInfo di in _directory.GetDirectories())
                dc.Add(new GxDirectory(_baseDirectory, di));
        }
        catch (Exception e)
        {
            setError(e);
        }
        return dc;
    }
    bool validSource()
    {
        if (_directory != null)
            return true;
        else
        {
            _lastError = -1;
            return false;
        }
    }
    void setError(Exception e)
    {
        if (e is UnauthorizedAccessException)
            _lastError = 100;
        else if (e.Message.IndexOf("directory is not empty") != -1)
            _lastError = 4;
        else
            _lastError = -1;
        _lastErrorDescription = e.Message;

        GXLogging.Error(log, "GXDirectory General Error", e);
    }
}

public class GxDirectoryCollection : ArrayList
{
    public GxDirectory Item(int i)
    {
        return (GxDirectory)(this[i - 1]);
    }
    public int ItemCount
    {
        get { return this.Count; }
    }
}

public class GxFileCollection : ArrayList
{
    public GxFile Item(int i)
    {
        return (GxFile)(this[i - 1]);
    }
    public int ItemCount
    {
        get { return this.Count; }
    }
}
