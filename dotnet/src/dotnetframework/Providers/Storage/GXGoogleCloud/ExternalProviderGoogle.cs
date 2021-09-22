using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using GeneXus.Services;
using GeneXus.Utils;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

namespace GeneXus.Storage.GXGoogleCloud
{
	public class ExternalProviderGoogle : ExternalProviderBase, ExternalProvider
    {
		public static String Name = "GOOGLECS";  //Google Cloud Storage
		const int BUCKET_EXISTS = 409;
        const int OBJECT_NOT_FOUND = 404;
        const string APPLICATION_NAME = "APPLICATION_NAME";
        const string PROJECT_ID = "PROJECT_ID";
        const string KEY = "KEY";
        const string BUCKET = "BUCKET_NAME";
		

        StorageClient Client { get; set; }
        StorageService Service { get; set; }
        String Project { get; set; }
        String Bucket { get; set; }
        UrlSigner Signer { get; set; }
		

        public string StorageUri
        {
            get { return $"https://{Bucket}.storage.googleapis.com/"; }
        }

		public override string GetName()
		{
			return Name;

		}

		public ExternalProviderGoogle() : this(null)
		{
		}

		public ExternalProviderGoogle(GXService providerService) : base(providerService)
		{
			Initialize();
		}

			
		private void Initialize()
		{
            GoogleCredential credentials;
			string key = GetEncryptedPropertyValue(KEY);

            using (Stream stream = KeyStream(key))
            {
                credentials = GoogleCredential.FromStream(stream).CreateScoped(StorageService.Scope.CloudPlatform);				
			}

			using (Stream stream = KeyStream(key))
			{
				Signer = UrlSigner.FromServiceAccountData(stream);
			}

			Client = StorageClient.Create(credentials);

            Service = new StorageService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = GetPropertyValue(APPLICATION_NAME)
            });

			Bucket = GetEncryptedPropertyValue(BUCKET);
			Project = GetPropertyValue(PROJECT_ID);

