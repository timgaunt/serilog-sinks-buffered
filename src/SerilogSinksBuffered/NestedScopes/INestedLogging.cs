namespace Serilog.Sinks.Buffered.NestedScopes
{
    public interface INestedLogging
    {
        NestedLoggingScope BeginScope();
        NestedLoggingScope BeginScope(string name);
    }
}