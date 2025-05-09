using System;
using System.IO.Compression;
using System.IO;
using System.Web;
using log4net;
using GeneXus;

namespace GamSaml20.Utils
{
	internal class Encoding
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Encoding));

		internal static byte[] DecodeAndInflateXmlParameter(string parm)
		{
			logger.Trace("DecodeAndInflateXmlParameter");
			string base64 = HttpUtility.UrlDecode(parm);
			byte[] bytes = Convert.FromBase64String(base64);
			byte[] buffer = new byte[4096];
			using (var init = new MemoryStream(bytes))
			{
				using (var decodedStream = new MemoryStream())
				{
					using (var zip = new DeflateStream(init, CompressionMode.Decompress))
					{
						int bytesRead;
						while ((bytesRead = zip.Read(buffer, 0, buffer.Length)) > 0)
							decodedStream.Write(buffer, 0, bytesRead);
					}
					return decodedStream.ToArray();
				}
			}
		}

		internal static string DelfateAndEncodeXmlParameter(string parm)
		{
			logger.Trace("DelfateAndEncodeXmlParameter");
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(parm);
			using (var output = new MemoryStream())
			{
				using (var zip = new DeflateStream(output, CompressionMode.Compress))
				{
					zip.Write(bytes, 0, bytes.Length);
				}
				string base64 = Convert.ToBase64String(output.ToArray());
				return HttpUtility.UrlEncode(base64);
			}
		}

		internal static string EncodeParameter(string parm)
		{
			logger.Trace("EncodeParameter");

			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(parm);
			string base64 = Convert.ToBase64String(bytes);
			return HttpUtility.UrlEncode(base64);
		}

		internal static byte[] DecodeParameter(string parm)
		{
			logger.Trace("DecodeParameter");
			string base64 = HttpUtility.UrlDecode(parm);
			return Convert.FromBase64String(base64);

		}

	}
}
