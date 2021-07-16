using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Functions.Tests
{
	public enum LoggerTypes
	{
		Null,
		List
	}

	public class TestFactory
	{
			
		public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
		{
			ILogger logger;

			if (type == LoggerTypes.List)
			{
				logger = new ListLogger();
			}
			else
			{
				logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
			}

			return logger;
		}

	}
}