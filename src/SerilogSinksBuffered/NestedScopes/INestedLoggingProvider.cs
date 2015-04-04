namespace SerilogSinksBuffered.NestedScopes
{
    public interface INestedLoggingProvider
    {
        INestedLogging GetCurrentNestedLogging();
    }
}