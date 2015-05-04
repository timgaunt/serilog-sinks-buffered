using System;

namespace Serilog.Sinks.Buffered.Web
{
    public static class HttpFlushExtensions
    {
        public static void Flush(this IFlushPerRequestLogs flush)
        {
            flush.Flush(RequestId.GetCurrentId());
        }

        public static void Flush(this IFlushPerRequestLogs flush, Func<Serilog.Events.LogEvent, bool> filter)
        {
            flush.Flush(RequestId.GetCurrentId(), filter);
        }
    }
}
