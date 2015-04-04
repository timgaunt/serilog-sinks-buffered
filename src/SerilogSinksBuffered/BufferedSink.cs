using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace SerilogSinksBuffered
{
    public class BufferedSink : Serilog.Core.ILogEventSink, IPerRequestLogger, IFlushPerRequestLogs
    {
        private const int DefaultBufferCapacity = 100;
        private const int ScanForDeadRequestsAfterNumberOfEvents = 100;
        private readonly LoggingLevelSpecification _allEventLevel;
        private readonly LoggingLevelSpecification _eventLevel;
        private readonly string _requestIdProperty;
        private readonly int _maxRequestAgeInSeconds;
        private readonly Dictionary<string, PerRequestLogBuffer> _buffers;
        private readonly ILogEventSink _sink;
        private int _currentScanCount = 0;

        public BufferedSink(LogEventLevel eventLevel, LogEventLevel allEventLevel, string requestIdProperty, int maxRequestAgeInSeconds, ILogEventSink innerSink)
        {
            _eventLevel = new LoggingLevelSpecification(eventLevel);
            _allEventLevel = new LoggingLevelSpecification(allEventLevel);
            _sink = innerSink;
            _requestIdProperty = requestIdProperty;
            _maxRequestAgeInSeconds = maxRequestAgeInSeconds;
            _buffers = new Dictionary<string, PerRequestLogBuffer>();
        }

        public void Emit(LogEvent logEvent)
        {
            ScanForDeadRequests();
            LogEventPropertyValue requestIdValue;
            if (logEvent.Properties.TryGetValue(_requestIdProperty, out requestIdValue))
            {
                string requestId = requestIdValue.ToString(null, null).Replace("\"", string.Empty);
                var buffer = GetOrCreateRequestBuffer(requestId);
                buffer.LogEvent(logEvent);
            }
            else
            {
                if (_eventLevel.IsSatisifedBy(logEvent.Level))
                {
                    _sink.Emit(logEvent);
                }
            }
        }

        private void ScanForDeadRequests()
        {
            _currentScanCount++;
            if (_currentScanCount < ScanForDeadRequestsAfterNumberOfEvents)
                return;

            DateTime now = DateTime.UtcNow;

            KeyValuePair<string, PerRequestLogBuffer>[] copyOfBuffers;
            lock (_buffers)
            {
                copyOfBuffers = _buffers.ToArray();
            }

            foreach (var requestBuffer in copyOfBuffers)
            {
                if (requestBuffer.Value == null) continue;
                var lastUsedAt = requestBuffer.Value.LastEventTime;
                var amountOfTimeSinceLastUse = now - lastUsedAt;
                if (amountOfTimeSinceLastUse.TotalSeconds > _maxRequestAgeInSeconds)
                {
                    CompleteAndRemove(requestBuffer.Key, requestBuffer.Value);
                }
            }
        }

        private PerRequestLogBuffer GetOrCreateRequestBuffer(string requestId)
        {
            PerRequestLogBuffer logBuffer = null;
            if (!_buffers.TryGetValue(requestId, out logBuffer))
            {                
                lock (_buffers)
                {
                    if (!_buffers.TryGetValue(requestId, out logBuffer))
                    {
                        logBuffer = new PerRequestLogBuffer(DefaultBufferCapacity, _eventLevel, _allEventLevel, _sink);
                        _buffers.Add(requestId, logBuffer);
                    }
                }
            }
            return logBuffer;
        }

        void IPerRequestLogger.Complete(string requestId)
        {
            PerRequestLogBuffer logBuffer = null;
            if (_buffers.TryGetValue(requestId, out logBuffer))
            {
                CompleteAndRemove(requestId, logBuffer);
            }
        }

        private void CompleteAndRemove(string requestId, PerRequestLogBuffer logBuffer)
        {
            try
            {
                logBuffer.Complete();
            }
            finally
            {
                lock (_buffers)
                {
                    _buffers.Remove(requestId);
                }
            }
        }

        void IFlushPerRequestLogs.Flush(string requestId)
        {
            IFlushPerRequestLogs me = this;
            me.Flush(requestId, le => true);
        }

        void IFlushPerRequestLogs.Flush(string requestId, Func<LogEvent, bool> filter)
        {
            PerRequestLogBuffer logBuffer = null;
            if (_buffers.TryGetValue(requestId, out logBuffer))
            {
                logBuffer.Flush(filter);
            }
        }

        void IFlushPerRequestLogs.FlushAll()
        {
            IFlushPerRequestLogs me = this;
            me.FlushAll(le => true);
        }

        void IFlushPerRequestLogs.FlushAll(Func<LogEvent, bool> filter)
        {
            PerRequestLogBuffer[] copyOfLogBuffers;
            lock (_buffers)
            {
                copyOfLogBuffers = _buffers.Select(i => i.Value).ToArray();
            }
            foreach (var logBuffer in copyOfLogBuffers)
            {
                logBuffer.Flush(filter);
            }
        }

    }

}
