using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Serilog.Events;

namespace SerilogWeb.Classic.Tests.Support
{
    public static class Extensions
    {
        public static object LiteralValue(this LogEventPropertyValue @this)
        {
            return ((ScalarValue)@this).Value;
        }

        public static LogEventPropertyValue ToSerilogNameValuePropertySequence(this NameValueCollection kvps)
        {
            return new SequenceValue(kvps.AllKeys
                .SelectMany(k => (kvps.GetValues(k) ?? new string[0]).Select(v => new { k, v }))
                .Select(kvp => new StructureValue(new List<LogEventProperty>
            {
                new LogEventProperty("Name", new ScalarValue(kvp.k)),
                new LogEventProperty("Value", new ScalarValue(kvp.v))
            })));
        }
    }
}
