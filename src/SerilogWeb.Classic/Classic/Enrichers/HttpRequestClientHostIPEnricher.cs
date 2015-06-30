// Copyright 2014 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Web;
using Serilog.Core;
using Serilog.Events;

namespace SerilogWeb.Classic.Enrichers
{
    /// <summary>
    /// Enrich log events with the Client IP Address.
    /// </summary>
    public class HttpRequestClientHostIPEnricher : ILogEventEnricher
    {
        /// <summary>
        /// Gets or sets a value indicating whether this enricher will check for possible HTTP proxies via X-FORWARDED-FOR headers.
        /// </summary>
        /// <value>
        /// <c>true</c> if [check for HTTP proxies]; otherwise, <c>false</c>.
        /// </value>
        public bool CheckForHttpProxies { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestClientHostIPEnricher"/> class with <see cref="CheckForHttpProxies"/> set to [true].
        /// </summary>
        public HttpRequestClientHostIPEnricher()
        {
            CheckForHttpProxies = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestClientHostIPEnricher" /> class.
        /// </summary>
        /// <param name="checkForHttpProxies">if set to <c>true</c> this Enricher also checks for HTTP proxies and their X-FORWARDED-FOR header.</param>
        public HttpRequestClientHostIPEnricher(bool checkForHttpProxies)
        {
            CheckForHttpProxies = checkForHttpProxies;
        }

        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string HttpRequestClientHostIPPropertyName = "HttpRequestClientHostIP";

        #region Implementation of ILogEventEnricher

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            if (HttpContext.Current == null)
                return;

            try
            {
            if (HttpContext.Current.Request == null)
                return;
            }
            catch (HttpException ex)
            {
              if (ex.Message.Equals("Request is not available in this context") || ex.ErrorCode == -2147467259)
              {
                // Expect this with ASP.NET integrated pipeline
                return;
              }
            }

            if (string.IsNullOrWhiteSpace(HttpContext.Current.Request.UserHostAddress))
                return;

            string userHostAddress;

            // Taking Proxy/-ies into consideration, too (if wanted and available)
            if (CheckForHttpProxies)
            {
                userHostAddress = !string.IsNullOrWhiteSpace(HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"])
                ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]
                : HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                userHostAddress = HttpContext.Current.Request.UserHostAddress;
            }

            if (string.IsNullOrWhiteSpace(userHostAddress))
                return;

            // As multiple proxies can be in place according to header spec (see http://en.wikipedia.org/wiki/X-Forwarded-For), we check for it and only extract the first address (which 'should' be the actual client one)
            if (userHostAddress.Contains(","))
            {
                userHostAddress = userHostAddress.Split(',').First().Trim();
            }

            var httpRequestClientHostIPProperty = new LogEventProperty(HttpRequestClientHostIPPropertyName, new ScalarValue(userHostAddress));
            logEvent.AddPropertyIfAbsent(httpRequestClientHostIPProperty);
        }

        #endregion
    }
}
