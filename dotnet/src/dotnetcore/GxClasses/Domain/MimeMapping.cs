using System;
using System.Collections.Generic;
using System.IO;

namespace GeneXus.Mime
{
	public static class MimeMapping
	{
		private static MimeMapping.MimeMappingDictionaryBase _mappingDictionary = (MimeMapping.MimeMappingDictionaryBase)new MimeMapping.MimeMappingDictionaryClassic();

		public static string GetMimeMapping(string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException(nameof(fileName));
			return MimeMapping._mappingDictionary.GetMimeMapping(fileName);
		}

		private abstract class MimeMappingDictionaryBase
		{
			private static readonly char[] _pathSeparatorChars = new char[3]
			{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar,
		Path.VolumeSeparatorChar
			};
			private readonly Dictionary<string, string> _mappings = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
			private bool _isInitialized;

			protected void AddMapping(string fileExtension, string mimeType)
			{
				this._mappings.Add(fileExtension, mimeType);
			}

			private void AddWildcardIfNotPresent()
			{
				if (this._mappings.ContainsKey(".*"))
					return;
				this.AddMapping(".*", "application/octet-stream");
			}

			private void EnsureMapping()
			{
				if (this._isInitialized)
					return;
				lock (this)
				{
					if (this._isInitialized)
						return;
					this.PopulateMappings();
					this.AddWildcardIfNotPresent();
					this._isInitialized = true;
				}
			}

			protected abstract void PopulateMappings();

			private static string GetFileName(string path)
			{
				int startIndex = path.LastIndexOfAny(MimeMapping.MimeMappingDictionaryBase._pathSeparatorChars);
				if (startIndex < 0)
					return path;
				return path.Substring(startIndex);
			}

			public string GetMimeMapping(string fileName)
			{
				this.EnsureMapping();
				fileName = MimeMapping.MimeMappingDictionaryBase.GetFileName(fileName);
				for (int startIndex = 0; startIndex < fileName.Length; ++startIndex)
				{
					string str;
					if ((int)fileName[startIndex] == 46 && this._mappings.TryGetValue(fileName.Substring(startIndex), out str))
						return str;
				}
				return this._mappings[".*"];
			}
		}

