namespace SerilogSinksBuffered.NestedScopes
{
    public interface INestedLogging
    {
        NestedLoggingScope BeginScope();
        NestedLoggingScope BeginScope(string name);
    }
}