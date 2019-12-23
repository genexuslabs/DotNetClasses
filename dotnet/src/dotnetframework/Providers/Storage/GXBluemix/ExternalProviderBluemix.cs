using GeneXus.Configuration;
using GeneXus.Services;
using JSIStudios.SimpleRESTServices.Client;
using log4net;
using net.openstack.Core;
using net.openstack.Core.Caching;
using net.openstack.Core.Domain;
using net.openstack.Core.Domain.Mapping;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Core.Providers;
using net.openstack.Core.Validators;
using net.openstack.Providers.Rackspace;
using net.openstack.Providers.Rackspace.Exceptions;
using net.openstack.Providers.Rackspace.Objects;
using net.openstack.Providers.Rackspace.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Thread = System.Threading.Thread;
using GeneXus.Utils;
using GeneXus.Encryption;

namespace GeneXus.Storage.GXBluemix
{
	public class ExternalProviderBluemix : ExternalProvider
	{
		const string USER = "STORAGE_PROVIDER_USER";
		const string PASSWORD = "STORAGE_PROVIDER_PASSWORD";
		const string SERVER_URL = "SERVER_URL";
		const string PROJECT_ID = "PROJECT_ID";
		const string REGION = "STORAGE_PROVIDER_REGION";
		const string PRIVATE_BUCKET = "PRIVATE_BUCKET_NAME";
		const string PUBLIC_BUCKET = "PUBLIC_BUCKET_NAME";
		const string FOLDER = "FOLDER_NAME";

		BluemixFilesProvider Client { get; set; }
		string PublicBucket { get; set; }
		string PrivateBucket { get; set; }
		string Folder { get; set; }
		string AuthToken { get; set; }
		public string StorageUri { get; set; }
		string PrivateTempKeyUrl { get; set; }

		public ExternalProviderBluemix()
		{
			GXService providerService = ServiceFactory.GetGXServices().Get(GXServices.STORAGE_SERVICE);
			var identityEndpoint = new Uri(providerService.Properties.Get(SERVER_URL));
			CloudIdentityWithProject identity = new CloudIdentityWithProject
			{
				Username = CryptoImpl.Decrypt(providerService.Properties.Get(USER)),
				Password = CryptoImpl.Decrypt(providerService.Properties.Get(PASSWORD)),
				ProjectName = providerService.Properties.Get(PROJECT_ID),
			};

			BlueMixIdentityProvider identityProvider = new BlueMixIdentityProvider(identityEndpoint, identity);

			GetStorageEndpoint(identityProvider, identity);

			Client = new BluemixFilesProvider(null, providerService.Properties.Get(REGION), identityProvider, null);
			PublicBucket = CryptoImpl.Decrypt(providerService.Properties.Get(PUBLIC_BUCKET));
			PrivateBucket = CryptoImpl.Decrypt(providerService.Properties.Get(PRIVATE_BUCKET));
			Folder = providerService.Properties.Get(FOLDER);
			AuthToken = identityProvider.AuthToken;

			CreateBuckets();
			CreateFolder(Folder);
		}

		public ExternalProviderBluemix(string proj, string url, string user, string pass, string bucket, string region)
		{
			var identityEndpoint = new Uri(url);
			CloudIdentityWithProject identity = new CloudIdentityWithProject
			{
				Username = user,
				Password = pass,
				ProjectName = proj,
			};

			BlueMixIdentityProvider identityProvider = new BlueMixIdentityProvider(identityEndpoint, identity);

			GetStorageEndpoint(identityProvider, identity);

			Client = new BluemixFilesProvider(null, region, identityProvider, null);
			PublicBucket = bucket;
			AuthToken = identityProvider.AuthToken;

			CreateBuckets();
		}

		private void GetStorageEndpoint(BlueMixIdentityProvider identityProvider, CloudIdentityWithProject identity)
		{
			UserAccessV3 user = identityProvider.GetUserAccess(identity);
			var catalog = user.Catalog;
			Endpoint objectStorageEndpoint = null;
			foreach (Catalog service in catalog)
				if (service.Type == "object-store")
					if (service.Endpoints.Where(e => e.Region_id == "dallas").Any())
						objectStorageEndpoint = service.Endpoints.Where(e => e.Region_id == "dallas").First();

			StorageUri = objectStorageEndpoint.Url;

			if (String.IsNullOrEmpty(StorageUri))
				throw new Exception("Can't find the object storage endpoint, please check the credentials in the storage configuration.");
		}

		private Dictionary<string, string> CreateObjectMetadata(string tableName, string fieldName, string key)
		{
			Dictionary<string, string> metadata = new Dictionary<string, string>();
			metadata.Add("Table", tableName);
			metadata.Add("Field", fieldName);
			metadata.Add("KeyValue", key);
			return metadata;
		}

		private void CreateBuckets()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("X-Auth-Token", AuthToken);
			PrivateTempKeyUrl = Guid.NewGuid().ToString();
			headers.Add("X-Container-Meta-Temp-URL-Key", PrivateTempKeyUrl);
			Client.CreateContainer(PrivateBucket, headers);

			headers = new Dictionary<string, string>();
			headers.Add("X-Auth-Token", AuthToken);
			headers.Add("X-Container-Read", ".r:*");
			Client.CreateContainer(PublicBucket, headers);
		}

		private void CreateFolder(string folder, string table = null, string field = null)
		{
			string name = StorageUtils.NormalizeDirectoryName(folder);
			if (table != null)
				name += table + StorageUtils.DELIMITER;
			if (field != null)
				name += field + StorageUtils.DELIMITER;
			using (var stream = new MemoryStream())
			{
				Client.CreateObject(PublicBucket, stream, name, "application/directory");
			}
		}

		public void Download(string externalFileName, string localFile, GxFileType fileType)
		{
			string bucket = GetBucket(fileType);
			string localDirectory = Path.GetDirectoryName(localFile);
			string localFileName = Path.GetFileName(localFile);
			Client.GetObjectSaveToFile(bucket, localDirectory, externalFileName, localFileName);
		}

