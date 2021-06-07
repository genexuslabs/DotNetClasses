using GeneXus.Configuration;
using GeneXus.Services;
using log4net;
using net.openstack.Core.Domain;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Core.Providers;
using net.openstack.Providers.Rackspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneXus.Utils;
using GeneXus.Encryption;

namespace GeneXus.Storage.GXOpenStack
{
	public class ExternalProviderOpenStack : ExternalProvider
	{
		private static CloudFilesProvider openstackFilesProvider;
		private static string publicBucketName;
		private static string privateBucketName;
		private static string folderName;
		private static string serverUrl;
		private static string storageUrl;

		public ExternalProviderOpenStack()
			: this(ServiceFactory.GetGXServices().Get(GXServices.STORAGE_SERVICE))
		{
		}

		public ExternalProviderOpenStack(GXService providerService)
		{
			var identityEndpoint = new Uri(providerService.Properties.Get("SERVER_URL"));
			var identity = new CloudIdentityWithProject
			{
				Username = CryptoImpl.Decrypt(providerService.Properties.Get("STORAGE_PROVIDER_USER")),
				Password = CryptoImpl.Decrypt(providerService.Properties.Get("STORAGE_PROVIDER_PASSWORD")),
				ProjectName = providerService.Properties.Get("TENANT_NAME"),
			};

			OpenStackIdentityProvider identityProvider = new OpenStackIdentityProvider(identityEndpoint, identity);

			GetStorageEndpoint(identityProvider, identity);

			openstackFilesProvider = new CloudFilesProvider(null, "regionOne", identityProvider, null);
			publicBucketName = CryptoImpl.Decrypt(providerService.Properties.Get("PUBLIC_BUCKET_NAME"));
			privateBucketName = CryptoImpl.Decrypt(providerService.Properties.Get("PRIVATE_BUCKET_NAME"));
			folderName = providerService.Properties.Get("FOLDER_NAME");
			serverUrl = providerService.Properties.Get("SERVER_URL");

			CreateBuckets();
			CreateFolder(folderName);
		}
		public string StorageUri
		{
			get { return storageUrl; }
		}
		public ExternalProviderOpenStack(string url, string user, string pass, string tenant, string bucket)
		{
			var identityEndpoint = new Uri(url);
			var identity = new CloudIdentityWithProject
			{
				Username = user,
				Password = pass,
				ProjectName = tenant,
			};

			OpenStackIdentityProvider identityProvider = new OpenStackIdentityProvider(identityEndpoint, identity);

			GetStorageEndpoint(identityProvider, identity);

			openstackFilesProvider = new CloudFilesProvider(null, "regionOne", identityProvider, null);
			publicBucketName = bucket;
			serverUrl = url;

			CreateBuckets();
		}

		private void GetStorageEndpoint(OpenStackIdentityProvider identityProvider, CloudIdentityWithProject identity)
		{
			UserAccess user = identityProvider.GetUserAccess(identity);
			var catalog = user.ServiceCatalog;
			Endpoint objectStorageEndpoint = null;
			foreach (ServiceCatalog service in catalog)
				if (service.Type == "object-store")
					if (service.Endpoints.Any())
						objectStorageEndpoint = service.Endpoints.First();

			storageUrl = objectStorageEndpoint.PublicURL;

			if (String.IsNullOrEmpty(storageUrl))
				throw new Exception("Couldn't found object storage endpoint, please check credentials in storage configuration.");
		}

		private Dictionary<string, string> CreateObjectMetadata(string tableName, string fieldName, string key)
		{
			Dictionary<string, string> metadata = new Dictionary<string, string>();
			metadata.Add("Table", tableName);
			metadata.Add("Field", fieldName);
			metadata.Add("KeyValue", key);
			return metadata;
		}

		private static void CreateBuckets()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("X-Container-Read", ".r:*");

			openstackFilesProvider.CreateContainer(publicBucketName, headers);
			openstackFilesProvider.CreateContainer(privateBucketName);
		}

		private static void CreateFolder(string folder, string table = null, string field = null)
		{
			string name = normalizeDirectoryName(folder);
			if (table != null)
				name += table + StorageUtils.DELIMITER;
			if (field != null)
				name += field + StorageUtils.DELIMITER;
			using (var stream = new MemoryStream())
			{
				openstackFilesProvider.CreateObject(publicBucketName, stream, name, "application/directory");
			}
		}

