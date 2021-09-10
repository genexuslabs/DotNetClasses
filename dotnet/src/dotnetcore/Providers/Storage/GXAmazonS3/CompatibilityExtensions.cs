using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Amazon.S3.IO
{
	public class AmazonS3ClientExtended: AmazonS3Client
	{
		public AmazonS3ClientExtended(AWSCredentials credentials, AmazonS3Config clientConfig) : base(credentials, clientConfig)
		{
		}

		public AmazonS3ClientExtended(AmazonS3Config clientConfig) : base(clientConfig)
		{
		}

		public ListObjectsResponse ListObjects(ListObjectsRequest request)
		{
			Task<ListObjectsResponse> result = ListObjectsAsync(request);
			result.Wait();
			return result.Result;
		}
	}
		//
		// Summary:
		//     Enumeration indicated whether a file system element is a file or directory.
		public enum FileSystemType
	{
		//
		// Summary:
		//     Type is a directory.
		Directory = 0,
		//
		// Summary: 
		//     Type is a file.
		File = 1
	}
}
namespace Amazon.S3.IO
{
	public sealed class S3FileInfo : IS3FileSystemInfo
	{
		private IAmazonS3 s3Client;
		private string bucket;
		private string key;
		/// <summary>
		/// Initialize a new instance of the S3FileInfo class for the specified S3 bucket and S3 object key.
		/// </summary>
		/// <param name="s3Client">S3 client which is used to access the S3 resources.</param>
		/// <param name="bucket">Name of the S3 bucket.</param>
		/// <param name="key">The S3 object key.</param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		public S3FileInfo(IAmazonS3 s3Client, string bucket, string key)
		{
			if (String.IsNullOrEmpty(bucket))
			{
				throw new ArgumentNullException(nameof(bucket));
			}
			if (String.IsNullOrEmpty(key) || String.Equals(key, "\\"))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (key.EndsWith("\\", StringComparison.Ordinal))
			{
				throw new ArgumentException("key is a directory name");
			}

			this.s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
			this.bucket = bucket;
			this.key = key;
		}
		/// <summary>
		/// Deletes the from S3.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public void Delete()
		{
			if (Exists)
			{
				var deleteObjectRequest = new DeleteObjectRequest
				{
					BucketName = bucket,
					Key = S3Helper.EncodeKey(key)
				};
				((Amazon.Runtime.Internal.IAmazonWebServiceRequest)deleteObjectRequest).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
				s3Client.DeleteObjectAsync(deleteObjectRequest).Wait();

				Directory.Create();
			}
		}
		/// <summary>
		/// Returns the parent S3DirectoryInfo.
		/// </summary>
		public S3DirectoryInfo Directory
		{
			get
			{
				int index = key.LastIndexOf('\\');
				string directoryName = null;
				if (index >= 0)
					directoryName = key.Substring(0, index);
				return new S3DirectoryInfo(s3Client, bucket, directoryName);
			}
		}
		/// <summary>
		/// Checks S3 if the file exists in S3 and return true if it does.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public bool Exists
		{
			get
			{
				bool bucketExists;
				return ExistsWithBucketCheck(out bucketExists);
			}
		}
		internal bool ExistsWithBucketCheck(out bool bucketExists)
		{
			bucketExists = true;
			try
			{
				var request = new GetObjectMetadataRequest
				{
					BucketName = bucket,
					Key = S3Helper.EncodeKey(key)
				};
				((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);

				// If the object doesn't exist then a "NotFound" will be thrown
				s3Client.GetObjectMetadataAsync(request).Wait();
				return true;
			}
			catch (AmazonS3Exception e)
			{
				if (string.Equals(e.ErrorCode, "NoSuchBucket"))
				{
					bucketExists = false;
					return false;
				}
				else if (string.Equals(e.ErrorCode, "NotFound"))
				{
					return false;
				}
				throw;
			}
		}
		/// <summary>
		/// Gets the file's extension.
		/// </summary>
		public string Extension
		{
			get
			{
				int index = Name.LastIndexOf('.');
				if (index == -1 || this.Name.Length <= (index + 1))
					return null;

				return this.Name.Substring(index + 1);
			}
		}
		/// <summary>
		/// Returns the full path including the bucket.
		/// </summary>
		public string FullName
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}:\\{1}", bucket, key);
			}
		}
		/// <summary>
		/// Returns the last time the file was modified.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public DateTime LastWriteTime
		{
			get
			{
				DateTime ret = DateTime.MinValue;
				if (Exists)
				{
					
					var request = new GetObjectMetadataRequest
					{
						BucketName = bucket,
						Key = S3Helper.EncodeKey(key),
					};
					((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
					Task<GetObjectMetadataResponse> task = s3Client.GetObjectMetadataAsync(request);
					task.Wait();
					var response = task.Result;
					ret = response.LastModified.ToLocalTime();
				}
				return ret;
			}

		}
		/// <summary>
		/// Returns the type of file system element.
		/// </summary>
		public FileSystemType Type
		{
			get
			{
				return FileSystemType.File;
			}
		}
		/// <summary>
		/// Returns the content length of the file.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public long Length
		{
			get
			{
				long ret = 0;
				if (Exists)
				{
					
					var request = new GetObjectMetadataRequest
					{
						BucketName = bucket,
						Key = S3Helper.EncodeKey(key),
					};
					((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
					Task<GetObjectMetadataResponse> t = s3Client.GetObjectMetadataAsync(request);
					var response = t.Result;
					ret = response.ContentLength;
				}
				return ret;
			}
		}
		/// <summary>
		/// Returns the file name without its directory name.
		/// </summary>
		public string Name
		{
			get
			{
				int index = key.LastIndexOf('\\');
				return key.Substring(index + 1);
			}
		}
		/// <summary>
		/// Returns the last time the file was modified in UTC.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public DateTime LastWriteTimeUtc
		{
			get
			{
				DateTime ret = DateTime.MinValue;
				if (Exists)
				{
					
					var request = new GetObjectMetadataRequest
					{
						BucketName = bucket,
						Key = S3Helper.EncodeKey(key),
					};
					((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
					Task<GetObjectMetadataResponse> t = s3Client.GetObjectMetadataAsync(request);
					var response = t.Result;
					ret = response.LastModified;
				}
				return ret;
			}
		}

	}
	public sealed class S3DirectoryInfo : IS3FileSystemInfo
	{
		const int MULTIPLE_OBJECT_DELETE_LIMIT = 1000;
		const int EVENTUAL_CONSISTENCY_SUCCESS_IN_ROW = 10;
		const int EVENTUAL_CONSISTENCY_POLLING_PERIOD = 1000;
		const long EVENTUAL_CONSISTENCY_MAX_WAIT = 30000;

		private IAmazonS3 s3Client;
		private string bucket;
		private string key;

		internal IAmazonS3 S3Client
		{
			get
			{
				return s3Client;
			}
		}
		internal string BucketName
		{
			get
			{
				return bucket;
			}
		}
		/// <summary>
		/// Initialize a new instance of the S3DirectoryInfo class for the specified S3 bucket and S3 object key.
		/// </summary>
		/// <param name="s3Client">S3 client which is used to access the S3 resources.</param>
		/// <param name="bucket">Name of the S3 bucket.</param>
		/// <param name="key">The S3 object key.</param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		public S3DirectoryInfo(IAmazonS3 s3Client, string bucket, string key)
		{
			if (s3Client == null)
			{
				throw new ArgumentNullException(nameof(s3Client));
			}
			if (String.IsNullOrEmpty(bucket) && !String.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be specified if bucket isn't");
			}

			this.s3Client = s3Client;
			this.bucket = bucket ?? String.Empty;
			this.key = key ?? String.Empty;

			if (!String.IsNullOrEmpty(bucket) && !String.IsNullOrEmpty(key) && !this.key.EndsWith("\\", StringComparison.Ordinal))
			{
				this.key = string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.key, "\\");
			}
			if (String.Equals(this.key, "\\", StringComparison.Ordinal))
			{
				this.key = String.Empty;
			}
		}
		/// <summary>
		/// Creates the directory in S3.  If no object key was specified when creating the S3DirectoryInfo then the bucket will be created.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public void Create()
		{
			bool bucketExists;
			if (!ExistsWithBucketCheck(out bucketExists))
			{
				if (String.IsNullOrEmpty(key))
				{
					var request = new PutBucketRequest
					{
						BucketName = bucket
					};
					((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
					s3Client.PutBucketAsync(request).Wait();

					WaitTillBucketS3StateIsConsistent(true);
				}
				else
				{
					if (!bucketExists)
					{
						var request = new PutBucketRequest
						{
							BucketName = bucket
						};
						((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
						s3Client.PutBucketAsync(request).Wait();

						WaitTillBucketS3StateIsConsistent(true);
					}

					var putObjectRequest = new PutObjectRequest
					{
						BucketName = bucket,
						Key = S3Helper.EncodeKey(key),
						InputStream = new MemoryStream()
					};
					((Amazon.Runtime.Internal.IAmazonWebServiceRequest)putObjectRequest).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);

					s3Client.PutObjectAsync(putObjectRequest).Wait();
				}
			}
		}

		/// <summary>
		/// Creates a sub directory inside the instance of S3DirectoryInfo.
		/// </summary>
		/// <param name="directory"></param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns></returns>
		public S3DirectoryInfo CreateSubdirectory(string directory)
		{
			S3DirectoryInfo ret = null;
			ret = GetDirectory(directory);
			ret.Create();
			return ret;
		}

		/// <summary>
		/// Deletes all the files in this directory as well as this directory.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <exception cref="T:Amazon.S3.Model.DeleteObjectsException"></exception>
		public void Delete()
		{
			Delete(false);
		}

		/// <summary>
		/// Deletes all the files in this directory as well as this directory.  If recursive is set to true then all sub directories will be 
		/// deleted as well.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <exception cref="T:Amazon.S3.Model.DeleteObjectsException"></exception>
		/// <param name="recursive">If true then sub directories will be deleted as well.</param>
		public void Delete(bool recursive)
		{
			if (String.IsNullOrEmpty(bucket))
			{
				throw new NotSupportedException();
			}

			if (recursive)
			{
				ListObjectsRequest listRequest = new ListObjectsRequest
				{
					BucketName = bucket,
					Prefix = S3Helper.EncodeKey(this.key)
				};
				((Amazon.Runtime.Internal.IAmazonWebServiceRequest)listRequest).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);

				DeleteObjectsRequest deleteRequest = new DeleteObjectsRequest
				{
					BucketName = bucket
				};
				((Amazon.Runtime.Internal.IAmazonWebServiceRequest)deleteRequest).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
				ListObjectsResponse listResponse = null;
				do
				{
					Task<ListObjectsResponse> t = s3Client.ListObjectsAsync(listRequest);
					t.Wait();
					listResponse = t.Result;

					// Sort to make sure the Marker for paging is set to the last lexiographical key.
					foreach (S3Object s3o in listResponse.S3Objects.OrderBy(x => x.Key))
					{
						deleteRequest.AddKey(s3o.Key);
						if (deleteRequest.Objects.Count == MULTIPLE_OBJECT_DELETE_LIMIT)
						{
							s3Client.DeleteObjectsAsync(deleteRequest).Wait();
							deleteRequest.Objects.Clear();
						}

						listRequest.Marker = s3o.Key;
					}

				} while (listResponse.IsTruncated);

				if (deleteRequest.Objects.Count > 0)
				{
					s3Client.DeleteObjectsAsync(deleteRequest).Wait();
				}
			}

			if (String.IsNullOrEmpty(key) && Exists)
			{
				var request = new DeleteBucketRequest { BucketName = bucket };
				((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
				s3Client.DeleteBucketAsync(request).Wait();
				WaitTillBucketS3StateIsConsistent(false);
			}
			else
			{
				if (!EnumerateFileSystemInfos().GetEnumerator().MoveNext() && Exists)
				{
					var request = new DeleteObjectRequest { BucketName = bucket, Key = S3Helper.EncodeKey(key) };
					((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);

					s3Client.DeleteObjectAsync(request).Wait();
					Parent.Create();
				}
			}
		}

		/// <summary>
		/// Return the S3DirectoryInfo of the parent directory.
		/// </summary>
		public S3DirectoryInfo Parent
		{
			get
			{
				S3DirectoryInfo ret = null;
				if (!String.IsNullOrEmpty(bucket) && !String.IsNullOrEmpty(key))
				{
					int last = key.LastIndexOf('\\');
					int secondlast = key.LastIndexOf('\\', last - 1);
					if (secondlast == -1)
					{
						ret = Bucket;
					}
					else
					{
						var bucketName = key.Substring(0, secondlast);
						ret = new S3DirectoryInfo(s3Client, bucket, bucketName);
					}
				}
				if (ret == null)
				{
					ret = Root;
				}
				return ret;
			}
		}

		/// <summary>
		/// Returns the S3DirectroyInfo for the S3 account.
		/// </summary>
		public S3DirectoryInfo Root
		{
			get
			{
				return new S3DirectoryInfo(s3Client, "", "");
			}
		}
		/// <summary>
		/// Enumerate the sub directories of this directory.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of directories.</returns>
		public IEnumerable<S3DirectoryInfo> EnumerateDirectories()
		{
			return EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Enumerate the sub directories of this directory.
		/// </summary>
		/// <param name="searchPattern">The search string. The default pattern is "*", which returns all directories.</param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of directories that matches searchPattern.</returns>
		public IEnumerable<S3DirectoryInfo> EnumerateDirectories(string searchPattern)
		{
			return EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Enumerate the sub directories of this directory.
		/// </summary>
		/// <param name="searchPattern">The search string. The default pattern is "*", which returns all directories.</param>
		/// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories. The default value is TopDirectoryOnly.</param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of directories that matches searchPattern and searchOption.</returns>
		public IEnumerable<S3DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
		{
			IEnumerable<S3DirectoryInfo> folders = null;
			if (String.IsNullOrEmpty(bucket))
			{
				var request = new ListBucketsRequest();
				((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
				Task<ListBucketsResponse> t = s3Client.ListBucketsAsync(request);
				t.Wait();
				folders = t.Result.Buckets.ConvertAll(s3Bucket => new S3DirectoryInfo(s3Client, s3Bucket.BucketName, ""));
			}
			else
			{
				var request = new ListObjectsRequest
				{
					BucketName = bucket,
					Delimiter = "/",
					Prefix = S3Helper.EncodeKey(key)
				};
				((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
				folders = new EnumerableConverter<string, S3DirectoryInfo>
					((IEnumerable<string>)(PaginatedResourceFactory.Create<string, ListObjectsRequest, ListObjectsResponse>(new PaginatedResourceInfo()
							.WithClient(s3Client)
							.WithMethodName("ListObjects")
							.WithRequest(request)
							.WithItemListPropertyPath("CommonPrefixes")
							.WithTokenRequestPropertyPath("Marker")
							.WithTokenResponsePropertyPath("NextMarker"))),
						prefix => new S3DirectoryInfo(s3Client, bucket, S3Helper.DecodeKey(prefix)));
			}

			//handle if recursion is set
			if (searchOption == SearchOption.AllDirectories)
			{
				IEnumerable<S3DirectoryInfo> foldersToAdd = new List<S3DirectoryInfo>();
				foreach (S3DirectoryInfo dir in folders)
				{
					foldersToAdd = foldersToAdd.Concat(dir.EnumerateDirectories(searchPattern, searchOption));
				}
				folders = folders.Concat(foldersToAdd);
			}

			//filter based on search pattern
			var regEx = WildcardToRegex(searchPattern);
			folders = folders.Where(s3dirInfo => Regex.IsMatch(s3dirInfo.Name, regEx, RegexOptions.IgnoreCase));
			return folders;
		}
		/// <summary>
		/// Enumerate the files of this directory.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of files.</returns>
		public IEnumerable<S3FileInfo> EnumerateFiles()
		{
			return EnumerateFiles("*", SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Enumerate the sub directories of this directory.
		/// </summary>
		/// <param name="searchPattern">The search string. The default pattern is "*", which returns all files.</param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of files that matches searchPattern.</returns>
		public IEnumerable<S3FileInfo> EnumerateFiles(string searchPattern)
		{
			return EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Enumerate the files of this directory.
		/// </summary>
		/// <param name="searchPattern">The search string. The default pattern is "*", which returns all files.</param>
		/// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories. The default value is TopDirectoryOnly.</param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of files that matches searchPattern and searchOption.</returns>
		public IEnumerable<S3FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
			IEnumerable<S3FileInfo> files = null;
			if (String.IsNullOrEmpty(bucket))
			{
				files = new List<S3FileInfo>();
			}
			else
			{
				var request = new ListObjectsRequest
				{
					BucketName = bucket,
					Delimiter = "/",
					Prefix = S3Helper.EncodeKey(key)
				};
				((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
				PaginatedResourceInfo pagingInfo = new PaginatedResourceInfo().WithClient(s3Client)
							.WithMethodName("ListObjects")
							.WithRequest(request)
							.WithItemListPropertyPath("S3Objects")
							.WithTokenRequestPropertyPath("Marker")
							.WithTokenResponsePropertyPath("NextMarker");

				files = new EnumerableConverter<S3Object, S3FileInfo>
					(((IEnumerable<S3Object>)(PaginatedResourceFactory.Create<S3Object, ListObjectsRequest, ListObjectsResponse>(pagingInfo)))
						.Where(s3Object => !String.Equals(S3Helper.DecodeKey(s3Object.Key), key, StringComparison.Ordinal) && !s3Object.Key.EndsWith("\\", StringComparison.Ordinal)),
						s3Object => new S3FileInfo(s3Client, bucket, S3Helper.DecodeKey(s3Object.Key)));
			}

			//handle if recursion is set
			if (searchOption == SearchOption.AllDirectories)
			{
				IEnumerable<S3DirectoryInfo> foldersToSearch = EnumerateDirectories();
				foreach (S3DirectoryInfo dir in foldersToSearch)
				{
					files = files.Concat(dir.EnumerateFiles(searchPattern, searchOption));
				}
			}

			//filter based on search pattern
			var regEx = WildcardToRegex(searchPattern);
			files = files.Where(s3fileInfo => Regex.IsMatch(s3fileInfo.Name, regEx, RegexOptions.IgnoreCase));
			return files;
		}

		/// <summary>
		/// Enumerate the files of this directory.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of files.</returns>
		public IEnumerable<IS3FileSystemInfo> EnumerateFileSystemInfos()
		{
			return EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Enumerate the files of this directory.
		/// </summary>
		/// <param name="searchPattern">The search string. The default pattern is "*", which returns all files.</param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of files that matches searchPattern.</returns>
		public IEnumerable<IS3FileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
		{
			return EnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Enumerate the files of this directory.
		/// </summary>
		/// <param name="searchPattern">The search string. The default pattern is "*", which returns all files.</param>
		/// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories. The default value is TopDirectoryOnly.</param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An enumerable collection of files that matches searchPattern and searchOption.</returns>
		public IEnumerable<IS3FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
			IEnumerable<IS3FileSystemInfo> files = EnumerateFiles(searchPattern, searchOption).Cast<IS3FileSystemInfo>();
			IEnumerable<IS3FileSystemInfo> folders = EnumerateDirectories(searchPattern, searchOption).Cast<IS3FileSystemInfo>();

			return files.Concat(folders);
		}
		/// <summary>
		/// Returns the S3DirectoryInfo for the specified sub directory.
		/// </summary>
		/// <param name="directory">Directory to get the S3DirectroyInfo for.</param>
		/// <returns>The S3DirectoryInfo for the specified sub directory.</returns>
		public S3DirectoryInfo GetDirectory(string directory)
		{
			S3DirectoryInfo ret = null;
			if (String.IsNullOrEmpty(bucket))
			{
				ret = new S3DirectoryInfo(s3Client, directory, "");
			}
			else
			{
				ret = new S3DirectoryInfo(s3Client, bucket, string.Format(CultureInfo.InvariantCulture, "{0}{1}", key, directory));
			}
			return ret;
		}
		/// <summary>
		/// Returns an array of IS3FileSystemInfos for the files in this directory.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An array of files.</returns>
		public IS3FileSystemInfo[] GetFileSystemInfos()
		{
			return GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Returns an array of IS3FileSystemInfos for the files in this directory.
		/// </summary>
		/// <param name="searchPattern">The search string. The default pattern is "*", which returns all files.</param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An array of files that matches searchPattern.</returns>
		public IS3FileSystemInfo[] GetFileSystemInfos(string searchPattern)
		{
			return GetFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Returns an array of IS3FileSystemInfos for the files in this directory.
		/// </summary>
		/// <param name="searchPattern">The search string. The default pattern is "*", which returns all files.</param>
		/// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories. The default value is TopDirectoryOnly.</param>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		/// <returns>An array of files that matches searchPattern and searchOption.</returns>
		public IS3FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
		{
			return EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();
		}
		static string WildcardToRegex(string pattern)
		{
			string newPattern = Regex.Escape(pattern).
				Replace("\\*", ".*").
				Replace("\\?", ".");
			return "^" + newPattern + "$";
		}
		/// <summary>
		/// Creating and deleting buckets can sometimes take a little bit of time.  To make sure
		/// users of this API do not experience the side effects of the eventual consistency
		/// we block until the state change has happened.
		/// </summary>
		/// <param name="exists"></param>
		void WaitTillBucketS3StateIsConsistent(bool exists)
		{
			int success = 0;
			bool currentState = false;
			var start = this.S3Client.Config.CorrectedUtcNow;
			do
			{
				Task<ListBucketsResponse> t = this.S3Client.ListBucketsAsync();
				t.Wait();
				var buckets = t.Result.Buckets; 
				currentState = buckets.FirstOrDefault(x => string.Equals(x.BucketName, this.BucketName)) != null;

				if (currentState == exists)
				{
					success++;

					if (success == EVENTUAL_CONSISTENCY_SUCCESS_IN_ROW)
						break;
				}
				else
				{
					success = 0;
				}

				Thread.Sleep(EVENTUAL_CONSISTENCY_POLLING_PERIOD);

			} while ((this.S3Client.Config.CorrectedUtcNow - start).TotalMilliseconds < EVENTUAL_CONSISTENCY_MAX_WAIT);
		}

		/// <summary>
		/// The S3DirectoryInfo for the root of the S3 bucket.
		/// </summary>
		public S3DirectoryInfo Bucket
		{
			get
			{
				return new S3DirectoryInfo(s3Client, bucket, "");
			}
		}
		/// <summary>
		/// Checks with S3 to see if the directory exists and if so returns true.
		/// 
		/// Due to Amazon S3's eventual consistency model this property can return false for newly created buckets.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public bool Exists
		{
			get
			{
				bool bucketExists;
				return ExistsWithBucketCheck(out bucketExists);
			}
		}
		/// <summary>
		/// UTC converted version of LastWriteTime.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public DateTime LastWriteTimeUtc
		{
			get
			{
				return LastWriteTime.ToUniversalTime();
			}
		}
		/// <summary>
		/// Returns the type of file system element.
		/// </summary>
		public FileSystemType Type
		{
			get
			{
				return FileSystemType.Directory;
			}
		}
		/// <summary>
		/// Returns the name of the folder.
		/// </summary>
		public string Name
		{
			get
			{
				string ret = String.Empty;
				if (!String.IsNullOrEmpty(bucket))
				{
					if (String.IsNullOrEmpty(key))
					{
						ret = bucket;
					}
					else
					{
						int end = key.LastIndexOf('\\');
						int start = key.LastIndexOf('\\', end - 1);
						return key.Substring(start + 1, end - start - 1);
					}
				}
				return ret;
			}
		}

		internal bool ExistsWithBucketCheck(out bool bucketExists)
		{
			bucketExists = true;
			try
			{
				if (String.IsNullOrEmpty(bucket))
				{
					return true;
				}
				else if (String.IsNullOrEmpty(key))
				{
					var request = new GetBucketLocationRequest()
					{
						BucketName = bucket
					};
					((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);

					try
					{
						s3Client.GetBucketLocationAsync(request).Wait();
						return true;
					}
					catch (AmazonS3Exception e)
					{
						if (string.Equals(e.ErrorCode, "NoSuchBucket"))
						{
							return false;
						}
						throw;
					}
				}
				else
				{
					var request = new ListObjectsRequest()
					{
						BucketName = this.bucket,
						Prefix = S3Helper.EncodeKey(key),
						MaxKeys = 1
					};
					((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);

					Task<ListObjectsResponse> t = s3Client.ListObjectsAsync(request);
					t.Wait();
					var response = t.Result;
					return response.S3Objects.Count > 0;
				}
			}
			catch (AmazonS3Exception e)
			{
				if (string.Equals(e.ErrorCode, "NoSuchBucket"))
				{
					bucketExists = false;
					return false;
				}
				else if (string.Equals(e.ErrorCode, "NotFound"))
				{
					return false;
				}
				throw;
			}
		}

		/// <summary>
		/// The full path of the directory including bucket name.
		/// </summary>
		public string FullName
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}:\\{1}", bucket, key);
			}
		}
		/// <summary>
		/// Returns empty string for directories.
		/// </summary>
		string IS3FileSystemInfo.Extension
		{
			get
			{
				return String.Empty;
			}
		}
		/// <summary>
		/// Returns the last write time of the the latest file written to the directory.
		/// </summary>
		/// <exception cref="T:System.Net.WebException"></exception>
		/// <exception cref="T:Amazon.S3.AmazonS3Exception"></exception>
		public DateTime LastWriteTime
		{
			get
			{
				DateTime ret = DateTime.MinValue;
				if (Exists)
				{
					if (String.IsNullOrEmpty(this.bucket))
					{
						ret = DateTime.MinValue;
						var listRequest = new ListBucketsRequest();
						((Amazon.Runtime.Internal.IAmazonWebServiceRequest)listRequest).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);
						Task<ListBucketsResponse> task = s3Client.ListBucketsAsync(listRequest);
						task.Wait();
						foreach (S3Bucket s3Bucket in task.Result.Buckets)
						{
							DateTime currentBucketLastWriteTime = new S3DirectoryInfo(s3Client, s3Bucket.BucketName, String.Empty).LastWriteTime;
							if (currentBucketLastWriteTime > ret)
							{
								ret = currentBucketLastWriteTime;
							}
						}
					}
					else
					{
						var request = new ListObjectsRequest
						{
							BucketName = bucket,
							Prefix = S3Helper.EncodeKey(key)
						};
						((Amazon.Runtime.Internal.IAmazonWebServiceRequest)request).AddBeforeRequestHandler(S3Helper.FileIORequestEventHandler);

						S3Object lastWrittenObject =
								((IEnumerable<S3Object>)
									PaginatedResourceFactory.Create<S3Object, ListObjectsRequest, ListObjectsResponse>(new PaginatedResourceInfo()
										.WithClient(s3Client)
										.WithItemListPropertyPath("S3Objects")
										.WithMethodName("ListObjects")
										.WithRequest(request)
										.WithTokenRequestPropertyPath("Marker")
										.WithTokenResponsePropertyPath("NextMarker")))
									.OrderByDescending(s3Object => s3Object.LastModified)
									.FirstOrDefault();
						if (lastWrittenObject != null)
						{
							ret = lastWrittenObject.LastModified;
						}
					}
				}

				return ret;
			}
		}
	}

	internal static class S3Helper
	{
		internal static string EncodeKey(string key)
		{
			return key.Replace('\\', '/');
		}
		internal static string DecodeKey(string key)
		{
			return key.Replace('/', '\\');
		}

		internal static void FileIORequestEventHandler(object sender, RequestEventArgs args)
		{
			WebServiceRequestEventArgs wsArgs = args as WebServiceRequestEventArgs;
			if (wsArgs != null)
			{
				string currentUserAgent = wsArgs.Headers[AWSSDKUtils.UserAgentHeader];
				wsArgs.Headers[AWSSDKUtils.UserAgentHeader] = currentUserAgent + " FileIO";
			}
		}

	}
	//
	// Summary:
	//     Common interface for both S3FileInfo and S3DirectoryInfo.
	public interface IS3FileSystemInfo
	{
		//
		// Summary:
		//     Returns true if the item exists in S3.
		bool Exists { get; }
		//
		// Summary:
		//     Returns the extension of the item.
		string Extension { get; }
		//
		// Summary:
		//     Returns the fully qualified path to the item.
		string FullName { get; }
		//
		// Summary:
		//     Returns the last modified time for this item from S3 in local timezone.
		DateTime LastWriteTime { get; }
		//
		// Summary:
		//     Returns the last modified time for this item from S3 in UTC timezone.
		DateTime LastWriteTimeUtc { get; }
		//
		// Summary:
		//     Returns the name of the item without parent information.
		string Name { get; }
		//
		// Summary:
		//     Indicates what type of item this object represents.
		FileSystemType Type { get; }

		//
		// Summary:
		//     Deletes this item from S3.
		void Delete();
	}

	internal class EnumerableConverter<T, U> : IEnumerable<U>
	{
		internal IEnumerable<T> baseEnum = null;
		internal Func<T, U> converter = null;

		internal EnumerableConverter(IEnumerable<T> start, Func<T, U> convert)
		{
			baseEnum = start;
			converter = convert;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}

		public IEnumerator<U> GetEnumerator()
		{
			return new ConvertingEnumerator<T, U>(this);
		}


	}

	internal class ConvertingEnumerator<T, U> : IEnumerator<U>
	{
		Func<T, U> convert;
		IEnumerator<T> getT;

		bool isConverted = false;
		U convertedCurrent = default(U);

		internal ConvertingEnumerator(EnumerableConverter<T, U> ec)
		{
			getT = ec.baseEnum.GetEnumerator();
			convert = ec.converter;
		}

		public bool MoveNext()
		{
			isConverted = false;
			convertedCurrent = default(U);
			return getT.MoveNext();
		}

		public void Reset()
		{
			isConverted = false;
			convertedCurrent = default(U);
			getT.Reset();
		}

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		public U Current
		{
			get
			{
				if (!isConverted)
				{
					convertedCurrent = convert(getT.Current);
					isConverted = true;
				}
				return convertedCurrent;
			}
		}

		public void Dispose()
		{
		}
	}
}