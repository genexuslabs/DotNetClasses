using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using Sftp.GeneXusSftp;
using System.IO;
using System;

namespace SecurityAPITest.Sftp
{
    [TestFixture]
	[RunIfRunSettingsConfigured]
	public class TestSftp: SecurityAPITestObject
    {
		protected static string host;
		protected static string user;
		protected static string known_hosts;
		protected static string password;
		protected static string keyPath;
		protected static string keyPassword;
		protected static string localPath;
		protected static string remoteDir;
		protected static string remoteFilePath;
		protected static string localDir;
		protected static string pwd;
	
		[SetUp]
		public virtual void SetUp()
		{
			host = TestContextParameter("gx_ftp_host");
			user = TestContextParameter("gx_sftp_user");
			string known_hosts_content_base64 = TestContextParameter("gx_ftp_known_hosts_content_base64");
			known_hosts = Path.Combine(BASE_PATH, "Temp", "sftptest", "key", "known_hosts");
			File.WriteAllBytes(known_hosts, Convert.FromBase64String(known_hosts_content_base64));
			password = TestContextParameter("gx_sftp_password");
			keyPath = Path.Combine(BASE_PATH, "Temp", "sftptest", "key", "id_rsa");
			string id_rsaConentBase64 = TestContextParameter("gx_ftp_id_rsa_content_base64");
			File.WriteAllBytes(keyPath, Convert.FromBase64String(id_rsaConentBase64));
			keyPassword = TestContextParameter("gx_sftp_key_password");
			localPath = Path.Combine(BASE_PATH, "Temp", "sftptest", "sftptest1.txt");
			remoteDir = "sftp";
			remoteFilePath = "sftp/sftptest1.txt";
			localDir = Path.Combine(BASE_PATH, "Temp", "sftptest", "back");
		}

		private SftpClient TestConnection(SftpOptions options)
		{
			SftpClient client = new SftpClient();
			bool connected = client.Connect(options);
			True(connected, client);
			return client;
		}

		private void TestPut(SftpClient client)
		{
			bool put = client.Put(localPath, remoteDir);
			True(put, client);
		}

		private void TestGet(SftpClient client)
		{
			bool get = client.Get(remoteFilePath, localDir);
			True(get, client);
		}

		[Test]
		public void TestWithUserPassword()
		{
			SftpOptions options = new SftpOptions();
			options.Host = host;
			options.User = user;
			options.Password = password;
			options.AllowHostKeyChecking = false;
			SftpClient client = TestConnection(options);
			TestPut(client);
			TestGet(client);
			client.Disconnect();
		}

		[Test]
		public void TestWithKey()
		{
			SftpOptions options = new SftpOptions();
			options.Host = host;
			options.User = user;
			options.Password = password;
			options.AllowHostKeyChecking = false;
			options.KeyPassword = keyPassword;
			SftpClient client = TestConnection(options);
			TestPut(client);
			TestGet(client);
			client.Disconnect();
		}

		[Test]
		public void TestWithKeyAndKnown_Hosts()
		{
			SftpOptions options = new SftpOptions();
			options.Host = host;
			options.User = user;
			options.Password = password;
			options.AllowHostKeyChecking = true;
			options.KeyPassword = keyPassword;
			options.KnownHostsPath = known_hosts;
			SftpClient client = TestConnection(options);
			TestPut(client);
			TestGet(client);
			client.Disconnect();
		}

		[Test]
		public void TestRoot()
		{
			SftpOptions o = new SftpOptions();
			o.Host = host;
			o.User = user;
			o.Password = password;
			o.AllowHostKeyChecking = false;
			SftpClient c = new SftpClient();
			c.Connect(o);
			bool put = c.Put(localPath, "/");
			True(put, c);
			bool get = c.Get("sftptest1.txt", localPath);
			True(get, c);
			c.Disconnect();
		}
	}
}
