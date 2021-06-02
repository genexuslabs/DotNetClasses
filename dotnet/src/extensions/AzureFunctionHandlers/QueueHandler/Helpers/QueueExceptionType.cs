using System;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Deploy.AzureFunctions.QueueHandler.Helpers
{
	static class QueueExceptionType
	{
		public const string SysRuntimeError = "[SystemRuntimeError]";
		public const string AppError = "[ApplicationError]";
	}
}
