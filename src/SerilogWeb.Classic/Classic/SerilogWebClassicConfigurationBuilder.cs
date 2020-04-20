using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Serilog;
using Serilog.Events;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// The Builder class to create a new SerilogWebClassicConfiguration
    /// </summary>
    public class SerilogWebClassicConfigurationBuilder
    {
        private static readonly Func<HttpContextBase, bool> AlwaysFalse = context => false;
        private static readonly string[] DefaultFilteredOutFormDataKeywords = { "password" };

        private bool IsEnabled { get; set; } = true;

        private LogEventLevel RequestLoggingLevel { get; set; }
        private ILogger CustomLogger { get; set; }
        private Func<HttpContextBase, bool> RequestFilter { get; set; }

        private LogEventLevel FormDataLoggingLevel { get; set; }
        private LogPostedFormDataOption LogPostedFormData { get; set; }
        private Func<HttpContextBase, bool> ShouldLogPostedFormData { get; set; }

        private bool FilterPasswordsInFormData { get; set; }
        private IEnumerable<String> FilteredKeywordsInFormData { get; set; }

        internal SerilogWebClassicConfigurationBuilder()
        {
            Reset();
        }

        internal SerilogWebClassicConfigurationBuilder(SerilogWebClassicConfiguration configToCopy)
        {
            if (configToCopy == null) throw new ArgumentNullException(nameof(configToCopy));
            CustomLogger = configToCopy.CustomLogger;
            IsEnabled = configToCopy.IsEnabled;
            RequestLoggingLevel = configToCopy.RequestLoggingLevel;
            RequestFilter = configToCopy.RequestFilter;
            FormDataLoggingLevel = configToCopy.FormDataLoggingLevel;
            LogPostedFormData = configToCopy.LogPostedFormData;
            ShouldLogPostedFormData = configToCopy.ShouldLogPostedFormData;
            FilterPasswordsInFormData = configToCopy.FilterPasswordsInFormData;
            FilteredKeywordsInFormData = configToCopy.FilteredKeywordsInFormData;
        }

        private void Reset()
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

        internal SerilogWebClassicConfiguration Build()
        {
            return new SerilogWebClassicConfiguration(
                isEnabled: IsEnabled,
                requestLoggingLevel: RequestLoggingLevel,
                requestFilter: RequestFilter,
                formDataLoggingLevel: FormDataLoggingLevel,
                customLogger: CustomLogger,
                logPostedFormData: LogPostedFormData,
                shouldLogPostedFormData: ShouldLogPostedFormData,
                filterPasswordsInFormData: FilterPasswordsInFormData,
                filteredKeywordsInFormData: FilteredKeywordsInFormData);
        }

        /// <summary>
        /// Disable the logging module completely. Not log events will be written for incoming Http requests
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfigurationBuilder Disable()
        {
            IsEnabled = false;
            return this;
        }

        /// <summary>
        /// Enable the logging completely so that log events are written for incoming requests.
        /// Is it enabled by default.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfigurationBuilder Enable()
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
        public SerilogWebClassicConfigurationBuilder LogAtLevel(LogEventLevel level)
        {
            RequestLoggingLevel = level;
            return this;
        }

        /// <summary>
        /// Use a user-specified Logger to write events for HTTP requests.
        /// </summary>
        /// <param name="customLogger">A custom logger to which events will be written</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfigurationBuilder UseLogger(ILogger customLogger)
        {
            CustomLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
            return this;
        }

        /// <summary>
        /// Use Log.Logger to write events for HTTP requests.
        /// This is the default.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfigurationBuilder UseDefaultLogger()
        {
            CustomLogger = null;
            return this;
        }

        /// <summary>
        /// Specify criteria for HTTP requests to be excluded from logging
        /// </summary>
        /// <param name="filter">A predicate that specify which requests will be filtered out</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfigurationBuilder IgnoreRequestsMatching(Func<HttpContextBase, bool> filter)
        {
            RequestFilter = filter ?? throw new ArgumentNullException(nameof(filter));
            return this;
        }

        /// <summary>
        /// Enable logging of the posted Form Data of the HTTP request.
        /// When present, FormData will be attached to all logged events when the log level Debug is enabled.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfigurationBuilder EnableFormDataLogging()
        {
            return EnableFormDataLogging(cfg => { });
        }


        /// <summary>
        /// Enables customized configuration of the Form Data logging behavior
        /// </summary>
        /// <param name="configure">Configuration method invocations</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfigurationBuilder EnableFormDataLogging(Action<FormDataLoggingConfigurationBuilder> configure)
        {
            ResetFormDataLogging();
            LogPostedFormData = LogPostedFormDataOption.Always;
            var formDataLogging = new FormDataLoggingConfigurationBuilder(this);
            configure(formDataLogging);
            return this;
        }

        /// <summary>
        /// Disable logging of the posted Form Data of the HTTP requests.
        /// No FormData will be attached to logged events.
        /// This is the default behavior.
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicConfigurationBuilder DisableFormDataLogging()
        {
            ResetFormDataLogging();
            return this;
        }


        /// <summary>
        /// Entry point for fine-grained configuration of FormData logging
        /// </summary>
        public sealed class FormDataLoggingConfigurationBuilder
        {
            private readonly SerilogWebClassicConfigurationBuilder _mainConfigBuilder;

            internal FormDataLoggingConfigurationBuilder(SerilogWebClassicConfigurationBuilder mainConfigBuilder)
            {
                _mainConfigBuilder = mainConfigBuilder;
            }

            /// <summary>
            /// Specify from which level FormData should be attached to log events when enabled.
            /// No Form Data will be attached when that level is not enabled for the logger.
            /// Default is Debug.
            /// </summary>
            /// <param name="level">The log level at which FormData is written</param>
            /// <returns>A configuration object to allow chaining</returns>
            public FormDataLoggingConfigurationBuilder AtLevel(LogEventLevel level)
            {
                _mainConfigBuilder.FormDataLoggingLevel = level;
                return this;
            }

            /// <summary>
            /// Specify that FormData should be attached to logged events only in case of error (status code >= 500)
            /// </summary>
            /// <returns>A configuration object to allow chaining</returns>
            public FormDataLoggingConfigurationBuilder OnlyOnError()
            {
                _mainConfigBuilder.LogPostedFormData = LogPostedFormDataOption.OnlyOnError;
                _mainConfigBuilder.ShouldLogPostedFormData = AlwaysFalse; // it is ignored, might as well reset it
                return this;
            }

            /// <summary>
            /// Specify that FormData should be attached to logged events only when the provided condition is true
            /// </summary>
            /// <param name="matchingFunction">The predicate that defines when FormData should be attached</param>
            /// <returns>A configuration object to allow chaining</returns>
            public FormDataLoggingConfigurationBuilder OnMatch(Func<HttpContextBase, bool> matchingFunction)
            {
                _mainConfigBuilder.LogPostedFormData = LogPostedFormDataOption.OnMatch;
                _mainConfigBuilder.ShouldLogPostedFormData = matchingFunction;
                return this;
            }

            /// <summary>
            /// Specify that possibly sensitive information should be preserved in the logged FormData
            /// Password-filtering is On by default and "offuscates" form data where the key contains "password"
            /// </summary>
            /// <returns>A configuration object to allow chaining</returns>
            public FormDataLoggingConfigurationBuilder DisablePasswordFiltering()
            {
                _mainConfigBuilder.FilterPasswordsInFormData = false;
                return this;
            }

            /// <summary>
            /// Specify which keywords should be offuscated in the logged FormData
            /// The Form Data values will be offuscated when the key contains one of the black-listed words.
            /// </summary>
            /// <param name="keywordBlackList">The black-listed keywords</param>
            /// <returns>A configuration object to allow chaining</returns>
            public FormDataLoggingConfigurationBuilder FilterKeywords(IEnumerable<string> keywordBlackList)
            {
                var keywords = new List<string>(keywordBlackList);
                if (!keywords.Any())
                {
                    return DisablePasswordFiltering();
                }

                _mainConfigBuilder.FilterPasswordsInFormData = true;
                _mainConfigBuilder.FilteredKeywordsInFormData = keywords;
                return this;
            }

            /// <summary>
            /// Specify that values for password fileds should be offuscated in the logged FormData
            /// The Form Data values will be offuscated when the key contains "password".
            /// </summary>
            /// <returns>A configuration object to allow chaining</returns>
            public FormDataLoggingConfigurationBuilder FilterKeywords()
            {
                return FilterKeywords(DefaultFilteredOutFormDataKeywords);
            }
        }

    }
}