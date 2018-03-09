using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SerilogWeb.Classic.Tests.Support;
using Xunit;

namespace SerilogWeb.Classic.Tests
{
    public class ClassicRequestEventHandlerTests : IDisposable
    {
        private LoggingLevelSwitch LevelSwitch { get; }
        private List<LogEvent> Events { get; }
        private LogEvent LastEvent => Events.LastOrDefault();
        private FakeHttpApplication App { get; }

        public ClassicRequestEventHandlerTests()
        {
            ApplicationLifecycleModule.ResetConfiguration();
            App = new FakeHttpApplication();
            Events = new List<LogEvent>();
            LevelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LevelSwitch)
                .WriteTo.Sink(new DelegatingSink(ev => Events.Add(ev)))
                .CreateLogger();
        }

        public void Dispose()
        {
            Log.CloseAndFlush();
            ApplicationLifecycleModule.ResetConfiguration();
        }

        [Theory]
        [InlineData("GET", "http://www.serilog.net", 403)]
        [InlineData("POST", "https://nblumhardt.com/", 302)]
        [InlineData("HEAD", "http://www.example.org", 200)]
        public void BasicRequestLogging(string httpMethod, string rawUrl, int httpStatus)
        {
            var sleepTimeMilliseconds = 4;

            App.SimulateRequest(httpMethod, rawUrl, httpStatus, sleepTimeMilliseconds);

            var evt = LastEvent;
            Assert.NotNull(evt);
            Assert.Equal(LogEventLevel.Information, evt.Level);
            Assert.Null(evt.Exception);
            Assert.Equal("HTTP {Method} {RawUrl} responded {StatusCode} in {ElapsedMilliseconds}ms", evt.MessageTemplate.Text);

            Assert.Equal($"{typeof(ApplicationLifecycleModule)}", evt.Properties[Constants.SourceContextPropertyName].LiteralValue());
            Assert.Equal(httpMethod, evt.Properties["Method"].LiteralValue());
            Assert.Equal(rawUrl, evt.Properties["RawUrl"].LiteralValue());
            Assert.Equal(httpStatus, evt.Properties["StatusCode"].LiteralValue());

            var recordedElapsed = (long)evt.Properties["ElapsedMilliseconds"].LiteralValue();
            Assert.True(recordedElapsed >= sleepTimeMilliseconds, "recordedElapsed >= sleepTimeMilliseconds");
        }

        [Theory]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public void RequestLoggingLevel(LogEventLevel requestLoggingLevel)
        {
            ApplicationLifecycleModule.RequestLoggingLevel = requestLoggingLevel;

            App.SimulateRequest();

            var evt = LastEvent;
            Assert.NotNull(evt);
            Assert.Equal(requestLoggingLevel, evt.Level);
        }

