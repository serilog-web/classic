using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Serilog;
using Serilog.Events;

namespace SerilogWeb.Classic
{
    internal class ClassicRequestEventHandler
    {
        const string StopWatchKey = "SerilogWeb.Classic.ApplicationLifecycleModule.StopWatch";

        private static readonly Func<HttpContextBase, bool> AlwaysTrue = context => true;
        private static readonly Func<HttpContextBase, bool> AlwaysFalse = context => false;
        private static readonly Func<HttpContextBase, bool> DefaultErrorStrategy = context => context.Response.StatusCode >= 500;

        // ReSharper disable once InconsistentNaming
        private readonly IHttpApplication application;
        private readonly SerilogWebModuleConfiguration _configuration;

        public ClassicRequestEventHandler(IHttpApplication application, SerilogWebModuleConfiguration configuration)
        {
            this.application = application ?? throw new ArgumentNullException(nameof(application));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        internal void OnBeginRequest()
        {
            if (_configuration.IsEnabled && application.Context != null)
            {
                application.Context.Items[StopWatchKey] = Stopwatch.StartNew();
            }
        }

        internal void OnLogRequest()
        {
            if (!_configuration.IsEnabled || application.Context == null)
                return;

            var stopwatch = application.Context.Items[StopWatchKey] as Stopwatch;
            if (stopwatch == null)
                return;

            stopwatch.Stop();

            var request = application.Request;
            if (request == null || _configuration.RequestFilter(application.Context))
                return;

            var error = application.Server.GetLastError();
            var level = error != null || application.Response.StatusCode >= 500 ? LogEventLevel.Error : _configuration.RequestLoggingLevel;

            if (level == LogEventLevel.Error && error == null && application.Context.AllErrors != null)
            {
                error = application.Context.AllErrors.LastOrDefault();
            }

            var logger = (_configuration.CustomLogger ?? Log.Logger).ForContext<ApplicationLifecycleModule>();
            if (logger.IsEnabled(_configuration.FormDataLoggingLevel) && FormLoggingStrategy(application.Context))
            {
                var form = request.Unvalidated.Form;
                if (form.HasKeys())
                {
                    var formData = form.AllKeys.SelectMany(k => (form.GetValues(k) ?? new string[0]).Select(v => new { Name = k, Value = FilterPasswords(k, v) }));
                    logger = logger.ForContext("FormData", formData, true);
                }
            }

            logger.Write(
                level,
                error,
                "HTTP {Method} {RawUrl} responded {StatusCode} in {ElapsedMilliseconds}ms",
                request.HttpMethod,
                request.RawUrl,
                application.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Filters configured keywords from being logged
        /// </summary>
        /// <param name="key">Key of the pair</param>
        /// <param name="value">Value of the pair</param>
        private string FilterPasswords(string key, string value)
        {
            if (_configuration.FilterPasswordsInFormData && _configuration.FilteredKeywordsInFormData.Any(keyword => key.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) != -1))
            {
                return "********";
            }

            return value;
        }

        private Func<HttpContextBase, bool> FormLoggingStrategy
        {
            get
            {
                switch (_configuration.LogPostedFormData)
                {
                    case LogPostedFormDataOption.Never:
                        return AlwaysFalse;
                    case LogPostedFormDataOption.Always:
                        return AlwaysTrue;
                    case LogPostedFormDataOption.OnlyOnError:
                        return DefaultErrorStrategy;
                    case LogPostedFormDataOption.OnMatch:
                        return _configuration.ShouldLogPostedFormData;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
