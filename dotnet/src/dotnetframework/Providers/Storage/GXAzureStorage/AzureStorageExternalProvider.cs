using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using GeneXus.Encryption;
using GeneXus.Mime;
using GeneXus.Services;
using GeneXus.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GeneXus.Storage.GXAzureStorage
{
	public class AzureStorageExternalProvider : ExternalProviderBase, ExternalProvider
	{
		public static String Name = "AZUREBS"; //Azure Blob Storage

		const string ACCOUNT_NAME = "ACCOUNT_NAME";
		const string ACCESS_KEY = "ACCESS_KEY";
		const string PUBLIC_CONTAINER = "PUBLIC_CONTAINER_NAME";
		const string PRIVATE_CONTAINER = "PRIVATE_CONTAINER_NAME";

		string Account { get; set; }
		string Key { get; set; }
		CloudBlobContainer PublicContainer { get; set; }
		CloudBlobContainer PrivateContainer { get; set; }
		CloudBlobClient Client { get; set; }
		
		public string StorageUri
		{
			get { return $"https://{Account}.blob.core.windows.net"; }
		}

		public override string GetName()
		{
			return Name;
		}

		public AzureStorageExternalProvider() : this(null)
		{
		}

		public AzureStorageExternalProvider(GXService providerService) : base(providerService)
		{
			Initialize();
		}

		private void Initialize()
		{
			Account = GetEncryptedPropertyValue(ACCOUNT_NAME);
			Key = GetEncryptedPropertyValue(ACCESS_KEY);

			string publicContainer = GetEncryptedPropertyValue(PUBLIC_CONTAINER);
			string privateContainer = GetEncryptedPropertyValue(PRIVATE_CONTAINER);

			
			StorageCredentials credentials = new StorageCredentials(Account, Key);
			CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, true);

			Client = storageAccount.CreateCloudBlobClient();

			PublicContainer = GetContainer(publicContainer, BlobContainerPublicAccessType.Blob);
			PrivateContainer = GetContainer(privateContainer, BlobContainerPublicAccessType.Off);
		}

		private CloudBlobContainer GetContainer(string container, BlobContainerPublicAccessType accessType)
		{
			CloudBlobContainer cloudContainer = Client.GetContainerReference(container);
			cloudContainer.CreateIfNotExistsAsync(accessType, null, null).GetAwaiter().GetResult();

			return cloudContainer;
		}

		public void Download(string externalFileName, string localFile, GxFileType fileType)
		{
			CloudBlockBlob blob = GetCloudBlockBlob(externalFileName, fileType);
			blob.DownloadToFileAsync(localFile, System.IO.FileMode.Create).GetAwaiter().GetResult();
		}

		public string Upload(string localFile, string externalFileName, GxFileType fileType)
		{
			CloudBlockBlob blob = GetCloudBlockBlob(externalFileName, fileType);

			blob.UploadFromFileAsync(localFile).GetAwaiter().GetResult();
			return GetURL(blob, fileType);
		}

		private CloudBlockBlob GetCloudBlockBlob(string externalFileName, GxFileType fileType)
		{
			CloudBlockBlob blob;
			if (fileType.HasFlag(GxFileType.Private))
			{
				blob = PrivateContainer.GetBlockBlobReference(externalFileName);
			}
			else
			{
				blob = PublicContainer.GetBlockBlobReference(externalFileName);
			}

			return blob;
		}
		
		private bool IsPrivateFile(GxFileType fileType)
		{			
			if (fileType.HasFlag(GxFileType.Private))
			{
				return true;
			}
			if (fileType.HasFlag(GxFileType.PublicRead))
			{
				return false;
			}
			return (this.defaultAcl == GxFileType.PublicRead) ? false : true;
		}

		public string Get(string objectName, GxFileType fileType, int urlMinutes)
		{
			CloudBlockBlob blob = GetCloudBlockBlob(objectName, fileType);

			if (blob.ExistsAsync().GetAwaiter().GetResult())
				return GetURL(blob, fileType, urlMinutes);

			return string.Empty;
		}

		public string GetUrl(string objectName, GxFileType fileType, int urlMinutes = 0)
		{
			CloudBlockBlob blob = GetCloudBlockBlob(objectName, fileType);
			return GetURL(blob, fileType, urlMinutes);
		}

		private string GetURL(CloudBlockBlob blob, GxFileType fileType, int urlMinutes = 0)
		{
			string url = StorageUri + StorageUtils.DELIMITER + blob.Container.Name + StorageUtils.DELIMITER + StorageUtils.EncodeUrl(blob.Name);
			if (IsPrivateFile(fileType))
			{
				SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
				sasConstraints.SharedAccessStartTime = DateTime.UtcNow;
				sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(ResolveExpiration(urlMinutes).TotalMinutes);
				sasConstraints.Permissions = SharedAccessBlobPermissions.Read;
				url += blob.GetSharedAccessSignature(sasConstraints);
			}
			return url;
		}

		public void Delete(string objectName, GxFileType fileType)
		{
			GetCloudBlockBlob(objectName, fileType).DeleteAsync().GetAwaiter().GetResult();
		}

		public bool Exists(string objectName, GxFileType fileType)
		{

			return GetCloudBlockBlob(objectName, fileType).ExistsAsync().GetAwaiter().GetResult();
		}

		public string Rename(string objectName, string newName, GxFileType fileType)
		{
			string ret = Copy(objectName, fileType, newName, fileType);
			Delete(objectName, fileType);
			return ret;
		}

		public string Copy(string objectName, GxFileType sourceFileType, string newName, GxFileType destFileType)
		{
			CloudBlockBlob sourceBlob = GetCloudBlockBlob(objectName, sourceFileType);
			CloudBlockBlob targetBlob = GetCloudBlockBlob(newName, destFileType);
			targetBlob.StartCopyAsync(sourceBlob).GetAwaiter().GetResult();
			return GetURL(targetBlob, destFileType);
		}

		public string Upload(string fileName, Stream stream, GxFileType fileType)
		{
			CloudBlockBlob blob = GetCloudBlockBlob(fileName, fileType);

			if (TryGetContentType(fileName, out string mimeType))
			{
				blob.Properties.ContentType = mimeType;
			}

			blob.UploadFromStreamAsync(stream).GetAwaiter().GetResult();
			return GetURL(blob, fileType);
		}

		public string Copy(string sourceUrl, string newName, string tableName, string fieldName, GxFileType fileType)
		{
			CloudBlockBlob sourceBlob = GetCloudBlockBlob(sourceUrl, GxFileType.Private);
			if (sourceBlob.ExistsAsync().GetAwaiter().GetResult())
			{
				CloudBlockBlob targetBlob = GetCloudBlockBlob(newName, fileType);
				newName = tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + newName;
				targetBlob.Metadata["Table"] = tableName;
				targetBlob.Metadata["Field"] = fieldName;
				targetBlob.Metadata["KeyValue"] = StorageUtils.EncodeUrl(newName);
				
				if (TryGetContentType(newName, out string mimeType, DEFAULT_CONTENT_TYPE))
				{
					targetBlob.Properties.ContentType = mimeType;
				}

				targetBlob.StartCopyAsync(sourceBlob).GetAwaiter().GetResult();
				targetBlob.SetPropertiesAsync().GetAwaiter().GetResult(); //Required to apply new object metadata
				return GetURL(targetBlob, fileType);
			}
			return string.Empty;
		}

		public Stream GetStream(string objectName, GxFileType fileType)
		{
			CloudBlockBlob blob = GetCloudBlockBlob(objectName, fileType);
			Stream stream = new MemoryStream();

			blob.DownloadToStreamAsync(stream).GetAwaiter().GetResult();
			return stream;
		}

		public long GetLength(string objectName, GxFileType fileType)
		{
			CloudBlockBlob blob = GetCloudBlockBlob(objectName, fileType);
			blob.FetchAttributesAsync().GetAwaiter().GetResult();
			return blob.Properties.Length;
		}

		public DateTime GetLastModified(string objectName, GxFileType fileType)
		{
			CloudBlockBlob blob = GetCloudBlockBlob(objectName, fileType);
			blob.FetchAttributesAsync().GetAwaiter().GetResult();
			return blob.Properties.LastModified.GetValueOrDefault().LocalDateTime;
		}

		public void CreateDirectory(string directoryName)
		{
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			CloudBlockBlob blob = PublicContainer.GetBlockBlobReference(directoryName);
			blob.UploadFromByteArrayAsync(Array.Empty<byte>(), 0, 0).GetAwaiter().GetResult();
		}

		public void DeleteDirectory(string directoryName)
		{
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			CloudBlobDirectory directory = PublicContainer.GetDirectoryReference(directoryName);
			foreach (IListBlobItem item in directory.ListBlobsSegmentedAsync(null).GetAwaiter().GetResult().Results)
			{
				
				if (item.GetType() == typeof(CloudBlockBlob))
				{
					CloudBlockBlob blob = (CloudBlockBlob)item;

					blob.DeleteAsync().GetAwaiter().GetResult();

				}
				else if (item.GetType() == typeof(CloudPageBlob))
				{
					CloudPageBlob pageBlob = (CloudPageBlob)item;

					pageBlob.DeleteAsync().GetAwaiter().GetResult();

				}
				else if (item.GetType() == typeof(CloudBlobDirectory))
				{
					CloudBlobDirectory dir = (CloudBlobDirectory)item;

					DeleteDirectory(dir.Prefix);
				}
			}
		}

		public bool ExistsDirectory(string directoryName)
		{
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			CloudBlobDirectory directory = PublicContainer.GetDirectoryReference(directoryName);
			return directory.ListBlobsSegmentedAsync(null).GetAwaiter().GetResult().Results.Any();
		}

		public void RenameDirectory(string directoryName, string newDirectoryName)
		{
			CreateDirectory(newDirectoryName);
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			newDirectoryName = StorageUtils.NormalizeDirectoryName(newDirectoryName);

			CloudBlobDirectory directory = PublicContainer.GetDirectoryReference(directoryName);
			foreach (IListBlobItem item in directory.ListBlobsSegmentedAsync(null).GetAwaiter().GetResult().Results)
			{
				string fileName = string.Empty;

				if (item.GetType() == typeof(CloudBlockBlob))
				{
					CloudBlockBlob blob = (CloudBlockBlob)item;
					fileName = Path.GetFileName(blob.Name);
					Copy(blob.Name, GxFileType.PublicRead, newDirectoryName + fileName, GxFileType.PublicRead);
					Delete(blob.Name, GxFileType.PublicRead);

				}
				else if (item.GetType() == typeof(CloudPageBlob))
				{
					CloudPageBlob pageBlob = (CloudPageBlob)item;
					fileName = Path.GetFileName(pageBlob.Name);
					Copy(directoryName + fileName, GxFileType.PublicRead, newDirectoryName + fileName, GxFileType.PublicRead);

				}
				else if (item.GetType() == typeof(CloudBlobDirectory))
				{
					CloudBlobDirectory dir = (CloudBlobDirectory)item;
					RenameDirectory(directoryName + dir.Prefix, newDirectoryName + dir.Prefix);
				}
			}
			DeleteDirectory(directoryName);
		}

		public List<string> GetSubDirectories(string directoryName)
		{
			List<string> directories = new List<string>();
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			CloudBlobDirectory directory = PublicContainer.GetDirectoryReference(directoryName);
			foreach (IListBlobItem item in directory.ListBlobsSegmentedAsync(null).GetAwaiter().GetResult().Results)
			{
				if (item.GetType() == typeof(CloudBlobDirectory))
				{
					CloudBlobDirectory dir = (CloudBlobDirectory)item;

					directories.Add(dir.Prefix);
				}
			}
			return directories;
		}

		public List<string> GetFiles(string directoryName, string filter = "")
		{
			List<string> files = new List<string>();
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			CloudBlobDirectory directory = PublicContainer.GetDirectoryReference(directoryName);
			foreach (IListBlobItem item in directory.ListBlobsSegmentedAsync(null).GetAwaiter().GetResult().Results)
			{
				if (item.GetType() == typeof(CloudBlockBlob))
				{
					CloudBlockBlob blob = (CloudBlockBlob)item;
					if (String.IsNullOrEmpty(filter) || blob.Name.Contains(filter))
						files.Add(blob.Name);
				}
			}
			return files;
		}

		public string GetDirectory(string directoryName)
		{
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			if (ExistsDirectory(directoryName))
				return PublicContainer.Name + StorageUtils.DELIMITER + directoryName;
			else
				return string.Empty;
		}

		public bool GetMessageFromException(Exception ex, SdtMessages_Message msg)
		{
			try
			{
				StorageException stoex = (StorageException)ex;
				msg.gxTpr_Id = stoex.RequestInformation?.ExtendedErrorInformation?.ErrorCode;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public string Save(Stream fileStream, string fileName, string tableName, string fieldName, GxFileType fileType)
		{
			string newName = tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + fileName;
			return Upload(newName, fileStream, fileType);
		}

		public bool TryGetObjectNameFromURL(string url, out string objectName)
		{
			string baseUrl = StorageUri + StorageUtils.DELIMITER + PublicContainer.Name + StorageUtils.DELIMITER;
			if (url.StartsWith(baseUrl))
			{
				objectName = url.Replace(baseUrl, string.Empty);
				return true;
			}
			baseUrl = StorageUri + StorageUtils.DELIMITER + PrivateContainer.Name + StorageUtils.DELIMITER;
			if (url.StartsWith(baseUrl))
			{
				objectName = url.Replace(baseUrl, string.Empty);
				return true;
			}
			objectName = null;
			return false;
		}

		public string GetBaseURL()
		{
			return StorageUri + StorageUtils.DELIMITER + PrivateContainer.Name + StorageUtils.DELIMITER;
		}

	}
}
