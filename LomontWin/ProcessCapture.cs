using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lomont.Win
{
    /// <summary>
    /// capture a process, provide input and output streams
    /// </summary>
    public class ProcessCapture
    {
        // anything other than black shows debug messages 
        public ConsoleColor color = ConsoleColor.Black;

        /// <summary>
        /// Given filename to execute, do so
        /// </summary>
        /// <param name="filename"></param>
        public ProcessCapture(string filename, string arguments = "")
        {
            // Console.WriteLine("Start " + filename);


            process.EnableRaisingEvents = true;
            process.OutputDataReceived += ProcessOutputDataReceived;
            process.ErrorDataReceived += ProcessErrorDataReceived;
            process.Exited += ProcessExited;

            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            //below line is optional if we want a blocking call
            //process.WaitForExit();
        }

        ~ProcessCapture()
        {
            // brutal kill if not earlier
            // todo - does not always work. Try https://gist.github.com/jvshahid/6fb2f91fa7fb1db23599
            if (!process.HasExited)
                process.Kill();
        }

        /// <summary>
        /// Write a message to the item
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            // Debug.WriteLine("PROC WRITE: " + message);
            WriteDebugMessage(message, true);
            process.StandardInput.WriteLine(message);
        }

        // read (and clear) any pending messages
        public List<string> Read(bool waitTillReceived)
        {
            var lines = new List<string>();
            do
            {
                while (messages.TryDequeue(out var result))
                {
                    // Debug.WriteLine("PROC READ: " + result);
                    WriteDebugMessage(result, false);
                    lines.Add(result);
                }
            } while (!waitTillReceived || !lines.Any());
            return lines;
        }

        public bool IsFinished => process.HasExited && !messages.Any();

        public bool WaitForExit(int maxMs)
        {
            return process.WaitForExit(maxMs);
        }

        #region Implementation

        void WriteDebugMessage(string message, bool isInput)
        {
            if (color != ConsoleColor.Black)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                if (isInput)
                    message = "In: " + message;
                else
                    message = "Out:" + message;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }

        void ProcessExited(object sender, EventArgs e)
        {
            //Console.WriteLine(string.Format("process exited with code {0}\n", process.ExitCode.ToString()));
        }
        void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            messages.Enqueue(e.Data);
            //Console.WriteLine("ERROR: " + e.Data + "\n");
        }

        ConcurrentQueue<string> errorText = new ConcurrentQueue<string>();

        public string GetErrorText()
        {
            var sb = new StringBuilder();
            while (errorText.TryDequeue(out string msg))
                sb.Append(msg);
            return sb.ToString();
        }
        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            errorText.Enqueue(e.Data);
            // Console.WriteLine("ERROR: " + e.Data + "\n");
        }

        ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
        void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            messages.Enqueue(e.Data);
            //Console.WriteLine(e.Data + "\n");
        }

        Process process = new Process();

        #endregion
    }
}
