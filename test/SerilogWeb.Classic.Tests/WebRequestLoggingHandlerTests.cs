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
    /// <summary>
    /// Tests that cover the WebRequestLoggingHandler without any static awkward state
    /// </summary>
    public class WebRequestLoggingHandlerTests : IDisposable
    {
        private LoggingLevelSwitch LevelSwitch { get; }
        private List<LogEvent> Events { get; }
        private LogEvent LastEvent => Events.LastOrDefault();
        private FakeHttpApplication App => TestContext.App;
        private TestContext TestContext { get; }

        public WebRequestLoggingHandlerTests()
        {
            SerilogWebClassic.ResetConfiguration();
            TestContext = new TestContext(new FakeHttpApplication());
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
        }

        [Theory]
        [InlineData("GET", "http://www.serilog.net", 403)]
        [InlineData("POST", "https://nblumhardt.com/", 302)]
        [InlineData("HEAD", "http://www.example.org", 200)]
        public void BasicRequestLogging(string httpMethod, string rawUrl, int httpStatus)
        {
            var sleepTimeMilliseconds = 4;

            TestContext.SimulateRequest(httpMethod, rawUrl, httpStatus, sleepTimeMilliseconds);

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

            Assert.False(evt.Properties.ContainsKey("FormData"), "no formData in default configuration");
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
            SerilogWebClassic.Configure(cfg => cfg.LogAtLevel(requestLoggingLevel));

            TestContext.SimulateRequest();

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

            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging());

            TestContext.SimulateForm(formData);

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

            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging());

            TestContext.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"] as SequenceValue;
            Assert.NotNull(formDataProperty);
            Assert.Equal(2, formDataProperty.Elements.Count);
            var firstKvp = formDataProperty.Elements.First() as StructureValue;
            Assert.Equal("Foo", firstKvp?.Properties?.FirstOrDefault(p => p.Name == "Name")?.Value?.LiteralValue());
            Assert.Equal("Bar", firstKvp?.Properties?.FirstOrDefault(p => p.Name == "Value")?.Value?.LiteralValue());

            var secondKvp = formDataProperty.Elements.Skip(1).First() as StructureValue;
            Assert.Equal("Foo", secondKvp?.Properties?.FirstOrDefault(p => p.Name == "Name")?.Value?.LiteralValue());
            Assert.Equal("Qux", secondKvp?.Properties?.FirstOrDefault(p => p.Name == "Value")?.Value?.LiteralValue());
        }


        [Fact]
        public void LogPostedFormDataAddsNoPropertyWhenThereIsNoFormData()
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging());

            TestContext.SimulateForm(new NameValueCollection());

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
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.AtLevel(LogEventLevel.Verbose)));

            LevelSwitch.MinimumLevel = LogEventLevel.Information;
            TestContext.SimulateForm(formData);

            // logging postedFormData in Verbose only
            // but current level is Information
            Assert.False(LastEvent.Properties.ContainsKey("FormData"), "evt.Properties.ContainsKey('FormData')");

            LevelSwitch.MinimumLevel = LogEventLevel.Debug;
            TestContext.SimulateForm(formData);

            // logging postedFormData in Verbose only
            // but current level is Debug
            Assert.False(LastEvent.Properties.ContainsKey("FormData"), "evt.Properties.ContainsKey('FormData')");

            LevelSwitch.MinimumLevel = LogEventLevel.Verbose;
            TestContext.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            var expected = formData.ToSerilogNameValuePropertySequence();
            Assert.Equal(expected.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void EnableFormDataLoggingShouldLogPostedFormData()
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Qux", "Baz" }
            };

            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging());

            TestContext.SimulateForm(formData);

            Assert.True(LastEvent.Properties.ContainsKey("FormData"), "LastEvent.Properties.ContainsKey('FormData')");
        }

        [Fact]
        public void DisableFormDataLoggingShouldNotLogPostedFormData()
        {
            var formData = new NameValueCollection
            {
                {"Foo","Bar" },
                {"Qux", "Baz" }
            };

            SerilogWebClassic.Configure(cfg => cfg.DisableFormDataLogging());

            TestContext.SimulateForm(formData);

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

            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.OnlyOnError()));

            TestContext.SimulateForm(formData, statusCode);

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

            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.OnMatch(ctx => shouldLogFormData)));

            TestContext.SimulateForm(formData);

            Assert.Equal(shouldLogFormData, LastEvent.Properties.ContainsKey("FormData"));
        }

        [Fact]
        public void FormDataExcludesPasswordKeysByDefault()
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms.AtLevel(LogEventLevel.Information)));

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


            TestContext.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            Assert.Equal(expectedLoggedData.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void PasswordFilteringCanBeDisabled()
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms
                 .AtLevel(LogEventLevel.Information)
                 .DisablePasswordFiltering()
            ));

            var formData = new NameValueCollection
            {
                {"password","Foo" },
                {"PASSWORD", "Bar" },
                {"EndWithPassword", "Qux" },
                {"PasswordPrefix", "Baz" },
                {"Other", "Value" }
            };

            TestContext.SimulateForm(formData);
            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            var expectedLoggedData = formData.ToSerilogNameValuePropertySequence();
            Assert.Equal(expectedLoggedData.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void PasswordBlackListCanBeCustomized()
        {
            SerilogWebClassic.Configure(cfg => cfg.EnableFormDataLogging(forms => forms
                 .AtLevel(LogEventLevel.Information)
                 .FilterKeywords(new List<string>
                     {
                        "badword", "forbidden", "restricted"
                     }
             )));

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

            TestContext.SimulateForm(formData);

            var formDataProperty = LastEvent.Properties["FormData"];
            Assert.NotNull(formDataProperty);
            Assert.Equal(expectedLoggedData.ToString(), formDataProperty.ToString());
        }

        [Fact]
        public void EnableDisable()
        {
            SerilogWebClassic.Configure(cfg => cfg.Disable());
            TestContext.SimulateRequest();
            Assert.Null(LastEvent);

            SerilogWebClassic.Configure(cfg => cfg.Enable());
            TestContext.SimulateRequest();
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
                SerilogWebClassic.Configure(cfg => cfg.UseLogger(myLogger));

                TestContext.SimulateRequest();

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
            SerilogWebClassic.Configure(cfg => cfg.IgnoreRequestsMatching(ctx =>
                 ctx.Request.RawUrl.ToLowerInvariant().Contains(ignoredPath.ToLowerInvariant())
                 || ctx.Request.HttpMethod == ignoredMethod));

            TestContext.SimulateRequest("GET", $"{ignoredPath}widgets");
            Assert.Null(LastEvent); // should be filtered out

            TestContext.SimulateRequest(ignoredMethod, "/index.html");
            Assert.Null(LastEvent); // should be filtered out

            TestContext.SimulateRequest("GET", "/index.html");
            Assert.NotNull(LastEvent);
        }

        [Theory]
        [InlineData(500, true)]
        [InlineData(501, true)]
        [InlineData(499, false)]
        public void StatusCodeBiggerThan500AreLoggedAsError(int httpStatusCode, bool isLoggedAsError)
        {
            TestContext.SimulateRequest(httpStatusCode: httpStatusCode);

            Assert.NotNull(LastEvent);
            Assert.Equal(isLoggedAsError, LastEvent.Level == LogEventLevel.Error);
        }

        [Fact]
        public void RequestWithServerLastErrorAreLoggedAsErrorWithException()
        {
            var theError = new InvalidOperationException("Epic fail", new NotImplementedException());
            TestContext.SimulateRequest(
                (req) => { },
                () =>
                {
                    App.Context.AddError(theError);
                    Assert.NotNull(App.Server.GetLastError());
                    return new FakeHttpResponse();
                });

            Assert.NotNull(LastEvent);
            Assert.Equal(LogEventLevel.Error, LastEvent.Level);
            Assert.Same(theError, LastEvent.Exception);
        }

        [Fact]
        public void RequestWithoutServerLastErrorButStatusCode500AreLoggedAsErrorWithLastAppErrorInException()
        {
            var firstError = new InvalidOperationException("Epic fail #1", new NotImplementedException());
            var secondError = new InvalidOperationException("Epic fail #2", new NotImplementedException());
            TestContext.SimulateRequest(
                (req) => { },
                () =>
                {
                    App.Context.AddError(firstError);
                    App.Context.AddError(secondError);
                    Assert.NotNull(App.Server.GetLastError());
                    App.Context.ClearError();
                    Assert.Null(App.Server.GetLastError());
                    return new FakeHttpResponse()
                    {
                        StatusCode = 500
                    };
                });

            Assert.NotNull(LastEvent);
            Assert.Equal(LogEventLevel.Error, LastEvent.Level);
            Assert.Same(secondError, LastEvent.Exception);
        }
    }
}
