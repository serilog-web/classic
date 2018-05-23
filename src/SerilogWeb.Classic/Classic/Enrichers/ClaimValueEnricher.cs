// Copyright 2015 Serilog Contributors
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
using System.Security.Claims;
using System.Web;
using Serilog.Core;
using Serilog.Events;

namespace SerilogWeb.Classic.Enrichers
{
    /// <summary>
    /// Enrich log events with the named Claim Value.
    /// </summary>
    public class ClaimValueEnricher : ILogEventEnricher
    {
        readonly string _claimProperty;
        readonly string _logEventProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimValueEnricher"/> class.
        /// </summary>
        public ClaimValueEnricher()
            : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimValueEnricher"/> class.
        /// </summary>
        /// <param name="claimProperty">The claim property name searched for value to enrich log events.</param>
        public ClaimValueEnricher(string claimProperty) : this(claimProperty, null)
        {
            _claimProperty = claimProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimValueEnricher"/> class.
        /// </summary>
        /// <param name="claimProperty">The claim property name searched for value to enrich log events.</param>
        /// <param name="logEventProperty">The property name added to enriched log events.</param>
        public ClaimValueEnricher(string claimProperty, string logEventProperty)
        {
            _claimProperty = claimProperty;
            _logEventProperty = logEventProperty ?? claimProperty;
        }

        /// <summary>
        /// Enrich the log event with found by name claim's value
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            if (HttpContext.Current == null)
                return;

            if (HttpContextCurrent.Request == null)
                return;

            var user = HttpContext.Current.User;
            if (user == null)
                return;

            var claims = ((ClaimsIdentity)user.Identity).Claims;

            var value = claims?.FirstOrDefault(c => c.Type == _claimProperty)?.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            var claimProperty = new LogEventProperty(_logEventProperty, new ScalarValue(value));
            logEvent.AddPropertyIfAbsent(claimProperty);
        }
    }
}