		private sealed class MimeMappingDictionaryClassic : MimeMapping.MimeMappingDictionaryBase
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
			protected override void PopulateMappings()
			{
				this.AddMapping(".323", "text/h323");
				this.AddMapping(".aaf", "application/octet-stream");
				this.AddMapping(".aca", "application/octet-stream");
				this.AddMapping(".accdb", "application/msaccess");
				this.AddMapping(".accde", "application/msaccess");
				this.AddMapping(".accdt", "application/msaccess");
				this.AddMapping(".acx", "application/internet-property-stream");
				this.AddMapping(".afm", "application/octet-stream");
				this.AddMapping(".ai", "application/postscript");
				this.AddMapping(".aif", "audio/x-aiff");
				this.AddMapping(".aifc", "audio/aiff");
				this.AddMapping(".aiff", "audio/aiff");
				this.AddMapping(".application", "application/x-ms-application");
				this.AddMapping(".art", "image/x-jg");
				this.AddMapping(".asd", "application/octet-stream");
				this.AddMapping(".asf", "video/x-ms-asf");
				this.AddMapping(".asi", "application/octet-stream");
				this.AddMapping(".asm", "text/plain");
				this.AddMapping(".asr", "video/x-ms-asf");
				this.AddMapping(".asx", "video/x-ms-asf");
				this.AddMapping(".atom", "application/atom+xml");
				this.AddMapping(".au", "audio/basic");
				this.AddMapping(".avi", "video/x-msvideo");
				this.AddMapping(".axs", "application/olescript");
				this.AddMapping(".bas", "text/plain");
				this.AddMapping(".bcpio", "application/x-bcpio");
				this.AddMapping(".bin", "application/octet-stream");
				this.AddMapping(".bmp", "image/bmp");
				this.AddMapping(".c", "text/plain");
				this.AddMapping(".cab", "application/octet-stream");
				this.AddMapping(".calx", "application/vnd.ms-office.calx");
				this.AddMapping(".cat", "application/vnd.ms-pki.seccat");
				this.AddMapping(".cdf", "application/x-cdf");
				this.AddMapping(".chm", "application/octet-stream");
				this.AddMapping(".class", "application/x-java-applet");
				this.AddMapping(".clp", "application/x-msclip");
				this.AddMapping(".cmx", "image/x-cmx");
				this.AddMapping(".cnf", "text/plain");
				this.AddMapping(".cod", "image/cis-cod");
				this.AddMapping(".cpio", "application/x-cpio");
				this.AddMapping(".cpp", "text/plain");
				this.AddMapping(".crd", "application/x-mscardfile");
				this.AddMapping(".crl", "application/pkix-crl");
				this.AddMapping(".crt", "application/x-x509-ca-cert");
				this.AddMapping(".csh", "application/x-csh");
				this.AddMapping(".css", "text/css");
				this.AddMapping(".csv", "application/octet-stream");
				this.AddMapping(".cur", "application/octet-stream");
				this.AddMapping(".dcr", "application/x-director");
				this.AddMapping(".deploy", "application/octet-stream");
				this.AddMapping(".der", "application/x-x509-ca-cert");
				this.AddMapping(".dib", "image/bmp");
				this.AddMapping(".dir", "application/x-director");
				this.AddMapping(".disco", "text/xml");
				this.AddMapping(".dll", "application/x-msdownload");
				this.AddMapping(".dll.config", "text/xml");
				this.AddMapping(".dlm", "text/dlm");
				this.AddMapping(".doc", "application/msword");
				this.AddMapping(".docm", "application/vnd.ms-word.document.macroEnabled.12");
				this.AddMapping(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
				this.AddMapping(".dot", "application/msword");
				this.AddMapping(".dotm", "application/vnd.ms-word.template.macroEnabled.12");
				this.AddMapping(".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
				this.AddMapping(".dsp", "application/octet-stream");
				this.AddMapping(".dtd", "text/xml");
				this.AddMapping(".dvi", "application/x-dvi");
				this.AddMapping(".dwf", "drawing/x-dwf");
				this.AddMapping(".dwp", "application/octet-stream");
				this.AddMapping(".dxr", "application/x-director");
				this.AddMapping(".eml", "message/rfc822");
				this.AddMapping(".emz", "application/octet-stream");
				this.AddMapping(".eot", "application/octet-stream");
				this.AddMapping(".eps", "application/postscript");
				this.AddMapping(".etx", "text/x-setext");
				this.AddMapping(".evy", "application/envoy");
				this.AddMapping(".exe", "application/octet-stream");
				this.AddMapping(".exe.config", "text/xml");
				this.AddMapping(".fdf", "application/vnd.fdf");
				this.AddMapping(".fif", "application/fractals");
				this.AddMapping(".fla", "application/octet-stream");
				this.AddMapping(".flr", "x-world/x-vrml");
				this.AddMapping(".flv", "video/x-flv");
				this.AddMapping(".gif", "image/gif");
				this.AddMapping(".gtar", "application/x-gtar");
				this.AddMapping(".gz", "application/x-gzip");
				this.AddMapping(".h", "text/plain");
				this.AddMapping(".hdf", "application/x-hdf");
				this.AddMapping(".hdml", "text/x-hdml");
				this.AddMapping(".hhc", "application/x-oleobject");
				this.AddMapping(".hhk", "application/octet-stream");
				this.AddMapping(".hhp", "application/octet-stream");
				this.AddMapping(".hlp", "application/winhlp");
				this.AddMapping(".hqx", "application/mac-binhex40");
				this.AddMapping(".hta", "application/hta");
				this.AddMapping(".htc", "text/x-component");
				this.AddMapping(".htm", "text/html");
				this.AddMapping(".html", "text/html");
				this.AddMapping(".htt", "text/webviewhtml");
				this.AddMapping(".hxt", "text/html");
				this.AddMapping(".ico", "image/x-icon");
				this.AddMapping(".ics", "application/octet-stream");
				this.AddMapping(".ief", "image/ief");
				this.AddMapping(".iii", "application/x-iphone");
				this.AddMapping(".inf", "application/octet-stream");
				this.AddMapping(".ins", "application/x-internet-signup");
				this.AddMapping(".isp", "application/x-internet-signup");
				this.AddMapping(".IVF", "video/x-ivf");
				this.AddMapping(".jar", "application/java-archive");
				this.AddMapping(".java", "application/octet-stream");
				this.AddMapping(".jck", "application/liquidmotion");
				this.AddMapping(".jcz", "application/liquidmotion");
				this.AddMapping(".jfif", "image/pjpeg");
				this.AddMapping(".jpb", "application/octet-stream");
				this.AddMapping(".jpe", "image/jpeg");
				this.AddMapping(".jpeg", "image/jpeg");
				this.AddMapping(".jpg", "image/jpeg");
				this.AddMapping(".js", "application/x-javascript");
				this.AddMapping(".jsx", "text/jscript");
				this.AddMapping(".latex", "application/x-latex");
				this.AddMapping(".lit", "application/x-ms-reader");
				this.AddMapping(".lpk", "application/octet-stream");
				this.AddMapping(".lsf", "video/x-la-asf");
				this.AddMapping(".lsx", "video/x-la-asf");
				this.AddMapping(".lzh", "application/octet-stream");
				this.AddMapping(".m13", "application/x-msmediaview");
				this.AddMapping(".m14", "application/x-msmediaview");
				this.AddMapping(".m1v", "video/mpeg");
				this.AddMapping(".m3u", "audio/x-mpegurl");
				this.AddMapping(".man", "application/x-troff-man");
				this.AddMapping(".manifest", "application/x-ms-manifest");
				this.AddMapping(".map", "text/plain");
				this.AddMapping(".mdb", "application/x-msaccess");
				this.AddMapping(".mdp", "application/octet-stream");
				this.AddMapping(".me", "application/x-troff-me");
				this.AddMapping(".mht", "message/rfc822");
				this.AddMapping(".mhtml", "message/rfc822");
				this.AddMapping(".mid", "audio/mid");
				this.AddMapping(".midi", "audio/mid");
				this.AddMapping(".mix", "application/octet-stream");
				this.AddMapping(".mmf", "application/x-smaf");
				this.AddMapping(".mno", "text/xml");
				this.AddMapping(".mny", "application/x-msmoney");
				this.AddMapping(".mov", "video/quicktime");
				this.AddMapping(".movie", "video/x-sgi-movie");
				this.AddMapping(".mp2", "video/mpeg");
				this.AddMapping(".mp3", "audio/mpeg");
				this.AddMapping(".mpa", "video/mpeg");
				this.AddMapping(".mpe", "video/mpeg");
				this.AddMapping(".mpeg", "video/mpeg");
				this.AddMapping(".mpg", "video/mpeg");
				this.AddMapping(".mpp", "application/vnd.ms-project");
				this.AddMapping(".mpv2", "video/mpeg");
				this.AddMapping(".ms", "application/x-troff-ms");
				this.AddMapping(".msi", "application/octet-stream");
				this.AddMapping(".mso", "application/octet-stream");
				this.AddMapping(".mvb", "application/x-msmediaview");
				this.AddMapping(".mvc", "application/x-miva-compiled");
				this.AddMapping(".nc", "application/x-netcdf");
				this.AddMapping(".nsc", "video/x-ms-asf");
				this.AddMapping(".nws", "message/rfc822");
				this.AddMapping(".ocx", "application/octet-stream");
				this.AddMapping(".oda", "application/oda");
				this.AddMapping(".odc", "text/x-ms-odc");
				this.AddMapping(".ods", "application/oleobject");
				this.AddMapping(".one", "application/onenote");
				this.AddMapping(".onea", "application/onenote");
				this.AddMapping(".onetoc", "application/onenote");
				this.AddMapping(".onetoc2", "application/onenote");
				this.AddMapping(".onetmp", "application/onenote");
				this.AddMapping(".onepkg", "application/onenote");
				this.AddMapping(".osdx", "application/opensearchdescription+xml");
				this.AddMapping(".p10", "application/pkcs10");
				this.AddMapping(".p12", "application/x-pkcs12");
				this.AddMapping(".p7b", "application/x-pkcs7-certificates");
				this.AddMapping(".p7c", "application/pkcs7-mime");
				this.AddMapping(".p7m", "application/pkcs7-mime");
				this.AddMapping(".p7r", "application/x-pkcs7-certreqresp");
				this.AddMapping(".p7s", "application/pkcs7-signature");
				this.AddMapping(".pbm", "image/x-portable-bitmap");
				this.AddMapping(".pcx", "application/octet-stream");
				this.AddMapping(".pcz", "application/octet-stream");
				this.AddMapping(".pdf", "application/pdf");
				this.AddMapping(".pfb", "application/octet-stream");
				this.AddMapping(".pfm", "application/octet-stream");
				this.AddMapping(".pfx", "application/x-pkcs12");
				this.AddMapping(".pgm", "image/x-portable-graymap");
				this.AddMapping(".pko", "application/vnd.ms-pki.pko");
				this.AddMapping(".pma", "application/x-perfmon");
				this.AddMapping(".pmc", "application/x-perfmon");
				this.AddMapping(".pml", "application/x-perfmon");
				this.AddMapping(".pmr", "application/x-perfmon");
				this.AddMapping(".pmw", "application/x-perfmon");
				this.AddMapping(".png", "image/png");
				this.AddMapping(".pnm", "image/x-portable-anymap");
				this.AddMapping(".pnz", "image/png");
				this.AddMapping(".pot", "application/vnd.ms-powerpoint");
				this.AddMapping(".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12");
				this.AddMapping(".potx", "application/vnd.openxmlformats-officedocument.presentationml.template");
				this.AddMapping(".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12");
				this.AddMapping(".ppm", "image/x-portable-pixmap");
				this.AddMapping(".pps", "application/vnd.ms-powerpoint");
				this.AddMapping(".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
				this.AddMapping(".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");
				this.AddMapping(".ppt", "application/vnd.ms-powerpoint");
				this.AddMapping(".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12");
				this.AddMapping(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
				this.AddMapping(".prf", "application/pics-rules");
				this.AddMapping(".prm", "application/octet-stream");
				this.AddMapping(".prx", "application/octet-stream");
				this.AddMapping(".ps", "application/postscript");
				this.AddMapping(".psd", "application/octet-stream");
				this.AddMapping(".psm", "application/octet-stream");
				this.AddMapping(".psp", "application/octet-stream");
				this.AddMapping(".pub", "application/x-mspublisher");
				this.AddMapping(".qt", "video/quicktime");
				this.AddMapping(".qtl", "application/x-quicktimeplayer");
				this.AddMapping(".qxd", "application/octet-stream");
				this.AddMapping(".ra", "audio/x-pn-realaudio");
				this.AddMapping(".ram", "audio/x-pn-realaudio");
				this.AddMapping(".rar", "application/octet-stream");
				this.AddMapping(".ras", "image/x-cmu-raster");
				this.AddMapping(".rf", "image/vnd.rn-realflash");
				this.AddMapping(".rgb", "image/x-rgb");
				this.AddMapping(".rm", "application/vnd.rn-realmedia");
				this.AddMapping(".rmi", "audio/mid");
				this.AddMapping(".roff", "application/x-troff");
				this.AddMapping(".rpm", "audio/x-pn-realaudio-plugin");
				this.AddMapping(".rtf", "application/rtf");
				this.AddMapping(".rtx", "text/richtext");
				this.AddMapping(".scd", "application/x-msschedule");
				this.AddMapping(".sct", "text/scriptlet");
				this.AddMapping(".sea", "application/octet-stream");
				this.AddMapping(".setpay", "application/set-payment-initiation");
				this.AddMapping(".setreg", "application/set-registration-initiation");
				this.AddMapping(".sgml", "text/sgml");
				this.AddMapping(".sh", "application/x-sh");
				this.AddMapping(".shar", "application/x-shar");
				this.AddMapping(".sit", "application/x-stuffit");
				this.AddMapping(".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12");
				this.AddMapping(".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide");
				this.AddMapping(".smd", "audio/x-smd");
				this.AddMapping(".smi", "application/octet-stream");
				this.AddMapping(".smx", "audio/x-smd");
				this.AddMapping(".smz", "audio/x-smd");
				this.AddMapping(".snd", "audio/basic");
				this.AddMapping(".snp", "application/octet-stream");
				this.AddMapping(".spc", "application/x-pkcs7-certificates");
				this.AddMapping(".spl", "application/futuresplash");
				this.AddMapping(".src", "application/x-wais-source");
				this.AddMapping(".ssm", "application/streamingmedia");
				this.AddMapping(".sst", "application/vnd.ms-pki.certstore");
				this.AddMapping(".stl", "application/vnd.ms-pki.stl");
				this.AddMapping(".sv4cpio", "application/x-sv4cpio");
				this.AddMapping(".sv4crc", "application/x-sv4crc");
				this.AddMapping(".svg", "image/svg+xml");
				this.AddMapping(".swf", "application/x-shockwave-flash");
				this.AddMapping(".t", "application/x-troff");
				this.AddMapping(".tar", "application/x-tar");
				this.AddMapping(".tcl", "application/x-tcl");
				this.AddMapping(".tex", "application/x-tex");
				this.AddMapping(".texi", "application/x-texinfo");
				this.AddMapping(".texinfo", "application/x-texinfo");
				this.AddMapping(".tgz", "application/x-compressed");
				this.AddMapping(".thmx", "application/vnd.ms-officetheme");
				this.AddMapping(".thn", "application/octet-stream");
				this.AddMapping(".tif", "image/tiff");
				this.AddMapping(".tiff", "image/tiff");
				this.AddMapping(".toc", "application/octet-stream");
				this.AddMapping(".tr", "application/x-troff");
				this.AddMapping(".trm", "application/x-msterminal");
				this.AddMapping(".tsv", "text/tab-separated-values");
				this.AddMapping(".ttf", "application/octet-stream");
				this.AddMapping(".txt", "text/plain");
				this.AddMapping(".u32", "application/octet-stream");
				this.AddMapping(".uls", "text/iuls");
				this.AddMapping(".ustar", "application/x-ustar");
				this.AddMapping(".vbs", "text/vbscript");
				this.AddMapping(".vcf", "text/x-vcard");
				this.AddMapping(".vcs", "text/plain");
				this.AddMapping(".vdx", "application/vnd.ms-visio.viewer");
				this.AddMapping(".vml", "text/xml");
				this.AddMapping(".vsd", "application/vnd.visio");
				this.AddMapping(".vss", "application/vnd.visio");
				this.AddMapping(".vst", "application/vnd.visio");
				this.AddMapping(".vsto", "application/x-ms-vsto");
				this.AddMapping(".vsw", "application/vnd.visio");
				this.AddMapping(".vsx", "application/vnd.visio");
				this.AddMapping(".vtx", "application/vnd.visio");
				this.AddMapping(".wav", "audio/wav");
				this.AddMapping(".wax", "audio/x-ms-wax");
				this.AddMapping(".wbmp", "image/vnd.wap.wbmp");
				this.AddMapping(".wcm", "application/vnd.ms-works");
				this.AddMapping(".wdb", "application/vnd.ms-works");
				this.AddMapping(".wks", "application/vnd.ms-works");
				this.AddMapping(".wm", "video/x-ms-wm");
				this.AddMapping(".wma", "audio/x-ms-wma");
				this.AddMapping(".wmd", "application/x-ms-wmd");
				this.AddMapping(".wmf", "application/x-msmetafile");
				this.AddMapping(".wml", "text/vnd.wap.wml");
				this.AddMapping(".wmlc", "application/vnd.wap.wmlc");
				this.AddMapping(".wmls", "text/vnd.wap.wmlscript");
				this.AddMapping(".wmlsc", "application/vnd.wap.wmlscriptc");
				this.AddMapping(".wmp", "video/x-ms-wmp");
				this.AddMapping(".wmv", "video/x-ms-wmv");
				this.AddMapping(".wmx", "video/x-ms-wmx");
				this.AddMapping(".wmz", "application/x-ms-wmz");
				this.AddMapping(".wps", "application/vnd.ms-works");
				this.AddMapping(".wri", "application/x-mswrite");
				this.AddMapping(".wrl", "x-world/x-vrml");
				this.AddMapping(".wrz", "x-world/x-vrml");
				this.AddMapping(".wsdl", "text/xml");
				this.AddMapping(".wvx", "video/x-ms-wvx");
				this.AddMapping(".x", "application/directx");
				this.AddMapping(".xaf", "x-world/x-vrml");
				this.AddMapping(".xaml", "application/xaml+xml");
				this.AddMapping(".xap", "application/x-silverlight-app");
				this.AddMapping(".xbap", "application/x-ms-xbap");
				this.AddMapping(".xbm", "image/x-xbitmap");
				this.AddMapping(".xdr", "text/plain");
				this.AddMapping(".xla", "application/vnd.ms-excel");
				this.AddMapping(".xlam", "application/vnd.ms-excel.addin.macroEnabled.12");
				this.AddMapping(".xlc", "application/vnd.ms-excel");
				this.AddMapping(".xlm", "application/vnd.ms-excel");
				this.AddMapping(".xls", "application/vnd.ms-excel");
				this.AddMapping(".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12");
				this.AddMapping(".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12");
				this.AddMapping(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
				this.AddMapping(".xlt", "application/vnd.ms-excel");
				this.AddMapping(".xltm", "application/vnd.ms-excel.template.macroEnabled.12");
				this.AddMapping(".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template");
				this.AddMapping(".xlw", "application/vnd.ms-excel");
				this.AddMapping(".xml", "text/xml");
				this.AddMapping(".xof", "x-world/x-vrml");
				this.AddMapping(".xpm", "image/x-xpixmap");
				this.AddMapping(".xps", "application/vnd.ms-xpsdocument");
				this.AddMapping(".xsd", "text/xml");
				this.AddMapping(".xsf", "text/xml");
				this.AddMapping(".xsl", "text/xml");
				this.AddMapping(".xslt", "text/xml");
				this.AddMapping(".xsn", "application/octet-stream");
				this.AddMapping(".xtp", "application/octet-stream");
				this.AddMapping(".xwd", "image/x-xwindowdump");
				this.AddMapping(".z", "application/x-compress");
				this.AddMapping(".zip", "application/x-zip-compressed");
			}
		}

	}

}
