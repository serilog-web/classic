using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SerilogWeb.Classic.Tests.Support;
using Xunit;

namespace SerilogWeb.Classic.Tests
{
    public class ClassicRequestEventHandlerTests : IDisposable
    {
        private List<LogEvent> Events { get; }
        private LoggingLevelSwitch LevelSwitch { get; } = new LoggingLevelSwitch();


        public ClassicRequestEventHandlerTests()
        {
            ApplicationLifecycleModule.ResetConfiguration();
            Events = new List<LogEvent>();
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
            var app = new FakeHttpApplication();
            app.Request.SetRawUrl(rawUrl);
            app.Request.SetHttpMethod(httpMethod);
            app.Response.StatusCode = httpStatus;
            var sleepTimeMilliseconds = 4;
            var eventHandler = new ClassicRequestEventHandler(app);
            eventHandler.OnBeginRequest();
            Thread.Sleep(sleepTimeMilliseconds); // need some time to have some duration of elapsed !
            eventHandler.OnLogRequest();

            var evt = Events.FirstOrDefault();
            Assert.NotNull(evt);
            Assert.Equal(LogEventLevel.Information, evt.Level);
            Assert.Null(evt.Exception);
            Assert.Equal("HTTP {Method} {RawUrl} responded {StatusCode} in {ElapsedMilliseconds}ms", evt.MessageTemplate.Text);

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
            LevelSwitch.MinimumLevel = requestLoggingLevel;
            ApplicationLifecycleModule.RequestLoggingLevel = requestLoggingLevel;

            var eventHandler = new ClassicRequestEventHandler(new FakeHttpApplication());
            eventHandler.OnBeginRequest();
            eventHandler.OnLogRequest();

            var evt = Events.FirstOrDefault();
            Assert.NotNull(evt);
            Assert.Equal(requestLoggingLevel, evt.Level);
        }

        // TODO : Errors / Exceptions etc
        // TODO : keywords / passwords
        // TODO : Form Data !
        // TODO : All the options on ApplicationLifecycleModule
        // TODO : set Enabled / Disabled
        // TODO : set Logger
        // TODO : set RequestFilter
        // TODO : set LogPostedFormData
        // TODO : set FilterPasswordsInFormData
        // TODO : set FilteredKeywordsInFormData
        // TODO : set RequestLoggingLevel
        // TODO : set RequestLoggingLevel
        // TODO : set FormDataLoggingLevel
        // TODO : set ShouldLogPostedFormData

    }
}
