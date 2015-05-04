using System.Web;
using Serilog.Sinks.Buffered.NestedScopes;

namespace Serilog.Sinks.Buffered.Web
{
    public class HttpNestedLoggingProvider : INestedLoggingProvider
    {
        public const string NestingCacheKey = "TheSiteDoctor.NestedLoggingScope";

        public INestedLogging GetCurrentNestedLogging()
        {
            var context = HttpContext.Current;
            if (context == null) return null;

            var nestedLogging = context.Items[NestingCacheKey] as INestedLogging;

            if (nestedLogging == null)
            {
                nestedLogging = new NestedLogging();

                context.Items[NestingCacheKey] = nestedLogging;

            }
            return nestedLogging;
        }
    }
}