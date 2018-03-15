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
        protected override SerilogWebClassicConfiguration ResetConfiguration()
        {
            SerilogWebClassic.Configuration.Reset();
            return SerilogWebClassic.Configuration;
        }

        protected override void SetRequestLoggingLevel(LogEventLevel level)
        {
            SerilogWebClassic.Configuration.LogAtLevel(level);
        }

        protected override void SetRequestLoggingFilter(Func<HttpContextBase, bool> exclude)
        {
            SerilogWebClassic.Configuration.IgnoreRequestsMatching(exclude);
        }

        protected override void EnableLogging()
        {
            SerilogWebClassic.Configuration.Enable();
        }

        protected override void DisableLogging()
        {
            SerilogWebClassic.Configuration.Disable();
        }

        protected override void EnableFormDataLoggingAlways()
        {
            SerilogWebClassic.Configuration.EnableFormDataLogging();
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level)
        {
            SerilogWebClassic.Configuration.EnableFormDataLogging(cfg => cfg.AtLevel(level));
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level, bool filterPasswords)
        {
            if (filterPasswords)
            {
                SerilogWebClassic.Configuration.EnableFormDataLogging(cfg => cfg
                    .AtLevel(level)
                    .FilterKeywords()
                );
            }
            else
            {
                SerilogWebClassic.Configuration.EnableFormDataLogging(cfg => cfg
                    .AtLevel(level)
                    .DisablePasswordFiltering()
                );
            }
        }

        protected override void EnableFormDataLoggingAlways(LogEventLevel level, IEnumerable<string> customBlackList)
        {
            SerilogWebClassic.Configuration.EnableFormDataLogging(cfg => cfg
                .AtLevel(level)
                .FilterKeywords(customBlackList)
            );
        }

        protected override void DisableFormDataLogging()
        {
            SerilogWebClassic.Configuration.DisableFormDataLogging();
        }

        protected override void EnableFormDataLoggingOnlyOnError()
        {
            SerilogWebClassic.Configuration.EnableFormDataLogging(cfg => cfg.OnlyOnError());
        }

        protected override void EnableFormDataLoggingOnMatch(Func<HttpContextBase, bool> matchFunction)
        {
            SerilogWebClassic.Configuration.EnableFormDataLogging(cfg => cfg.OnMatch(matchFunction));
        }

        protected override void SetCustomLogger(ILogger logger)
        {
            SerilogWebClassic.Configuration.UseLogger(logger);
        }

        protected override void ResetLogger()
        {
            SerilogWebClassic.Configuration.UseDefaultLogger();
        }
    }
}
