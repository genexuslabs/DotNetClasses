using System;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Deploy.AzureFunctions.ServiceBusHandler.Helpers
{
	static class ServiceBusExceptionType
	{
		public const string SysRuntimeError = "[SystemRuntimeError]";
		public const string AppError = "[ApplicationError]";
	}
}
