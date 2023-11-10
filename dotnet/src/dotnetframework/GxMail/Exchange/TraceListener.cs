using Microsoft.Exchange.WebServices.Data;

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
