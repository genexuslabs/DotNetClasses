using System;
using GeneXus.Data;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class GxConnectionTest
	{
		[Fact]
		public void AzureADAuthenticationConnectionString()
		{
			GxSqlServer sqlserver = new GxSqlServer();
			string server = "localhost";
			string port = "1433";
			string user = "testuser";
			string password = "testpassword";
			string database = "initaldb";
			string additionalConnectionString;

			additionalConnectionString = "Authentication=Active Directory Integrated;";
			string connStr = sqlserver.BuildConnectionStringImpl(server, user, password, database, port, string.Empty, additionalConnectionString);
			Assert.DoesNotContain(password, connStr, StringComparison.OrdinalIgnoreCase);
			Assert.Contains(user, connStr, StringComparison.OrdinalIgnoreCase);

			additionalConnectionString = "Authentication=Active Directory Interactive;";
			connStr = sqlserver.BuildConnectionStringImpl(server, user, password, database, port, string.Empty, additionalConnectionString);
			Assert.DoesNotContain(password, connStr, StringComparison.OrdinalIgnoreCase);
			Assert.Contains(user, connStr, StringComparison.OrdinalIgnoreCase);

			additionalConnectionString = "Authentication=Active Directory Service Principal; Encrypt=True;";
			connStr = sqlserver.BuildConnectionStringImpl(server, user, password, database, port, string.Empty, additionalConnectionString);
			Assert.Contains(password, connStr, StringComparison.OrdinalIgnoreCase);
			Assert.Contains(user, connStr, StringComparison.OrdinalIgnoreCase);

			additionalConnectionString= "Authentication=Active Directory Managed Identity;";
			connStr = sqlserver.BuildConnectionStringImpl(server, user, password, database, port, string.Empty, additionalConnectionString);
			Assert.DoesNotContain(password, connStr, StringComparison.OrdinalIgnoreCase);
			Assert.Contains(user, connStr, StringComparison.OrdinalIgnoreCase);


			additionalConnectionString = "Authentication=Active Directory Device Code Flow;";
			connStr = sqlserver.BuildConnectionStringImpl(server, user, password, database, port, string.Empty, additionalConnectionString);
			Assert.DoesNotContain(password, connStr, StringComparison.OrdinalIgnoreCase);
			Assert.DoesNotContain(user, connStr, StringComparison.OrdinalIgnoreCase);

		}
	}
}
