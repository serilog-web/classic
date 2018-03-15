using System;
using Serilog;
using Serilog.Events;
using SerilogWeb.Classic;

namespace SerilogWeb.Test
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            SerilogWebClassic.Configuration
                    .IgnoreRequestsMatching(ctx => ctx.Request.Url.PathAndQuery.StartsWith("/__browserLink"))
                    .EnableFormDataLogging(formData => formData
                                                .AtLevel(LogEventLevel.Debug)
                                                .OnMatch(ctx => ctx.Response.StatusCode >= 400));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception} {Properties:j}" )
                .CreateLogger();
        }
    }
}