        [Fact]
        public void LogPostedFormData()
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Qux", "Baz" }
            };
            
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;

            App.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            var expected = formData.ToSerilogNameValuePropertySequence();
            Assert.Equal(expected.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void LogPostedFormDataHandlesMultipleValuesPerKey()
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Foo", "Qux" }
            };

            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;

            App.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"] as SequenceValue;
            Assert.NotNull(formDataProperty);
            Assert.Equal(2, formDataProperty.Elements.Count);
            var firstKvp = formDataProperty.Elements.First() as StructureValue;
            Assert.Equal("Foo", firstKvp?.Properties?.FirstOrDefault(p=> p.Name == "Name")?.Value?.LiteralValue());
            Assert.Equal("Bar", firstKvp?.Properties?.FirstOrDefault(p=> p.Name == "Value")?.Value?.LiteralValue());

            var secondKvp = formDataProperty.Elements.Skip(1).First() as StructureValue;
            Assert.Equal("Foo", secondKvp?.Properties?.FirstOrDefault(p => p.Name == "Name")?.Value?.LiteralValue());
            Assert.Equal("Qux", secondKvp?.Properties?.FirstOrDefault(p => p.Name == "Value")?.Value?.LiteralValue());
        }

        [Fact]
        public void LogPostedFormDataSetToAlwaysIgnoresShouldLogPostedFormData()
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Qux", "Baz" }
            };

            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.ShouldLogPostedFormData = ctx => false; // never log form data

            App.SimulateForm(formData);

            Assert.True(LastEvent.Properties.ContainsKey("FormData"), "LastEvent.Properties.ContainsKey('FormData')");
        }

        [Fact]
        public void LogPostedFormDataSetToNeverIgnoresShouldLogPostedFormData()
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Qux", "Baz" }
            };
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Never;
            ApplicationLifecycleModule.ShouldLogPostedFormData = ctx => true; // always log form data

            App.SimulateForm(formData);

            Assert.False(LastEvent.Properties.ContainsKey("FormData"), "LastEvent.Properties.ContainsKey('FormData')");
        }

        [Theory]
        [InlineData(200, false)]
        [InlineData(302, false)]
        [InlineData(401, false)]
        [InlineData(403, false)]
        [InlineData(404, false)]
        [InlineData(499, false)]
        [InlineData(500, true)]
        [InlineData(502, true)]
        public void LogPostedFormDataSetToOnlyOnErrorShouldLogPostedFormDataOnErrorStatusCodes(int statusCode, bool shouldLogFormData)
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Qux", "Baz" }
            };

            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.OnlyOnError;
            ApplicationLifecycleModule.ShouldLogPostedFormData = ctx => false;

            App.SimulateForm(formData, statusCode);

            Assert.Equal(shouldLogFormData, LastEvent.Properties.ContainsKey("FormData"));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LogPostedFormDataSetOnMatchMustUseResultOfShouldLogPostedFormData(bool shouldLogFormData)
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Qux", "Baz" }
            };

            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.OnMatch;
            ApplicationLifecycleModule.ShouldLogPostedFormData = ctx => shouldLogFormData;

            App.SimulateForm(formData);

            Assert.Equal(shouldLogFormData, LastEvent.Properties.ContainsKey("FormData"));
        }


        [Fact]
        public void LogPostedFormDataAddsNoPropertyWhenThereIsNoFormData()
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;

            App.SimulateForm(new NameValueCollection());

            var evt = LastEvent;
            Assert.NotNull(evt);
            Assert.False(evt.Properties.ContainsKey("FormData"), "evt.Properties.ContainsKey('FormData')");
        }

        [Fact]
        public void LogPostedFormDataTakesIntoAccountFormDataLoggingLevel()
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Qux", "Baz" }
            };
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.FormDataLoggingLevel = LogEventLevel.Verbose;

            LevelSwitch.MinimumLevel = LogEventLevel.Information;
            App.SimulateForm(formData);

            // logging postedFormData in Verbose only
            // but current level is Information
            Assert.False(LastEvent.Properties.ContainsKey("FormData"), "evt.Properties.ContainsKey('FormData')");

            LevelSwitch.MinimumLevel = LogEventLevel.Debug;
            App.SimulateForm(formData);

            // logging postedFormData in Verbose only
            // but current level is Debug
            Assert.False(LastEvent.Properties.ContainsKey("FormData"), "evt.Properties.ContainsKey('FormData')");

            LevelSwitch.MinimumLevel = LogEventLevel.Verbose;
            App.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            var expected = formData.ToSerilogNameValuePropertySequence();
            Assert.Equal(expected.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void FormDataExcludesPasswordKeysByDefault()
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.FormDataLoggingLevel = LogEventLevel.Information;

            var formData = new NameValueCollection
            {
                {"password","Foo" },
                {"PASSWORD", "Bar" },
                {"EndWithPassword", "Qux" },
                {"PasswordPrefix", "Baz" },
                {"Other", "Value" }
            };
            var expectedLoggedData = new NameValueCollection
            {
                {"password","********" },
                {"PASSWORD", "********" },
                {"EndWithPassword", "********" },
                {"PasswordPrefix", "********" },
                {"Other", "Value" },
            }.ToSerilogNameValuePropertySequence();
            

            App.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            Assert.Equal(expectedLoggedData.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void PasswordFilteringCanBeDisabled()
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.FormDataLoggingLevel = LogEventLevel.Information;
            var formData = new NameValueCollection
            {
                {"password","Foo" },
                {"PASSWORD", "Bar" },
                {"EndWithPassword", "Qux" },
                {"PasswordPrefix", "Baz" },
                {"Other", "Value" }
            };

            ApplicationLifecycleModule.FilterPasswordsInFormData = false;

            App.SimulateForm(formData);
            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            var expectedLoggedData = formData.ToSerilogNameValuePropertySequence();
            Assert.Equal(expectedLoggedData.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void PasswordBlackListCanBeCustomized()
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.FormDataLoggingLevel = LogEventLevel.Information;
            ApplicationLifecycleModule.FilteredKeywordsInFormData = new List<string>
            {
                "badword", "forbidden", "restricted"
            };

            var formData = new NameValueCollection
            {
                {"password","Foo" },
                {"badword", "Bar" },
                {"VeryBadWord", "Qux" },
                {"forbidden", "Baz" },
                {"ThisIsRestricted", "Value" }
            };
            var expectedLoggedData = new NameValueCollection
            {
                {"password","Foo" },
                {"badword", "********" },
                {"VeryBadWord", "********" },
                {"forbidden", "********" },
                {"ThisIsRestricted", "********" }
            }.ToSerilogNameValuePropertySequence();

            App.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            Assert.Equal(expectedLoggedData.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void EnableDisable()
        {
            ApplicationLifecycleModule.IsEnabled = false;
            App.SimulateRequest();
            Assert.Null(LastEvent);

            ApplicationLifecycleModule.IsEnabled = true;
            App.SimulateRequest();
            Assert.NotNull(LastEvent);
        }

        [Fact]
        public void CustomLogger()
        {
            var myLogEvents = new List<LogEvent>();
            using (var myLogger = new LoggerConfiguration()
                .WriteTo.Sink(new DelegatingSink(ev => myLogEvents.Add(ev)))
                .CreateLogger())
            {
                ApplicationLifecycleModule.Logger = myLogger;

                App.SimulateRequest();

                Assert.Null(LastEvent);

                var myEvent = myLogEvents.FirstOrDefault();
                Assert.NotNull(myEvent);
                Assert.Equal($"{typeof(ApplicationLifecycleModule)}",
                    myEvent.Properties[Constants.SourceContextPropertyName].LiteralValue());
            }
        }

        [Fact]
        public void RequestFiltering()
        {
            var ignoredPath = "/ignoreme/";
            var ignoredMethod = "HEAD";
            ApplicationLifecycleModule.RequestFilter = ctx =>
                ctx.Request.RawUrl.ToLowerInvariant().Contains(ignoredPath.ToLowerInvariant())
                || ctx.Request.HttpMethod == ignoredMethod;

            App.SimulateRequest("GET", $"{ignoredPath}widgets");
            Assert.Null(LastEvent); // should be filtered out

            App.SimulateRequest(ignoredMethod, "/index.html");
            Assert.Null(LastEvent); // should be filtered out

            App.SimulateRequest("GET", "/index.html");
            Assert.NotNull(LastEvent);
        }

        // TODO : Errors / Exceptions etc
    }
}