		public string Upload(string localFile, string externalFileName, GxFileType fileType)
		{
			string bucket = GetBucket(fileType);
			Client.CreateObjectFromFile(bucket, localFile, externalFileName);
			return StorageUri + StorageUtils.DELIMITER + bucket + StorageUtils.DELIMITER + StorageUtils.EncodeUrl(externalFileName);
		}

		private string GetBucket(GxFileType fileType)
		{
			return (fileType.HasFlag(GxFileType.Private)) ? PrivateBucket : PublicBucket;
		}

		public string Get(string externalFileName, GxFileType fileType, int urlMinutes)
		{
			string bucket = GetBucket(fileType);
			if (Exists(externalFileName, fileType))
			{
				if (fileType.HasFlag(GxFileType.Private))
					return Client.CreateTemporaryPublicUri(HttpMethod.GET, bucket, externalFileName, PrivateTempKeyUrl, DateTimeOffset.Now.AddMinutes(urlMinutes)).ToString();
				return StorageUri + StorageUtils.DELIMITER + bucket + StorageUtils.DELIMITER + StorageUtils.EncodeUrl(externalFileName);
			}
			return string.Empty;
		}

		public void Delete(string objectName, GxFileType fileType)
		{
			Client.DeleteObject(GetBucket(fileType), objectName);
		}

		public bool Exists(string objectName, GxFileType fileType)
		{
			try
			{
				Client.GetObjectMetaData(GetBucket(fileType), objectName);
				return true;
			}
			catch (ItemNotFoundException)
			{
				return false;
			}
		}

		public string Rename(string objectName, string newName, GxFileType fileType)
		{
			string bucket = GetBucket(fileType);
			Copy(objectName, fileType, newName, fileType);
			Delete(objectName, fileType);
			return StorageUri + StorageUtils.DELIMITER + bucket + StorageUtils.DELIMITER + StorageUtils.EncodeUrl(newName);
		}

		public string Copy(string objectName, GxFileType sourceFileType, string newName, GxFileType destFileType)
		{
			string sourceBucket = GetBucket(sourceFileType);
			string destBucket = GetBucket(destFileType);
			Client.CopyObject(sourceBucket, objectName, destBucket, newName);
			return Get(objectName, destFileType, 0);
		}

		public string Upload(string fileName, Stream stream, GxFileType fileType)
		{
			string bucket = GetBucket(fileType);
			if (Path.GetExtension(fileName).Equals(".tmp"))
				Client.CreateObject(bucket, stream, fileName, "image / jpeg");
			else
				Client.CreateObject(bucket, stream, fileName);

			return Get(fileName, fileType, 0);
		}

		public string Copy(string url, string newName, string tableName, string fieldName, GxFileType fileType)
		{
			string bucket = GetBucket(fileType);
			string resourceKey = Folder + StorageUtils.DELIMITER + tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + newName;
			CreateFolder(Folder, tableName, fieldName);
			url = StorageUtils.DecodeUrl(url.Replace(StorageUri + StorageUtils.DELIMITER + bucket + StorageUtils.DELIMITER, ""));

			Copy(url, fileType, resourceKey, fileType);
			Client.UpdateObjectMetadata(bucket, resourceKey, CreateObjectMetadata(tableName, fieldName, resourceKey));

			return Get(resourceKey, fileType, 0);
		}

		public Stream GetStream(string objectName, GxFileType fileType)
		{
			MemoryStream stream = new MemoryStream();
			Client.GetObject(GetBucket(fileType), objectName, stream);
			return stream;
		}

		public string GetDirectory(string directoryName)
		{
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			if (ExistsDirectory(directoryName))
				return PublicBucket + StorageUtils.DELIMITER + directoryName;
			else
				return "";
		}

		public long GetLength(string objectName, GxFileType fileType)
		{
			foreach (ContainerObject obj in Client.ListObjects(GetBucket(fileType)))
				if (obj.Name.Equals(objectName))
					return obj.Bytes;
			return 0;
		}

		public DateTime GetLastModified(string objectName, GxFileType fileType)
		{
			foreach (ContainerObject obj in Client.ListObjects(GetBucket(fileType)))
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
			foreach (ContainerObject obj in Client.ListObjects(PublicBucket, prefix: StorageUtils.NormalizeDirectoryName(directoryName)))
			{
				objs.Add(obj.Name);
			}
			objs.Add(directoryName);
			Client.DeleteObjects(PublicBucket, objs);
		}

		public bool ExistsDirectory(string directoryName)
		{
			List<String> directories = GetDirectories();
			return directories.Contains(directoryName) || directories.Contains(StorageUtils.NormalizeDirectoryName(directoryName));
		}

		public void RenameDirectory(string directoryName, string newDirectoryName)
		{
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			newDirectoryName = StorageUtils.NormalizeDirectoryName(newDirectoryName);

			foreach (ContainerObject obj in Client.ListObjects(PublicBucket, prefix: directoryName))
			{
				Client.CopyObject(PublicBucket, obj.Name, PublicBucket, obj.Name.Replace(directoryName, newDirectoryName));
			}
			DeleteDirectory(directoryName);
		}

		public List<string> GetSubDirectories(string directoryName)
		{
			return GetDirectories(StorageUtils.NormalizeDirectoryName(directoryName));
		}

