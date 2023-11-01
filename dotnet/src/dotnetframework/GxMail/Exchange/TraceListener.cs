using log4net;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.Mail.Exchange
{
    public class TraceListener : ITraceListener
    {
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<TraceListener>();
		public void Trace(string traceType, string traceMessage)
        {
			GXLogging.Debug(log, string.Format("Trace Type: {0} - Message: {1}", traceType, traceMessage));           
        }         
    }
}
