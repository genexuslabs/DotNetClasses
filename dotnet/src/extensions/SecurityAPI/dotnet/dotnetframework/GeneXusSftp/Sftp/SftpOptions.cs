using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Sftp.GeneXusSftp
{
	[SecuritySafeCritical]
	public class SftpOptions : SecurityAPIObject
	{
		private string host;
		public string Host
		{
			get { return host; }
			set { host = value; }
		}
		private int port;
		public int Port
		{
			get { return port; }
			set { port = value; }
		}
		private string user;
		public string User
		{
			get { return user; }
			set { user = value; }
		}
		private string password;
		public string Password
		{
			get { return password; }
			set { password = value; }
		}
		private string keyPath;
		public string KeyPath
		{
			get { return keyPath; }
			set { SetKeyPath(value); }
		}
		private string keyPassword;
		public string KeyPassword
		{
			get { return keyPassword; }
			set { keyPassword = value; }
		}
		private bool allowHostKeyChecking;
		public bool AllowHostKeyChecking
		{
			get { return allowHostKeyChecking; }
			set { allowHostKeyChecking = value; }
		}
		private string knownHostsPath;
		public string KnownHostsPath
		{
			get { return knownHostsPath; }
			set { SetKnownHostsPath(value); }
		}
		private ExtensionsWhiteList _whiteList;
		public ExtensionsWhiteList WhiteList
		{
			get { return this._whiteList; }
			set { this._whiteList = value; }
		}

		[SecuritySafeCritical]
		public SftpOptions() : base()
		{
			this.host = "";
			this.port = 22;
			this.user = "";
			this.password = "";
			this.keyPath = "";
			this.keyPassword = "";
			this.allowHostKeyChecking = true;
			this.knownHostsPath = "";
			this._whiteList = null;
		}

		private void SetKeyPath(String value)
		{
			//C# apps allways runs on windows, shouldn't correct \\ on local paths
			//string path = $"/{value.Replace(@"\", "/")}";
			string path = value;
			if (!(SecurityUtils.extensionIs(path, ".key") || SecurityUtils.extensionIs(path, ".pem")
					|| SecurityUtils.extensionIs(path, "")))
			{
				this.error.setError("OP001",
						"Private key must be base64 encoded file (Valid extensions: .pem, .key, empty)");
			}
			else
			{

				this.keyPath = path;
			}
		}

		private void SetKnownHostsPath(String value)
		{
			//C# apps allways runs on windows, shouldn't correct \\ on local paths
			//var path = $"/{value.Replace(@"\", "/")}";
			string path = value;
			if (!SecurityUtils.extensionIs(path, ""))
			{
				this.error.setError("OP002", "No extension is allowed for known_hosts file");
			}
			else
			{
				this.knownHostsPath = path;
			}
		}
	}
}
