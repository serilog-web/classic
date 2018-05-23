using Serilog.Configuration;
using SerilogWeb.Classic.Enrichers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog
{
    /// <summary>
    /// Extends <see cref="LoggerConfiguration"/> to add enrichers for SerilogWeb.Classic's logging module 
    /// </summary>
    public static class SerilogWebClassicLoggerConfigurationExtensions
    {
        /// <summary>
        /// Enrich log events with the named Claim Value.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <param name="claimProperty">The claim property name searched for value to enrich log events.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithClaimValue(
            this LoggerEnrichmentConfiguration enrichmentConfiguration,
            string claimProperty)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With(new ClaimValueEnricher(claimProperty));
        }

        /// <summary>
        /// Enrich log events with the named Claim Value.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <param name="claimProperty">The claim property name searched for value to enrich log events.</param>
        /// <param name="logEventProperty">The property name added to enriched log events.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithClaimValue(
            this LoggerEnrichmentConfiguration enrichmentConfiguration,
            string claimProperty,
            string logEventProperty)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With(new ClaimValueEnricher(claimProperty, logEventProperty));
        }


        /// <summary>
        /// Enrich log events with the Client IP Address.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <param name="checkForHttpProxies">if set to <c>true</c> this Enricher also checks for HTTP proxies and their X-FORWARDED-FOR header.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithHttpRequestClientHostIP(
            this LoggerEnrichmentConfiguration enrichmentConfiguration,
            bool checkForHttpProxies = true)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With(new HttpRequestClientHostIPEnricher(checkForHttpProxies));
        }

        /// <summary>
        /// Enrich log events with the Client Host Name.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithHttpRequestClientHostName(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<HttpRequestClientHostNameEnricher>();
        }
    }
}
