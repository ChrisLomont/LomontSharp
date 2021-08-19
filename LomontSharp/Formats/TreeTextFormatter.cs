using System;
using System.Collections.Generic;
using System.IO;

namespace Lomont.Formats
{
    public static class TreeTextFormatter
    {

        public enum Style
        {
            /// <summary>
            /// Use ASCII characters
            /// </summary>
            Ascii,
            /// <summary>
            /// Use Unicode characters
            /// </summary>
            Unicode
        }

        /// <summary>
        /// Dump the given tree structure to a text format
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="output">Where to write to</param>
        /// <param name="root">The root node</param>
        /// <param name="getChildrenFunction">How to get children from a node</param>
        /// <param name="formatNodeFunction">How to format a node</param>
        /// <param name="style">The style for formatting</param>
        public static void Format<TNode>(
            TextWriter output,
            TNode root,
            Func<TNode, IList<TNode>> getChildrenFunction,
            Func<TNode, string> formatNodeFunction = null,
            Style style = Style.Unicode
            )
        {
            if (formatNodeFunction == null)
                formatNodeFunction = n => n.ToString();

            var (bar, tee, end, spc) =
                style == 0
                    ? (" |  ", " +--", " \\--", "    ")
                    : (" \u2502 ", " \u251c\u2500", " \u2514\u2500", "   ");

            Recurse(root, "", false);

            void Recurse(TNode node, string prefix, bool last)
            {

                output.WriteLine($"{prefix}{formatNodeFunction(node)}");
                if (prefix.Length >= tee.Length)
                {
                    prefix = prefix.Substring(0, prefix.Length - tee.Length);
                    prefix += !last ? bar : spc;
                }

                var children = getChildrenFunction(node);
                var childCount = children.Count;
                for (var i = 0; i < childCount; ++i)
                {
                    var chPrefix = i != childCount - 1 ? tee : end;
                    Recurse(children[i], prefix + chPrefix, i == childCount - 1);
                }
            }

        }
    }
}
