using System;
using System.Web;
using SerilogWeb.Classic.Enrichers;

namespace SerilogWeb.Classic
{
    internal class HttpApplicationWrapper : IHttpApplication
    {
        private readonly HttpApplication _httpApplication;

        public HttpApplicationWrapper(HttpApplication httpApplication)
        {
            _httpApplication = httpApplication ?? throw new ArgumentNullException(nameof(httpApplication));
        }

        public HttpContextBase Context
        {
            get
            {
                if (_httpApplication.Context == null) return null;
                return new HttpContextWrapper(_httpApplication.Context);
            }
        }

        public HttpRequestBase Request
        {
            get
            {
                var req = HttpContextCurrent.Request;
                if (req == null) return null;
                return new HttpRequestWrapper(req);
            }
        }

        public HttpResponseBase Response
        {
            get
            {
                var resp = _httpApplication.Response;
                if (resp == null) return null;
                return new HttpResponseWrapper(resp);
            }
        }

        public HttpServerUtilityBase Server
        {
            get
            {
                var server = _httpApplication.Server;
                if (server == null) return null;
                return new HttpServerUtilityWrapper(server);
            }
        }
    }
}