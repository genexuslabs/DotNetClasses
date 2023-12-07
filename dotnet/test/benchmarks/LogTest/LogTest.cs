using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using GeneXus.Diagnostics;
using Xunit;
namespace BenchmarkTest
{
#if DEBUG
	[Config(typeof(CustomBenchmarkConfig))]
#endif
	public class LoggerBenchmark
	{
		[Params(100)]
		public int N;
		const int Debug = 5;

		[Benchmark]
		public void LogOnPerformanceTest()
		{
			for ( int i = 0; i < N; i++)
			{
				Log.Write("Test Message", "Test Topic", Debug);
			}
		}
	}
	public class CustomBenchmarkConfig : ManualConfig
	{
		public CustomBenchmarkConfig()
		{
			WithOptions(ConfigOptions.DisableOptimizationsValidator);
		}
	}
	public class PerformanceTests
	{
		const double MAX_NANOSECONDS = 150000; 
		[Fact]
		public void RunLoggerBenchmark()
		{
			Summary summary = BenchmarkRunner.Run<LoggerBenchmark>();
			Assert.Single(summary.Reports);

			foreach (BenchmarkReport report in summary.Reports)
			{
				Assert.NotEmpty(report.AllMeasurements);
				Measurement runMeasure = report.AllMeasurements.FirstOrDefault();
				Assert.InRange(runMeasure.Nanoseconds, low: 0, high: MAX_NANOSECONDS);
				
			}
		}
	}
}