using System;
using System.Collections.Generic;
using System.Web;
using Serilog;
using Serilog.Events;

namespace SerilogWeb.Classic.Tests
{
    /// <summary>
    /// Tests that prove that using SerilogWebClassic.Configuration fluent API has the expected outcome
    /// and works the same as ApplicationLifecycleModule used to
    /// </summary>
    public class SerilogWebClassicConfigConformanceTests : ModuleConfigurationContractTests
    {
        protected override void ResetConfiguration()
        {
            SerilogWebClassic.ResetConfiguration();
        }

        protected override void SetRequestLoggingLevel(LogEventLevel level)
        {
            SerilogWebClassic.Configure(cfg => cfg.LogAtLevel(level));
        }

        protected override void SetRequestLoggingFilter(Func<HttpContextBase, bool> exclude)
        {
            SerilogWebClassic.Configure(cfg => cfg.IgnoreRequestsMatching(exclude));
        }

        protected override void EnableLogging()
        {
            SerilogWebClassic.Configure(cfg => cfg.Enable());
        }

        protected override void DisableLogging()
        {
            SerilogWebClassic.Configure(cfg => cfg.Disable());
        }

        protected override void EnableFormDataLoggingAlways()
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging());
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level)
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.AtLevel(level)));
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level, bool filterPasswords)
        {
            if (filterPasswords)
            {
                SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms
                    .AtLevel(level)
                    .FilterKeywords()
                ));
            }
            else
            {
                SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms
                    .AtLevel(level)
                    .DisablePasswordFiltering()
                ));
            }
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level, IEnumerable<string> customBlackList)
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms
                .AtLevel(level)
                .FilterKeywords(customBlackList)
            ));
        }

        protected override void DisableFormDataLogging()
        {
            SerilogWebClassic.Configure(cfg => cfg.DisableFormDataLogging());
        }

        protected override void EnableFormDataLoggingOnlyOnError()
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.OnlyOnError()));
        }

        protected override void EnableFormDataLoggingOnMatch(Func<HttpContextBase, bool> matchFunction)
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.OnMatch(matchFunction)));
        }

        protected override void SetCustomLogger(ILogger logger)
        {
            SerilogWebClassic.Configure(cfg => cfg.UseLogger(logger));
        }

        protected override void ResetLogger()
        {
            SerilogWebClassic.Configure(cfg => cfg.UseDefaultLogger());
        }
    }
}
