using System;
using System.Diagnostics;
using System.Linq;
using Serilog;
using Serilog.Events;
using SerilogWeb.Classic.Extensions;

namespace SerilogWeb.Classic
{
    internal class WebRequestLoggingHandler
    {
        private const string StopWatchKey = "SerilogWeb.Classic.ApplicationLifecycleModule.StopWatch";
        private const string HttpRequestEventMessageTemplate = "HTTP {Method} {RawUrl} responded {StatusCode} in {ElapsedMilliseconds}ms";

        private readonly IHttpApplication _application;

        public WebRequestLoggingHandler(IHttpApplication application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
        }

        internal void OnBeginRequest(SerilogWebClassicConfiguration configuration)
        {
            if (configuration.IsEnabled && _application.Context != null)
            {
                _application.Context.Items[StopWatchKey] = Stopwatch.StartNew();
            }
        }

        internal void OnLogRequest(SerilogWebClassicConfiguration configuration)
        {
            if (!configuration.IsEnabled || _application.Context == null)
                return;

            var stopwatch = _application.Context.Items[StopWatchKey] as Stopwatch;
            if (stopwatch == null)
                return;

            stopwatch.Stop();

            var request = _application.Request;
            if (request == null || configuration.RequestFilter(_application.Context))
                return;

            var error = _application.Context.GetLastSerilogWebError() ?? _application.Server.GetLastError();

            var level = error != null || _application.Response.StatusCode >= 500 ? LogEventLevel.Error : configuration.RequestLoggingLevel;

            if (level == LogEventLevel.Error && error == null && _application.Context.AllErrors != null)
            {
                error = _application.Context.AllErrors.LastOrDefault();
            }

            var logger = (configuration.CustomLogger ?? Log.Logger).ForContext<ApplicationLifecycleModule>();
            if (logger.IsEnabled(configuration.FormDataLoggingLevel) && configuration.FormLoggingStrategy(_application.Context))
            {
                var form = request.Unvalidated.Form;
                if (form.HasKeys())
                {
                    var formData = form.AllKeys.SelectMany(k => (form.GetValues(k) ?? new string[0]).Select(v => new { Name = k, Value = configuration.FilterPasswords(k, v) }));
                    logger = logger.ForContext("FormData", formData, true);
                }
            }

            logger.Write(
                level,
                error,
                HttpRequestEventMessageTemplate,
                request.HttpMethod,
                request.RawUrl,
                _application.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
