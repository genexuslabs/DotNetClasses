using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using GeneXus.Diagnostics;
using Xunit;
namespace BenchmarkTest
{
	public class LoggerBenchmark
	{
		[Params(1000)]
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
	public class PerformanceTests
	{
#if RELEASE
		const double MAX_NANOSECONDS = 150000; 
		[Fact]
		public void RunLoggerBenchmark()
		{
			Summary summary = BenchmarkRunner.Run<LoggerBenchmark>();
			Assert.NotEmpty(summary.Reports);
			foreach (BenchmarkReport report in summary.Reports)
			{
				Assert.NotEmpty(report.AllMeasurements);
				foreach (Measurement runMeasure in report.AllMeasurements)
				{
					Assert.InRange(runMeasure.Nanoseconds, low: 0, high: MAX_NANOSECONDS);
				}
			}
		}
#endif
	}
}