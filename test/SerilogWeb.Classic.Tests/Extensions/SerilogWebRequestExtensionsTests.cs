using System;
using SerilogWeb.Classic.Extensions;
using SerilogWeb.Classic.Tests.Support;
using Xunit;

namespace SerilogWeb.Classic.Tests.Extensions
{
    public class SerilogWebRequestExtensionsTests
    {
        [Fact]
        public void AddSerilogWebErrorCanBeRetrievedOnHttpContextBase()
        {
            var app = new FakeHttpApplication();
            var context = app.Context;

            var original = new Exception("It failed");
            context.AddSerilogWebError(original);

            var returned = context.GetLastSerilogWebError();

            Assert.Same(original, returned);
        }

        [Fact]
        public void GetLastSerilogWebErrorReturnsNullWhenThereIsNoError()
        {
            var app = new FakeHttpApplication();
            var context = app.Context;

            var returned = context.GetLastSerilogWebError();

            Assert.Null(returned);
        }

        [Fact]
        public void GetLastSerilogWebErrorReturnsLastAddedError()
        {
            var app = new FakeHttpApplication();
            var context = app.Context;

            var first = new Exception("It failed once");
            context.AddSerilogWebError(first);

            var second = new Exception("It failed twice");
            context.AddSerilogWebError(second);

            var returned = context.GetLastSerilogWebError();

            Assert.Same(second, returned);
        }
    }
}