		private string GetBucket(GxFileType fileType)
		{
			if (fileType.HasFlag(GxFileType.Private))
			{
				return privateBucketName;
			}
			return publicBucketName;
		}

		public void Download(string externalFileName, string localFile, GxFileType fileType)
		{
			string localDirectory = Path.GetDirectoryName(localFile);
			string localFileName = Path.GetFileName(localFile);
			openstackFilesProvider.GetObjectSaveToFile(GetBucket(fileType), localDirectory, externalFileName, localFileName);
		}

		public string Upload(string localFile, string externalFileName, GxFileType fileType)
		{
			openstackFilesProvider.CreateObjectFromFile(GetBucket(fileType), localFile, externalFileName);
			return GetURL(externalFileName, fileType);
		}

		public string Get(string externalFileName, GxFileType fileType, int urlMinutes)
		{
			openstackFilesProvider.GetObjectMetaData(GetBucket(fileType), externalFileName);
			return GetURL(externalFileName, fileType);
		}

		public void Delete(string objectName, GxFileType fileType)
		{
			openstackFilesProvider.DeleteObject(GetBucket(fileType), objectName);
		}

		public bool Exists(string objectName, GxFileType fileType)
		{
			try
			{
				openstackFilesProvider.GetObjectMetaData(GetBucket(fileType), objectName);
				return true;
			}
			catch (ItemNotFoundException)
			{
				return false;
			}
		}

		private string GetURL(string objectName, GxFileType fileType)
		{
			return storageUrl + StorageUtils.DELIMITER + GetBucket(fileType) + StorageUtils.DELIMITER + objectName;
		
		}
		public string Rename(string objectName, string newName, GxFileType fileType)
		{
			Copy(objectName, fileType, newName, fileType);
			Delete(objectName, fileType);
			return GetURL(newName, fileType);
		}

		public string Copy(string objectName, GxFileType sourceFileType, string newName, GxFileType targetFileType)
		{
			openstackFilesProvider.CopyObject(GetBucket(sourceFileType), objectName, GetBucket(targetFileType), newName);
			return GetURL(newName, targetFileType);
		}

		public string GetDirectory(string directoryName)
		{
			directoryName = normalizeDirectoryName(directoryName);
			if (ExistsDirectory(directoryName))
				return publicBucketName + StorageUtils.DELIMITER + directoryName;
			else
				return string.Empty;
		}

		public long GetLength(string objectName, GxFileType fileType)
		{
			foreach (ContainerObject obj in openstackFilesProvider.ListObjects(GetBucket(fileType)))
				if (obj.Name.Equals(objectName))
					return obj.Bytes;
			return 0;
		}

		public DateTime GetLastModified(string objectName, GxFileType fileType)
		{
			foreach (ContainerObject obj in openstackFilesProvider.ListObjects(GetBucket(fileType)))
				if (obj.Name.Equals(objectName))
					return obj.LastModified.UtcDateTime;
			return new DateTime();
		}

		public void CreateDirectory(string directoryName)
		{
			CreateFolder(directoryName);
		}

		public void DeleteDirectory(string directoryName)
		{
			List<string> objs = new List<string>();
			foreach (ContainerObject obj in openstackFilesProvider.ListObjects(publicBucketName, prefix: normalizeDirectoryName(directoryName)))
			{
				objs.Add(obj.Name);
			}
			openstackFilesProvider.DeleteObjects(publicBucketName, objs);
		}

		public bool ExistsDirectory(string directoryName)
		{
			List<String> directories = GetDirectories();
			return directories.Contains(directoryName) || directories.Contains(normalizeDirectoryName(directoryName));
		}

		public void RenameDirectory(string directoryName, string newDirectoryName)
		{
			directoryName = normalizeDirectoryName(directoryName);
			newDirectoryName = normalizeDirectoryName(newDirectoryName);
			foreach (ContainerObject obj in openstackFilesProvider.ListObjects(publicBucketName, prefix: directoryName))
			{
				openstackFilesProvider.CopyObject(publicBucketName, obj.Name, publicBucketName, obj.Name.Replace(directoryName, newDirectoryName));
			}
			DeleteDirectory(directoryName);
		}

