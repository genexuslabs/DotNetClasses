using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using GeneXus.Encryption;
using GeneXus.Services;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GeneXus.Storage.GXAmazonS3
{
	public class ExternalProviderS3 : ExternalProvider
	{
		const int PRIVATE_URL_MINUTES_EXPIRATION = 86400; // 24 hours
		const string ACCESS_KEY_ID = "STORAGE_PROVIDER_ACCESSKEYID";
		const string SECRET_ACCESS_KEY = "STORAGE_PROVIDER_SECRETACCESSKEY";
		const string REGION = "STORAGE_PROVIDER_REGION";
		const string ENDPOINT = "STORAGE_ENDPOINT";
		const string BUCKET = "BUCKET_NAME";
		const string FOLDER = "FOLDER_NAME";

		IAmazonS3 Client { get; set; }
		string Bucket { get; set; }
		string Folder { get; set; }
		string Endpoint { get; set; }

		public string StorageUri
		{
			get { return $"https://{Bucket}.{Endpoint}/"; }
		}

		public string GetBaseURL()
		{
			return StorageUri + Folder + StorageUtils.DELIMITER;
		}

		public ExternalProviderS3()
			: this(ServiceFactory.GetGXServices().Get(GXServices.STORAGE_SERVICE))
		{
		}

		public ExternalProviderS3(GXService providerService)
		{
			string keyId = CryptoImpl.Decrypt(providerService.Properties.Get(ACCESS_KEY_ID));
			string keySecret = CryptoImpl.Decrypt(providerService.Properties.Get(SECRET_ACCESS_KEY));
			AWSCredentials credentials = null;
			if (!string.IsNullOrEmpty(keyId) && !string.IsNullOrEmpty(keySecret))
			{
				credentials = new BasicAWSCredentials(keyId, keySecret);
			}

			var region = Amazon.RegionEndpoint.GetBySystemName(providerService.Properties.Get(REGION));
			Endpoint = providerService.Properties.Get(ENDPOINT);

			AmazonS3Config config = new AmazonS3Config()
			{
				ServiceURL = Endpoint,
				RegionEndpoint = region
			};

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
			Bucket = CryptoImpl.Decrypt(providerService.Properties.Get(BUCKET));
			Folder = providerService.Properties.Get(FOLDER);

			CreateBucket();
			CreateFolder(Folder);
		}

		private void AddObjectMetadata(MetadataCollection metadata, string tableName, string fieldName, string key)
		{
			metadata.Add("Table", tableName);
			metadata.Add("Field", fieldName);
			metadata.Add("KeyValue", EncodeNonAsciiCharacters(key));
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
					UseClientRegion = true,
					// Every bucket is public
					CannedACL = S3CannedACL.PublicRead
				};

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
			PutObjectResponse result = PutObject(objectRequest);
			return Get(objectName, fileType);
		}

		private static bool IsPrivateUpload(GxFileType fileType)
		{
			return fileType.HasFlag(GxFileType.Private);
		}

		public string Get(string objectName, GxFileType fileType, int urlMinutes = PRIVATE_URL_MINUTES_EXPIRATION)
		{
			bool isPrivate = IsPrivateUpload(fileType);
			if (Exists(objectName, fileType))
				if (isPrivate)
					return GetPreSignedUrl(objectName, urlMinutes);
				else
					return StorageUri + StorageUtils.EncodeUrl(objectName);
			else
				return string.Empty;
		}

		private string GetPreSignedUrl(string objectName, int urlMinutes)
		{
			GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
			{
				BucketName = Bucket,
				Key = objectName,
				Expires = DateTime.Now.AddMinutes(urlMinutes)
			};
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
			bool exists = true;
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
			return StorageUri + StorageUtils.EncodeUrl(newName);
		}

		public string Copy(string objectName, GxFileType sourceFileType, string newName, GxFileType destFileType)
		{
			CopyObjectRequest request = new CopyObjectRequest
			{
				SourceBucket = Bucket,
				SourceKey = objectName,
				DestinationBucket = Bucket,
				DestinationKey = newName,
				CannedACL = GetCannedACL(destFileType)
			};
			CopyObject(request);
			return StorageUri + StorageUtils.EncodeUrl(newName);
		}

		private static S3CannedACL GetCannedACL(GxFileType destFileType)
		{
			return (destFileType.HasFlag(GxFileType.Private)) ? S3CannedACL.Private : S3CannedACL.PublicRead;
		}

		public string Upload(string fileName, Stream stream, GxFileType destFileType)
		{
			PutObjectRequest objectRequest = new PutObjectRequest()
			{
				BucketName = Bucket,
				Key = fileName,
				InputStream = stream,
				CannedACL = GetCannedACL(destFileType)
			};
			if (Path.GetExtension(fileName).Equals(".tmp"))
				objectRequest.ContentType = "image/jpeg";
			PutObjectResponse result = PutObject(objectRequest);
			return Get(fileName, destFileType);
		}
		string EncodeNonAsciiCharacters(string value)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in value)
			{
				if (c > 127)
				{
					// This character is too big for ASCII
					string encodedValue = "\\u" + ((int)c).ToString("x4");
					sb.Append(encodedValue);
				}
				else
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}
		string DecodeEncodedNonAsciiCharacters(string value)
		{
			return Regex.Replace(
				value,
				@"\\u(?<Value>[a-zA-Z0-9]{4})",
				m => {
					return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
				});
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
				CannedACL = GetCannedACL(destFileType)
			};
			AddObjectMetadata(request.Metadata, tableName, fieldName, resourceKey);
			CopyObject(request);

			return StorageUri + StorageUtils.EncodeUrl(resourceKey);
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
				AddObjectMetadata(objectRequest.Metadata, tableName, fieldName, resourceKey);
				PutObjectResponse result = PutObject(objectRequest);

				return "https://" + Bucket + ".s3.amazonaws.com/" + resourceKey;
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
					Rename(directoryName.Replace("\\", StorageUtils.DELIMITER) + StorageUtils.DELIMITER + file.Name, newDirectoryName.Replace("\\", StorageUtils.DELIMITER) + StorageUtils.DELIMITER + file.Name, GxFileType.Public);
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

		public bool GetObjectNameFromURL(string url, out string objectName)
		{
			if (url.StartsWith(StorageUri))
			{
				objectName = url.Replace(StorageUri, string.Empty);
				return true;
			}
			objectName = null;
			return false;
		}
	}
}
