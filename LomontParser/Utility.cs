using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Parser
{
    /// <summary>
    /// Things to help make parsers, such as regular expressions, 
    /// common converters (like float regex to float value, ...)
    /// </summary>
    public static class Utility
    {
        public const string OpenCStyleBlockComment = "/\\*";
        public const string CloseCStyleBlockComment = "\\*/";
    }
}
