using System;
using System.IO;
using GeneXus.Configuration;
using GeneXus.Utils;
using Xunit;

namespace UnitTesting
{
	public class FileIOTests
	{
		[Fact]
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
		[Fact]
		public void PathSourceTest()
		{
			Config.ConfigFileName = "client.exe.config";
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
		}
		[Fact]
		public void GXDBFilePathTest()
		{
			string[] filesName = { "content%2f..%2f..%2f..%2fdocument.aspx","content%2f%2e%2e%2f%2e%2e%2f%2e%2e%2fdocument","content%5c%2e%2e%5c%2e%2e%5c%2e%2e%5cdocument",
				"content%5c..%5c..%5c..%5cdocument.aspx","content%255c%252e%252e%255c%252e%252e%255c%252e%252e%255cdocument.aspx","content%255c..%255c..%255c..%255cdocument.aspx",
				"content%c0%af..%c0%af..%c0%af..%c0%afdocument.aspx","content%c1%9c..%c1%9c..%c1%9c..%c1%9cdocument.aspx",
				@"http://localhost/base/content%2f..%2f..%2f..%2fdocument.aspx"};

			foreach (string fileName in filesName)
			{
				string newFileName = GXDbFile.GenerateUri(fileName, false, false);
				string baseDir = Directory.GetCurrentDirectory();

				string fullPath = Path.Combine(baseDir, newFileName);
				string fullPath2 = Path.GetFullPath(fullPath);
				bool isOK = fullPath2.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase);
				Assert.True(isOK);
			}
		}
	}
}
