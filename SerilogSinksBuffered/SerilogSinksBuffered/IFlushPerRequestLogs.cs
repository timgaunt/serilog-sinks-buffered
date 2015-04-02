using System;

namespace SerilogSinksBuffered
{
    public interface IFlushPerRequestLogs
    {
        void Flush(string requestId);
        void Flush(string requestId, Func<Serilog.Events.LogEvent, bool> filter);
        void FlushAll();
        void FlushAll(Func<Serilog.Events.LogEvent, bool> filter);
    }
}