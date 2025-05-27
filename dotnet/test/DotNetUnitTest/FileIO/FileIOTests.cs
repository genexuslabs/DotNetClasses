using System;
using System.IO;
using System.Runtime.InteropServices;
using DotNetUnitTest;
using GeneXus.Configuration;
using GeneXus.Printer;
using GeneXus.Services;
using GeneXus.Utils;
using Xunit;

namespace UnitTesting
{
	public class FileIOTests : FileSystemTest
	{
		public FileIOTests()
		{
#if !NETCORE
			Config.ConfigFileName = Path.Combine(BaseDir, "client.exe.config");
#endif
		}
		[WindowsOnlyFact]
		public void FileSharedToCopy()
		{
			string target = @"\\192.168.86.3\printer";
			GxFile f = new GxFile();
			f.Source = Path.Combine(BaseDir, "Document.txt");
			f.Copy(target);
			Assert.Equal(-1, f.ErrCode);
			Assert.NotEqual(new NullReferenceException().Message, f.ErrDescription);
		}
		[WindowsOnlyFact]
		public void FileSourceTest()
		{
			GxFileInfo fi = new GxFileInfo(string.Empty);
			string source = @"\\fs-server\test\files\xml\error.xml";
			fi.Source = source;
			string expected = source;
			Assert.True(fi.FullName == expected);

			source = "file:///D:/Models/37121/CSharpModel/web/PublicTempStorage/multimedia/iso_8859-1_ad09d880ffad4b42b0e238d17476f476.txt";
			fi.Source = source;
			expected = @"D:\Models\37121\CSharpModel\web\PublicTempStorage\multimedia\iso_8859-1_ad09d880ffad4b42b0e238d17476f476.txt";
			Assert.True(fi.FullName == expected);

		}
		[Fact(Skip = "temporarily disabled")]
		public void PathSourceTest()
		{
			string blobPath = Preferences.getBLOB_PATH();
			Assert.True(!string.IsNullOrEmpty(blobPath));
			Uri uri;
			string path = @"\\fs-server\test\files\xml\error.xml" ;
			
			PathUtil.AbsoluteUri(path, out uri);
			Assert.True(uri.Scheme == Uri.UriSchemeFile);
			Assert.True(uri.IsAbsoluteUri);
			Assert.True(new FileInfo(uri.LocalPath).FullName == new FileInfo(path).FullName);

			path = @"C:\Models\ImageType\CSharpModel\web\PrivateTempStorage\blob250dac8b-78f8-492b-b3d1-f1c0759c3167.jpg";
			PathUtil.AbsoluteUri(path, out uri);
			Assert.True(uri.Scheme == Uri.UriSchemeFile);
			Assert.True(uri.IsAbsoluteUri);
			Assert.True(new FileInfo(uri.LocalPath).FullName == new FileInfo(path).FullName);

			path = "/ImageType.NetEnvironment/PublicTempStorage/multimedia/myimg_8e1604b16eda43e59694f9aeb0b33e77.jpg";
			PathUtil.AbsoluteUri(path, out uri);
			Assert.True(uri.Scheme == Uri.UriSchemeFile);
			Assert.True(uri.IsAbsoluteUri);
			Assert.True(new FileInfo(uri.LocalPath).FullName == new FileInfo("PublicTempStorage/multimedia/myimg_8e1604b16eda43e59694f9aeb0b33e77.jpg").FullName);

			path = "https://localhost/ImageType.NetEnvironment/PublicTempStorage/multimedia/myimg_8e1604b16eda43e59694f9aeb0b33e77.jpg";
			PathUtil.AbsoluteUri(path, out uri);
			Assert.True(uri.Scheme == Uri.UriSchemeHttps);
			Assert.True(uri.IsAbsoluteUri);

			path = "file:///app/test/files/xml/error.xml";
			PathUtil.AbsoluteUri(path, out uri);
			Assert.True(uri.Scheme == Uri.UriSchemeFile);
			Assert.True(uri.IsAbsoluteUri);

		}
		[Fact]
		public void GXDBFilePathTest()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}
			string[] filesName = { "content/../../../document.aspx","content%2f..%2f..%2f..%2fdocument.aspx","content%2f%2e%2e%2f%2e%2e%2f%2e%2e%2fdocument","content%5c%2e%2e%5c%2e%2e%5c%2e%2e%5cdocument",
				"content%5c..%5c..%5c..%5cdocument.aspx","content%255c%252e%252e%255c%252e%252e%255c%252e%252e%255cdocument.aspx","content%255c..%255c..%255c..%255cdocument.aspx",
				"content%c0%af..%c0%af..%c0%af..%c0%afdocument.aspx","content%c1%9c..%c1%9c..%c1%9c..%c1%9cdocument.aspx"};

