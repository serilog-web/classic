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
using System.Web;
using Serilog;
using Serilog.Events;
using SerilogWeb.Classic.Enrichers;
using System.Collections.Generic;
using System.Diagnostics;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// HTTP module that logs application request and error events.
    /// </summary>
    public class ApplicationLifecycleModule : IHttpModule
    {
        private const string StopWatchKey = "SerilogWeb.Classic.ApplicationLifecycleModule.StopWatch";

        static volatile LogPostedFormDataOption _logPostedFormData = LogPostedFormDataOption.Never;
        static volatile bool _isEnabled = true;
        static volatile bool _filterPasswordsInFormData = true;
        static volatile IEnumerable<string> _filteredKeywords = new[] { "password" };
        static volatile LogEventLevel _requestLoggingLevel = LogEventLevel.Information;
        static volatile LogEventLevel _formDataLoggingLevel = LogEventLevel.Debug;

        #pragma warning disable 612 // allow obsolete call to keep backwards compatability
        static volatile Func<HttpContext, bool> _shouldLogPostedFormData = context => (LogPostedFormData == LogPostedFormDataOption.Always ||
            (LogPostedFormData == LogPostedFormDataOption.OnlyOnError && HttpContext.Current.Response.StatusCode >= 500));
        #pragma warning restore 612

        static volatile ILogger _logger;

        /// <summary>
        /// The globally-shared logger.
        /// 
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"/>
        public static ILogger Logger
        {
            get
            {
                return (_logger ?? Log.Logger).ForContext<ApplicationLifecycleModule>();
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _logger = value;
            }
        }

        /// <summary>
        /// Register the module with the application (called automatically;
        /// do not call this explicitly from your code).
        /// </summary>
        public static void Register()
        {
            HttpApplication.RegisterModule(typeof(ApplicationLifecycleModule));
        }

        /// <summary>
        /// When set to Always, form data will be written via an event (using
        /// severity from FormDataLoggingLevel).  When set to OnlyOnError, this
        /// will only be written if the Response has a 500 status.
        /// The default is Never. Requires that <see cref="IsEnabled"/> is also
        /// true (which it is, by default).
        /// </summary>
        /// <remarks>
        /// Obsolete. Use <see cref="ShouldLogPostedFormData"/> to configure
        /// when to log posted form data
        /// </remarks>
        [Obsolete]
        public static LogPostedFormDataOption LogPostedFormData
        {
            get { return _logPostedFormData; }
            set { _logPostedFormData = value; }
        }

        /// <summary>
        /// When set to true (the default), any field containing password will 
        /// not have its value logged when DebugLogPostedFormData is enabled
        /// </summary>
        public static bool FilterPasswordsInFormData
        {
            get { return _filterPasswordsInFormData; }
            set { _filterPasswordsInFormData = value; }
        }

        /// <summary>
        /// When FilterPasswordsInFormData is true, any field containing keywords in this list will 
        /// not have its value logged when DebugLogPostedFormData is enabled
        /// </summary>
        public static IEnumerable<String> FilteredKeywordsInFormData
        {
            get { return _filteredKeywords; }
            set { _filteredKeywords = value; }
        }

        /// <summary>
        /// When set to true, request details and errors will be logged. The default
        /// is true.
        /// </summary>
        public static bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        /// <summary>
        /// The level at which to log HTTP requests. The default is Information.
        /// </summary>
        public static LogEventLevel RequestLoggingLevel
        {
            get { return _requestLoggingLevel; }
            set { _requestLoggingLevel = value; }
        }


        /// <summary>
        /// The level at which to log form values
        /// </summary>
        public static LogEventLevel FormDataLoggingLevel
        {
            get { return _formDataLoggingLevel; }
            set { _formDataLoggingLevel = value; }
        }

        /// <summary>
        /// The function used to determine if posted form data should be logged.
        /// </summary>
        /// <remarks>If set the LogPostedFormData value will be ignored </remarks>
        public static Func<HttpContext, bool> ShouldLogPostedFormData
        {
            get { return _shouldLogPostedFormData; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _shouldLogPostedFormData = value;
            }
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="application">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        public void Init(HttpApplication application)
        {
            application.BeginRequest += (sender, args) =>
            {
                if(_isEnabled && application.Context != null)
                {
                    application.Context.Items[StopWatchKey] = Stopwatch.StartNew();
                }                
            };

            application.LogRequest += (sender, args) =>
            {
                if (_isEnabled && application.Context != null)
                {
                    var stopwatch = application.Context.Items[StopWatchKey] as Stopwatch;
                    if (stopwatch == null)
                        return;

                    stopwatch.Stop();

                    var request = HttpContextCurrent.Request;
                    if (request == null)
                        return;

                    var error = application.Server.GetLastError();
                    var level = error != null ? LogEventLevel.Error : _requestLoggingLevel;

                    var logger = Logger;
                    if (logger.IsEnabled(_formDataLoggingLevel) && ShouldLogPostedFormData(HttpContext.Current))
                    {
                        var form = request.Unvalidated.Form;
                        if (form.HasKeys())
                        {
                            var formData = form.AllKeys.SelectMany(k => (form.GetValues(k) ?? new string[0]).Select(v => new { Name = k, Value = FilterPasswords(k, v) }));
                            logger = logger.ForContext("FormData", formData, true);
                        }
                    }

                    logger.Write(
                        level,
                        error,
                        "HTTP {Method} {RawUrl} responded {StatusCode} in {ElapsedMilliseconds}ms", 
                        request.HttpMethod, 
                        request.RawUrl, 
                        application.Response.StatusCode, 
                        stopwatch.ElapsedMilliseconds);
                }                
            };
        }

        /// <summary>
        /// Filters configured keywords from being logged
        /// </summary>
        /// <param name="key">Key of the pair</param>
        /// <param name="value">Value of the pair</param>
        static string FilterPasswords(string key, string value)
        {
            if (_filterPasswordsInFormData && _filteredKeywords.Any(keyword => key.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) != -1))
            {
                return "********";
            }

            return value;
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
