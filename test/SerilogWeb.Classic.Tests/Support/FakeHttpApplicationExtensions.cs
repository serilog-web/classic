using System;
using System.Collections.Specialized;
using System.Threading;
using System.Web;

namespace SerilogWeb.Classic.Tests.Support
{
    public static class FakeHttpApplicationExtensions
    {
        private static void SimulateRequest(this FakeHttpApplication self, Action<FakeHttpRequest> customizeRequest,
            Func<HttpResponseBase> responseFactory = null)
        {
            Func<HttpResponseBase> createResponse = responseFactory ?? (() => new FakeHttpResponse());
            self.Reset();
            customizeRequest(self.Request);
            var eventHandler = new ClassicRequestEventHandler(self);
            eventHandler.OnBeginRequest();
            self.Response = createResponse();
            eventHandler.OnLogRequest();
        }

        public static void SimulateRequest(this FakeHttpApplication self, string httpMethod = "GET", string httpUrl = "https://www.example.org/", int httpStatusCode = 200, int sleepDurationMilliseconds = 0)
        {
            self.SimulateRequest(req =>
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

        public static void SimulateForm(this FakeHttpApplication self, NameValueCollection formData)
        {
            self.SimulateRequest(req =>
            {
                req.SetHttpMethod("POST");
                req.Unvalidated.Form.Clear();
                req.Unvalidated.Form.Add(formData);
            });
        }
    }
}