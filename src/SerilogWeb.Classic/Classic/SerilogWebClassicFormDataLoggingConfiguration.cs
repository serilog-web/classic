using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Serilog.Events;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// Entry point for fine-grained configuration of FormData logging
    /// </summary>
    public sealed class SerilogWebClassicFormDataLoggingConfiguration
    {
        private readonly SerilogWebClassicConfiguration _moduleConfiguration;

        internal SerilogWebClassicFormDataLoggingConfiguration(SerilogWebClassicConfiguration moduleConfiguration)
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
        public SerilogWebClassicFormDataLoggingConfiguration AtLevel(LogEventLevel level)
        {
            _moduleConfiguration.FormDataLoggingLevel = level;
            return this;
        }

        /// <summary>
        /// Specify that FormData should be attached to logged events only in case of error (Status > 500)
        /// </summary>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicFormDataLoggingConfiguration OnlyOnError()
        {
            _moduleConfiguration.LogPostedFormData = LogPostedFormDataOption.OnlyOnError;
            _moduleConfiguration.ShouldLogPostedFormData = SerilogWebClassicConfiguration.AlwaysFalse; // it is ignored, might as well reset it
            return this;
        }

        /// <summary>
        /// Specify that FormData should be attached to logged events only when the provided condistion is true
        /// </summary>
        /// <param name="matchingFunction">The predicate that defines when FormData should be attached</param>
        /// <returns>A configuration object to allow chaining</returns>
        public SerilogWebClassicFormDataLoggingConfiguration OnMatch(Func<HttpContextBase, bool> matchingFunction)
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
        public SerilogWebClassicFormDataLoggingConfiguration DisablePasswordFiltering()
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
        public SerilogWebClassicFormDataLoggingConfiguration FilterKeywords(IEnumerable<string> keywordBlackList)
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
        public SerilogWebClassicFormDataLoggingConfiguration FilterKeywords()
        {
            return FilterKeywords(SerilogWebClassicConfiguration.DefaultFilteredOutFormDataKeywords);
        }
    }
}