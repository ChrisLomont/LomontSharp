using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Lomont.Utility
{
    /// <summary>
    /// Embed colors into string as text commands
    /// They are added using the F for foreground symbol, B for Background, and E for color end, as
    /// $"{F}F00{E} this is red foreground and {B}0000FF{E} now a blue background"
    /// </summary>
    public static class ColoredText
    {
        // todo - allow these colors:
        // 123;45;123;
        // or one of Black,Blue,Cyan,DarkBlue,DarkCyan,DarkGray,DarkGreen,DarkMagenta,DarkRed,DarkYellow,Gray,Green,Magenta,Red,White,Yellow
        //  todo - allow popping to last color, or reset to default color, by fore and back or both


        static readonly Regex matchColor = new(
            $"({B}|{F})([0-9a-fA-F]{{3}}|[0-9a-fA-F]{{6}}){E}",
            RegexOptions.Compiled);

        /// <summary>
        /// Trigger a foreground color string
        /// 'Points forward'
        /// </summary>
        public static char F => '⬠';

        // color format: Fore: `F0F` back _F88_

        /// <summary>
        /// Trigger a background color string
        /// 'Points backward'
        /// </summary>
        public static char B => '⬟';

        /// <summary>
        /// Color input end char
        /// </summary>
        public static char E =>';';

        /// <summary>
        /// Format a hex color into r,g,b in 0-255
        /// color is form F0A or of form FF80C3
        /// </summary>
        /// <param name="colorText"></param>
        /// <returns></returns>
        static (int red, int green, int blue, bool valid) ParseColor(string colorText)
        {
            if (colorText.Length == 3)
                return (
                    ParseHex(colorText[0]) * 16,
                    ParseHex(colorText[1]) * 16,
                    ParseHex(colorText[2]) * 16,
                    true
                );
            if (colorText.Length == 6)
                return (
                    ParseHex(colorText[0]) * 16 + ParseHex(colorText[1]),
                    ParseHex(colorText[2]) * 16 + ParseHex(colorText[3]),
                    ParseHex(colorText[4]) * 16 + ParseHex(colorText[5]),
                    true
                );

            return (255, 0, 255, false); // error color

            static int ParseHex(char ch) => ch switch
            {
                >= '0' and <= '9' => ch - '0',
                >= 'a' and <= 'f' => ch - 'a' + 10,
                >= 'A' and <= 'F' => ch - 'A' + 10,
                _ => throw new SyntaxErrorException()
            };
        }

        public record ColoredToken
        {
            public Color Foreground;    // color change to make to foreground
            public bool SetForeground=false;
            public Color Background;    // color change to make to background
            public bool SetBackground =false;
            public string Text;         // text using above settings
        }

        /// <summary>
        /// Color, 0-255 each component
        /// </summary>
        public record Color(int Red=255, int Green=0, int Blue=255);

        /// <summary>
        /// Return text with no colors left
        /// </summary>
        /// <param name="text"></param>
        public static string Decolorize(string text)
        {
            var sb = new StringBuilder();
            foreach (var t in Colorize(text))
                sb.Append(t.Text);
            return sb.ToString();
        }

        /// <summary>
        /// Split text into colorized tokens
        /// TODO - rewrite so no (or minimal) allocations for speed
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IEnumerable<ColoredToken> Colorize(string text)
        {
            // todo - check the valid return bool from parse colors
            if (string.IsNullOrEmpty(text))
            {
                yield return new ColoredToken{Text=""};
                yield break;
            }

            var words = matchColor.Split(text); //split on delimiters, includes matches

            // current colors and states
            var cur = new ColoredToken();

            var index = 0;
            while (index < words.Length)
            { // consume some words
                if (String.IsNullOrEmpty(words[index]))
                    index++;
                else if (words[index][0] == F)
                {
                    var (r,g,b,v) = ParseColor(words[index + 1]);
                    if (!v) Trace.TraceError($"Error parsing color {words[index+1]}");
                    cur.Foreground = new Color(r,g,b);
                    cur.SetForeground = true;
                    index += 2; 
                }
                else if (words[index][0] == B)
                {
                    var (r, g, b, v) = ParseColor(words[index + 1]);
                    if (!v) Trace.TraceError($"Error parsing color {words[index + 1]}");
                    cur.Background = new Color(r, g, b);
                    cur.SetBackground = true;
                    index += 2;
                }
                else
                {
                    // write word, reset parameters
                    yield return new ColoredToken
                    {
                        Foreground = cur.Foreground,
                        SetForeground = cur.SetForeground,
                        Background = cur.Background,
                        SetBackground = cur.SetBackground,
                        Text = words[index]
                    };
                    cur.SetBackground = cur.SetForeground = false;
                    index++;
                }
            }
        }

        /// <summary>
        /// Format 24 bit color using ANSI escape codes
        /// </summary>
        /// <param name="color"></param>
        /// <param name="foreground"></param>
        /// <returns></returns>
        public static string FormatColorAnsi(Color color, bool foreground)
        {
            var (r, g, b) = color;
            //ESC[38;2;⟨r⟩;⟨g⟩;⟨b⟩ m Select RGB foreground color
            //ESC[48;2;⟨r⟩;⟨g⟩;⟨b⟩ m Select RGB background color
            const char esc = (char)0x1b;

            var type = foreground ? 38 : 48;
            return $"{esc}[{type};2;{r};{g};{b}m";
        }


        /// <summary>
        /// Format text as ANSI
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FormatAnsi(string text)
        {
            string msg = "";
            foreach (var token in Colorize(text))
            {
                if (token.SetForeground)
                    msg += FormatColorAnsi(token.Foreground, true);
                if (token.SetBackground)
                    msg += FormatColorAnsi(token.Background, false);
                msg += token.Text;
            }
            return msg;
        }

        /// <summary>
        /// Format a sequence of text messages as a HTML document
        /// </summary>
        /// <returns></returns>
        public static string FormatHtml(string line)
        {
            var sb = new StringBuilder();

            sb.Append("<span style=\"font-family:'Consolas'; background-color:#000000; color:#FFFFFF\">"); // wrap all

            Color f=null, b=null;
            bool colorOpen = false, hasFore = false, hasBack = false;

            foreach (var token in Colorize(line))
            {
                if (token.SetForeground)
                {
                    // close any open
                    if (colorOpen) sb.Append("</span>");
                    f = token.Foreground;
                    hasFore = true;
                    SetColor();
                }

                if (token.SetBackground)
                {
                    // close any open
                    if (colorOpen) sb.Append("</span>");
                    b = token.Background;
                    hasBack = true;
                    SetColor();
                }

                //msg += token.Text;
                sb.Append(token.Text);
            }
            // close any open color
            if (colorOpen) sb.Append("</span>");
            sb.Append("</span>"); // unwrap all

            return sb.ToString();

            void SetColor()
            {
                colorOpen = true;
                var ft = hasFore ? $"color:#{formatColorHtml(f)}" : "";
                var bt = hasBack? $"background-color:#{formatColorHtml(b)}" : "";
                var split = (hasFore && hasBack) ? "; " : "";
                sb.Append($"<span style=\"{ft}{split}{bt}\">");
            }

            string formatColorHtml(Color c)
            {
                return $"{c.Red:X2}{c.Green:X2}{c.Blue:X2}";
            }
        }

    }
}

