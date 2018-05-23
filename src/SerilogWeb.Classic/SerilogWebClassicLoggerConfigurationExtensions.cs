using Serilog.Configuration;
using Serilog.Core;
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

        /// <summary>
        /// Enrich log events with a HttpRequestId GUID.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithHttpRequestId(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<HttpRequestIdEnricher>();
        }

        /// <summary>
        /// Enrich log events with a HttpRequestNumber unique within the current
        /// logging session.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithHttpRequestNumber(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<HttpRequestNumberEnricher>();
        }

        /// <summary>
        /// Enrich log events with the Url of the Request.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <param name="useRawUrl">if set to <c>true</c> this Enricher uses the Raw Url of the Request.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithHttpRequestUrl(
            this LoggerEnrichmentConfiguration enrichmentConfiguration,
            bool useRawUrl = true)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));

            ILogEventEnricher urlEnricher;
            if (useRawUrl)
                urlEnricher = new HttpRequestRawUrlEnricher();
            else
                urlEnricher = new HttpRequestUrlEnricher();
            
            return enrichmentConfiguration.With(urlEnricher);
        }

        /// <summary>
        /// Enrich log events with a HttpRequestTraceId GUID matching the
        /// RequestTraceIdentifier assigned by IIS and used throughout
        /// ASP.NET/ETW. IIS ETW tracing must be enabled for this to work.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithHttpRequestTraceId(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<HttpRequestTraceIdEnricher>();
        }

        /// <summary>
        /// Enrich log events with the HTTP Request Type.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithHttpRequestType(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<HttpRequestTypeEnricher>();
        }
    }
}
