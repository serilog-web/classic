using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SerilogWeb.Classic.Tests.Support;
using Xunit;

namespace SerilogWeb.Classic.Tests
{
    /// <summary>
    /// Tests that cover the new static config API : SerilogWebClassic.Configuration
    /// Checks for feature parity with the old API
    /// </summary>
    public class WebRequestLoggingHandlerFluentConfigTests : IDisposable
    {
        private LoggingLevelSwitch LevelSwitch { get; }
        private List<LogEvent> Events { get; }
        private LogEvent LastEvent => Events.LastOrDefault();
        private TestContext TestContext { get; }

        public WebRequestLoggingHandlerFluentConfigTests()
        {
            SerilogWebClassic.Configuration.Reset();
            TestContext = new TestContext(new FakeHttpApplication(), SerilogWebClassic.Configuration);
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
            SerilogWebClassic.Configuration.Reset();
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
            SerilogWebClassic.Configuration.LogAtLevel(requestLoggingLevel);

            TestContext.SimulateRequest();

            var evt = LastEvent;
            Assert.NotNull(evt);
            Assert.Equal(requestLoggingLevel, evt.Level);
        }
    }
}
