namespace Serilog.Sinks.Buffered.NestedScopes
{
    public interface INestedLoggingProvider
    {
        INestedLogging GetCurrentNestedLogging();
    }
}