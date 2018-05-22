using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
            Server = new FakeHttpServerUtility(this);
        }
    }


    public class FakeHttpContext : HttpContextBase
    {
        private readonly FakeHttpApplication _fakeHttpApplication;
        private bool _errorCleared = false;

        public FakeHttpContext(FakeHttpApplication fakeHttpApplication)
        {
            _fakeHttpApplication = fakeHttpApplication ?? throw new ArgumentNullException(nameof(fakeHttpApplication));
            Items = new Hashtable();
        }

        public override IDictionary Items { get; }

        public override Exception[] AllErrors => Errors.ToArray();

        public override HttpRequestBase Request => _fakeHttpApplication.Request;

        public override HttpResponseBase Response => _fakeHttpApplication.Response;

        public override void AddError(Exception errorInfo)
        {
            Errors.Add(errorInfo);
        }

        public override Exception Error
        {
            get
            {
                // trying to mimick the behavior of System.Web.HttpContext
                if (_errorCleared) return null;
                return Errors.FirstOrDefault();
            }
        }

        private List<Exception> Errors { get; } = new List<Exception>();

        public override void ClearError()
        {
            _errorCleared = true;
        }
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
        private readonly FakeHttpApplication _httpApplication;

        public FakeHttpServerUtility(FakeHttpApplication httpApplication)
        {
            _httpApplication = httpApplication ?? throw new ArgumentNullException(nameof(httpApplication));
        }

        public override Exception GetLastError()
        {
            return _httpApplication.Context.Error;
        }
    }
}