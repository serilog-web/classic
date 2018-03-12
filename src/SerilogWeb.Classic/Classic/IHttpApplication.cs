using System.Web;

namespace SerilogWeb.Classic
{
    internal interface IHttpApplication
    {
        HttpContextBase Context { get; }

        HttpRequestBase Request { get; }

        HttpResponseBase Response { get; }

        HttpServerUtilityBase Server { get; }
    }
}