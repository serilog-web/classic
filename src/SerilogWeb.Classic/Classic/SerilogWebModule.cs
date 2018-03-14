using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Serilog;
using Serilog.Events;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// The configuration entry point for SerilogWeb
    /// </summary>
    public static class SerilogWebModule
    {
        /// <summary>
        /// The configuration entry point for SerilogWeb.Classic's logging module
        /// </summary>
        public static SerilogWebModuleConfiguration Configuration { get; } = new SerilogWebModuleConfiguration();
    }

    /// <summary>
    /// The configuration entry point for SerilogWeb.Classic's logging module 
    /// </summary>
    public class SerilogWebModuleConfiguration
    {
        private static readonly Func<HttpContextBase, bool> AlwaysFalse = context => false;
        private static readonly string[] DefaultFilteredOutFormDataKeywords = { "password" };

        internal SerilogWebModuleConfiguration()
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
        public SerilogWebModuleConfiguration Disable()
        {
            IsEnabled = false;
            return this;
        }

        /// <summary>
        /// Enable the logging completely so that log events are written for incoming requests.
        /// Is it enabled by default.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebModuleConfiguration Enable()
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
        public SerilogWebModuleConfiguration LogAtLevel(LogEventLevel level)
        {
            RequestLoggingLevel = level;
            return this;
        }

        /// <summary>
        /// Use a user-specified Logger to write events for HTTP requests.
        /// </summary>
        /// <param name="customLogger">A custom logger to which events will be written</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebModuleConfiguration UseLogger(ILogger customLogger)
        {
            CustomLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
            return this;
        }

        /// <summary>
        /// Use Log.Logger to write events for HTTP requests.
        /// This is the default.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebModuleConfiguration UseDefaultLogger()
        {
            CustomLogger = null;
            return this;
        }

        /// <summary>
        /// Specify criteria for HTTP requests to be excluded from logging
        /// </summary>
        /// <param name="filter">A predicate that specify which requests will be filtered out</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebModuleConfiguration IgnoreRequestsMatching(Func<HttpContextBase, bool> filter)
        {
            RequestFilter = filter;
            return this;
        }

        /// <summary>
        /// Enable logging of the posted Form Data of the HTTP request.
        /// When present, FormData will be attached to all logged events when the log level Debug is enabled.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebModuleConfiguration EnableFormDataLogging()
        {
            return EnableFormDataLogging(cfg => { });
        }

        /// <summary>
        /// Enables customized configuration of the Form Data logging behavior
        /// </summary>
        /// <param name="configure">Configuration method invocations</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebModuleConfiguration EnableFormDataLogging(Action<SerilogWebModuleFormDataLoggingConfiguration> configure)
        {
            ResetFormDataLogging();
            LogPostedFormData = LogPostedFormDataOption.Always;
            var formDataLogging = new SerilogWebModuleFormDataLoggingConfiguration(this);
            configure(formDataLogging);
            return this;
        }

        /// <summary>
        /// Disable logging of the posted Form Data of the HTTP requests.
        /// No FormData will be attached to logged events.
        /// This is the default behavior.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebModuleConfiguration DisableFormDataLogging()
        {
            ResetFormDataLogging();
            return this;
        }

        internal bool IsEnabled { get; set; } = true;

        internal LogEventLevel RequestLoggingLevel { get; set; }
        internal ILogger CustomLogger { get; set; }
        internal Func<HttpContextBase, bool> RequestFilter { get; set; }

        internal LogEventLevel FormDataLoggingLevel { get; set; }
        internal LogPostedFormDataOption LogPostedFormData { get; set; }
        internal Func<HttpContextBase, bool> ShouldLogPostedFormData { get; set; }

        internal bool FilterPasswordsInFormData { get; set; }
        internal IEnumerable<String> FilteredKeywordsInFormData { get; set; }

        /// <summary>
        /// Entry point for fine-grained configuration of FormData logging
        /// </summary>
        public class SerilogWebModuleFormDataLoggingConfiguration
        {
            private readonly SerilogWebModuleConfiguration _moduleConfiguration;

            internal SerilogWebModuleFormDataLoggingConfiguration(SerilogWebModuleConfiguration moduleConfiguration)
            {
                _moduleConfiguration = moduleConfiguration;
            }

            /// <summary>
            /// Specify from which level FormData should be attached to log events when enabled.
            /// No Form Data will be attached when that level is not enabled for the logger.
            /// Default is Debug.
            /// </summary>
            /// <param name="level">The log level at which FormData is written</param>
            /// <returns>A configuration object to allow chaining</returns>
            public SerilogWebModuleFormDataLoggingConfiguration AtLevel(LogEventLevel level)
            {
                _moduleConfiguration.FormDataLoggingLevel = level;
                return this;
            }

            /// <summary>
            /// Specify that FormData should be attached to logged events only in case of error (Status > 500)
            /// </summary>
            /// <returns>A configuration object to allow chaining</returns>
            public SerilogWebModuleFormDataLoggingConfiguration OnlyOnError()
            {
                _moduleConfiguration.LogPostedFormData = LogPostedFormDataOption.OnlyOnError;
                _moduleConfiguration.ShouldLogPostedFormData = AlwaysFalse; // it is ignored, might as well reset it
                return this;
            }

            /// <summary>
            /// Specify that FormData should be attached to logged events only when the provided condistion is true
            /// </summary>
            /// <param name="matchingFunction">The predicate that defines when FormData should be attached</param>
            /// <returns>A configuration object to allow chaining</returns>
            public SerilogWebModuleFormDataLoggingConfiguration OnMatch(Func<HttpContextBase, bool> matchingFunction)
            {
                _moduleConfiguration.LogPostedFormData = LogPostedFormDataOption.OnMatch;
                _moduleConfiguration.ShouldLogPostedFormData = matchingFunction;
                return this;
            }

            /// <summary>
            /// Specify that possibly sensitive information should be preserved in the logged FormData
            /// Password-filtering is On by default and "offuscates" form data where the key contains "password"
            /// </summary>
            /// <returns>A configuration object to allow chaining</returns>
            public SerilogWebModuleFormDataLoggingConfiguration DisablePasswordFiltering()
            {
                _moduleConfiguration.FilterPasswordsInFormData = false;
                return this;
            }

            /// <summary>
            /// Specify which keywords should be offuscated in the logged FormData
            /// The Form Data values will be offuscated when the key contains one of the black-listed words.
            /// </summary>
            /// <param name="keywordBlackList">The black-listed keywords</param>
            /// <returns>A configuration object to allow chaining</returns>
            public SerilogWebModuleFormDataLoggingConfiguration FilterKeywords(IEnumerable<string> keywordBlackList)
            {
                var keywords = new List<string>(keywordBlackList);
                if (!keywords.Any())
                {
                    return DisablePasswordFiltering();
                }

                _moduleConfiguration.FilterPasswordsInFormData = true;
                _moduleConfiguration.FilteredKeywordsInFormData = keywords;
                return this;
            }

            /// <summary>
            /// Specify that values for password fileds should be offuscated in the logged FormData
            /// The Form Data values will be offuscated when the key contains "password".
            /// </summary>
            /// <returns>A configuration object to allow chaining</returns>
            public SerilogWebModuleFormDataLoggingConfiguration FilterKeywords()
            {
                return FilterKeywords(DefaultFilteredOutFormDataKeywords);
            }
        }
    }
}
