using GeneXus.Application;
using Xunit;
using System;
using GeneXus.Configuration;
using GeneXus.Data.NTier;

namespace UnitTesting
{
	public class ConfigTest
	{
		[Fact(Skip="SetEnvironmentVarsBeforeRunning")]
		public void ConMappingTest()
		{
			string customUser = "customUser";
			string customDB = "customDB";
			Environment.SetEnvironmentVariable("GXConnectionDefaultDB", customDB);
			Environment.SetEnvironmentVariable("GXConnectionDefaultUser", customUser);
			GxContext context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			string database, user;
			Config.GetValueOf("Connection-Default-DB", out database);
			Config.GetValueOf("Connection-Default-User", out user);
			Assert.Contains(customDB, database, StringComparison.OrdinalIgnoreCase);
			Assert.Contains(customUser, user, StringComparison.OrdinalIgnoreCase);
		}

	}
}
