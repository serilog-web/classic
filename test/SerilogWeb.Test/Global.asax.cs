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
            ApplicationLifecycleModule.FormDataLoggingLevel = LogEventLevel.Debug;
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.OnMatch;
            ApplicationLifecycleModule.ShouldLogPostedFormData = context => context.Response.StatusCode >= 400;

            ApplicationLifecycleModule.RequestFilter = context => context.Request.Url.PathAndQuery.StartsWith("/__browserLink");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace()
                .CreateLogger();
        }
    }
}