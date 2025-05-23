using System;
using System.Net.Sockets;
using GeneXus.Application;
using Xunit;

namespace xUnitTesting
{
	public class GxFtpTest
	{
		[Theory]
		[InlineData("127.0.0.1", AddressFamily.InterNetwork)] // IPv4
		[InlineData("localhost", AddressFamily.InterNetwork)] // Hostname resolving to IPv4
		public void FtpAddressFamilyTest(string host, AddressFamily addressFamily)
		{
			GxContext context = new GxContext();
			int code;
			string msg;
			Console.WriteLine("Testing AddressFamily for host: " + host + " with address family: " + addressFamily);
			context.FtpInstance.Connect(host, "admin", "P@ssw0rd");
			context.FtpInstance.GetErrorCode(out code);
			Assert.Equal(1, code);
			context.FtpInstance.GetStatusText(out msg);
			Assert.Equal("Login Failed.", msg);
		}
	
	}
}
