using System;
using System.Collections.Specialized;
using System.Threading;
using System.Web;

namespace SerilogWeb.Classic.Tests.Support
{
    public class TestContext
    {

        public TestContext(FakeHttpApplication fakeHttpApplication)
        {
            App = fakeHttpApplication;
        }

        public FakeHttpApplication App { get; }
        private SerilogWebClassicConfiguration Config => SerilogWebClassic.Configuration;

        internal void SimulateRequest(Action<FakeHttpRequest> customizeRequest,
            Func<HttpResponseBase> responseFactory = null)
        {
            Func<HttpResponseBase> createResponse = responseFactory ?? (() => new FakeHttpResponse());
            App.Reset();
            customizeRequest(App.Request);
            var eventHandler = new WebRequestLoggingHandler(App);
            eventHandler.OnBeginRequest(Config);
            App.Response = createResponse();
            eventHandler.OnLogRequest(Config);
        }


        internal void SimulateRequest(string httpMethod = "GET", string httpUrl = "https://www.example.org/", int httpStatusCode = 200, int sleepDurationMilliseconds = 0)
        {
            SimulateRequest(
                req =>
                {
                    req.SetRawUrl(httpUrl);
                    req.SetHttpMethod(httpMethod);
                },
                () =>
                {
                    if (sleepDurationMilliseconds > 0)
                    {
                        Thread.Sleep(sleepDurationMilliseconds);
                    }
                    return new FakeHttpResponse() { StatusCode = httpStatusCode };
                });
        }

        internal void SimulateForm(NameValueCollection formData, int responseStatusCode = 200)
        {
            SimulateRequest(
                req =>
                {
                    req.SetHttpMethod("POST");
                    req.Unvalidated.Form.Clear();
                    req.Unvalidated.Form.Add(formData);
                },
                () => new FakeHttpResponse() { StatusCode = responseStatusCode });
        }
    }
}