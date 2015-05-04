namespace Serilog.Sinks.Buffered.NestedScopes
{
    public class InMemoryNestedLoggingProvider : INestedLoggingProvider
    {
        private INestedLogging _nestedLogging;

        public INestedLogging GetCurrentNestedLogging()
        {
            if (_nestedLogging == null)
            {
                _nestedLogging = new NestedLogging();
            }
            return _nestedLogging;
        }
    }
}