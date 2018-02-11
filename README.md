# SerilogWeb.Classic [![Build status](https://ci.appveyor.com/api/projects/status/lpf4kdc7su3l67be?svg=true)](https://ci.appveyor.com/project/serilog-web/classic) [![NuGet](https://img.shields.io/nuget/v/SerilogWeb.Classic.svg)](https://www.nuget.org/packages/serilogweb.classic)

Web request logging and enrichment for classic ASP.NET applications (System.Web).

**Package** - [SerilogWeb.Classic](http://nuget.org/packages/serilogweb.classic)
| **Platforms** - .NET 4.5

_This package replaces the Serilog.Extras.Web package previously included in the [Serilog project](https://github.com/serilog/serilog)._

_This package is designed for full framework ASP.NET applications. For ASP.NET Core, have a look at [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore)_

When you work with an ASP.NET web application, this package adds 
- additional enrichers 
- an `HttpModule` to enhance the logging output. 

When working with ASP.NET MVC (not Core) or ASP.NET Web API, you may also want to have a look at [SerilogWeb.Classic.Mvc](https://github.com/serilog-web/classic-mvc) and [SerilogWeb.Classic.WebAPI](https://github.com/serilog-web/classic-webapi)

## Enrichers
The following enrichers are available in the `SerilogWeb.Classic.Enrichers` namespace:

*  **ClaimValueEnricher** : adds a property contaning the value of a given claim from the current `ClaimsIdentity` User
*  **HttpRequestClientHostIPEnricher** : adds a property `HttpRequestClientHostIP` containing  `Request.UserHostAddress` (optionally checking for proxy header)
*  **HttpRequestClientHostNameEnricher** : adds a property `HttpRequestClientHostName` containing  `Request.UserHostName`
*  **HttpRequestIdEnricher** : adds a property `HttpRequestId` with a GUID used to identify requests.
*  **HttpRequestNumberEnricher** : adds a property `HttpRequestNumber` with an incrementing number per request.
*  **HttpRequestRawUrlEnricher** : adds a property `HttpRequestRawUrl` with the Raw Url of the Request.
*  **HttpRequestTraceIdEnricher** : adds a property `HttpRequestTraceId` with a GUID matching the RequestTraceIdentifier assigned by IIS and used throughout ASP.NET/ETW. (IIS ETW tracing must be enabled for this to work)
*  **HttpRequestTypeEnricher** : adds a property `HttpRequestType` with the Request Type (`GET` or `POST`).
*  **HttpRequestUrlEnricher** : adds a property `HttpRequestUrl` with the Url of the Request.
*  **HttpRequestUrlReferrerEnricher** : adds a property `HttpRequestUrlReferrer` with the UrlReferrer of the Request.
*  **HttpRequestUserAgentEnricher** : adds a property `HttpRequestUserAgent` with the User Agent of the Request.
*  **HttpSessionIdEnricher** : adds a property `HttpSessionId` with the current ASP.NET session id.
*  **UserNameEnricher** : adds a property `UserName` with the current username or, when anonymous, a defined value. By default this is set to _(anonymous)_.


```csharp
var log = new LoggerConfiguration()
    .WriteTo.Console()
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

## HttpModule
The **ApplicationLifecycleModule** will automatically be enabled and will write information events about the current method and url that is being accessed. Optionally you also store any form data that is posted to the server. When an unhandled exception occurs, the module will capture it and log it as an error event.

### Configuration

No configuration is necessary, but most of the built-in behavior can be changed using properties on the `ApplicationLifecycleModule` class.

By default, all requests will be logged at the _Information_ level. To change this (i.e. to generate less events under normal conditions) use the `RequestLoggingLevel` property:

```csharp
ApplicationLifecycleModule.RequestLoggingLevel = LogEventLevel.Debug;
```

To enable the posting of form data:

```csharp
ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
// or
ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.OnlyOnError;
```

Any fields containing the phrase 'password' will be filtered from the logged form data.  This can be disabled with:

```csharp
ApplicationLifecycleModule.FilterPasswordsInFormData = false;
```

If you want to disable the logging completely, use the following statement:

```csharp
ApplicationLifecycleModule.IsEnabled = false;
```
