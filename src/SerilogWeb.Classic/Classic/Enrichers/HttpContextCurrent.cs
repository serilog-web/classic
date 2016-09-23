using System.Diagnostics;
using System.Web;

namespace SerilogWeb.Classic.Enrichers
{
    /// <summary>
    /// This helper class is used to handle special case introduced by ASP.NET integrated pipeline 
    /// when HttpContextCurrent.Request may throw instead of returning null.
    /// </summary>
    static class HttpContextCurrent
    {
        /// <summary>
        /// Gets the <see cref="T:System.Web.HttpRequest"/> object for the current HTTP request.
        /// </summary>
        /// 
        /// <returns>
        /// The current HTTP request.
        /// </returns>

        // Attribute added to suppress possible exceptions from breaking into debugger when running in "Just my code" mode.
        [DebuggerNonUserCode]
        internal static HttpRequest Request
        {
            get
            {
                HttpContext httpContext = HttpContext.Current;
                if (httpContext == null)
                    return null;
                try
                {
                    return httpContext.Request;
                }
                catch (HttpException)
                {
                    // No need to check the type of the exception - only one exception can be thrown by .Request and we want to ignore it.
                    return null;
                }
            }
        }
    }
}