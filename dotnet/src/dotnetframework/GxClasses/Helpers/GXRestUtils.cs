using System;
using System.Web;
using GeneXus.Application;
using GeneXus.Http;
using System.Collections.Generic;
#if NETCORE
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
#endif
using log4net;
using System.IO;
using Jayrock.Json;



namespace GeneXus.Utils
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]

	public class GxRestPrefix
	{
        public static int UPLOAD_TIMEOUT = 10;
        public static string UPLOAD_PREFIX = "gxupload:";
        public static string ENCODED_PREFIX = "Encoded:";
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	internal class GXFormData
	{
		public static string FORMDATA_REFERENCE = "gxformdataref:";
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	internal class GxUploadHelper
	{

		internal static bool IsUploadURL(HttpContext httpContext)
		{
			return httpContext.Request.GetRawUrl().EndsWith(HttpHelper.GXOBJECT, StringComparison.OrdinalIgnoreCase);
		}
		internal static void CacheUploadFile(string fileGuid, string realFileName, string realFileExtension, GxFile temporalFile, IGxContext gxContext)
		{
			CacheAPI.FilesCache.Set(fileGuid, JSONHelper.Serialize(new UploadCachedFile() { path = temporalFile.GetURI(), fileExtension = realFileExtension, fileName = realFileName }), GxRestPrefix.UPLOAD_TIMEOUT);
			GXFileWatcher.Instance.AddTemporaryFile(temporalFile, gxContext);
		}
		internal static string GetUploadFileGuid()
		{
			return Guid.NewGuid().ToString("N");
		}
		internal static string GetUploadFileId(string fileGuid)
		{
			return GxRestPrefix.UPLOAD_PREFIX + fileGuid;
		}
		internal static bool IsUpload(string filename)
		{
			return (!string.IsNullOrEmpty(filename) && filename.StartsWith(GxRestPrefix.UPLOAD_PREFIX));
		}

		internal static string UploadPath(string filename)
		{
			UploadCachedFile uploadFile = GetUploadFileObject(filename);
			return uploadFile != null ? uploadFile.path : string.Empty;
		}

		internal static string UploadName(string uploadFileId)
		{
			UploadCachedFile uploadFile = GetUploadFileObject(uploadFileId);
			return uploadFile != null ? uploadFile.fileName : string.Empty;
		}
		internal static string UploadExtension(string uploadFileId)
		{
			UploadCachedFile uploadFile = GetUploadFileObject(uploadFileId);
			return uploadFile != null ? uploadFile.fileExtension : string.Empty;
		}
		internal static UploadCachedFile GetUploadFileObject(string filename)
		{
			if (!string.IsNullOrEmpty(filename) && filename.StartsWith(GxRestPrefix.UPLOAD_PREFIX))
			{
				string fkey = filename.Substring(filename.IndexOf(':') + 1);
				if (CacheAPI.FilesCache.Contains(fkey))
				{
					string uploadJson = CacheAPI.FilesCache.Get(fkey);
					UploadCachedFile uploadFile = JSONHelper.Deserialize<UploadCachedFile>(uploadJson);
					return uploadFile;

				}
			}
			return null;
		}

	}
	public class UploadCachedFile
	{
		public string path { get; set; }
		public string fileName { get; set; }
		public string fileExtension { get; set; }
	}

	public class RestAPIHelpers
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.RestAPIHelpers));

		public static Dictionary<string, object> ReadRestBodyParameters(Stream stream)
		{
			var bodyParameters = new Dictionary<string, object>();
			using (StreamReader streamReader = new StreamReader(stream))
			{
				if (!streamReader.EndOfStream)
				{
					try
					{
						string json = streamReader.ReadToEnd();
						var data = JSONHelper.ReadJSON<dynamic>(json);						
						if (data is JObject jobj)
						{
							foreach (string name in jobj.Names)
							{
								bodyParameters.Add(name.ToLower(), jobj[name]);
							}
						}
						else if (data is JArray jArray )
						{							
							bodyParameters.Add(string.Empty, jArray);
						}
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, ex, "Parsing error in Body ");

					}
				}
			}
			return bodyParameters;
		}
	}
}
