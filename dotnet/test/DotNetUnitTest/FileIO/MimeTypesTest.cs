using System.Collections.Generic;
using GeneXus.Application;
using GeneXus.Mime;
using Xunit;

namespace UnitTesting
{
	public class MimeTypesTest
	{
		[Fact]
		public void TestMimeTypeToExtension()
		{
			GxContext context = new GxContext();
			List<string[]> extensions = new List<string[]>();
			extensions.Add(new string[] { "wav", "audio/x-wav" });
			extensions.Add(new string[] { "jpg", "image/jpeg" });
			extensions.Add(new string[] { "jpg", "application/jpg" });
			extensions.Add(new string[] { "jpeg", "application/jpeg" });
			extensions.Add(new string[] { "png", "image/png" });
			extensions.Add(new string[] { "png", "image/x-png" });
			extensions.Add(new string[] { "zip", "application/zip" });
			extensions.Add(new string[] { "zip", "application/x-zip-compressed" });
			extensions.Add(new string[] { "rar", "application/x-rar-compressed" });
			extensions.Add(new string[] { "m4a", "audio/x-m4a" });
			extensions.Add(new string[] { "m4a", "audio/mp4" });
			extensions.Add(new string[] { "aif", "audio/aiff" });
			extensions.Add(new string[] { "ram", "audio/vnd.rn-realaudio" });

			foreach (string[] map in extensions)
			{
				string extension = context.ExtensionForContentType(map[1]);
				if (map[0] != extension)
					Assert.Equal(map[0], map[1] + " " + extension);
				Assert.Equal(map[0], extension);
			}
		}
		[Fact]
		public void TestExtensionToMimeType()
		{
			GxContext context = new GxContext();
			List<string[]> extensions = new List<string[]>();
			extensions.Add(new string[] { "3g2", "video/3gpp2" });
			extensions.Add(new string[] { "3gp", "video/3gpp" });
			extensions.Add(new string[] { "a3gpp", "audio/3gpp" });
			extensions.Add(new string[] { "aif", "audio/x-aiff" });
			extensions.Add(new string[] { "au", "audio/basic" });
			extensions.Add(new string[] { "avi", "video/x-msvideo" });
			extensions.Add(new string[] { "bmp", "image/bmp" });
			extensions.Add(new string[] { "caf", "audio/x-caf" });
			extensions.Add(new string[] { "divx", "video/x-divx" });
			extensions.Add(new string[] { "dll", "application/x-msdownload" });
			extensions.Add(new string[] { "exe", "application/octet-stream" });
			extensions.Add(new string[] { "gif", "image/gif" });
			extensions.Add(new string[] { "gz", "application/x-gzip" });
			extensions.Add(new string[] { "htm", MediaTypesNames.TextHtml });
			extensions.Add(new string[] { "html", MediaTypesNames.TextHtml });
			extensions.Add(new string[] { "jfif", "image/pjpeg" });
			extensions.Add(new string[] { "jpe", "image/jpeg" });
			extensions.Add(new string[] { "jpeg", "image/jpeg" });
			extensions.Add(new string[] { "jpg", "image/jpeg" });
			extensions.Add(new string[] { "m4a", "audio/mp4" });
			extensions.Add(new string[] { "mov", "video/quicktime" });
			extensions.Add(new string[] { "mp3", "audio/mpeg" });
			extensions.Add(new string[] { "mp4", "video/mp4" });
			extensions.Add(new string[] { "mpeg", "video/mpeg" });
			extensions.Add(new string[] { "mpg", "video/mpeg" });
			extensions.Add(new string[] { "pdf", "application/pdf" });
			extensions.Add(new string[] { "png", "image/png" });
			extensions.Add(new string[] { "ps", "application/postscript" });
			extensions.Add(new string[] { "qt", "video/quicktime" });
			extensions.Add(new string[] { "ram", "audio/x-pn-realaudio" });
			extensions.Add(new string[] { "rar", "application/x-rar-compressed" });
			extensions.Add(new string[] { "rtf", "text/rtf" });
			extensions.Add(new string[] { "rtx", "text/richtext" });
			extensions.Add(new string[] { "svg", "image/svg+xml" });
			extensions.Add(new string[] { "tar", "application/x-tar" });
			extensions.Add(new string[] { "tgz", "application/x-compressed" });
			extensions.Add(new string[] { "tif", "image/tiff" });
			extensions.Add(new string[] { "tiff", "image/tiff" });
			extensions.Add(new string[] { "txt", "text/plain" });
			extensions.Add(new string[] { "wav", "audio/wav" });
			extensions.Add(new string[] { "xml", "text/xml" });
			extensions.Add(new string[] { "zip", "application/zip" });
			foreach (string[] map in extensions)
			{
				string mimeType = context.GetContentType(map[0]);
				Assert.Equal(map[1], mimeType);
			}
		}
	}
}
