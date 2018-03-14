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
        private static SerilogWebModuleConfiguration Config => SerilogWebModule.Configuration;

        /// <summary>
        /// The globally-shared logger.
        /// 
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"/>
        public static ILogger Logger
        {
            get => (Config.CustomLogger ?? Log.Logger).ForContext<ApplicationLifecycleModule>();
            set => Config.CustomLogger = value ?? throw new ArgumentNullException(nameof(value));
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
        public static Func<HttpContextBase, bool> RequestFilter
        {
            get => Config.RequestFilter;
            set => Config.RequestFilter = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// When set to Always, form data will be written via an event (using
        /// severity from FormDataLoggingLevel).  When set to OnlyOnError, this
        /// will only be written if the Response has a 500 status.
        /// When set to OnMatch <see cref="ShouldLogPostedFormData"/>
        /// is executed to determine if form data is logged.
        /// The default is Never. Requires that <see cref="IsEnabled"/> is also
        /// true (which it is, by default).
        /// </summary>
        public static LogPostedFormDataOption LogPostedFormData
        {
            get => Config.LogPostedFormData;
            set => Config.LogPostedFormData = value;
        }

        /// <summary>
        /// When set to true (the default), any field containing password will 
        /// not have its value logged when DebugLogPostedFormData is enabled
        /// </summary>
        public static bool FilterPasswordsInFormData
        {
            get => Config.FilterPasswordsInFormData;
            set => Config.FilterPasswordsInFormData = value;
        }

        /// <summary>
        /// When FilterPasswordsInFormData is true, any field containing keywords in this list will 
        /// not have its value logged when DebugLogPostedFormData is enabled
        /// </summary>
        public static IEnumerable<String> FilteredKeywordsInFormData
        {
            get => Config.FilteredKeywordsInFormData;
            set => Config.FilteredKeywordsInFormData = value;
        }

        /// <summary>
        /// When set to true, request details and errors will be logged. The default
        /// is true.
        /// </summary>
        public static bool IsEnabled
        {
            get => Config.IsEnabled;
            set => Config.IsEnabled = value;
        }

        /// <summary>
        /// The level at which to log HTTP requests. The default is Information.
        /// </summary>
        public static LogEventLevel RequestLoggingLevel
        {
            get => Config.RequestLoggingLevel;
            set => Config.RequestLoggingLevel = value;
        }


        /// <summary>
        /// The level at which to log form values
        /// </summary>
        public static LogEventLevel FormDataLoggingLevel
        {
            get => Config.FormDataLoggingLevel;
            set => Config.FormDataLoggingLevel = value;
        }

        /// <summary>
        /// Custom predicate to determine whether form data should be logged. 
        /// <see cref="LogPostedFormData"/> must be set to OnMatch for this to execute.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Func<HttpContextBase, bool> ShouldLogPostedFormData
        {
            get => Config.ShouldLogPostedFormData;
            set => Config.ShouldLogPostedFormData = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="application">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        public void Init(HttpApplication application)
        {
            var appWrapper = new HttpApplicationWrapper(application);
            var eventHandler = new ClassicRequestEventHandler(appWrapper);

            application.BeginRequest += (sender, args) =>
            {
                eventHandler.OnBeginRequest();
            };

            application.LogRequest += (sender, args) =>
            {
                eventHandler.OnLogRequest();
            };
        }

        /// <summary>
        /// Allows to reset the module to its default configuration.
        /// Useful when testing !
        /// </summary>
        internal static void ResetConfiguration()
        {
            Config.Reset();
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
