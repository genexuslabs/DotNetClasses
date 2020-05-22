using System;
using System.IO;
using GeneXus.Configuration;
using GeneXus.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTesting
{
	[TestClass]
	public class FileIOTests
	{
		[TestMethod]
		public void FileSourceTest()
		{
			GxFileInfo fi = new GxFileInfo(string.Empty);
			string source = @"\\fs-server\test\files\xml\error.xml";
			fi.Source = source;
			string expected = source;
			Assert.IsTrue(fi.FullName == expected);


			source = "file:///D:/Models/37121/CSharpModel/web/PublicTempStorage/multimedia/iso_8859-1_ad09d880ffad4b42b0e238d17476f476.txt";
			fi.Source = source;
			expected = @"D:\Models\37121\CSharpModel\web\PublicTempStorage\multimedia\iso_8859-1_ad09d880ffad4b42b0e238d17476f476.txt";
			Assert.IsTrue(fi.FullName == expected);

		}
		[TestMethod]
		public void PathSourceTest()
		{
			Config.ConfigFileName = "client.exe.config";
			string blobPath = Preferences.getBLOB_PATH();
			Assert.IsTrue(!string.IsNullOrEmpty(blobPath));
			Uri uri;
			string path = @"\\fs-server\test\files\xml\error.xml" ;
			
			PathUtil.AbsoluteUri(path, out uri);
			Assert.IsTrue(uri.Scheme == Uri.UriSchemeFile);
			Assert.IsTrue(uri.IsAbsoluteUri);
			Assert.IsTrue(new FileInfo(uri.LocalPath).FullName == new FileInfo(path).FullName);

			path = @"C:\Models\ImageType\CSharpModel\web\PrivateTempStorage\blob250dac8b-78f8-492b-b3d1-f1c0759c3167.jpg";
			PathUtil.AbsoluteUri(path, out uri);
			Assert.IsTrue(uri.Scheme == Uri.UriSchemeFile);
			Assert.IsTrue(uri.IsAbsoluteUri);
			Assert.IsTrue(new FileInfo(uri.LocalPath).FullName == new FileInfo(path).FullName);

			path = "/ImageType.NetEnvironment/PublicTempStorage/multimedia/myimg_8e1604b16eda43e59694f9aeb0b33e77.jpg";
			PathUtil.AbsoluteUri(path, out uri);
			Assert.IsTrue(uri.Scheme == Uri.UriSchemeFile);
			Assert.IsTrue(uri.IsAbsoluteUri);
			Assert.IsTrue(new FileInfo(uri.LocalPath).FullName == new FileInfo("PublicTempStorage/multimedia/myimg_8e1604b16eda43e59694f9aeb0b33e77.jpg").FullName);
		}
	}
}
