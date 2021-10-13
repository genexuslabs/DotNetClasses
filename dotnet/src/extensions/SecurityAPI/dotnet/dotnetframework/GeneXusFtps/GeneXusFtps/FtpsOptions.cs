using GeneXusFtps.GeneXusFtpsUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;
using System;
using System.Security;

namespace GeneXusFtps.GeneXusFtps
{
	[SecuritySafeCritical]
	public class FtpsOptions : SecurityAPIObject
	{
		private string _host;
		public string Host
		{
			get { return this._host; }
			set { this._host = value; }
		}
		private int _port;
		public int Port
		{
			get { return this._port; }
			set { this._port = value; }
		}
		private string _user;
		public string User
		{
			get { return this._user; }
			set { this._user = value; }
		}
		private string _password;
		public string Password
		{
			get { return this._password; }
			set { this._password = value; }
		}
		private bool _forceEncryption;
		public bool ForceEncryption
		{
			get { return this._forceEncryption; }
			set { this._forceEncryption = value; }
		}
		private FtpConnectionMode _connectionMode;
		public string ConnectionMode
		{
			get { return FtpConnectionModeUtils.valueOf(this._connectionMode, this.error); }
			set { this._connectionMode = FtpConnectionModeUtils.getFtpMode(value, this.error); }
		}
		private FtpEncoding _encoding;
		public string Encoding
		{
			get { return FtpEncodingUtils.valueOf(this._encoding, this.error); }
			set { this._encoding = FtpEncodingUtils.getFtpEncoding(value, this.error); }
		}
		private FtpEncryptionMode _encryptionMode;
		public string EncryptionMode
		{
			get { return FtpEncryptionModeUtils.valueOf(this._encryptionMode, this.error); }
			set { this._encryptionMode = FtpEncryptionModeUtils.getFtpEncryptionMode(value, this.error); }
		}
		private string _trustStorePath;
		public string TrustStorePath
		{
			get { return this._trustStorePath; }
			set { SetTrustStorePath(value); }
		}

		private string _trustStorePassword;
		public string TrustStorePassword
		{
			get { return this._trustStorePassword; }
			set { this._trustStorePassword = value; }
		}
		private FtpsProtocol _protocol;
		public string Protocol
		{
			get { return FtpsProtocolUtils.valueOf(this._protocol, this.error); }
			set { this._protocol = FtpsProtocolUtils.getFtpsProtocol(value, this.error); }
		}

		private ExtensionsWhiteList _whiteList;
		public ExtensionsWhiteList WhiteList
		{
			get { return this._whiteList; }
			set { this._whiteList = value; }
		}

		[SecuritySafeCritical]
		public FtpsOptions() : base()
		{
			this._host = "";
			this._port = 21;
			this._user = "";
			this._password = "";
			this._forceEncryption = true;
			this._connectionMode = FtpConnectionMode.PASSIVE;
			this._encoding = FtpEncoding.BINARY;
			this._encryptionMode = FtpEncryptionMode.EXPLICIT;
			this._trustStorePath = "";
			this._trustStorePassword = "";
			this._protocol = FtpsProtocol.TLS1_2;
			this._whiteList = null;
		}

		[SecuritySafeCritical]
		public void SetTrustStorePath(String value)
		{
			if (!(SecurityUtils.extensionIs(value, ".pfx") || SecurityUtils.extensionIs(value, ".p12")
					|| SecurityUtils.extensionIs(value, ".jks") || SecurityUtils.extensionIs(value, ".crt")))
			{
				error.setError("FO001", "Unexpected extension for trust store); valid extensions: .p12 .jks .pfx");
			}
			else
			{
				this._trustStorePath = value;
			}
		}

		[SecuritySafeCritical]
		internal FtpConnectionMode GetFtpConnectionMode()
		{
			return this._connectionMode;
		}

		[SecuritySafeCritical]
		internal FtpEncryptionMode GetFtpEncryptionMode()
		{
			return this._encryptionMode;
		}

		[SecuritySafeCritical]
		internal FtpEncoding GetFtpEncoding()
		{
			return this._encoding;
		}

		[SecuritySafeCritical]
		internal FtpsProtocol GetFtpsProtocol()
		{
			return this._protocol;
		}
	}
}
