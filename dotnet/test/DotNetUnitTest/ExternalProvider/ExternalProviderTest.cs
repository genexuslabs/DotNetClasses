using System;
using System.Globalization;
using System.IO;
using System.Net;
using DotNetUnitTest;
using GeneXus.Storage.GXAmazonS3;
using GeneXus.Storage.GXAzureStorage;
using GeneXus.Storage.GXGoogleCloud;
using Xunit;

#pragma warning disable CA1031 // Do not catch general exception types
namespace UnitTesting
{
	public abstract class ExternalProviderTest
	{		
		private GeneXus.Services.ExternalProvider provider;
		private static String TEST_SAMPLE_FILE_NAME = "text.txt";
		private static String TEST_SAMPLE_FILE_PATH = Path.Combine("resources", TEST_SAMPLE_FILE_NAME).ToString(CultureInfo.InvariantCulture);


		public ExternalProviderTest(string providerName, Type externalProviderType)
		{
			

			bool testEnabled = Environment.GetEnvironmentVariable(providerName + "_TEST_ENABLED") == "true";

			Assume.True(testEnabled);

			provider = (GeneXus.Services.ExternalProvider)Activator.CreateInstance(externalProviderType);

			Assert.NotNull(provider);
		}

		[Fact]
		public void TestUploadPublicMethod()
		{
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, GxFileType.PublicRead);
			EnsureUrl(upload, GxFileType.PublicRead);
		}

