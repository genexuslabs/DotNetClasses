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
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(TraceListener));
        
        public void Trace(string traceType, string traceMessage)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(String.Format("Trace Type: {0} - Message: {1}", traceType, traceMessage));
            }            
        }         
    }
}
