using System;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.Buffered
{
    public static class BufferedSinkConfigExtensions
    {
        public static LoggerConfiguration Buffer(this LoggerConfiguration config,
            LogEventLevel eventLevel, 
            LogEventLevel allEventLevel,
            string requestIdProperty, 
            int maxRequestAgeInSeconds,
            out BufferedSink bufferedSink,
            Action<LoggerConfiguration> configureLogger,            
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            var lc = new LoggerConfiguration()
                .MinimumLevel.Verbose();

            configureLogger(lc);

            var log = lc.CreateLogger();
            var innerLoggerAsSink = (ILogEventSink) log;
            var buffer = new BufferedSink(eventLevel, allEventLevel, requestIdProperty, maxRequestAgeInSeconds, innerLoggerAsSink);
            bufferedSink = buffer;
            return config.WriteTo.Sink(buffer);
        }
    }
}