		[Fact]
		public void TestUploadDefaultMethod()
		{
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, GxFileType.Default);
			EnsureUrl(upload, GxFileType.Default);
		}

		[Fact]
		public void TestUploadAndCopyDefault()
		{
			TestUploadAndCopyByAcl(GxFileType.Default, GxFileType.Default);
		}

		[Fact]
		public void TestUploadAndCopyPrivate()
		{
			TestUploadAndCopyByAcl(GxFileType.Private, GxFileType.Private);
		}

		[Fact]
		public void TestUploadAndCopyPublic()
		{
			TestUploadAndCopyByAcl(GxFileType.PublicRead, GxFileType.PublicRead);
		}

		[Fact]
		public void TestUploadAndCopyMixed()
		{
			TestUploadAndCopyByAcl(GxFileType.Default, GxFileType.Private);
		}

		[Fact]
		public void TestUploadAndCopyPrivateToPublic()
		{
			TestUploadAndCopyByAcl(GxFileType.Private, GxFileType.PublicRead);
		}

		[Fact]
		public void TestUploadAndCopyPublicToPrivate()
		{
			TestUploadAndCopyByAcl(GxFileType.PublicRead, GxFileType.Private);
		}

		public void TestUploadAndCopyByAcl(GxFileType aclUpload, GxFileType aclCopy)
		{
			String copyFileName = "test-upload-and-copy.txt";
			DeleteSafe(TEST_SAMPLE_FILE_PATH);
			DeleteSafe(copyFileName);
			String upload = provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, aclUpload);
			Assert.True(UrlExists(upload), "Not found URL: " + upload);

			String copyUrl = TryGet(copyFileName, aclCopy);
			Assert.False(UrlExists(copyUrl), "URL cannot exist: " + copyUrl);

			provider.Copy(TEST_SAMPLE_FILE_NAME, aclUpload, copyFileName, aclCopy);
			upload = provider.Get(copyFileName, aclCopy, 100);
			EnsureUrl(upload, aclCopy);
		}

		[Fact]
		public void TestCopyMethod()
		{
			String copyFileName = "copy-text.txt";
			Copy(copyFileName, GxFileType.PublicRead);
		}

		[Fact]
		public void TestCopyPrivateMethod()
		{
			String copyFileName = "copy-text-private.txt";
			Copy(copyFileName, GxFileType.Private);
		}

		private void Copy(String copyFileName, GxFileType acl)
		{
			provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, acl);
			String upload = provider.Get(TEST_SAMPLE_FILE_NAME, acl, 100);
			EnsureUrl(upload, acl);

			DeleteSafe(copyFileName);
			Wait(1000); //Google CDN replication seems to be delayed.

			String urlCopy = TryGet(copyFileName, GxFileType.PublicRead);
			Assert.False(UrlExists(urlCopy), "URL cannot exist: " + urlCopy);

			provider.Copy("text.txt", acl, copyFileName, acl);
			upload = provider.Get(copyFileName, acl, 100);
			EnsureUrl(upload, acl);
		}

		[Fact]
		public void TestMultimediaUpload()
		{
			String copyFileName = "copy-text-private.txt";
			GxFileType acl = GxFileType.Private;

			provider.Upload(TEST_SAMPLE_FILE_PATH, TEST_SAMPLE_FILE_NAME, acl);
			String upload = provider.Get(TEST_SAMPLE_FILE_NAME, acl, 100);
			EnsureUrl(upload, acl);

			provider.Delete(copyFileName, acl);
			provider.Copy("text.txt", acl, copyFileName, acl);
			upload = TryGet(copyFileName, acl);
			EnsureUrl(upload, acl);
		}

		[Fact]
		public void TestGetMethod()
		{
			TestUploadPublicMethod();
			String url = provider.Get("text.txt", GxFileType.PublicRead, 10);
			EnsureUrl(url, GxFileType.PublicRead);
		}

		[Fact]
		public void TestGetObjectName()
		{
			TestUploadPublicMethod();
			string url = provider.Get(TEST_SAMPLE_FILE_NAME, GxFileType.PublicRead, 10);
			Assert.True(UrlExists(url));
			string objectName;
			provider.TryGetObjectNameFromURL(url, out objectName);
			Assert.Equal("text.txt", objectName);
		}

		[Fact]
		public void TestDownloadMethod()
		{
			String downloadPath = Path.Combine("resources", "test", TEST_SAMPLE_FILE_NAME);
			TestUploadPublicMethod();

			try
			{
				File.Delete(downloadPath);
			}
			catch (Exception) { }

			provider.Download(TEST_SAMPLE_FILE_NAME, downloadPath, GxFileType.PublicRead);
			Assert.True(File.Exists(downloadPath));
		}

		[Fact]
		public void TestDeleteFile()
		{
			GxFileType acl = GxFileType.PublicRead;
			TestUploadPublicMethod();
			String url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			EnsureUrl(url, acl);
			provider.Delete(TEST_SAMPLE_FILE_NAME, acl);

			url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			Assert.False(UrlExists(url));
		}

		[Fact]
		public void TestDeleteFilePrivate()
		{
			GxFileType acl = GxFileType.Private;
			TestUploadPrivateMethod();
			String url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			EnsureUrl(url, acl);
			provider.Delete(TEST_SAMPLE_FILE_NAME, acl);

			url = TryGet(TEST_SAMPLE_FILE_NAME, acl);
			Assert.False(UrlExists(url));
		}

		[Fact]
		public void TestUploadPrivateMethod()
		{
			GxFileType acl = GxFileType.Private;
			String externalFileName = "text-private-2.txt";

			DeleteSafe(externalFileName);
			String signedUrl = provider.Upload(TEST_SAMPLE_FILE_PATH, externalFileName, acl);
			EnsureUrl(signedUrl, acl);
			signedUrl = provider.Get(externalFileName, acl, 10);
			EnsureUrl(signedUrl, acl);

		}


		private String TryGet(String objectName, GxFileType acl)
		{
			String getValue = "";
			try
			{
				getValue = provider.Get(objectName, acl, 5);
			}
			catch (Exception)
			{

			}
			return getValue;
		}

		private String GetSafe(String objectName, GxFileType acl)
		{
			try
			{
				return provider.Get(objectName, acl, 100);
			}
			catch (Exception)
			{

			}
			return String.Empty;
		}

		private bool DeleteSafe(String objectName)
		{
			DeleteSafeImpl(objectName, GxFileType.Private);
			DeleteSafeImpl(objectName, GxFileType.PublicRead);
			return true;
		}

		private bool DeleteSafeImpl(String objectName, GxFileType acl)
		{
			try
			{
				provider.Delete(objectName, acl);
			}
			catch (Exception)
			{

			}
			return true;
		}

		private static void Wait(int milliseconds)
		{			
			System.Threading.Thread.Sleep(milliseconds);
		}

		private static void EnsureUrl(String signedOrUnsignedUrl, GxFileType acl)
		{
			Assert.True(UrlExists(signedOrUnsignedUrl), "Resource not found: " + signedOrUnsignedUrl);
			if (acl == GxFileType.Private)
			{
				String noSignedUrl = signedOrUnsignedUrl.Substring(0, signedOrUnsignedUrl.IndexOf('?') + 1);
				Assert.False(UrlExists(noSignedUrl), "Resource must be private: " + noSignedUrl);
			}
		}

#pragma warning disable CA1054 // Uri parameters should not be strings
		protected static bool UrlExists(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return false;
			}
			bool exists = false;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			try
			{
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					exists = response.StatusCode == HttpStatusCode.OK;
				}
			}
			catch (WebException)
			{

			}
			return exists;
		}
#pragma warning restore CA1054 // Uri parameters should not be strings
	}
}
#pragma warning restore CA1031 // Do not catch general exception types