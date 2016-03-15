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

### Autofac
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