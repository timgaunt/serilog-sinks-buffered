using System;
using System.Web;

namespace Serilog.Sinks.Buffered.Web
{
    public class PerRequestLoggingModule : IHttpModule
    {
        private HttpApplication _context;
        private IPerRequestLogger _logger;

        public static Func<IPerRequestLogger> ResolvePerRequestLogger
        {
            get;
            set;
        }

        public void Init(HttpApplication context)
        {
            _context = context;
            _context.EndRequest += context_EndRequest;
            if (ResolvePerRequestLogger == null)
            {
                throw new InvalidOperationException("PerRequestLoggingModule.ResolvePerRequestLogger has not been configured");
            }
            _logger = ResolvePerRequestLogger();
            if (_logger == null)
            {
                throw new InvalidOperationException("PerRequestLoggingModule.ResolvePerRequestLogger returned null, expected an IPerRequestLogger");
            }
        }

        public void Dispose()
        {
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            var requestId = RequestId.GetCurrentId();
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                _logger.Complete(requestId);
            }
        }
    }
}
