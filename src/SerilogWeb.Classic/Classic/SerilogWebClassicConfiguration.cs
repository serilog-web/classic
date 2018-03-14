using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using Serilog;
using Serilog.Events;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// The configuration entry point for SerilogWeb.Classic's logging module 
    /// </summary>
    public sealed class SerilogWebClassicConfiguration
    {
        internal static readonly Func<HttpContextBase, bool> AlwaysFalse = context => false;
        internal static readonly string[] DefaultFilteredOutFormDataKeywords = { "password" };

        internal SerilogWebClassicConfiguration()
        {
            Reset();
        }

        internal void Reset()
        {
            CustomLogger = null;
            IsEnabled = true;
            RequestLoggingLevel = LogEventLevel.Information;
            RequestFilter = AlwaysFalse;
            ResetFormDataLogging();
        }

        private void ResetFormDataLogging()
        {
            FormDataLoggingLevel = LogEventLevel.Debug;
            LogPostedFormData = LogPostedFormDataOption.Never;
            ShouldLogPostedFormData = AlwaysFalse;
            FilterPasswordsInFormData = true;
            FilteredKeywordsInFormData = DefaultFilteredOutFormDataKeywords;
        }

        /// <summary>
        /// Disable the logging module completely. Not log events will be written for incoming Http requests
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration Disable()
        {
            IsEnabled = false;
            return this;
        }

        /// <summary>
        /// Enable the logging completely so that log events are written for incoming requests.
        /// Is it enabled by default.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration Enable()
        {
            IsEnabled = true;
            return this;
        }


        /// <summary>
        /// Configure at which level HTTP requests are logged.
        /// Default is Information
        /// </summary>
        /// <param name="level">The level to override the default value</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration LogAtLevel(LogEventLevel level)
        {
            RequestLoggingLevel = level;
            return this;
        }

        /// <summary>
        /// Use a user-specified Logger to write events for HTTP requests.
        /// </summary>
        /// <param name="customLogger">A custom logger to which events will be written</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration UseLogger(ILogger customLogger)
        {
            CustomLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
            return this;
        }

        /// <summary>
        /// Use Log.Logger to write events for HTTP requests.
        /// This is the default.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration UseDefaultLogger()
        {
            CustomLogger = null;
            return this;
        }

        /// <summary>
        /// Specify criteria for HTTP requests to be excluded from logging
        /// </summary>
        /// <param name="filter">A predicate that specify which requests will be filtered out</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration IgnoreRequestsMatching(Func<HttpContextBase, bool> filter)
        {
            RequestFilter = filter ?? throw new ArgumentNullException(nameof(filter));
            return this;
        }

        /// <summary>
        /// Enable logging of the posted Form Data of the HTTP request.
        /// When present, FormData will be attached to all logged events when the log level Debug is enabled.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration EnableFormDataLogging()
        {
            return EnableFormDataLogging(cfg => { });
        }

        /// <summary>
        /// Enables customized configuration of the Form Data logging behavior
        /// </summary>
        /// <param name="configure">Configuration method invocations</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration EnableFormDataLogging(Action<SerilogWebClassicFormDataLoggingConfiguration> configure)
        {
            ResetFormDataLogging();
            LogPostedFormData = LogPostedFormDataOption.Always;
            var formDataLogging = new SerilogWebClassicFormDataLoggingConfiguration(this);
            configure(formDataLogging);
            return this;
        }

        /// <summary>
        /// Disable logging of the posted Form Data of the HTTP requests.
        /// No FormData will be attached to logged events.
        /// This is the default behavior.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfiguration DisableFormDataLogging()
        {
            ResetFormDataLogging();
            return this;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool IsEnabled { get; set; } = true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal LogEventLevel RequestLoggingLevel { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal ILogger CustomLogger { get; private set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal Func<HttpContextBase, bool> RequestFilter { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal LogEventLevel FormDataLoggingLevel { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal LogPostedFormDataOption LogPostedFormData { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal Func<HttpContextBase, bool> ShouldLogPostedFormData { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool FilterPasswordsInFormData { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal IEnumerable<String> FilteredKeywordsInFormData { get; set; }

        internal ILogger Logger
        {
            get => (CustomLogger ?? Log.Logger).ForContext<ApplicationLifecycleModule>();
            set => CustomLogger = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}