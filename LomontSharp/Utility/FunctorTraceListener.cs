using System;
using System.Diagnostics;

namespace Lomont.Utility
{
    /// <summary>
    /// Trace messages sent to a functor as strings
    /// </summary>
    public class FunctorTraceListener : TraceListener

    {

        string curText = "";

        Action<string> messageSink;
        public FunctorTraceListener(Action<string> messageSink)
        {
            this.messageSink = messageSink;
        }


        public override void Write(string? message)
        {
            curText += message;

            while (true)
            {
                var index = curText.IndexOf('\n');
                if (index < 0) break;

                var txt = curText.Substring(0, index+1);
                messageSink(txt);
                curText = curText.Substring(index+1);
            }

        }

        public override void WriteLine(string? message)
        {
            Write(message + Environment.NewLine);
        }
    }
}
