using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;

using Amazon.S3.Transfer;
using GeneXus.Services;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace GeneXus.Storage.GXAmazonS3
{
	public class ExternalProviderS3 : ExternalProviderBase, ExternalProvider
	{
		public const string Name = "AWSS3";
		
		const string ACCESS_KEY = "ACCESS_KEY";
		const string SECRET_ACCESS_KEY = "SECRET_KEY";
		const string STORAGE_CUSTOM_ENDPOINT = "CUSTOM_ENDPOINT";
		const string STORAGE_ENDPOINT = "ENDPOINT";
		const string BUCKET = "BUCKET_NAME";
		const string REGION = "REGION";
		const string STORAGE_CUSTOM_ENDPOINT_VALUE = "custom";

		const string DEFAULT_ENDPOINT = "s3.amazonaws.com";
		const string DEFAULT_REGION = "us-east-1";
		
		[Obsolete("Use Property ACCESS_KEY instead", false)]
		const string ACCESS_KEY_ID_DEPRECATED = "STORAGE_PROVIDER_ACCESSKEYID";
		[Obsolete("Use Property SECRET_ACCESS_KEY instead", false)]
		const string SECRET_ACCESS_KEY_DEPRECATED = "STORAGE_PROVIDER_SECRETACCESSKEY";
		[Obsolete("Use Property REGION instead", false)]
		const string REGION_DEPRECATED = "STORAGE_PROVIDER_REGION";
		[Obsolete("Use Property STORAGE_ENDPOINT instead", false)]
		const string ENDPOINT_DEPRECATED = "STORAGE_ENDPOINT";
		[Obsolete("Use Property STORAGE_CUSTOM_ENDPOINT instead", false)]
		const string STORAGE_CUSTOM_ENDPOINT_DEPRECATED = "STORAGE_CUSTOM_ENDPOINT";
		
		string _storageUri;

		IAmazonS3 Client { get; set; }
		string Bucket { get; set; }
		string Endpoint { get; set; }
		string Region { get; set; }

		bool forcePathStyle = false;
		bool customEndpoint = false;
		
		public string StorageUri
		{
			get {
				return _storageUri;
			}
		}

		public string GetBaseURL()
		{
			return StorageUri + Folder + StorageUtils.DELIMITER;
		}

		public ExternalProviderS3(): this(null)
		{			
		}

		public ExternalProviderS3(GXService providerService): base(providerService)
		{
			Initialize();
		}

		private void Initialize() { 
			string keyId = GetEncryptedPropertyValue(ACCESS_KEY, ACCESS_KEY_ID_DEPRECATED);
			string keySecret = GetEncryptedPropertyValue(SECRET_ACCESS_KEY, SECRET_ACCESS_KEY_DEPRECATED);
			AWSCredentials credentials = null;
			if (!string.IsNullOrEmpty(keyId) && !string.IsNullOrEmpty(keySecret))
			{
				credentials = new BasicAWSCredentials(keyId, keySecret);
			}

			var region = Amazon.RegionEndpoint.GetBySystemName(GetPropertyValue(REGION, REGION_DEPRECATED, DEFAULT_REGION));

			AmazonS3Config config = new AmazonS3Config()
			{
				RegionEndpoint = region
			};

			Endpoint = GetPropertyValue(STORAGE_ENDPOINT, ENDPOINT_DEPRECATED, DEFAULT_ENDPOINT);
			if (Endpoint == STORAGE_CUSTOM_ENDPOINT_VALUE)
			{
				Endpoint = GetPropertyValue(STORAGE_CUSTOM_ENDPOINT, STORAGE_CUSTOM_ENDPOINT_DEPRECATED);
				forcePathStyle = true;
				config.ForcePathStyle = forcePathStyle;
				config.ServiceURL = Endpoint;
				customEndpoint = true;
			}
			else
			{
				if (region == Amazon.RegionEndpoint.USEast1)
				{
					Amazon.AWSConfigsS3.UseSignatureVersion4 = true;
				}
			}

#if NETCORE
			if (credentials != null)
			{
				Client = new AmazonS3ClientExtended(credentials, config);
			}
			else
			{
				Client = new AmazonS3ClientExtended(config);
			}						
#else
			if (credentials != null)
			{
				Client = new AmazonS3Client(credentials, config);
			}
			else
			{
				Client = new AmazonS3Client(config);
			}

#endif
			Bucket = GetEncryptedPropertyValue(BUCKET);
			Region = region.SystemName;

			SetURI();
			CreateBucket();
			CreateFolder(Folder);
		}

		private void SetURI()
		{
			if (customEndpoint)
			{
				_storageUri = !Endpoint.EndsWith("/") ? $"{Endpoint}/{Bucket}/": $"{Endpoint}{Bucket}/";
			}
			else
			{
				if (Region == DEFAULT_REGION)
				{
					_storageUri = (forcePathStyle) ? $"{Endpoint}/" : $"https://{Bucket}.{Endpoint}/";
				}
				else
				{
					_storageUri = $"https://{Bucket}.{Endpoint.Replace("s3.amazonaws.com", $"s3.{Region.ToLower()}.amazonaws.com")}/";
				}
			}
		}

		private void AddObjectMetadata(MetadataCollection metadata, string tableName, string fieldName, string key)
		{
			metadata.Add("Table", tableName);
			metadata.Add("Field", fieldName);
			metadata.Add("KeyValue", StorageUtils.EncodeNonAsciiCharacters(key));
		}

		private void CreateFolder(string folder, string table = null, string field = null)
		{
			string key = StorageUtils.NormalizeDirectoryName(folder);
			if (table != null)
				key += table + StorageUtils.DELIMITER;
			if (field != null)
				key += field + StorageUtils.DELIMITER;
			PutObjectRequest putObjectRequest = new PutObjectRequest
			{
				BucketName = Bucket,
				Key = key
			};

			PutObject(putObjectRequest);
		}

		private DeleteObjectResponse DeleteObject(DeleteObjectRequest request)
		{
			return Client.DeleteObjectAsync(request).GetAwaiter().GetResult();
		}

		private CopyObjectResponse CopyObject(CopyObjectRequest request)
		{
			return Client.CopyObjectAsync(request).GetAwaiter().GetResult();
		}

		private PutObjectResponse PutObject(PutObjectRequest request)
		{
			return Client.PutObjectAsync(request).GetAwaiter().GetResult();
		}

		private GetObjectResponse GetObject(GetObjectRequest request)
		{
			return Client.GetObjectAsync(request).GetAwaiter().GetResult();
		}

		private GetObjectResponse GetObject(string bucketName, string key)
		{
			return Client.GetObjectAsync(bucketName, key).GetAwaiter().GetResult();
		}

		private void PutBucket(PutBucketRequest request)
		{
			Client.PutBucketAsync(request).GetAwaiter().GetResult();
		}

		private bool DoesS3BucketExist()
		{
			return Client.DoesS3BucketExistAsync(Bucket).GetAwaiter().GetResult();
		}

		void WriteResponseStreamToFile(GetObjectResponse response, string filePath)
		{
			response.WriteResponseStreamToFileAsync(filePath, false, CancellationToken.None).GetAwaiter().GetResult();
		}

		private void CreateBucket()
		{
			if (!DoesS3BucketExist())
			{
				PutBucketRequest request = new PutBucketRequest
				{
					BucketName = Bucket,
					UseClientRegion = true
				};
				if (defaultAcl == GxFileType.PublicRead) {
					request.CannedACL = S3CannedACL.PublicRead;
				}
				PutBucket(request);
			}
		}

		public string Upload(string localFile, string objectName, GxFileType fileType)
		{
			PutObjectRequest objectRequest = new PutObjectRequest()
			{
				BucketName = Bucket,
				Key = objectName,
				FilePath = localFile,
				CannedACL = GetCannedACL(fileType)
			};
			PutObject(objectRequest);
			return GetUrlImpl(objectName, fileType);
		}

		private bool IsPrivateUpload(GxFileType fileType)
		{
			return GetCannedACL(fileType) != S3CannedACL.PublicRead;
		}

		public string Get(string objectName, GxFileType fileType, int urlMinutes = 0)
		{
			if (Exists(objectName, fileType))
			{
				return GetUrlImpl(objectName, fileType, urlMinutes);
			}
			else
				return string.Empty;
		}

		public string GetUrl(string objectName, GxFileType fileType, int urlMinutes = 0)
		{			
			return GetUrlImpl(objectName, fileType, urlMinutes);
		}

		private string GetUrlImpl(string objectName, GxFileType fileType, int urlMinutes = 0)
		{
			bool isPrivate = IsPrivateUpload(fileType);
			return (isPrivate)? GetPreSignedUrl(objectName, ResolveExpiration(urlMinutes).TotalMinutes): StorageUri + StorageUtils.EncodeUrlPath(objectName);
			
		}

		private string GetPreSignedUrl(string objectName, double urlMinutes)
		{
			GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
			{
				BucketName = Bucket,
				Key = objectName,
				Expires = DateTime.Now.AddMinutes(urlMinutes),				
			};
			if (customEndpoint && StorageUri.StartsWith("http://"))
			{
				request.Protocol = Protocol.HTTP;
			}
			return Client.GetPreSignedURL(request);
		}

		public void Download(string objectName, string localFile, GxFileType fileType)
		{
			GetObjectRequest request = new GetObjectRequest
			{
				BucketName = Bucket,
				Key = objectName,
			};

			GetObjectResponse response = GetObject(request);
			WriteResponseStreamToFile(response, localFile);
		}

		public void Delete(string objectName, GxFileType fileType)
		{
			DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest
			{
				BucketName = Bucket,
				Key = objectName
			};
			DeleteObject(deleteObjectRequest);
		}
		//https://github.com/aws/aws-sdk-net/blob/master/sdk/src/Services/S3/Custom/_bcl/IO/S3FileInfo.cs
		public bool Exists(string objectName, GxFileType fileType)
		{
			bool exists;
			try
			{
				exists = new S3FileInfo(Client, Bucket, objectName).Exists;
			}
			catch (Exception)
			{
				exists = false;
			}
			return exists;
		}

		public string Rename(string objectName, string newName, GxFileType fileType)
		{
			Copy(objectName, fileType, newName, fileType);
			Delete(objectName, fileType);
			return StorageUri + StorageUtils.EncodeUrlPath(newName);
		}

		public string Copy(string objectName, GxFileType sourceFileType, string newName, GxFileType destFileType)
		{
			CopyObjectRequest request = new CopyObjectRequest
			{
				SourceBucket = Bucket,
				SourceKey = objectName,
				DestinationBucket = Bucket,
				DestinationKey = newName,
				CannedACL = GetCannedACL(destFileType),
				MetadataDirective = S3MetadataDirective.REPLACE
			};

			if (TryGetContentType(newName, out string mimeType, DEFAULT_CONTENT_TYPE))
			{
				request.ContentType = mimeType;
			}

			CopyObject(request);
			return StorageUri + StorageUtils.EncodeUrlPath(newName);
		}

		private S3CannedACL GetCannedACL(GxFileType acl)
		{
			if (acl.HasFlag(GxFileType.Private))
			{
				return S3CannedACL.Private;
			}
			else if (acl.HasFlag(GxFileType.PublicRead))
			{
				return S3CannedACL.PublicRead;
			}
			else if (this.defaultAcl == GxFileType.Private)
			{
				return S3CannedACL.Private;
			}
			else
			{
				return S3CannedACL.PublicRead;
			}
		}

		const long MIN_MULTIPART_POST = 1024 * 1024 * 5; //5MB
		const long MULITIPART_POST_PART_SIZE = 1024 * 1024 * 6; // 6 MB.

		public string Upload(string fileName, Stream stream, GxFileType destFileType)
		{
			bool doSimpleUpload = stream.Length <= MIN_MULTIPART_POST;
			if (doSimpleUpload)
			{
				return UploadSimple(fileName, stream, destFileType);
			}
			else
			{
				return UploadMultipart(fileName, stream, destFileType);
			}			
		}

		private string UploadMultipart(string fileName, Stream stream, GxFileType destFileType)
		{			
			TransferUtility transfer = new TransferUtility(Client);
			var uploadRequest = new TransferUtilityUploadRequest
			{
				BucketName = Bucket,
				Key = fileName,				
				PartSize = MULITIPART_POST_PART_SIZE,
				InputStream = stream,
				CannedACL = GetCannedACL(destFileType)
			};

			if (TryGetContentType(fileName, out string mimeType))
			{
				uploadRequest.ContentType = mimeType;
			}

			transfer.Upload(uploadRequest);

			return Get(fileName, destFileType);
		}

		private string UploadSimple(string fileName, Stream stream, GxFileType destFileType)
		{			
			PutObjectRequest objectRequest = new PutObjectRequest()
			{
				BucketName = Bucket,
				Key = fileName,
				InputStream = stream,
				CannedACL = GetCannedACL(destFileType)
			};
			if (TryGetContentType(fileName, out string mimeType))
			{
				objectRequest.ContentType = mimeType;
			}
			PutObject(objectRequest);

			return Get(fileName, destFileType);
		}

		public string Copy(string url, string newName, string tableName, string fieldName, GxFileType destFileType)
		{
			url = StorageUtils.DecodeUrl(url);
			string resourceKey = Folder + StorageUtils.DELIMITER + tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + newName;
			
			CreateFolder(Folder, tableName, fieldName);
			url = url.Replace(StorageUri, string.Empty);

			CopyObjectRequest request = new CopyObjectRequest
			{
				SourceBucket = Bucket,
				SourceKey = url,
				DestinationBucket = Bucket,
				DestinationKey = resourceKey,
				CannedACL = GetCannedACL(destFileType),
				MetadataDirective = S3MetadataDirective.REPLACE
			};

			if (TryGetContentType(newName, out string mimeType, DEFAULT_CONTENT_TYPE))
			{
				request.ContentType = mimeType;
			}

			AddObjectMetadata(request.Metadata, tableName, fieldName, resourceKey);
			CopyObject(request);

			return StorageUri + StorageUtils.EncodeUrlPath(resourceKey);
		}

		public string Save(Stream fileStream, string fileName, string tableName, string fieldName, GxFileType destFileType)
		{
			string resourceKey = Folder + StorageUtils.DELIMITER + tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + fileName;
			try
			{
				CreateFolder(Folder, tableName, fieldName);
				PutObjectRequest objectRequest = new PutObjectRequest()
				{
					BucketName = Bucket,
					Key = resourceKey,
					InputStream = fileStream,
					CannedACL = GetCannedACL(destFileType)
				};
				if (TryGetContentType(fileName, out string mimeType))
				{
					objectRequest.ContentType = mimeType;
				}

				AddObjectMetadata(objectRequest.Metadata, tableName, fieldName, resourceKey);
				PutObjectResponse result = PutObject(objectRequest);

				return StorageUri + resourceKey;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public Stream GetStream(string objectName, GxFileType destFileType)
		{
			GetObjectResponse getObjRespone = GetObject(Bucket, objectName);
			MemoryStream stream = new MemoryStream();
			getObjRespone.ResponseStream.CopyTo(stream);
			return stream;
		}

		public long GetLength(string objectName, GxFileType destFileType)
		{
			return new S3FileInfo(Client, Bucket, objectName).Length;
		}

		public DateTime GetLastModified(string objectName, GxFileType destFileType)
		{
			return new S3FileInfo(Client, Bucket, objectName).LastWriteTimeUtc;
		}

		public void CreateDirectory(string directoryName)
		{
			CreateFolder(StorageUtils.NormalizeDirectoryName(directoryName));
		}

		public void DeleteDirectory(string directoryName)
		{
			S3DirectoryInfo s3DirectoryInfo = new S3DirectoryInfo(Client, Bucket, directoryName);
			s3DirectoryInfo.Delete(true);
		}

		public bool ExistsDirectory(string directoryName)
		{
			return new S3DirectoryInfo(Client, Bucket, directoryName).Exists;
		}

		public void RenameDirectory(string directoryName, string newDirectoryName)
		{

			S3DirectoryInfo s3DirectoryInfo = new S3DirectoryInfo(Client, Bucket, directoryName);
			if (!new S3DirectoryInfo(Client, Bucket, newDirectoryName).Exists)
				CreateFolder(StorageUtils.NormalizeDirectoryName(newDirectoryName));
			foreach (IS3FileSystemInfo file in s3DirectoryInfo.GetFileSystemInfos())
			{
				if (file.Type == FileSystemType.Directory)
					RenameDirectory(directoryName + "\\" + file.Name, newDirectoryName + "\\" + file.Name);
				else
					Rename(directoryName.Replace("\\", StorageUtils.DELIMITER) + StorageUtils.DELIMITER + file.Name, newDirectoryName.Replace("\\", StorageUtils.DELIMITER) + StorageUtils.DELIMITER + file.Name, GxFileType.PublicRead);
			}
			s3DirectoryInfo.Delete();
		}

		private List<string> Get(string directoryName, FileSystemType type, string filter = "*")
		{
			filter = (string.IsNullOrEmpty(filter)) ? "*" : filter;
			List<string> files = new List<string>();
			S3DirectoryInfo s3DirectoryInfo = GetDirectoryInfo(directoryName);
			IEnumerable<IS3FileSystemInfo> elements = null;

			switch (type)
			{
				case FileSystemType.Directory:
					elements = s3DirectoryInfo.EnumerateDirectories(filter, SearchOption.TopDirectoryOnly);
					break;
				case FileSystemType.File:
					elements = s3DirectoryInfo.EnumerateFiles(filter, SearchOption.TopDirectoryOnly);
					break;
				default:
					throw new NotImplementedException(type.ToString());
			}
			foreach (IS3FileSystemInfo file in elements)
			{
				files.Add(directoryName + StorageUtils.DELIMITER + file.Name);
			}
			return files;
		}

		private S3DirectoryInfo GetDirectoryInfo(string directoryName)
		{
			return new S3DirectoryInfo(Client, Bucket, directoryName.Replace("\\", StorageUtils.DELIMITER).Replace("/", "\\"));
		}

		public List<string> GetSubDirectories(string directoryName)
		{
			return Get(directoryName, FileSystemType.Directory);
		}

		public List<string> GetFiles(string directoryName, string filter = "")
		{
			return Get(directoryName, FileSystemType.File, filter);
		}

		public string GetDirectory(string directoryName)
		{
			S3DirectoryInfo directory = new S3DirectoryInfo(Client, Bucket, directoryName);
			if (directory.Exists)
				return StorageUtils.NormalizeDirectoryName(directory.FullName);
			else
				return string.Empty;
		}

		public bool GetMessageFromException(Exception ex, SdtMessages_Message msg)
		{
			try
			{
				AmazonS3Exception s3ex = (AmazonS3Exception)ex;
				msg.gxTpr_Id = s3ex.ErrorCode;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool TryGetObjectNameFromURL(string url, out string objectName)
		{
			if (url.StartsWith(StorageUri))
			{
				objectName = url.Replace(StorageUri, string.Empty);
				return true;
			}
			objectName = null;
			return false;
		}

		public override string GetName()
		{
			return Name;
		}
	
	}
}
