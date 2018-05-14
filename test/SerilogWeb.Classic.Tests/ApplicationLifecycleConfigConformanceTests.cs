using System;
using System.Collections.Generic;
using System.Web;
using Serilog;
using Serilog.Events;
#pragma warning disable 618

namespace SerilogWeb.Classic.Tests
{
    /// <summary>
    /// Tests that prove that using ApplicationLifecycle public static properties has the expected outcome
    /// </summary>
    public class ApplicationLifecycleConfigConformanceTests : ModuleConfigurationContractTests
    {
        protected override void ResetConfiguration()
        {
            ApplicationLifecycleModule.ResetConfiguration();
        }

        protected override void SetRequestLoggingLevel(LogEventLevel level)
        {
            ApplicationLifecycleModule.RequestLoggingLevel = level;
        }

        protected override void SetRequestLoggingFilter(Func<HttpContextBase, bool> exclude)
        {
            ApplicationLifecycleModule.RequestFilter = exclude;
        }

        protected override void EnableLogging()
        {
            ApplicationLifecycleModule.IsEnabled = true;
        }

        protected override void DisableLogging()
        {
            ApplicationLifecycleModule.IsEnabled = false;
        }

        protected override void EnableFormDataLoggingAlways()
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level)
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.FormDataLoggingLevel = level;
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level, bool filterPasswords)
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.FormDataLoggingLevel = level;
            ApplicationLifecycleModule.FilterPasswordsInFormData = filterPasswords;
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level, IEnumerable<string> customBlackList)
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.FormDataLoggingLevel = level;
            ApplicationLifecycleModule.FilteredKeywordsInFormData = customBlackList;
        }

        protected override void DisableFormDataLogging()
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Never;
        }

        protected override void EnableFormDataLoggingOnlyOnError()
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.OnlyOnError;
        }

        protected override void EnableFormDataLoggingOnMatch(Func<HttpContextBase, bool> matchFunction)
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.OnMatch;
            ApplicationLifecycleModule.ShouldLogPostedFormData = matchFunction;
        }

        protected override void SetCustomLogger(ILogger logger)
        {
            ApplicationLifecycleModule.Logger = logger;
        }

        protected override void ResetLogger()
        {
            ApplicationLifecycleModule.Logger = Log.Logger;
        }
    }
}
