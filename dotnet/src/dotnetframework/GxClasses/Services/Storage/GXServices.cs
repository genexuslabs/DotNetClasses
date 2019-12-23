using GeneXus.Application;
using GeneXus.Utils;
using GeneXus.XML;
using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if NETCORE
using GxClasses.Helpers;
#endif
using System.Reflection.Emit;

namespace GeneXus.Services
{
	public class GXServices
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Services.GXServices));
		public static string STORAGE_SERVICE = "Storage";
		public static string CACHE_SERVICE = "Cache";
		public static string WEBNOTIFICATIONS_SERVICE = "WebNotifications";
		private static string[] SERVICES_FILE = new string[] { "CloudServices.dev.config", "CloudServices.config" };
		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		private Dictionary<string, GXService> services = new Dictionary<string, GXService>();
		private static GXServices s_instance = null;
		private static object syncRoot = new Object();

		public static GXServices Instance
		{
			get
			{
				if (s_instance == null)
				{
					lock (syncRoot)
					{
						if (s_instance == null)
						{
							foreach (string file in SERVICES_FILE)
							{
								LoadFromFile(file, ref s_instance);
							}
						}
					}
				}
				return s_instance;
			}
		}

		public static void LoadFromFile(string fileName, ref GXServices services)
		{
			string filePath = ServicesFilePath(fileName);
			if (File.Exists(filePath))
			{
				try
				{
					if (services == null)
						services = new GXServices();
					GXLogging.Debug(log, "Loading service:", filePath);
					GXXMLReader reader = new GXXMLReader();
					reader.Open(filePath);
					reader.ReadType(1, "Services");
					reader.Read();
					if (reader.ErrCode == 0)
					{
						while (reader.Name != "Services")
						{
							services.ProcessService(reader);
							reader.Read();
							if (reader.Name == "Service" && reader.NodeType == 2) //</Service>
								reader.Read();
						}
						reader.Close();
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Couldn't create service from:", filePath, ex);
					throw ex;
				}
			}
		}

		private static string ServicesFilePath(string file)
		{
			string path = GxContext.StaticPhysicalPath();
			if (path.EndsWith("\\bin"))
				return Path.Combine(path.Substring(0, path.LastIndexOf("\\bin")), file);
			else
				return Path.Combine(path, file);
		}

		private void ProcessService(GXXMLReader reader)
		{
			reader.ReadType(1, "Name");
			String name = reader.Value;

			reader.ReadType(1, "Type");
			String type = reader.Value;

			reader.ReadType(1, "ClassName");
			String className = reader.Value;

			String allowMultiple = string.Empty;
			reader.Read();
			if (reader.Name == "AllowMultiple")
			{ 
				allowMultiple = reader.Value;
				reader.Read();
			}

			GXProperties properties = ProcessProperties(reader);


			GXService service = new GXService();
			service.Name = name;
			service.Type = type;
			service.ClassName = className;
			service.Properties = properties;
			service.AllowMultiple = string.IsNullOrEmpty(allowMultiple) ? false : bool.Parse(allowMultiple);
			if (service.AllowMultiple)
				services.Add($"{service.Type}:{service.Name}", service);
			else
				services.Add(type, service);

		}

		private GXProperties ProcessProperties(GXXMLReader reader)
		{
			GXProperties properties = new GXProperties();
			reader.Read();
			while (reader.Name == "Property")
			{
				reader.ReadType(1, "Name");
				string name = reader.Value;
				reader.ReadType(1, "Value");
				string value = reader.Value;
				properties.Add(name, value);
				reader.Read();
				reader.Read();
			}
			return properties;
		}

		public GXService Get(string type)
		{
			if (services.ContainsKey(type))
			{
				return services[type];
			}
			return null;
		}
	}

	public class GXService
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string ClassName { get; set; }
		public bool AllowMultiple { get; set; }
		public GXProperties Properties { get; set; }
	}

	public class ServiceFactory
	{
		private static ExternalProvider externalProvider = null;
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Services.ServiceFactory));

		public static GXServices GetGXServices()
		{
			return GXServices.Instance;
		}

		public static ExternalProvider GetExternalProvider()
		{
			if (externalProvider == null)
			{
				GXService providerService = GetGXServices().Get(GXServices.STORAGE_SERVICE);
				if (providerService != null)
				{
					try
					{
						string typeFullName = providerService.ClassName;
						GXLogging.Debug(log, "Loading storage provider:", typeFullName);
#if !NETCORE
						Type type = Type.GetType(typeFullName, true, true);
#else
						Type type = new AssemblyLoader(FileUtil.GetStartupDirectory()).GetType(typeFullName);
#endif

						externalProvider = (ExternalProvider)Activator.CreateInstance(type);
					}
					catch (Exception e)
					{
						GXLogging.Error(log, "Couldn´t connect to external storage provider.", e.Message, e);
						throw e;
					}
				}
			}
			return externalProvider;
		}
	}


	public interface ExternalProvider
	{
		string Upload(string fileName, Stream stream, GxFileType fileType);
		string Upload(string localFile, string objectName, GxFileType fileType);
		void Download(string objectName, string localFile, GxFileType fileType);
		string Get(string objectName, GxFileType fileType, int urlMinutes);
		void Delete(string objectName, GxFileType fileType);
		bool Exists(string objectName, GxFileType fileType);
		string Rename(string objectName, string newName, GxFileType fileType);
		string Copy(string objectName, GxFileType sourceFileType, string newName, GxFileType destinationFileType);
		string Copy(string url, string newName, string tableName, string fieldName, GxFileType fileType);
		string Save(Stream fileStream, string fileName, string tableName, string fieldName, GxFileType fileType);
		long GetLength(string objectName, GxFileType fileType);
		DateTime GetLastModified(string objectName, GxFileType fileType);
		void CreateDirectory(string directoryName);
		void DeleteDirectory(string directoryName);
		string GetDirectory(string directoryName);
		bool ExistsDirectory(string directoryName);
		void RenameDirectory(string directoryName, string newDirectoryName);
		List<String> GetFiles(string directoryName, string filter = "");
		List<String> GetSubDirectories(string directoryName);
		Stream GetStream(string objectName, GxFileType fileType);
		bool GetMessageFromException(Exception ex, SdtMessages_Message msg);
		bool GetObjectNameFromURL(string url, out string objectName);
		string GetBaseURL();
		string StorageUri { get; }
	}
}
