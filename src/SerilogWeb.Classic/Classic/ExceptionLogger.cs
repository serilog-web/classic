using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// Add the following line in Global.asax.cs
    /// to log WebApi faults
    /// <code>
    /// protected void Application_Start()
    ///   {
    ///       GlobalConfiguration.Configure(SerilogWeb.Classic.ExceptionLogger.Register);
    ///   }
    /// </code>
    /// </summary>
    public class ExceptionLogger : IExceptionLogger
    {

        /// <summary>
        /// 
        /// </summary>
        public static Action<HttpConfiguration> Register
            => c => c.Services.Add(typeof (IExceptionLogger),
                new ExceptionLogger());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            HttpContext.Current.AddError(context.Exception);
            
            return Task.FromResult(0);
        }
    }
}
