# SerilogWeb.Classic

[![Build status](https://ci.appveyor.com/api/projects/status/sa2hgifjj5oi0yp7/branch/master?svg=true)](https://ci.appveyor.com/project/serilog-web/classic/branch/master)

Web request logging and enrichment for classic ASP.NET applications (System.Web).

**Package** - [[SerilogWeb.Classic|http://nuget.org/packages/serilogweb.classic]]
| **Platforms** - .NET 4.5

_This package replaces the Serilog.Extras.Web package previously included in the [Serilog project](https://github.com/serilog/serilog)._

When you work with an ASP.NET web application, this package adds additional enrichers and an `HttpModule` to enhance the logging output. The following enrichers are available:

*  **HttpRequestId** A GUID used to identify requests.
*  **HttpRequestNumber** an incrementing number per request.
*  **HttpRequestTraceId** GUID matching the RequestTraceIdentifier assigned by IIS and used throughout ASP.NET/ETW. IIS ETW tracing must be enabled for this to work.
*  **HttpSessionId** The current ASP.NET session id.
*  **UserName** The current username or, when anonymous, a defined value. By default this is set to _(anonymous)_.

```csharp
var log = new LoggerConfiguration()
    .WriteTo.ColoredConsole()
    .Enrich.With<HttpRequestIdEnricher>()
    .Enrich.With<UserNameEnricher>()
    .CreateLogger();
```

To override the username enricher behaviour:

```csharp
var log = new LoggerConfiguration()
    .WriteTo.ColoredConsole()
    .Enrich.With(new UserNameEnricher("not known yet", System.Environment.UserName))
    .CreateLogger();
```

The **ApplicationLifecycleModule** will automatically be enabled and will write information events about the current method and url that is being accessed. Optionally you also store any form data that is posted to the server.
When an unhandled exception occurs, the module will capture it and log it as an error event.

To enable the posting of form data:

```
ApplicationLifecycleModule.DebugLogPostedFormData = true;
```

If you want to disable the logging completely, use the following statement:

```
ApplicationLifecycleModule.IsEnabled = false;
```