		private List<String> GetDirectories(string directoryName = null)
		{
			List<string> subdir = new List<string>();
			foreach (ContainerObject obj in Client.ListObjects(PublicBucket, prefix: directoryName))
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
						string dir = StorageUtils.NormalizeDirectoryName(directoryName + name);
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

		public List<string> GetFiles(string directoryName, string filter = "")
		{
			directoryName = StorageUtils.NormalizeDirectoryName(directoryName);
			List<string> files = new List<string>();
			foreach (ContainerObject obj in Client.ListObjects(PublicBucket, prefix: directoryName))
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
				return obj.Name.Replace(StorageUtils.NormalizeDirectoryName(directory), "").Split(delimiter).Length == 1 && !String.IsNullOrEmpty(obj.Name.Replace(StorageUtils.NormalizeDirectoryName(directory), "").Split(delimiter)[0]);
		}

		public bool GetMessageFromException(Exception ex, SdtMessages_Message msg)
		{
			try
			{
				ResponseException rex = (ResponseException)ex;
				msg.gxTpr_Id = rex.Response.Status;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public string Save(Stream fileStream, string fileName, string tableName, string fieldName, GxFileType fileType)
		{
			CreateFolder(Folder, tableName, fieldName);
			string resourceKey = Folder + StorageUtils.DELIMITER + tableName + StorageUtils.DELIMITER + fieldName + StorageUtils.DELIMITER + fileName;
			return Upload(resourceKey, fileStream, fileType);
		}

		public bool GetObjectNameFromURL(string url, out string objectName)
		{
			string baseUrl = StorageUri + StorageUtils.DELIMITER + PublicBucket + StorageUtils.DELIMITER;
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
			return StorageUri + StorageUtils.DELIMITER + PublicBucket + StorageUtils.DELIMITER + Folder + StorageUtils.DELIMITER;
		}

		#region Extension of openstack.net (issue: https://github.com/openstacknetsdk/openstack.net/issues/503) to enable openstack Identity API v3: http://developer.openstack.org/api-ref-identity-v3.html

		public class IdentityTokenV3 : IdentityToken
		{
			public new string Id { get; set; }
		}

		public class BlueMixIdentityProvider : CloudIdentityProvider
		{
			public string AuthToken { get; set; }
			new ICache<UserAccessV3> TokenCache { get; }

			public BlueMixIdentityProvider(Uri urlBase, CloudIdentity defaultIdentity)
				: this(urlBase, defaultIdentity, null, null)
			{
				AuthToken = null;
			}

			public BlueMixIdentityProvider(Uri urlBase, CloudIdentity defaultIdentity, IRestService restService, ICache<UserAccess> tokenCache)
				: base(defaultIdentity, restService, tokenCache, urlBase)
			{
				AuthToken = null;
				if (urlBase == null)
					throw new ArgumentNullException(nameof(urlBase));
			}

			public new IdentityToken GetToken(CloudIdentity identity, bool forceCacheRefresh = false)
			{
				CheckIdentity(identity);

				GetUserAccess(identity, forceCacheRefresh);

				IdentityTokenV3 token = new IdentityTokenV3();
				token.Id = AuthToken;
				return token;
			}
			public new UserAccessV3 GetUserAccess(CloudIdentity identity, bool forceCacheRefresh = false)
			{
				identity = identity ?? DefaultIdentity;

				CloudIdentityWithProject identityWithProject = identity as CloudIdentityWithProject;

				if (string.IsNullOrEmpty(identityWithProject.Password))
					throw new NotSupportedException(string.Format("The {0} identity must specify a password.", typeof(CloudIdentityWithProject)));
				if (!string.IsNullOrEmpty(identityWithProject.APIKey))
					throw new NotSupportedException(string.Format("The {0} identity does not support API key authentication.", typeof(CloudIdentityWithProject)));

				var auth = Authorization.CreateCredentials(identityWithProject);
				var projectId = identityWithProject.ProjectId != null ? JToken.FromObject(identityWithProject.ProjectId) : string.Empty;

				var response = ExecuteRESTRequest<JObject>(identity, new Uri(UrlBase, "/v3/auth/tokens"), HttpMethod.POST, auth, isTokenRequest: true);
				if (response == null || response.Data == null)
					return null;

				AuthToken = response.Headers[0].Value;
				JToken userAccessObject = response.Data["token"];
				if (userAccessObject == null)
					return null;

				UserAccessV3 access = userAccessObject.ToObject<UserAccessV3>();
				if (access == null)
					return null;

				string key = string.Format("{0}:{1}/{2}", UrlBase, identityWithProject.ProjectId, identityWithProject.Username);

				return access;
			}

			protected override string LookupServiceTypeKey(IServiceType serviceType)
			{
				return serviceType.Type;
			}

		}

		public class Authorization
		{
			//Generates the JSON with the credentials to authenticate to the Identity API v3
			public static JObject CreateCredentials(CloudIdentityWithProject identityWithProject)
			{
				return new JObject(
									new JProperty("auth", new JObject(
										new JProperty("identity", new JObject(
											new JProperty("methods", new JArray("password")),
											new JProperty("password", new JObject(
												new JProperty("user", new JObject(
													new JProperty("id", JValue.CreateString(identityWithProject.Username)),
													new JProperty("password", JValue.CreateString(identityWithProject.Password)))))))),
										new JProperty("scope", new JObject(
											new JProperty("project", new JObject(
												new JProperty("id", JValue.CreateString(identityWithProject.ProjectName)))))))));
			}
		}

		internal class EncodeDecodeProviderAux : IEncodeDecodeProvider
		{

			private static readonly EncodeDecodeProviderAux _default = new EncodeDecodeProviderAux();

			public static EncodeDecodeProviderAux Default
			{
				get
				{
					return _default;
				}
			}

			public string UrlEncode(string stringToEncode)
			{
				if (stringToEncode == null)
					return null;

				return UriUtility.UriEncode(stringToEncode, UriPart.AnyUrl);
			}

			public string UrlDecode(string stringToDecode)
			{
				if (stringToDecode == null)
					return null;

				return UriUtility.UriDecode(stringToDecode);
			}
		}

		public class BluemixFilesProvider : CloudFilesProvider
		{
			private readonly IObjectStorageValidator _cloudFilesValidator;
			private readonly IEncodeDecodeProvider _encodeDecodeProvider;
			private readonly IObjectStorageMetadataProcessor _cloudFilesMetadataProcessor;
			private readonly IStatusParser _statusParser;
			private readonly IObjectMapper<BulkDeleteResponse, BulkDeletionResults> _bulkDeletionResultMapper;
			public BluemixFilesProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService)
				: base(defaultIdentity, defaultRegion, identityProvider, restService)
			{
				_cloudFilesValidator = CloudFilesValidator.Default;
				_encodeDecodeProvider = EncodeDecodeProviderAux.Default;
				_cloudFilesMetadataProcessor = CloudFilesMetadataProcessor.Default;
				_statusParser = HttpStatusCodeParser.Default;
				_bulkDeletionResultMapper = new BulkDeletionResultMapper(_statusParser);
			}

			public new string GetServiceEndpointCloudFiles(CloudIdentity identity, string region = null, bool useInternalUrl = false)
			{
				string serviceType = "object-store";

				if (serviceType == null)
					throw new ArgumentNullException("serviceType");
				if (string.IsNullOrEmpty(serviceType))
					throw new ArgumentException("serviceType cannot be empty");
				CheckIdentity(identity);

				identity = GetDefaultIdentity(identity);

				var userAccess = ((BlueMixIdentityProvider)IdentityProvider).GetUserAccess(identity);

				if (userAccess == null || userAccess.Catalog == null)
					throw new UserAuthenticationException("Unable to authenticate user and retrieve authorized service endpoints.");

				IEnumerable<Catalog> services = userAccess.Catalog.Where(sc => string.Equals(sc.Type, serviceType, StringComparison.OrdinalIgnoreCase));

				IEnumerable<Tuple<Catalog, Endpoint>> endpoints =
					services.SelectMany(service => service.Endpoints.Select(endpoint => Tuple.Create(service, endpoint)));

				string effectiveRegion = region;
				if (string.IsNullOrEmpty(effectiveRegion))
				{
					if (!string.IsNullOrEmpty(DefaultRegion))
						effectiveRegion = DefaultRegion;
					else if (!string.IsNullOrEmpty(userAccess.User.DefaultRegion))
						effectiveRegion = userAccess.User.DefaultRegion;
				}

				IEnumerable<Tuple<Catalog, Endpoint>> regionEndpoints =
					endpoints.Where(i => string.Equals(i.Item2.Region ?? string.Empty, effectiveRegion ?? string.Empty, StringComparison.OrdinalIgnoreCase));

				if (regionEndpoints.Any())
					endpoints = regionEndpoints;
				else
					endpoints = endpoints.Where(i => string.IsNullOrEmpty(i.Item2.Region));

				if (effectiveRegion == null && !endpoints.Any())
					throw new NoDefaultRegionSetException("No region was provided, the service does not provide a region-independent endpoint, and there is no default region set for the user's account.");

				Tuple<Catalog, Endpoint> serviceEndpoint = endpoints.Where(e => e.Item2.Url.Contains("softlayer")).FirstOrDefault();
				if (serviceEndpoint == null)
					throw new UserAuthorizationException("The user does not have access to the requested service or region.");

				return serviceEndpoint.Item2.Url;
			}
			public new ObjectStore CreateContainer(string container, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container)));

				var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.PUT, headers: headers);

