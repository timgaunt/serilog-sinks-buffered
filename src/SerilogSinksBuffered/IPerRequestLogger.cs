namespace SerilogSinksBuffered
{
    public interface IPerRequestLogger
    {
        void Complete(string requestId);
    }
}