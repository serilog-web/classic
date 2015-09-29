using Serilog;
using SerilogWeb.Classic;
using System;

namespace SeriLogWeb.Test
{
    public partial class Default : System.Web.UI.Page
    {
        public static readonly ILogger Log =
            Serilog.Log.Logger =
            new LoggerConfiguration()
            .Enrich
            .FromLogContext()
            .Enrich.WithMachineName()
            .WriteTo
            .RollingFile(
                pathFormat: @"c:\Logs\{Date}.log"
            )
            .MinimumLevel
            .Debug()
            .CreateLogger();

        protected void Page_Load(object sender, EventArgs e)
        {
            Log.Information("Page viewed!");
        }
    }
}