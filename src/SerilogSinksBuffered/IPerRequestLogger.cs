namespace Serilog.Sinks.Buffered
{
    public interface IPerRequestLogger
    {
        void Complete(string requestId);
    }
}