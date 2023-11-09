using GeneXus;
using GeneXus.Configuration;
using Xunit;

namespace DotNetUnitTest.Log
{
	public class LogTest
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<LogTest>();
		[Fact]
		public void TestLogOutput()
		{
			Config.LoadConfiguration();
			GXLogging.Debug(log, "Test Debug");
			GXLogging.Info(log, "Test Info");
			GXLogging.Warn(log, "Test Warn");
			GXLogging.Trace(log, "Test Trace {p1}", "parameter1");
		} 
	}
}
