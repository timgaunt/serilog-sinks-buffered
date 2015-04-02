using Serilog.Events;

namespace SerilogSinksBuffered
{
    public class LoggingLevelSpecification
    {
        private readonly LogEventLevel _levelToMeet;

        public LoggingLevelSpecification(Serilog.Events.LogEventLevel levelToMeet)
        {
            _levelToMeet = levelToMeet;
        }

        public bool IsSatisifedBy(Serilog.Events.LogEventLevel level)
        {
            return (level >= _levelToMeet);
        }
    }
}
