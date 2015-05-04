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
            Guid requestId;
            var requestIdItem = HttpContext.Current.Items[RequestIdItemName];
            if (requestIdItem == null)
            {
                requestIdItem = Guid.NewGuid();
                HttpContext.Current.Items[RequestIdItemName] = requestIdItem;
            }
            return ((Guid)requestIdItem).ToString();
        }
    }
}