			foreach (string fileName in filesName)
			{
				string newFileName = GXDbFile.ResolveUri($"{GXDbFile.Scheme}:{fileName}", false);
				string baseDir = Preferences.getBLOB_PATH();
				try
				{
					bool isOK = new Uri(newFileName).LocalPath.StartsWith(Path.GetFullPath(baseDir), StringComparison.OrdinalIgnoreCase);
					Assert.True(isOK);
				}
				catch (Exception ex)
				{

					Assert.True(false,  $"FileName:{newFileName} Error:{ex.Message} ExternalProvider:{ServiceFactory.GetExternalProvider()?.GetType().FullName} fileName:{fileName}");
				}
			}
		}

		[WindowsOnlyFact]
		public void PathUtilGetValidFileName()
		{
			string path = "file:///C:/Models/Upload/CSharpModel/web/PublicTempStorage/multimedia/Screen%20Shot%202016-02-15%20at%2011.41.55%20AM_ff107a3ba9fb4564bb4e1bf7f74d5fbf.png";
			string fileName = PathUtil.GetValidFileName(path, "_");
			Assert.StartsWith("Screen Shot 2016-02-15 at 11.41.55 AM", fileName, StringComparison.OrdinalIgnoreCase);

			path = "http://localhost/Upload/PublicTempStorage/multimedia/Screen%20Shot%202016-02-15%20at%2011.41.55%20AM_2c0f533f07d2401a8d1c5f8023b59f6c.png";
			fileName = PathUtil.GetValidFileName(path, "_");
			Assert.StartsWith("Screen Shot 2016-02-15 at 11.41.55 AM", fileName, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void ReportUtilAddPathLinux()
		{
			string name = "/mnt/c/Models/DockerReport/NETModel/Web/PublicTempStorage/clientreportebee6af4-7554-4283-b246-e1600e49b103.pdf";
			string path = "/mnt/c/Models/DockerReport/NETModel/Web/";
			string fullPath = ReportUtils.AddPath(name, path);
			Assert.Equal(name, fullPath);
		}

		[Fact]
		public void ReportUtilAddPathWindows()
		{
			string name = "PublicTempStorage/clientreportebee6af4-7554-4283-b246-e1600e49b103.pdf";
			string path = "C:/Models/Report/NETModel/Web/";
			string fullPath = ReportUtils.AddPath(name, path);

			Assert.Equal(Path.Combine(path, name), fullPath);
		}
		[Fact]
		public void ReportUtilAddPathHttp()
		{
			string name = "http://localhost:5000/WebApp/PublicTempStorage/clientreportebee6af4-7554-4283-b246-e1600e49b103.pdf";
			string path = "C:/Models/Report/NETModel/Web/";
			string fullPath = ReportUtils.AddPath(name, path);
			Assert.Equal(name, fullPath);
		}
		[Fact]
		public void ReportUtilAddPathWithIllegalCharacters()
		{
			string name = "https://chart.googleapis.com/chart?chs=400x400&cht=qr&chl=http://sistemas.gov/nfceweb/consultarNFCe.jsp?p=13231205514674000128650020009504049878593990|2|1|09|1337.07|4558626967746769617A304E4B7A6D34504B4E61524A474F4D32513D|1|14D0A30916C6C7EA709E7E33E330EE3F290FE25D";
			string path = "C:/Models/Report/NETModel/Web/";
			string fullPath = ReportUtils.AddPath(name, path);

			Assert.Equal(name, fullPath);
		}
		[Fact]
		public void Length_ShouldUpdate_WhenFileChanges()
		{
			string tempFilePath = Path.Combine(BaseDir, "FileChanges.txt"); 
			File.WriteAllText(tempFilePath, "Initial content");
			GxFileInfo gxFileInfo = new GxFileInfo(tempFilePath);
			long initialLength = gxFileInfo.Length;
			File.AppendAllText(tempFilePath, "\nAdditional content");
			long updatedLength = gxFileInfo.Length;
			Assert.NotEqual(initialLength, updatedLength);
			File.Delete(tempFilePath);
		}
	}
}