            CreateBucket();
            CreateFolder(Folder);
        }

        private Stream KeyStream(string key)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(key);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void CreateBucket()
        {
			try
			{
				//objects in bucket are public by default
				ObjectAccessControl defaultAccess = new ObjectAccessControl();
				defaultAccess.Entity = "allUsers";
				defaultAccess.Role = "READER";

				Bucket bucket = new Bucket();
				bucket.Name = Bucket;
				bucket.DefaultObjectAcl = new List<ObjectAccessControl> { defaultAccess };
				
				if (Client.GetBucket(Bucket) == null)
				{
					try
					{
						Client.CreateBucket(Project, bucket);
					}
					catch (GoogleApiException ex)
					{
						if (ex.Error.Code != BUCKET_EXISTS)
							throw ex;
					}
				}
			}
			catch (Exception) {
				//We should not fail when Bucket cannot be created. Bucket should be created beforehand as Credentials may not allow Bucket creation.
			}
        }

        private void CreateFolder(String name, string table = null, string field = null)
        {
            name = StorageUtils.NormalizeDirectoryName(name);
            if (table != null)
                name += table + StorageUtils.DELIMITER;
            if (field != null)
                name += field + StorageUtils.DELIMITER;
            
            using (var stream = new MemoryStream())
            {
				Service.Objects.Insert(
					bucket: Bucket,
					stream: stream,
					contentType: "application/x-directory",
					body: new Google.Apis.Storage.v1.Data.Object()
					{
						Name = name,
						Bucket = Bucket
					}
				).Upload();
            }
        }

        public string Upload(string fileName, Stream stream, GxFileType fileType)
        {
            Google.Apis.Storage.v1.Data.Object obj = new Google.Apis.Storage.v1.Data.Object();
            obj.Name = fileName;
            obj.Bucket = Bucket;

			if (TryGetContentType(fileName, out string mimeType, DEFAULT_CONTENT_TYPE))
			{
				obj.ContentType = mimeType;
			}

            Client.UploadObject(obj, stream, GetUploadOptions(fileType));
			return GetURL(fileName, fileType);
        }

        private Dictionary<string, string> CreateObjectMetadata(string tableName, string fieldName, string name)
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("Table", tableName);
            metadata.Add("Field", fieldName);
            metadata.Add("KeyValue", name);
            return metadata;
        }

        public string Copy(string objectName, GxFileType sourceFileType, string newName, GxFileType targetFileType)
		{			
			Client.CopyObject(Bucket, objectName, Bucket, newName, GetCopyOptions(targetFileType));
			return GetURL(objectName, targetFileType);
		}

		

		private PredefinedObjectAcl GetPredefinedAcl(GxFileType acl)
		{
			PredefinedObjectAcl objectPredefinedObjectAcl = PredefinedObjectAcl.PublicRead;
			if (acl.HasFlag(GxFileType.Private))
			{
				objectPredefinedObjectAcl = PredefinedObjectAcl.ProjectPrivate;
			}
			else if (acl.HasFlag(GxFileType.PublicRead))
			{
				objectPredefinedObjectAcl = PredefinedObjectAcl.PublicRead;
			}
			else if (this.defaultAcl == GxFileType.Private)
			{
				objectPredefinedObjectAcl = PredefinedObjectAcl.Private;
			}
			return objectPredefinedObjectAcl;
		}

		private CopyObjectOptions GetCopyOptions(GxFileType acl)
		{
			return new CopyObjectOptions() { DestinationPredefinedAcl = GetPredefinedAcl(acl) };
		}

		private UploadObjectOptions GetUploadOptions(GxFileType acl)
		{
			return new UploadObjectOptions() { PredefinedAcl = GetPredefinedAcl(acl) };
		}

		public void Delete(string objectName, GxFileType fileType)
		{
			Client.DeleteObject(Bucket, objectName);
        }

        public string Upload(string localFile, string objectName, GxFileType fileType)
		{
            using (FileStream stream = new FileStream(localFile, FileMode.Open))
			{
				TryGetContentType(objectName, out string mimeType, DEFAULT_CONTENT_TYPE);
				Google.Apis.Storage.v1.Data.Object obj = Client.UploadObject(Bucket, objectName, mimeType, stream, GetUploadOptions(fileType));
				return GetURL(objectName, fileType);
			}
		}

		

		public void Download(string objectName, string localFile, GxFileType fileType)
        {
            using (var fileStream = new System.IO.FileStream(localFile, FileMode.Create))
            {
                Client.DownloadObject(Bucket, objectName, fileStream);
            }
        }

        public string Rename(string objectName, string newName, GxFileType fileType)
		{
            string url = Copy(objectName, fileType, newName, fileType);
            Delete(objectName, fileType);
            return url;
        }

        public bool Exists(string objectName, GxFileType fileType)
		{
            bool exists = true;
            try
            {
                Client.GetObject(Bucket, objectName);
            }
            catch (GoogleApiException ex)
            {
                if (ex.Error.Code == OBJECT_NOT_FOUND)
                    exists = false;
            }
            return exists;
        }

        public string Get(string objectName, GxFileType fileType, int urlMinutes)
        {
			objectName = objectName.Replace("%2F", "/"); //Google Cloud Storage Bug. https://github.com/googleapis/google-cloud-dotnet/pull/3677
			if (Exists(objectName, fileType))
			{
				return GetURL(objectName, fileType, urlMinutes);
			}
			return string.Empty;
        }

		public string GetUrl(string objectName, GxFileType fileType, int urlMinutes)
		{
			return GetURL(objectName, fileType, urlMinutes);
		}

		private bool IsPrivateResource(GxFileType fileType)
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

		private string GetURL(string objectName, GxFileType fileType, int urlMinutes = 0)
		{
			if (IsPrivateResource(fileType))
				return Signer.Sign(Bucket, StorageUtils.EncodeUrl(objectName), ResolveExpiration(urlMinutes), HttpMethod.Get);
			else
			{
				return StorageUri + StorageUtils.EncodeUrl(objectName);
			}
		}
		
        public string Copy(string url, string newName, string tableName, string fieldName, GxFileType fileType)
        {
            newName = Folder + StorageUtils.DELIMITER + tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + newName;
            url = StorageUtils.DecodeUrl(url.Replace(StorageUri, string.Empty));
             
			Google.Apis.Storage.v1.Data.Object obj = Client.CopyObject(Bucket, url, Bucket, newName, GetCopyOptions(fileType));
			
            obj.Metadata = CreateObjectMetadata(tableName, fieldName, newName);
			if (TryGetContentType(newName, out string mimeType, DEFAULT_CONTENT_TYPE))
			{
				obj.ContentType = mimeType;
			}
			Client.UpdateObject(obj);
            return GetURL(newName, fileType, 0);
        }

        public Stream GetStream(string objectName, GxFileType fileType)
		{
            var stream = new MemoryStream();
            Client.DownloadObject(Bucket, objectName, stream);
            return stream;
        }

        public DateTime GetLastModified(string objectName, GxFileType fileType)
		{
            return Client.GetObject(Bucket, objectName).Updated.GetValueOrDefault();
        }

        public long GetLength(string objectName, GxFileType fileType)
		{
			var obj = Client.GetObject(Bucket, objectName);
			return (long)obj.Size.GetValueOrDefault();
        }

        public void CreateDirectory(string directoryName)
        {
            CreateFolder(directoryName);
        }

        public void DeleteDirectory(string directoryName)
        {
            directoryName = StorageUtils.NormalizeDirectoryName(directoryName);

            foreach (Google.Apis.Storage.v1.Data.Object item in Client.ListObjects(Bucket, directoryName))
            {
                if (!item.Name.EndsWith(StorageUtils.DELIMITER))
                    Delete(item.Name, GxFileType.PublicRead);
            }
            foreach (string subdir in GetSubDirectories(directoryName))
                DeleteDirectory(subdir);
            if (Exists(directoryName, GxFileType.PublicRead))
                Delete(directoryName, GxFileType.PublicRead);
        }

        public bool ExistsDirectory(string directoryName)
        {
            directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			return Client.ListObjects(Bucket, directoryName).Any();
		}

        public string GetDirectory(string directoryName)
        {
            if (ExistsDirectory(directoryName))
                return Bucket + StorageUtils.DELIMITER + directoryName;

			return String.Empty;
        }

        public List<string> GetFiles(string directoryName, string filter = "")
        {
            directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
            List<string> files = new List<string>();
            ObjectsResource.ListRequest request = Service.Objects.List(Bucket);
            request.Prefix = directoryName;
            Google.Apis.Storage.v1.Data.Objects response;
            do
            {
                response = request.Execute();
                if (response.Items == null)
                {
                    continue;
                }
                foreach (Google.Apis.Storage.v1.Data.Object item in response.Items)
                {
                    if (IsFile(item.Name, directoryName) && (String.IsNullOrEmpty(filter) || item.Name.Contains(filter)))
                    {
                        files.Add(item.Name);
                    }
                }
                request.PageToken = response.NextPageToken;
            } while (response.NextPageToken != null);
            return files;
        }

        public List<string> GetSubDirectories(string directoryName)
        {
            directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
            List<string> subDirs = new List<string>();
            ObjectsResource.ListRequest request = Service.Objects.List(Bucket);
            request.Prefix = directoryName;
            request.Delimiter = StorageUtils.DELIMITER;
            Google.Apis.Storage.v1.Data.Objects response;
            do
            {
                response = request.Execute();
                if (response.Prefixes == null)
                {
                    continue;
                }
                foreach (string dir in response.Prefixes)
                {
                    subDirs.Add(dir);
                }
                request.PageToken = response.NextPageToken;
            } while (response.NextPageToken != null);
            return subDirs;
        }

        public void RenameDirectory(string directoryName, string newDirectoryName)
        {
            directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
            newDirectoryName = StorageUtils.NormalizeDirectoryName(newDirectoryName);
            ObjectsResource.ListRequest request = Service.Objects.List(Bucket);
            request.Prefix = directoryName;
            Google.Apis.Storage.v1.Data.Objects response;
            do
            {
                response = request.Execute();
                foreach (Google.Apis.Storage.v1.Data.Object item in response.Items)
                {
                    if (IsFile(item.Name))
                    {
                        Copy(item.Name, GxFileType.PublicRead, item.Name.Replace(directoryName, newDirectoryName), GxFileType.PublicRead);
                        Delete(item.Name, GxFileType.PublicRead);
                    }
                }
                CreateDirectory(newDirectoryName);
                if (Exists(directoryName, GxFileType.PublicRead))
                    Delete(directoryName, GxFileType.PublicRead);
                request.PageToken = response.NextPageToken;
            } while (response.NextPageToken != null);
            foreach (string subdir in GetSubDirectories(directoryName))
            {
                RenameDirectory(subdir, subdir.Replace(directoryName, newDirectoryName));
                DeleteDirectory(subdir);
            }
        }

        private bool IsFile(string objectName, string directoryName = "")
        {
            return !objectName.EndsWith(StorageUtils.DELIMITER) && (String.IsNullOrEmpty(directoryName) || !objectName.Remove(0, directoryName.Length).Contains(StorageUtils.DELIMITER));
        }

        public bool GetMessageFromException(Exception ex, SdtMessages_Message msg)
        {
            try
            {
                GoogleApiException gex = (GoogleApiException)ex;
                msg.gxTpr_Id = gex.HttpStatusCode.ToString(); //should be the Status Code https://github.com/google/google-api-dotnet-client/issues/899
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

		public string Save(Stream fileStream, string fileName, string tableName, string fieldName, GxFileType fileType)
		{
			fileName = Folder + StorageUtils.DELIMITER + tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + fileName;
			return Upload(fileName, fileStream, fileType);
		}
		public bool TryGetObjectNameFromURL(string url, out string objectName)
		{
			string baseUrl = StorageUri;
			if (url.StartsWith(baseUrl))
			{
				objectName = url.Replace(baseUrl, string.Empty);
				objectName = objectName.Replace("%2F", "/"); //Google Cloud Storage Bug. https://github.com/googleapis/google-cloud-dotnet/pull/3677
				return true;
			}
			baseUrl = $"https://storage.googleapis.com/{Bucket}/";
			if (url.StartsWith(baseUrl))
			{
				objectName = url.Replace(baseUrl, string.Empty);
				objectName = objectName.Replace("%2F", "/"); //Google Cloud Storage Bug. https://github.com/googleapis/google-cloud-dotnet/pull/3677
				return true;
			}
			objectName = null;
			return false;
		}
		public string GetBaseURL()
		{
			return StorageUri + StorageUtils.DELIMITER + Folder + StorageUtils.DELIMITER;
		}

	}
}

