using System;
using System.Collections;
using System.Web;

namespace SerilogWeb.Classic.Tests.Support
{
    public class FakeHttpApplication : IHttpApplication
    {
        public HttpContextBase Context { get; } = new FakeHttpContext();
        public FakeHttpRequest Request { get; } = new FakeHttpRequest();
        HttpRequestBase IHttpApplication.Request => Request;
        public HttpResponseBase Response { get; } = new FakeHttpResponse();
        public HttpServerUtilityBase Server { get; } = new FakeHttpServerUtility();
    }


    public class FakeHttpContext : HttpContextBase
    {
        public FakeHttpContext()
        {
            Items = new Hashtable();
        }

        public override IDictionary Items { get; }

        public override Exception[] AllErrors => new Exception[0];
    }

    public class FakeHttpRequest : HttpRequestBase
    {
        private string _rawUrl = "http://www.example.com";
        private string _httpMethod = "GET";

        public void SetRawUrl(string rawUrl)
        {
            _rawUrl = rawUrl;
        }

        public void SetHttpMethod(string httpMethod)
        {
            _httpMethod = httpMethod;
        }

        public override string RawUrl => _rawUrl;
        public override string HttpMethod => _httpMethod;
    }

    public class FakeHttpResponse : HttpResponseBase
    {
        public override int StatusCode { get; set; } = 200;
    }

    public class FakeHttpServerUtility : HttpServerUtilityBase
    {
        public override Exception GetLastError()
        {
            return null;
        }
    }
}