using System;
using System.Web;
using SerilogWeb.Classic.Enrichers;

namespace Serilog.Sinks.Buffered.Web
{
    public static class RequestId
    {
        static readonly string RequestIdItemName = typeof(HttpRequestIdEnricher).Name + "+RequestId";

        public static string GetCurrentId()
        {
            if (HttpContext.Current == null)
            {
                return "NO-CONTEXT";
            }

            var requestIdItem = HttpContext.Current.Items[RequestIdItemName];
            if (requestIdItem != null)
            {
                return requestIdItem.ToString();
            }

            requestIdItem = Guid.NewGuid();
            HttpContext.Current.Items[RequestIdItemName] = requestIdItem;
            return requestIdItem.ToString();
        }
    }
}
