using System;
using System.Diagnostics;
using System.Web.Configuration;
using GeneXus.Application;
using GeneXus.Utils;
using GeneXus.Web.Security;
using Xunit;

namespace UnitTesting
{
	public class SignatureTest
	{
		[Fact]
		public void SignSecurityToken()
		{
			GxContext context = new GxContext();
			string signed = WebSecurityHelper.Sign("WFPROTOTYPER", string.Empty, "Customer.CustomerRegistration", SecureTokenHelper.SecurityMode.Sign, context);
			Assert.NotEmpty(signed);

		}
	}
}
