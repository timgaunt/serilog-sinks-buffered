using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Buffered.NestedScopes;

namespace Serilog.Sinks.Buffered
{
    public class PerRequestLogBuffer
    {
        private class LogEventEntry
        {
            public LogEventEntry(LogEvent @event, bool writeToSink)
            {
                Event = @event;
                WriteToSink = writeToSink;
                NestingLevel = GetNestingLevel(@event);
            }

            public LogEvent Event { get; set; }
            public bool WriteToSink { get; set; }
            int[] NestingLevel { get; set; }

            public bool IsSiblingOrParentOf(LogEventEntry entry)
            {
                var nestingLevelRoot = entry.NestingLevel.Take(NestingLevel.Length);

                return NestingLevel.SequenceEqual(nestingLevelRoot);
            }

            private int[] GetNestingLevel(LogEvent @event)
            {
                LogEventPropertyValue nestingLevel = null;

                @event.Properties.TryGetValue(NestedLoggingSettings.NestingLevelPropertyName,
                    out nestingLevel);

                if (nestingLevel == null) return new int[0];

                var items = (Serilog.Events.SequenceValue)nestingLevel;

                return items.Elements.OfType<Serilog.Events.ScalarValue>().Select(e => (int)e.Value).ToArray();
            }
        }

        private readonly LoggingLevelSpecification _eventLevel;
        private readonly LoggingLevelSpecification _allEventLevel;
        private readonly ILogEventSink _sink;
        private readonly List<LogEventEntry> _eventEntries;
        private bool _completed;

        public PerRequestLogBuffer(int initialCapacity, LoggingLevelSpecification eventLevel, LoggingLevelSpecification allEventLevel,
            ILogEventSink sink)
        {
            _eventLevel = eventLevel;
            _allEventLevel = allEventLevel;
            _sink = sink;
            _eventEntries = new List<LogEventEntry>(initialCapacity);
        }

        public void LogEvent(LogEvent @event)
        {
            var entry = new LogEventEntry(@event, false);
            _eventEntries.Add(entry);
            LastEventTime = @event.Timestamp;
            CheckTriggerLevel(entry);
        }

        private void CheckTriggerLevel(LogEventEntry entry)
        {
            if (_allEventLevel.IsSatisifedBy(entry.Event.Level))
            {
                MarkEventsForWriteOnTrigger(entry);
            }
        }

        public DateTimeOffset LastEventTime { get; private set; }

        private void MarkEventsForWriteOnTrigger(LogEventEntry triggeringEntry)
        {
            var copyOfEvents = _eventEntries.ToArray();
            foreach (var entry in copyOfEvents)
            {
                if (triggeringEntry.IsSiblingOrParentOf(entry))
                {
                    entry.WriteToSink = true;
                }

                if (entry == triggeringEntry)
                {
                    break;
                }
            }
        }

        private void MarkEventsForWrite()
        {
            MarkEventsForWrite(e => _eventLevel.IsSatisifedBy(e.Level));            
        }

        internal void Flush(Func<Serilog.Events.LogEvent, bool> filter)
        {
            if (_completed) return;
            MarkEventsForWrite(filter);
        }

        private void MarkEventsForWrite(Func<LogEvent, bool> filter)
        {
            var copyOfEvents = _eventEntries.ToArray();
            foreach (var entry in copyOfEvents)
            {
                if (filter(entry.Event))
                {
                    entry.WriteToSink = true;
                }
            }
        }

        private void WriteEventsToSinks()
        {
            var copyOfEvents = _eventEntries.ToArray();
            foreach (var entry in copyOfEvents.Where(e => e.WriteToSink))
            {
                _sink.Emit(entry.Event);
            }
        }

        public void Complete()
        {
            MarkEventsForWrite();
            WriteEventsToSinks();
            _completed = true;
        }

    }
}