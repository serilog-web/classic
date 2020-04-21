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
using System.Web;
using Serilog;
using Serilog.Events;
using System.Collections.Generic;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// HTTP module that logs application request and error events.
    /// </summary>
    public class ApplicationLifecycleModule : IHttpModule
    {
        private static SerilogWebClassicConfiguration Config => SerilogWebClassic.Configuration;

        /// <summary>
        /// The globally-shared logger.
        /// 
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"/>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg.UseLogger(ILogger customLogger))")]
        public static ILogger Logger
        {
            get => Config.Logger;
            set => SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                isEnabled: Config.IsEnabled,
                requestLoggingLevel: Config.RequestLoggingLevel,
                requestFilter: Config.RequestFilter,
                formDataLoggingLevel: Config.FormDataLoggingLevel,
                customLogger: value,
                logPostedFormData: Config.LogPostedFormData,
                shouldLogPostedFormData: Config.ShouldLogPostedFormData,
                filterPasswordsInFormData: Config.FilterPasswordsInFormData,
                filteredKeywordsInFormData: Config.FilteredKeywordsInFormData
            );
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
        /// Custom predicate to filter which requests are logged. If the value
        /// returned is true then the request will be filtered and not logged.        
        /// </summary>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg.IgnoreRequestsMatching(filter))")]
        public static Func<HttpContextBase, bool> RequestFilter
        {
            get => Config.RequestFilter;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                    isEnabled: Config.IsEnabled,
                    requestLoggingLevel: Config.RequestLoggingLevel,
                    requestFilter: value,
                    formDataLoggingLevel: Config.FormDataLoggingLevel,
                    customLogger: Config.CustomLogger,
                    logPostedFormData: Config.LogPostedFormData,
                    shouldLogPostedFormData: Config.ShouldLogPostedFormData,
                    filterPasswordsInFormData: Config.FilterPasswordsInFormData,
                    filteredKeywordsInFormData: Config.FilteredKeywordsInFormData
                    );
            }
        }

        /// <summary>
        /// When set to Always, form data will be written via an event (using
        /// severity from FormDataLoggingLevel).  When set to OnlyOnError, this
        /// will only be written if the Response has status code >= 500.
        /// When set to OnMatch <see cref="ShouldLogPostedFormData"/>
        /// is executed to determine if form data is logged.
        /// The default is Never. Requires that <see cref="IsEnabled"/> is also
        /// true (which it is, by default).
        /// </summary>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging()) or SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => cfg.XXX))")]
        public static LogPostedFormDataOption LogPostedFormData
        {
            get => Config.LogPostedFormData;
            set => SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                isEnabled: Config.IsEnabled,
                requestLoggingLevel: Config.RequestLoggingLevel,
                requestFilter: Config.RequestFilter,
                formDataLoggingLevel: Config.FormDataLoggingLevel,
                customLogger: Config.CustomLogger,
                logPostedFormData: value,
                shouldLogPostedFormData: Config.ShouldLogPostedFormData,
                filterPasswordsInFormData: Config.FilterPasswordsInFormData,
                filteredKeywordsInFormData: Config.FilteredKeywordsInFormData
            );
        }

        /// <summary>
        /// When set to true (the default), any field containing password will 
        /// not have its value logged when DebugLogPostedFormData is enabled
        /// </summary>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.FilterKeywords())) or SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.DisablePasswordFiltering()))")]
        public static bool FilterPasswordsInFormData
        {
            get => Config.FilterPasswordsInFormData;
            set => SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                isEnabled: Config.IsEnabled,
                requestLoggingLevel: Config.RequestLoggingLevel,
                requestFilter: Config.RequestFilter,
                formDataLoggingLevel: Config.FormDataLoggingLevel,
                customLogger: Config.CustomLogger,
                logPostedFormData: Config.LogPostedFormData,
                shouldLogPostedFormData: Config.ShouldLogPostedFormData,
                filterPasswordsInFormData: value,
                filteredKeywordsInFormData: Config.FilteredKeywordsInFormData
            );
        }

        /// <summary>
        /// When FilterPasswordsInFormData is true, any field containing keywords in this list will 
        /// not have its value logged when DebugLogPostedFormData is enabled
        /// </summary>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.FilterKeywords(keywords))")]
        public static IEnumerable<String> FilteredKeywordsInFormData
        {
            get => Config.FilteredKeywordsInFormData;
            set => SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                isEnabled: Config.IsEnabled,
                requestLoggingLevel: Config.RequestLoggingLevel,
                requestFilter: Config.RequestFilter,
                formDataLoggingLevel: Config.FormDataLoggingLevel,
                customLogger: Config.CustomLogger,
                logPostedFormData: Config.LogPostedFormData,
                shouldLogPostedFormData: Config.ShouldLogPostedFormData,
                filterPasswordsInFormData: Config.FilterPasswordsInFormData,
                filteredKeywordsInFormData: value
            );
        }

        /// <summary>
        /// When set to true, request details and errors will be logged. The default
        /// is true.
        /// </summary>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg.Enable()) or SerilogWebClassic.Configure(cfg => cfg.Disable())")]
        public static bool IsEnabled
        {
            get => Config.IsEnabled;
            set => SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                isEnabled: value,
                requestLoggingLevel: Config.RequestLoggingLevel,
                requestFilter: Config.RequestFilter,
                formDataLoggingLevel: Config.FormDataLoggingLevel,
                customLogger: Config.CustomLogger,
                logPostedFormData: Config.LogPostedFormData,
                shouldLogPostedFormData: Config.ShouldLogPostedFormData,
                filterPasswordsInFormData: Config.FilterPasswordsInFormData,
                filteredKeywordsInFormData: Config.FilteredKeywordsInFormData
            );
        }

        /// <summary>
        /// The level at which to log HTTP requests. The default is Information.
        /// </summary>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg.LogAtLevel(level))")]
        public static LogEventLevel RequestLoggingLevel
        {
            get => Config.RequestLoggingLevel;
            set => SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                isEnabled: Config.IsEnabled,
                requestLoggingLevel: value,
                requestFilter: Config.RequestFilter,
                formDataLoggingLevel: Config.FormDataLoggingLevel,
                customLogger: Config.CustomLogger,
                logPostedFormData: Config.LogPostedFormData,
                shouldLogPostedFormData: Config.ShouldLogPostedFormData,
                filterPasswordsInFormData: Config.FilterPasswordsInFormData,
                filteredKeywordsInFormData: Config.FilteredKeywordsInFormData
            );
        }


        /// <summary>
        /// The level at which to log form values
        /// </summary>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg..EnableFormDataLogging(forms => forms.AtLevel(level)))")]
        public static LogEventLevel FormDataLoggingLevel
        {
            get => Config.FormDataLoggingLevel;
            set => SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                isEnabled: Config.IsEnabled,
                requestLoggingLevel: Config.RequestLoggingLevel,
                requestFilter: Config.RequestFilter,
                formDataLoggingLevel: value,
                customLogger: Config.CustomLogger,
                logPostedFormData: Config.LogPostedFormData,
                shouldLogPostedFormData: Config.ShouldLogPostedFormData,
                filterPasswordsInFormData: Config.FilterPasswordsInFormData,
                filteredKeywordsInFormData: Config.FilteredKeywordsInFormData
            );
        }

        /// <summary>
        /// Custom predicate to determine whether form data should be logged. 
        /// <see cref="LogPostedFormData"/> must be set to OnMatch for this to execute.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.OnMatch(matchingFunction)))")]
        public static Func<HttpContextBase, bool> ShouldLogPostedFormData
        {
            get => Config.ShouldLogPostedFormData;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                SerilogWebClassic.Configuration = new SerilogWebClassicConfiguration(
                    isEnabled: Config.IsEnabled,
                    requestLoggingLevel: Config.RequestLoggingLevel,
                    requestFilter: Config.RequestFilter,
                    formDataLoggingLevel: Config.FormDataLoggingLevel,
                    customLogger: Config.CustomLogger,
                    logPostedFormData: Config.LogPostedFormData,
                    shouldLogPostedFormData: value,
                    filterPasswordsInFormData: Config.FilterPasswordsInFormData,
                    filteredKeywordsInFormData: Config.FilteredKeywordsInFormData
                );
            }
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="application">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        public void Init(HttpApplication application)
        {
            var appWrapper = new HttpApplicationWrapper(application);
            var eventHandler = new WebRequestLoggingHandler(appWrapper);

            application.BeginRequest += (sender, args) =>
            {
                eventHandler.OnBeginRequest(Config);
            };

            application.LogRequest += (sender, args) =>
            {
                eventHandler.OnLogRequest(Config);
            };
        }

        /// <summary>
        /// Allows to reset the module to its default configuration.
        /// Useful when testing !
        /// </summary>
        [Obsolete("Obsolete since v4.1 - Use SerilogWebClassic.ResetConfiguration()")]
        internal static void ResetConfiguration()
        {
            SerilogWebClassic.ResetConfiguration();
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
