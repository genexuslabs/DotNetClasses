using GeneXusFtps.GeneXusFtps;
using NUnit.Framework;
using SecurityAPITest.SecurityAPICommons.commons;
using System.IO;

namespace SecurityAPITest.Ftps
{
	[TestFixture]
	[RunIfRunSettingsConfigured]
	public class TestFtpsDomainSpaces : SecurityAPITestObject
	{
		protected static string host;
		protected static string user;
		protected static string password;
		protected static string localPath;
		protected static string remoteDir;
		protected static string remoteFilePath;
		protected static string localDir;
		protected static string pwd;
		protected static int port;
		protected static string connectionMode;
		protected static bool forceEncryption;
		protected static string encoding;
		protected static string encryptionMode;
		protected static string protocol;

		[SetUp]
		public virtual void SetUp()
		{
			host = TestContextParameter("gx_ftp_host");
			user = TestContextParameter("gx_ftps_user");
			password = TestContextParameter("gx_ftps_password");
			localPath = Path.Combine(BASE_PATH, "Temp", "ftps", "ftpstest.txt");
			remoteDir = "/files";
			remoteFilePath = "/files/ftpstest.txt";
			localDir = Path.Combine(BASE_PATH, "Temp", "ftps", "back");
			port = 21;
			connectionMode = "PASSIVE ";
			forceEncryption = true;
			encoding = " BINARY";
			encryptionMode = " EXPLICIT";
			protocol = "TLS1_2 ";
		}

		private FtpsClient TestConnection(FtpsOptions options)
		{
			FtpsClient client = new FtpsClient();
			bool connected = client.Connect(options);
			True(connected, client);
			return client;
		}

		private void TestPut(FtpsClient client)
		{
			bool put = client.Put(localPath, remoteDir);
			True(put, client);
		}

		private void TestGet(FtpsClient client)
		{
			bool get = client.Get(remoteFilePath, localDir);
			True(get, client);
		}

		[Test]
		public void TestSFtpsSpaces()
		{
			FtpsOptions options = new FtpsOptions();
			options.Host = host;
			options.User  = user;
			options.Password = password;
			options.Port = port;
			options.ForceEncryption = forceEncryption;
			options.ConnectionMode = connectionMode;
			options.EncryptionMode = encryptionMode;
			options.Protocol = protocol;
			FtpsClient client = TestConnection(options);
			TestPut(client);
			TestGet(client);
			client.Disconnect();
		}
	}
}
