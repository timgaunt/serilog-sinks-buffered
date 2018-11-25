# serilog-sinks-buffered
A buffered sink for SeriLog


You can use it as you would any other sink:

```c#
BufferedSink bufferedSink;

var log = Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.FromLogContext()
    .Buffer(
        // The level at which events are always sent (not buffered)
        LogEventLevel.Information,
        // The level at which all events are sent for the request
        LogEventLevel.Error,
        // The id to use to identify a request
        HttpRequestIdEnricher.HttpRequestIdPropertyName,
        // Request timeout in seconds (events are discarded after this time)
        4 * 60,
        // The buffered sink for use elsewhere
        out bufferedSink,
        // The event that is called when the events are triggered
        (lc) => {
            // Wire up your usual Sinks here
            lc.Enrich.FromLogContext()
                .WriteTo.ColoredConsole()
        }
    ).CreateLogger();

Log.Logger = log;
```


## Registering the PerRequestLoggingModule (to support web request buffering)

You'll need to reference the namespaces `Microsoft.Web.Infrastructure.DynamicModuleHelper` and `Serilog.Sinks.Buffered.Web`

### Autofac
```c#
private static void RegisterSerilog(IKernel kernel)
{
    IPerRequestLogger bufferedSink = SeriLogBootstrapper.RegisterSerilog();

    PerRequestLoggingModule.ResolvePerRequestLogger = () => bufferedSink;

    _builder.RegisterInstance(bufferedSink).As<IPerRequestLogger>();
    _builder.RegisterInstance(bufferedSink).As<IFlushPerRequestLogs>();

    DynamicModuleUtility.RegisterModule(typeof(PerRequestLoggingModule));
}
```

### Ninject
```c#
private static void RegisterSerilog(IKernel kernel)
{
    var bufferedSink = SerilogBootstrapper.RegisterSerilog();

    PerRequestLoggingModule.ResolvePerRequestLogger = () => bufferedSink;

    kernel.Bind<IPerRequestLogger>().ToMethod(c => bufferedSink);
    kernel.Bind<IFlushPerRequestLogs>().ToMethod(c => bufferedSink);

    DynamicModuleUtility.RegisterModule(typeof(PerRequestLoggingModule));
}
```

## ASPNETCORE 2.1 example

Add to Program.cs:
```c#
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = SerilogConfig.CreateLogger(); // Creates SerilogConfig.BufferedSink
        ...
```

Add to Startup.cs:
```c#
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseBufferedLog(SerilogConfig.BufferedSink); // Important that this is first in method
        ...
```

Implement middleware to handle request lifecycle:
```c#
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Serilog.Sinks.Buffered;
using System;

namespace aspnetcore.Serilog
{
    public static class BufferedLogMiddlewareExtensions
    {
        public static void UseBufferedLog(this IApplicationBuilder app, IPerRequestLogger bufferedSink)
        {
            if (bufferedSink == null)
            {
                Console.WriteLine("UseBufferedLog given a null bufferedSink and will not be active");
            }
            else
            {
                app.Use(async (context, next) =>
                {
                    try
                    {
                        // Call the next delegate/middleware in the pipeline
                        await next();
                    }
                    finally
                    {
                        try
                        {
                            bufferedSink?.Complete(context.TraceIdentifier);
                        }
                        catch (Exception ex)
                        {
                            // Unlikely that we'll get an exception here but just in case, write it to stdout
                            Console.WriteLine(ex.ToString());
                        }
                    }
                });
            }
        }
    }
}
```

Implement SerilogConfig static class to hold the BufferedSink:
```c#
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Buffered;
using System;
using Serilog.Core;

namespace aspnetcore.Serilog
{
    public static class SerilogConfig
    {
        public static BufferedSink BufferedSink { get; private set; }

        public static ILogger CreateLogger()
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .CreateBufferedLogger(
                    LogEventLevel.Warning,
                    LogEventLevel.Error,
                    lc => lc.WriteTo.Console());

            return logger;
        }

        /// <summary>
        /// Creates a logger which buffers events by request until a trigger level event occurs.
        /// </summary>
        /// <param name="config">The config</param>
        /// <param name="eventLevel">The level at which events are always sent (not buffered)</param>
        /// <param name="allEventLevel">The level to trigger that buffered events are sent for the request</param>
        /// <param name="configureLogger">The actions to configure the logger with output sinks</param>
        /// <returns></returns>
        private static ILogger CreateBufferedLogger(this LoggerConfiguration config, LogEventLevel eventLevel,
            LogEventLevel allEventLevel, Action<LoggerConfiguration> configureLogger)
        {
            const string SerilogRequestIdPropName = "RequestId";
            const int maxRequestAgeInSeconds = 4 * 60; // Log events are discarded after this time

            var lc = new LoggerConfiguration().MinimumLevel.Verbose();
            configureLogger(lc);

            BufferedSink =
                new BufferedSink(
                    eventLevel, allEventLevel, SerilogRequestIdPropName, maxRequestAgeInSeconds,
                    lc.CreateLogger());

            return config
                .Enrich.FromLogContext() // Important for the internal operation of BufferedSink
                .WriteTo.Sink(BufferedSink))
                .CreateLogger();
        }
    }
}
```
