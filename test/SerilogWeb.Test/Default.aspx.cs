using System;
using Serilog;

namespace SerilogWeb.Test
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Log.Information("Page viewed!");

             //throw new InvalidOperationException("Unlucky this time!");
        }

        protected void Fail(object sender, EventArgs e)
        {
            throw new InvalidOperationException("Kablooey");
        }
    }
}