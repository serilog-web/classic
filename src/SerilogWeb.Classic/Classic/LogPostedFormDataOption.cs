using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerilogWeb.Classic
{
    /// <summary>
    /// Describes options for logging form request data
    /// </summary>
    public enum LogPostedFormDataOption
    {
        /// <summary>
        /// Posted form values are never logged
        /// </summary>
        Never,
        /// <summary>
        /// Posted form values are always logged
        /// </summary>
        Always,
        /// <summary>
        /// Posted form values are logged if Response.StatusCode >= 500
        /// </summary>
        OnlyOnError
    }
}