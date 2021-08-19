using System;
using System.Diagnostics;

namespace Lomont.Utility
{
    /// <summary>
    /// Colored console trace listener, default colors for trace events
    /// Also allows 24-bit color embedding on consoles that support ANSI codes
    /// Embed as '⊢' then color string (maximal parse) for foreground
    /// '⊣' then color for background 
    /// Colors reset after call
    ///
    /// TODO - allow colors to be 'popped' off a stack, or reset to default, since would be useful
    ///
    /// Color is of form:
    /// FFEE0C;
    /// or
    /// FE8;
    ///
    /// Each color set is only for one trace call
    /// </summary>
    public class ColoredTraceListener : ConsoleTraceListener

    {
        public override void Write(string? message)
        {
            var colors = Save();
            message = ExpandColor(message);
            base.Write(message);
            Restore(colors);
        }

        public override void WriteLine(string? message)
        {
            Write(message + Environment.NewLine);
        }

        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType, int id,
            string message
        ) => TraceEvent(eventCache, source, eventType, id, "{0}", message);

        (ConsoleColor fore,ConsoleColor back) Save()
        {
            return (Console.ForegroundColor, Console.BackgroundColor);
        }

        void Restore((ConsoleColor fore, ConsoleColor back) tup)
        {
            Console.ForegroundColor = tup.fore;
            Console.BackgroundColor = tup.back;
        }
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string format, params object[] args)

        {
            var colors = Save();
            Console.ForegroundColor = eventType switch
            {
            
                TraceEventType.Verbose => ConsoleColor.DarkGray,
                TraceEventType.Information => ConsoleColor.White,
                TraceEventType.Warning => ConsoleColor.Yellow,
                TraceEventType.Error => ConsoleColor.Red,
                TraceEventType.Critical => ConsoleColor.Magenta,
                TraceEventType.Start => ConsoleColor.DarkCyan,
                TraceEventType.Stop => ConsoleColor.DarkCyan,
                _ => colors.fore
            };
            var msg = ExpandColor(String.Format(format, args));
            base.TraceEvent(eventCache, source, eventType, id, msg);
            Restore(colors);
        }

        public static void Test()
        {
            var tc = new ColoredTraceListener();
            // listen to messages
            Trace.Listeners.Add(tc);

            Trace.TraceError("ERROR");
            Trace.TraceError($"ERROR {ColoredText.F}0000FF;blue");
            Trace.WriteLine($"{ColoredText.F}0F0;green{ColoredText.F}80A0FF; next {ColoredText.B}F0F;{ColoredText.F}00FF80; back ");
            Trace.TraceWarning("Warning");
            Trace.WriteLine("Normal");

            Trace.Listeners.Remove(tc);
        }


        string ExpandColor(string text)
        {
#if true
            return ColoredText.FormatAnsi(text);
#else // old
            if (string.IsNullOrEmpty(text))
                return text;

            do
            {
                var m = matchColor.Match(text);
                if (!m.Success) 
                    break;
                var mt = m.Value;
                var foreground = mt[0] == F;
                var (r, g, b) = ParseColor(mt.Substring(1, mt.Length - 2));
                var ct = FormatColor(r, g, b, foreground);
                text = text.Replace(m.Value, ct);
            } while (true);
            return text;
#endif
        }
    }
}
