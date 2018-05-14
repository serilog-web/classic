using System;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// The configuration entry point for SerilogWeb
    /// </summary>
    public static class SerilogWebClassic
    {
        /// <summary>
        /// The configuration entry point for SerilogWeb.Classic's logging module
        /// </summary>
        internal static SerilogWebClassicConfiguration Configuration { get; set; } = SerilogWebClassicConfiguration.Default;

        /// <summary>
        /// The configuration entry point for SerilogWeb.Classic's logging module
        /// </summary>
        /// <param name="configure">A configuration pipeline</param>
        public static void Configure(Func<SerilogWebClassicConfigurationBuilder, SerilogWebClassicConfigurationBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var newConfig = Configuration.Edit(configure);
            Configuration = newConfig;
        }

        internal static void ResetConfiguration()
        {
            Configuration = SerilogWebClassicConfiguration.Default;
        }
    }
}
