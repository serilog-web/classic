using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;

namespace SerilogWeb.Classic.Tests.Support
{
    public class FakeHttpApplication : IHttpApplication
    {
        public HttpContextBase Context { get; private set; }
        public FakeHttpRequest Request { get; private set; }
        HttpRequestBase IHttpApplication.Request => Request;
        public HttpResponseBase Response { get; set; }
        public HttpServerUtilityBase Server { get; private set; }

        public FakeHttpApplication()
        {
            Reset();
        }

        public void Reset()
        {
            Context = new FakeHttpContext(this);
            Request = new FakeHttpRequest();
            Response = null;
            Server = new FakeHttpServerUtility();
        }
    }


    public class FakeHttpContext : HttpContextBase
    {
        private readonly FakeHttpApplication _fakeHttpApplication;

        public FakeHttpContext(FakeHttpApplication fakeHttpApplication)
        {
            _fakeHttpApplication = fakeHttpApplication ?? throw new ArgumentNullException(nameof(fakeHttpApplication));
            Items = new Hashtable();
        }

        public override IDictionary Items { get; }

        public override Exception[] AllErrors => new Exception[0];

        public override HttpRequestBase Request => _fakeHttpApplication.Request;
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

        public override UnvalidatedRequestValuesBase Unvalidated { get; } = new FakeUnvalidatedRequestValues();
    }

    public class FakeUnvalidatedRequestValues : UnvalidatedRequestValuesBase
    {
        public override NameValueCollection Form { get; } = new NameValueCollection();
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