				switch (response.StatusCode)
				{
					case HttpStatusCode.Created:
						return ObjectStore.ContainerCreated;

					case HttpStatusCode.Accepted:
						return ObjectStore.ContainerExists;

					default:
						throw new ResponseException(string.Format("Unexpected status {0} returned by Create Container.", response.StatusCode), response);
				}
			}

			public new void CreateObject(string container, Stream stream, string objectName, string contentType = null, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (stream == null)
					throw new ArgumentNullException(nameof(stream));
				if (objectName == null)
					throw new ArgumentNullException(nameof(objectName));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(objectName))
					throw new ArgumentException("objectName cannot be empty");
				if (chunkSize < 0)
					throw new ArgumentOutOfRangeException(nameof(chunkSize));
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				_cloudFilesValidator.ValidateObjectName(objectName);

				if (stream.Length > LargeFileBatchThreshold)
				{
					throw new ArgumentException("objectName is too big");
				}
				var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

				RequestSettings settings = BuildDefaultRequestSettings();
				settings.ChunkRequest = true;
				settings.ContentType = contentType;

				StreamRESTRequest(identity, urlPath, HttpMethod.PUT, stream, chunkSize, headers: headers, progressUpdated: progressUpdated, requestSettings: settings);
			}

			public new void CreateObjectFromFile(string container, string filePath, string objectName = null, string contentType = null, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (filePath == null)
					throw new ArgumentNullException(nameof(filePath));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(filePath))
					throw new ArgumentException("filePath cannot be empty");
				if (chunkSize < 0)
					throw new ArgumentOutOfRangeException(nameof(chunkSize));
				CheckIdentity(identity);

				if (string.IsNullOrEmpty(objectName))
					objectName = Path.GetFileName(filePath);

				using (var stream = File.OpenRead(filePath))
				{
					CreateObject(container, stream, objectName, contentType, chunkSize, headers, region, progressUpdated, useInternalUrl, identity);
				}
			}
			public new void UpdateObjectMetadata(string container, string objectName, Dictionary<string, string> metadata, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (objectName == null)
					throw new ArgumentNullException(nameof(objectName));
				if (metadata == null)
					throw new ArgumentNullException(nameof(metadata));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(objectName))
					throw new ArgumentException("objectName cannot be empty");
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				_cloudFilesValidator.ValidateObjectName(objectName);

				var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (KeyValuePair<string, string> m in metadata)
				{
					if (string.IsNullOrEmpty(m.Key))
						throw new ArgumentException("metadata cannot contain any empty keys");

					headers.Add(ObjectMetaDataPrefix + m.Key, EncodeUnicodeValue(m.Value));
				}

				var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

				RequestSettings settings = BuildDefaultRequestSettings();
				// make sure the content type is not changed by the metadata operation
				settings.ContentType = null;

				ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, headers: headers, settings: settings);
			}

			public new Dictionary<string, string> GetObjectMetaData(string container, string objectName, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (objectName == null)
					throw new ArgumentNullException(nameof(objectName));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(objectName))
					throw new ArgumentException("objectName cannot be empty");
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				_cloudFilesValidator.ValidateObjectName(objectName);
				var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

				var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.HEAD);

				var processedHeaders = _cloudFilesMetadataProcessor.ProcessMetadata(response.Headers);

				return processedHeaders[ProcessedHeadersMetadataKey];
			}

			public new void GetObject(string container, string objectName, Stream outputStream, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, bool verifyEtag = false, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (objectName == null)
					throw new ArgumentNullException(nameof(objectName));
				if (outputStream == null)
					throw new ArgumentNullException(nameof(outputStream));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(objectName))
					throw new ArgumentException("objectName cannot be empty");
				if (chunkSize < 0)
					throw new ArgumentOutOfRangeException(nameof(chunkSize));
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				_cloudFilesValidator.ValidateObjectName(objectName);

				var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

				long? initialPosition;
				try
				{
					initialPosition = outputStream.Position;
				}
				catch (NotSupportedException)
				{
					if (verifyEtag)
						throw;

					initialPosition = null;
				}

				// This flag indicates whether the outputStream needs to be set prior to copying data.
				// See: https://github.com/openstacknetsdk/openstack.net/issues/297
				bool requiresPositionReset = false;

				var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.GET, (resp, isError) =>
				{
					if (resp == null)
						return new Response(0, null, null);

					string body;

					if (!isError)
					{
						using (var respStream = resp.GetResponseStream())
						{
							// The second condition will throw a proper NotSupportedException if the position
							// cannot be checked.
							if (requiresPositionReset && outputStream.Position != initialPosition)
								outputStream.Position = initialPosition.Value;

							requiresPositionReset = true;
							CopyStream(respStream, outputStream, chunkSize, progressUpdated);
						}

						body = "[Binary]";
					}
					else
					{
						using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
						{
							body = reader.ReadToEnd();
						}
					}

					var respHeaders = resp.Headers.AllKeys.Select(key => new HttpHeader(key, resp.GetResponseHeader(key))).ToList();

					return new Response(resp.StatusCode, respHeaders, body);
				}, headers: headers);

			}

			public new void GetObjectSaveToFile(string container, string saveDirectory, string objectName, string fileName = null, int chunkSize = 65536, Dictionary<string, string> headers = null, string region = null, bool verifyEtag = false, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (saveDirectory == null)
					throw new ArgumentNullException(nameof(saveDirectory));
				if (objectName == null)
					throw new ArgumentNullException(nameof(objectName));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(saveDirectory))
					throw new ArgumentException("saveDirectory cannot be empty");
				if (string.IsNullOrEmpty(objectName))
					throw new ArgumentException("objectName cannot be empty");
				if (chunkSize < 0)
					throw new ArgumentOutOfRangeException(nameof(chunkSize));
				CheckIdentity(identity);

				if (string.IsNullOrEmpty(fileName))
					fileName = objectName;

				var filePath = Path.Combine(saveDirectory, string.IsNullOrEmpty(fileName) ? objectName : fileName);

				try
				{
					using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
					{
						GetObject(container, objectName, fileStream, chunkSize, headers, region, verifyEtag, progressUpdated, useInternalUrl, identity);
					}
				}
				catch (InvalidETagException)
				{
					File.Delete(filePath);
					throw;
				}
			}

			private static string EncodeUnicodeValue(string value)
			{
				if (value == null)
					return null;

				return Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(value));
			}
			internal void CheckResponse(Response response)
			{
				if (response == null)
					throw new ArgumentNullException(nameof(response));

				ResponseCodeValidator.Validate(response);
			}
			protected new Response<T> ExecuteRESTRequest<T>(CloudIdentity identity, Uri absoluteUri, HttpMethod method, object body = null, Dictionary<string, string> queryStringParameter = null, Dictionary<string, string> headers = null, bool isRetry = false, bool isTokenRequest = false, RequestSettings settings = null)
			{
				if (absoluteUri == null)
					throw new ArgumentNullException(nameof(absoluteUri));
				CheckIdentity(identity);

				return ExecuteRESTRequest<Response<T>>(identity, absoluteUri, method, body, queryStringParameter, headers, isRetry, isTokenRequest, settings, RestService.Execute<T>);
			}
			protected new Response ExecuteRESTRequest(CloudIdentity identity, Uri absoluteUri, HttpMethod method, object body = null, Dictionary<string, string> queryStringParameter = null, Dictionary<string, string> headers = null, bool isRetry = false, bool isTokenRequest = false, RequestSettings settings = null)
			{
				if (absoluteUri == null)
					throw new ArgumentNullException(nameof(absoluteUri));
				CheckIdentity(identity);

				return ExecuteRESTRequest<Response>(identity, absoluteUri, method, body, queryStringParameter, headers, isRetry, isTokenRequest, settings, RestService.Execute);
			}

			protected new Response ExecuteRESTRequest(CloudIdentity identity, Uri absoluteUri, HttpMethod method, Func<HttpWebResponse, bool, Response> buildResponseCallback, object body = null, Dictionary<string, string> queryStringParameter = null, Dictionary<string, string> headers = null, bool isRetry = false, bool isTokenRequest = false, RequestSettings settings = null)
			{
				if (absoluteUri == null)
					throw new ArgumentNullException(nameof(absoluteUri));
				CheckIdentity(identity);

				return ExecuteRESTRequest<Response>(identity, absoluteUri, method, body, queryStringParameter, headers, isRetry, isTokenRequest, settings,
					(uri, requestMethod, requestBody, requestHeaders, requestQueryParams, requestSettings) => RestService.Execute(uri, requestMethod, buildResponseCallback, requestBody, requestHeaders, requestQueryParams, requestSettings));
			}

			private T ExecuteRESTRequest<T>(CloudIdentity identity, Uri absoluteUri, HttpMethod method, object body, Dictionary<string, string> queryStringParameter, Dictionary<string, string> headers, bool isRetry, bool isTokenRequest, RequestSettings requestSettings,
		Func<Uri, HttpMethod, string, Dictionary<string, string>, Dictionary<string, string>, RequestSettings, T> callback) where T : Response
			{
				if (absoluteUri == null)
					throw new ArgumentNullException(nameof(absoluteUri));
				CheckIdentity(identity);

				identity = GetDefaultIdentity(identity);

				if (requestSettings == null)
					requestSettings = BuildDefaultRequestSettings();

				if (headers == null)
					headers = new Dictionary<string, string>();

				if (!isTokenRequest)
					headers["X-Auth-Token"] = ((IdentityTokenV3)((BlueMixIdentityProvider)IdentityProvider).GetToken(identity, isRetry)).Id;

				string bodyStr = null;
				if (body != null)
				{
					if (body is JObject)
						bodyStr = body.ToString();
					else if (body is string)
						bodyStr = body as string;
					else
						bodyStr = JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
				}

				if (string.IsNullOrEmpty(requestSettings.UserAgent))
					requestSettings.UserAgent = DefaultUserAgent;

				var response = callback(absoluteUri, method, bodyStr, headers, queryStringParameter, requestSettings);

				// on errors try again 1 time.
				if (response.StatusCode == HttpStatusCode.Unauthorized && !isRetry && !isTokenRequest)
				{
					return ExecuteRESTRequest<T>(identity, absoluteUri, method, body, queryStringParameter, headers, true, isTokenRequest, requestSettings, callback);
				}

				CheckResponse(response);

				return response;
			}

			protected new Response StreamRESTRequest(CloudIdentity identity, Uri absoluteUri, HttpMethod method, Stream stream, int chunkSize, long maxReadLength = 0, Dictionary<string, string> queryStringParameter = null, Dictionary<string, string> headers = null, bool isRetry = false, RequestSettings requestSettings = null, Action<long> progressUpdated = null)
			{
				if (absoluteUri == null)
					throw new ArgumentNullException(nameof(absoluteUri));
				if (stream == null)
					throw new ArgumentNullException(nameof(stream));
				if (chunkSize <= 0)
					throw new ArgumentOutOfRangeException(nameof(chunkSize));
				if (maxReadLength < 0)
					throw new ArgumentOutOfRangeException(nameof(maxReadLength));
				CheckIdentity(identity);

				identity = GetDefaultIdentity(identity);

				if (requestSettings == null)
					requestSettings = BuildDefaultRequestSettings();

				requestSettings.Timeout = TimeSpan.FromMilliseconds(14400000); // Need to pass this in.

				if (headers == null)
					headers = new Dictionary<string, string>();

				headers["X-Auth-Token"] = ((IdentityTokenV3)((BlueMixIdentityProvider)IdentityProvider).GetToken(identity, isRetry)).Id; ;

				if (string.IsNullOrEmpty(requestSettings.UserAgent))
					requestSettings.UserAgent = DefaultUserAgent;

				long? initialPosition;
				try
				{
					initialPosition = stream.Position;
				}
				catch (NotSupportedException)
				{
					initialPosition = null;
				}

				Response response;
				try
				{
					response = RestService.Stream(absoluteUri, method, stream, chunkSize, maxReadLength, headers, queryStringParameter, requestSettings, progressUpdated);
				}
				catch (ProtocolViolationException)
				{
					ServicePoint servicePoint = ServicePointManager.FindServicePoint(absoluteUri);
					if (servicePoint.ProtocolVersion < HttpVersion.Version11)
					{
						// this is a workaround for issue #333
						// https://github.com/openstacknetsdk/openstack.net/issues/333
						// http://stackoverflow.com/a/22976809/138304
						int maxIdleTime = servicePoint.MaxIdleTime;
						servicePoint.MaxIdleTime = 0;
						Thread.Sleep(1);
						servicePoint.MaxIdleTime = maxIdleTime;
					}

					response = RestService.Stream(absoluteUri, method, stream, chunkSize, maxReadLength, headers, queryStringParameter, requestSettings, progressUpdated);
				}

				// on errors try again 1 time.
				if (response.StatusCode == HttpStatusCode.Unauthorized && !isRetry && initialPosition != null)
				{
					bool canRetry;

					try
					{
						if (stream.Position != initialPosition.Value)
							stream.Position = initialPosition.Value;

						canRetry = true;
					}
					catch (NotSupportedException)
					{
						// unable to retry the operation
						canRetry = false;
					}

					if (canRetry)
					{
						return StreamRESTRequest(identity, absoluteUri, method, stream, chunkSize, maxReadLength, queryStringParameter, headers, true, requestSettings, progressUpdated);
					}
				}

				CheckResponse(response);

				return response;
			}

			public new void DeleteObject(string container, string objectName, Dictionary<string, string> headers = null, bool deleteSegments = true, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (objectName == null)
					throw new ArgumentNullException(nameof(objectName));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(objectName))
					throw new ArgumentException("objectName cannot be empty");
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				_cloudFilesValidator.ValidateObjectName(objectName);

				Dictionary<string, string> objectHeader = null;
				if (deleteSegments)
					objectHeader = GetObjectHeaders(container, objectName, region, useInternalUrl, identity);

				if (deleteSegments && objectHeader != null && objectHeader.Any(h => h.Key.Equals(ObjectManifest, StringComparison.OrdinalIgnoreCase)))
				{
					var objects = ListObjects(container, region: region, useInternalUrl: useInternalUrl,
												   identity: identity);

					if (objects != null && objects.Any())
					{
						var segments = objects.Where(f => f.Name.StartsWith(string.Format("{0}.seg", objectName)));
						var delObjects = new List<string> { objectName };
						if (segments.Any())
							delObjects.AddRange(segments.Select(s => s.Name));

						DeleteObjects(container, delObjects, headers, region, useInternalUrl, identity);
					}
					else
						throw new Exception(string.Format("Object \"{0}\" in container \"{1}\" does not exist.", objectName, container));
				}
				else
				{
					var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

					ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE, headers: headers);
				}
			}

			public new Dictionary<string, string> GetObjectHeaders(string container, string objectName, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (objectName == null)
					throw new ArgumentNullException(nameof(objectName));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(objectName))
					throw new ArgumentException("objectName cannot be empty");
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				_cloudFilesValidator.ValidateObjectName(objectName);
				var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

				var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.HEAD);

				var processedHeaders = _cloudFilesMetadataProcessor.ProcessMetadata(response.Headers);

				return processedHeaders[ProcessedHeadersHeaderKey];
			}

			public new IEnumerable<ContainerObject> ListObjects(string container, int? limit = null, string marker = null, string markerEnd = null, string prefix = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (limit < 0)
					throw new ArgumentOutOfRangeException(nameof(limit));
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container)));

				var queryStringParameter = new Dictionary<string, string>();

				if (limit != null)
					queryStringParameter.Add("limit", limit.ToString());

				if (!string.IsNullOrEmpty(marker))
					queryStringParameter.Add("marker", marker);

				if (!string.IsNullOrEmpty(markerEnd))
					queryStringParameter.Add("end_marker", markerEnd);

				if (!string.IsNullOrEmpty(prefix))
					queryStringParameter.Add("prefix", prefix);

				var response = ExecuteRESTRequest<ContainerObject[]>(identity, urlPath, HttpMethod.GET, null, queryStringParameter);

				if (response == null || response.Data == null)
					return null;

				return response.Data;
			}

			public new void CopyObject(string sourceContainer, string sourceObjectName, string destinationContainer, string destinationObjectName, string destinationContentType = null, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (sourceContainer == null)
					throw new ArgumentNullException(nameof(sourceContainer));
				if (sourceObjectName == null)
					throw new ArgumentNullException(nameof(sourceObjectName));
				if (destinationContainer == null)
					throw new ArgumentNullException(nameof(destinationContainer));
				if (destinationObjectName == null)
					throw new ArgumentNullException(nameof(destinationObjectName));
				if (string.IsNullOrEmpty(sourceContainer))
					throw new ArgumentException("sourceContainer cannot be empty");
				if (string.IsNullOrEmpty(sourceObjectName))
					throw new ArgumentException("sourceObjectName cannot be empty");
				if (string.IsNullOrEmpty(destinationContainer))
					throw new ArgumentException("destinationContainer cannot be empty");
				if (string.IsNullOrEmpty(destinationObjectName))
					throw new ArgumentException("destinationObjectName cannot be empty");
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(sourceContainer);
				_cloudFilesValidator.ValidateObjectName(sourceObjectName);

				_cloudFilesValidator.ValidateContainerName(destinationContainer);
				_cloudFilesValidator.ValidateObjectName(destinationObjectName);

				var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(sourceContainer), _encodeDecodeProvider.UrlEncode(sourceObjectName)));

				if (headers == null)
					headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				headers.Add(Destination, string.Format("{0}/{1}", UriUtility.UriEncode(destinationContainer, UriPart.AnyUrl, Encoding.UTF8), UriUtility.UriEncode(destinationObjectName, UriPart.AnyUrl, Encoding.UTF8)));

				RequestSettings settings = BuildDefaultRequestSettings();
				if (destinationContentType != null)
				{
					settings.ContentType = destinationContentType;
				}
				else
				{
					// make sure to preserve the content type during the copy operation
					settings.ContentType = null;
				}

				ExecuteRESTRequest(identity, urlPath, HttpMethod.COPY, headers: headers, settings: settings);
			}

			public new void DeleteObjects(string container, IEnumerable<string> objects, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (objects == null)
					throw new ArgumentNullException(nameof(objects));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");

				BulkDelete(objects.Select(o => new KeyValuePair<string, string>(container, o)), headers, region, useInternalUrl, identity);
			}

			public new void BulkDelete(IEnumerable<KeyValuePair<string, string>> items, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				var urlPath = new Uri(string.Format("{0}/?bulk-delete", GetServiceEndpointCloudFiles(identity, region, useInternalUrl)));

				var encoded = items.Select(
					pair =>
					{
						if (string.IsNullOrEmpty(pair.Key))
							throw new ArgumentException("items", "items cannot contain any entries with a null or empty key (container name)");
						if (string.IsNullOrEmpty(pair.Value))
							throw new ArgumentException("items", "items cannot contain any entries with a null or empty value (object name)");
						_cloudFilesValidator.ValidateContainerName(pair.Key);
						_cloudFilesValidator.ValidateObjectName(pair.Value);

						return string.Format("/{0}/{1}", _encodeDecodeProvider.UrlEncode(pair.Key), _encodeDecodeProvider.UrlEncode(pair.Value));
					});
				var body = string.Join("\n", encoded.ToArray());

				var response = ExecuteRESTRequest<BulkDeleteResponse>(identity, urlPath, HttpMethod.POST, body: body, headers: headers, settings: new JSIStudios.SimpleRESTServices.Client.Json.JsonRequestSettings { ContentType = "text/plain" });

				Status status;
				if (_statusParser.TryParse(response.Data.Status, out status))
				{
					if (status.Code != 200 && !response.Data.Errors.Any())
					{
						response.Data.AllItems = encoded;
						throw new BulkDeletionException(response.Data.Status, _bulkDeletionResultMapper.Map(response.Data));
					}
				}
			}

			public new Uri CreateTemporaryPublicUri(HttpMethod method, string container, string objectName, string key, DateTimeOffset expiration, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
			{
				if (container == null)
					throw new ArgumentNullException(nameof(container));
				if (objectName == null)
					throw new ArgumentNullException(nameof(objectName));
				if (key == null)
					throw new ArgumentNullException(nameof(key));
				if (string.IsNullOrEmpty(container))
					throw new ArgumentException("container cannot be empty");
				if (string.IsNullOrEmpty(objectName))
					throw new ArgumentException("objectName cannot be empty");
				if (string.IsNullOrEmpty(key))
					throw new ArgumentException("key cannot be empty");
				CheckIdentity(identity);

				_cloudFilesValidator.ValidateContainerName(container);
				_cloudFilesValidator.ValidateObjectName(objectName);

				Uri baseAddress = new Uri(GetServiceEndpointCloudFiles(identity, region, useInternalUrl), UriKind.Absolute);

				StringBuilder body = new StringBuilder();
				body.Append(method.ToString().ToUpperInvariant()).Append('\n');
				body.Append(ToTimestamp(expiration) / 1000).Append('\n');
				body.Append(baseAddress.PathAndQuery).Append('/').Append(container).Append('/').Append(objectName);

				using (HMAC hmac = HMAC.Create())
				{
					hmac.HashName = "SHA1";
					hmac.Key = Encoding.UTF8.GetBytes(key);
					byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body.ToString()));
					string sig = string.Join(string.Empty, Array.ConvertAll(hash, i => i.ToString("x2")));
					string expires = (ToTimestamp(expiration) / 1000).ToString();

					return new Uri(String.Format("{0}/{1}/{2}?temp_url_sig={3}&temp_url_expires={4}", baseAddress, container, objectName, sig, expires));
				}
			}
			private long ToTimestamp(DateTimeOffset dateTimeOffset)
			{
				DateTimeOffset Epoch = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.Zero);
				return (long)(dateTimeOffset - Epoch).TotalMilliseconds;
			}

		}
		internal class BulkDeletionResultMapper : IObjectMapper<BulkDeleteResponse, BulkDeletionResults>
		{
			private readonly IStatusParser _statusParser;

			public BulkDeletionResultMapper(IStatusParser statusParser)
			{
				_statusParser = statusParser;
			}

			public BulkDeletionResults Map(BulkDeleteResponse from)
			{
				var successfulObjects = from.AllItems.Where(i => !from.IsItemError(i));
				var failedObjects = from.Errors.Select(e =>
				{
					var eParts = e.ToArray();
					Status errorStatus;
					string errorItem;

					if (eParts.Length != 2)
					{
						errorStatus = new Status(0, "Unknown");
						errorItem = string.Format("The error array has an unexpected length. Array: {0}", string.Join("||", eParts));
					}
					else
					{
						errorItem = eParts[1];
						if (!_statusParser.TryParse(eParts[0], out errorStatus))
						{
							errorItem = eParts[0];
							if (!_statusParser.TryParse(eParts[1], out errorStatus))
							{
								errorStatus = new Status(0, "Unknown");
								errorItem = string.Format("The error array is in an unknown format. Array: {0}", string.Join("||", eParts));
							}
						}
					}

					return new BulkDeletionFailedObject(errorItem, errorStatus);
				});

				return new BulkDeletionResults(successfulObjects, failedObjects);
			}

			public BulkDeleteResponse Map(BulkDeletionResults to)
			{
				throw new NotImplementedException();
			}
		}
		internal class CloudFilesMetadataProcessor : IObjectStorageMetadataProcessor
		{

			private static readonly CloudFilesMetadataProcessor _default = new CloudFilesMetadataProcessor();

			public static CloudFilesMetadataProcessor Default
			{
				get
				{
					return _default;
				}
			}

			public virtual Dictionary<string, Dictionary<string, string>> ProcessMetadata(IList<HttpHeader> httpHeaders)
			{
				if (httpHeaders == null)
					throw new ArgumentNullException(nameof(httpHeaders));

				var pheaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (var header in httpHeaders)
				{
					if (header == null)
						throw new ArgumentException("httpHeaders cannot contain any null values");
					if (string.IsNullOrEmpty(header.Key))
						throw new ArgumentException("httpHeaders cannot contain any values with a null or empty key");

					if (header.Key.StartsWith(CloudFilesProvider.AccountMetaDataPrefix, StringComparison.OrdinalIgnoreCase))
					{
						metadata.Add(header.Key.Substring(CloudFilesProvider.AccountMetaDataPrefix.Length), DecodeUnicodeValue(header.Value));
					}
					else if (header.Key.StartsWith(CloudFilesProvider.ContainerMetaDataPrefix, StringComparison.OrdinalIgnoreCase))
					{
						metadata.Add(header.Key.Substring(CloudFilesProvider.ContainerMetaDataPrefix.Length), DecodeUnicodeValue(header.Value));
					}
					else if (header.Key.StartsWith(CloudFilesProvider.ObjectMetaDataPrefix, StringComparison.OrdinalIgnoreCase))
					{
						metadata.Add(header.Key.Substring(CloudFilesProvider.ObjectMetaDataPrefix.Length), DecodeUnicodeValue(header.Value));
					}
					else
					{
						pheaders.Add(header.Key, header.Value);
					}
				}

				var processedHeaders = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
				{
					{CloudFilesProvider.ProcessedHeadersHeaderKey, pheaders},
					{CloudFilesProvider.ProcessedHeadersMetadataKey, metadata}
				};

				return processedHeaders;
			}

			private string DecodeUnicodeValue(string value)
			{
				return Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(value));
			}
		}

		[JsonObject(MemberSerialization.OptIn)]
		internal class BulkDeleteResponse
		{
			[JsonProperty("Number Not Found")]
			public int NumberNotFound { get; set; }

			[JsonProperty("Response Status")]
			public string Status { get; set; }

			[JsonProperty("Errors")]
			public IEnumerable<IEnumerable<string>> Errors { get; set; }

			[JsonProperty("Number Deleted")]
			public int NumberDeleted { get; set; }

			[JsonProperty("Response Body")]
			public string ResponseBody { get; set; }

			public IEnumerable<string> AllItems { get; set; }

			public bool IsItemError(string s)
			{
				return Errors.Any(e => e.Any(e2 => e2.Equals(s)));
			}
		}
		#region Json Classes
		[JsonObject(MemberSerialization.OptIn)]
		public class Endpoint
		{
			[JsonProperty("region_id")]
			public string Region_id { get; set; }
			[JsonProperty("url")]
			public string Url { get; set; }
			[JsonProperty("region")]
			public string Region { get; set; }
			[JsonProperty("interface")]
			public string @Interface { get; set; }
			[JsonProperty("id")]
			public string Id { get; set; }
		}
		[JsonObject(MemberSerialization.OptIn)]
		public class Catalog
		{
			[JsonProperty("endpoints")]
			public Endpoint[] Endpoints { get; set; }
			[JsonProperty("type")]
			public string Type { get; set; }
			[JsonProperty("id")]
			public string Id { get; set; }
			[JsonProperty("name")]
			public string Name { get; set; }
		}

		[JsonObject(MemberSerialization.OptIn)]
		public class UserAccessV3 : UserAccess
		{
			[JsonProperty("catalog")]
			public Catalog[] Catalog { get; set; }

			[JsonExtensionData]
			public IDictionary<string, JToken> _additionalData;
		}
		#endregion
		#endregion

	}
}