		public List<string> GetSubDirectories(string directoryName)
		{
			return GetDirectories(normalizeDirectoryName(directoryName));
		}

		private List<String> GetDirectories(string directoryName = null)
		{
			List<string> subdir = new List<string>();
			foreach (ContainerObject obj in openstackFilesProvider.ListObjects(publicBucketName, prefix: directoryName))
			{
				if (directoryName == null)
				{
					string dir = "";
					string[] parts = obj.Name.Split(Convert.ToChar(StorageUtils.DELIMITER));
					for (int i = 0; i < parts.Length - 1; i++)
					{
						dir += parts[i] + StorageUtils.DELIMITER;
						if (!subdir.Contains(dir))
							subdir.Add(dir);
					}
				}
				else
				{
					string name = obj.Name.Replace(directoryName, "");
					int i = name.IndexOf(StorageUtils.DELIMITER);
					if (i != -1)
					{
						name = name.Substring(0, i);
						string dir = normalizeDirectoryName(directoryName + name);
						if (!subdir.Contains(dir))
							subdir.Add(dir);
					}
				}

			}
			if (directoryName != null)
			{
				subdir.Remove(directoryName);
			}
			return subdir;
		}

		private bool IsDirectory(ContainerObject obj, string directoryName = null)
		{
			if (directoryName == null)
				return obj.Name.EndsWith(StorageUtils.DELIMITER);
			else
			{
				string[] name = obj.Name.Replace(normalizeDirectoryName(directoryName), "").Split(Convert.ToChar(StorageUtils.DELIMITER));
				return name.Length == 2 && String.IsNullOrEmpty(name[1]);
			}
		}

		public List<string> GetFiles(string directoryName, string filter = "")
		{
			directoryName = normalizeDirectoryName(directoryName);
			List<string> files = new List<string>();
			foreach (ContainerObject obj in openstackFilesProvider.ListObjects(publicBucketName, prefix: directoryName))
			{
				if (IsFile(obj, directoryName) && (String.IsNullOrEmpty(filter) || obj.Name.Contains(filter)))
					files.Add(obj.Name);
			}
			return files;
		}

		private bool IsFile(ContainerObject obj, string directory = null)
		{
			char delimiter = Convert.ToChar(StorageUtils.DELIMITER);
			if (directory == null)
				return obj.Name.Split(delimiter).Length == 1;
			else
				return obj.Name.Replace(normalizeDirectoryName(directory), "").Split(delimiter).Length == 1 && !String.IsNullOrEmpty(obj.Name.Replace(normalizeDirectoryName(directory), "").Split(delimiter)[0]);
		}

		private static String normalizeDirectoryName(String directoryName)
		{
			directoryName.Replace("\\", StorageUtils.DELIMITER);
			if (!directoryName.EndsWith(StorageUtils.DELIMITER))
				return directoryName + StorageUtils.DELIMITER;
			return directoryName;
		}

		public string Upload(string fileName, Stream stream, GxFileType fileType)
		{			
			openstackFilesProvider.CreateObject(GetBucket(fileType), stream, fileName);
			return GetURL(fileName, fileType);
		}

		public string Copy(string url, string newName, string tableName, string fieldName, GxFileType fileType)
		{
			throw new NotImplementedException();
		}

		public Stream GetStream(string objectName, GxFileType fileType)
		{
			using (Stream stream = new MemoryStream())
			{
				openstackFilesProvider.GetObject(GetBucket(fileType), objectName, stream);
				return stream;
			}
		}

        public bool GetMessageFromException(Exception ex, SdtMessages_Message msg)
        {
            return false;
        }

		public string Save(Stream fileStream, string fileName, string tableName, string fieldName, GxFileType fileType)
		{
			fileName = tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + fileName;
			return Upload(fileName, fileStream, fileType);
		}

		public string GetBaseURL()
		{
			return storageUrl + StorageUtils.DELIMITER + publicBucketName + StorageUtils.DELIMITER;
		}

		public bool TryGetObjectNameFromURL(string url, out string objectName)
		{
			string baseUrl = storageUrl + StorageUtils.DELIMITER + publicBucketName + StorageUtils.DELIMITER;
			if (url.StartsWith(baseUrl))
			{
				objectName = url.Replace(baseUrl, string.Empty);
				return true;
			}
			objectName = null;
			return false;
		}

	}
}