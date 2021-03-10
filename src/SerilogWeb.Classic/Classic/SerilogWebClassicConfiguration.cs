using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Serilog;
using Serilog.Events;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// The configuration entry point for SerilogWeb.Classic's logging module 
    /// </summary>
    internal sealed class SerilogWebClassicConfiguration
    {
        private static readonly Func<HttpContextBase, bool> AlwaysTrue = context => true;
        private static readonly Func<HttpContextBase, bool> AlwaysFalse = context => false;
        private static readonly Func<HttpContextBase, bool> DefaultErrorStrategy = context => context.Response.StatusCode >= 500;

        internal SerilogWebClassicConfiguration(
            bool isEnabled,
            LogEventLevel requestLoggingLevel,
            Func<HttpContextBase, TimeSpan, LogEventLevel> requestContextLogLevel,
            Func<HttpContextBase, bool> requestFilter,
            LogEventLevel formDataLoggingLevel,
            ILogger customLogger,
            LogPostedFormDataOption logPostedFormData,
            Func<HttpContextBase, bool> shouldLogPostedFormData,
            bool filterPasswordsInFormData,
            IEnumerable<string> filteredKeywordsInFormData)
        {
            IsEnabled = isEnabled;
            RequestLoggingLevel = requestLoggingLevel;
            RequestContextLogLevel = requestContextLogLevel;
            RequestFilter = requestFilter;
            FormDataLoggingLevel = formDataLoggingLevel;
            CustomLogger = customLogger;
            LogPostedFormData = logPostedFormData;
            ShouldLogPostedFormData = shouldLogPostedFormData;
            FilterPasswordsInFormData = filterPasswordsInFormData;
            FilteredKeywordsInFormData = filteredKeywordsInFormData;

            switch (logPostedFormData)
            {
                case LogPostedFormDataOption.Never:
                    FormLoggingStrategy = AlwaysFalse;
                    break;
                case LogPostedFormDataOption.Always:
                    FormLoggingStrategy = AlwaysTrue;
                    break;
                case LogPostedFormDataOption.OnlyOnError:
                    FormLoggingStrategy = DefaultErrorStrategy;
                    break;
                case LogPostedFormDataOption.OnMatch:
                    FormLoggingStrategy = shouldLogPostedFormData;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logPostedFormData), logPostedFormData, $"Should be a valid {nameof(LogPostedFormDataOption)}");
            }
        }

        internal static readonly SerilogWebClassicConfiguration Default = new SerilogWebClassicConfigurationBuilder().Build();

        internal SerilogWebClassicConfiguration Edit(Func<SerilogWebClassicConfigurationBuilder, SerilogWebClassicConfigurationBuilder> configure)
        {
            return (configure(new SerilogWebClassicConfigurationBuilder(this))).Build();
        }

        internal bool IsEnabled { get; }

        internal LogEventLevel RequestLoggingLevel { get; }
        internal Func<HttpContextBase, TimeSpan, LogEventLevel> RequestContextLogLevel { get; }
        internal ILogger CustomLogger { get; }
        internal Func<HttpContextBase, bool> RequestFilter { get; }

        internal LogEventLevel FormDataLoggingLevel { get; }
        internal LogPostedFormDataOption LogPostedFormData { get; }
        internal Func<HttpContextBase, bool> ShouldLogPostedFormData { get; }

        internal bool FilterPasswordsInFormData { get; }
        internal IEnumerable<string> FilteredKeywordsInFormData { get; }

        internal ILogger Logger => (CustomLogger ?? Log.Logger).ForContext<ApplicationLifecycleModule>();


        internal Func<HttpContextBase, bool> FormLoggingStrategy { get; }


        /// <summary>
        /// Filters configured keywords from being logged
        /// </summary>
        /// <param name="key">Key of the pair</param>
        /// <param name="value">Value of the pair</param>
        internal string FilterPasswords(string key, string value)
        {
            if (!FilterPasswordsInFormData)
            {
                return value;
            }

            if (key == null)
            {
                return value;
            }

            if (FilteredKeywordsInFormData.Any(keyword => key.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) != -1))
            {
                return "********";
            }

            return value;
        }
    }
}