using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace SerilogWeb.Classic.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class SerilogWebRequestExtensions
    {
        /// <summary>
        /// The key under which we store Exceptions to be logged by SerilogWeb.Classic in HttpContext.Current.Items
        /// </summary>
        private const string SerilogWebErrorKey = "SerilogWebClassic_Errors";

        /// <summary>
        /// Adds an error so that it can be logged by the SerilogWeb module at the end of the request process
        /// </summary>
        /// <param name="self">the HttpContextBase</param>
        /// <param name="exception">the Exception to log</param>
        public static void AddSerilogWebError(this HttpContextBase self, Exception exception)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            AddSerilogWebErrorInternal(self.Items, exception);

        }

        /// <summary>
        /// Adds an error so that it can be logged by the SerilogWeb module at the end of the request process
        /// </summary>
        /// <param name="self">the HttpContext</param>
        /// <param name="exception">the Exception to log</param>
        public static void AddSerilogWebError(this HttpContext self, Exception exception)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            AddSerilogWebErrorInternal(self.Items, exception);
        }

        private static void AddSerilogWebErrorInternal(IDictionary items, Exception ex)
        {
            Stack<Exception> errors = null;
            if (items.Contains(SerilogWebErrorKey))
            {
                errors = items[SerilogWebErrorKey] as Stack<Exception>;
            }

            errors = errors ?? new Stack<Exception>();

            errors.Push(ex);
            items[SerilogWebErrorKey] = errors;
        }


        /// <summary>
        /// Retrieves the last error stored through <see cref="AddSerilogWebError(System.Web.HttpContextBase,System.Exception)"/> or null
        /// </summary>
        /// <param name="self">the HttpContextBase</param>
        public static Exception GetLastSerilogWebError(this HttpContextBase self)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return GetLastSerilogWebErrorInternal(self.Items);
        }

        /// <summary>
        /// Retrieves the last error stored through <see cref="AddSerilogWebError(System.Web.HttpContext,System.Exception)"/> or null
        /// </summary>
        /// <param name="self">the HttpContextBase</param>
        public static Exception GetLastSerilogWebError(this HttpContext self)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return GetLastSerilogWebErrorInternal(self.Items);
        }

        private static Exception GetLastSerilogWebErrorInternal(IDictionary items)
        {
            if (!items.Contains(SerilogWebErrorKey))
            {
                return null;
            }

            var errors = items[SerilogWebErrorKey] as Stack<Exception>;

            if (errors == null || errors.Count == 0)
            {
                return null;
            }

            return errors.Peek();
        }
    }
}
