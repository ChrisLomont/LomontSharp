using System;
using System.Text;
using System.Windows;

namespace Lomont.WPF.Utility
{
    /// <summary>
    /// Clipboard helper functions
    /// </summary>
    public static class ClipboardHelper
    {
        /// <summary>
        /// Set the given text and html to the clipboard
        /// If one null, skipped.
        /// </summary>
        /// <param name="htmlText"></param>
        /// <param name="plainText"></param>
        public static void SetClipboard(string htmlText, string plainText)
        {
            var set = false;

            var dao = new DataObject();
            if (!string.IsNullOrEmpty(htmlText))
            {
                dao.SetData(DataFormats.Html, WrapHtml(htmlText));
                set = true;
            }

            if (!string.IsNullOrEmpty(plainText))
            {
                dao.SetData(DataFormats.Text, plainText);
                dao.SetData(DataFormats.UnicodeText, plainText); // needed for some readers
                set = true;
            }

            if (set)
                Clipboard.SetDataObject(dao);
        }

        #region Implementation
        /// <summary>
        /// Wrap html for clipboard
        /// Embeds in special clipboard format
        /// Clipboard UTF8, Dotnet UTF16, so must do some math
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        static string WrapHtml(string html)
        {

            // to do as HTML https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/platform-apis/aa767917(v=vs.85)?redirectedfrom=MSDN
            // to save html and txt at same time http://csharphelper.com/blog/2014/09/copy-and-paste-data-in-multiple-formats-to-the-clipboard-in-c/

            // used in counter below
            char[] byteCount = new char[1];

            var sb = new StringBuilder();
            sb.AppendLine(Header);
            sb.AppendLine(@"<!DOCTYPE>");

            // get position for header
            var fragStart = Utf8Count();

            // get body, html prefix and suffix as needed
            var (prefix, suffix) = ("", "");
            Wrap(html, "HTML", ref prefix, ref suffix);
            Wrap(html, "BODY", ref prefix, ref suffix);

            // prefix, html, suffix
            sb.Append(prefix);
            sb.Append(html);
            sb.Append(suffix);

            // position for header
            var fragEnd = Utf8Count();

            // backpatch, only need to scan header
            sb.Replace("<<<<<<1", Header.Length.ToString("D7"), 0, Header.Length);
            sb.Replace("<<<<<<2", Utf8Count().ToString("D7"), 0, Header.Length);
            sb.Replace("<<<<<<3", fragStart.ToString("D7"), 0, Header.Length);
            sb.Replace("<<<<<<4", fragEnd.ToString("D7"), 0, Header.Length);

            return sb.ToString();

            static void Wrap(string html, string tag, ref string prefix, ref string suffix)
            {
                if (html.IndexOf($"<{tag}", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    prefix += $"<{tag}>";
                    suffix += $"</{tag}>";
                }
            }
            // compute bytes needed for UTF-8
            int Utf8Count()
            {
                var count = 0;
                for (var i = 0; i < sb.Length; i++)
                {
                    byteCount[0] = sb[i];
                    count += Encoding.UTF8.GetByteCount(
                        byteCount);
                }
                return count;
            }
        }

        const string Header =
            "Version:0.9\n" +
            "StartHTML:<<<<<<1\n" +
            "EndHTML:<<<<<<2\n" +
            "StartFragment:<<<<<<3\n" +
            "EndFragment:<<<<<<4\n" +
            "StartSelection:<<<<<<3\n" +
            "EndSelection:<<<<<<4\n";
        #endregion



    }
}
