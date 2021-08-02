﻿using SecurityAPICommons.Commons;
using System;
using System.Security;

namespace GeneXusFtps.GeneXusFtpsUtils
{
	[SecuritySafeCritical]
	public enum FtpsProtocol
	{

		NONE, TLS1_0, TLS1_1, TLS1_2, SSLv2, SSLv3
	}

	[SecuritySafeCritical]
	public class FtpsProtocolUtils
	{

		[SecuritySafeCritical]
		public static FtpsProtocol getFtpsProtocol(String ftpsProtocol, Error error)
		{
			switch (ftpsProtocol.Trim())
			{
				case "TLS1_0":
					return FtpsProtocol.TLS1_0;
				case "TLS1_1":
					return FtpsProtocol.TLS1_1;
				case "TLS1_2":
					return FtpsProtocol.TLS1_2;
				case "SSLv2":
					return FtpsProtocol.SSLv2;
				case "SSLv3":
					return FtpsProtocol.SSLv3;
				default:
					error.setError("FP001", "Unknown protocol");
					return FtpsProtocol.NONE;

			}

		}

		[SecuritySafeCritical]
		public static String valueOf(FtpsProtocol ftpsProtocol, Error error)
		{
			switch (ftpsProtocol)
			{
				case FtpsProtocol.TLS1_0:
					return "TLS1_0";
				case FtpsProtocol.TLS1_1:
					return "TLS1_1";
				case FtpsProtocol.TLS1_2:
					return "TLS1_2";
				case FtpsProtocol.SSLv2:
					return "SSLv2";
				case FtpsProtocol.SSLv3:
					return "SSLv3";
				default:
					error.setError("FP002", "Unknown protocol");
					return "";
			}
		}

	}
}
