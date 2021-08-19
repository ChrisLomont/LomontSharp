using System;
using System.Collections.Generic;

namespace Lomont.Algorithms
{
    public static class BoyerMoore
    {
        /// <summary>
        /// Find pattern in text, return list of start indices
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int FindFirst(ReadOnlySpan<byte> text, ReadOnlySpan<byte> pattern, int maxOffset = -1)
        {
            var m = pattern.Length;
            var n = text.Length;

            // make bad char table
            var badChar = new int[256];
            for (var i = 0; i < 256; i++)
                badChar[i] = -1;
            for (var i = 0; i < pattern.Length; i++)
                badChar[pattern[i]] = i;

            if (maxOffset == -1) maxOffset = Int32.MaxValue;

            var s = 0;
            while (s <= (n - m) && s < maxOffset)
            {
                var j = m - 1;

                while (j >= 0 && pattern[j] == text[s + j])
                    --j;

                if (j < 0)
                {
                    return s;
                    s += (s + m < n) ? m - badChar[text[s + m]] : 1;
                }
                else
                {
                    s += Math.Max(1, j - badChar[text[s + j]]);
                }
            }

            return -1;
        }

        /// <summary>
        /// Find pattern in text, return list of start indices
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static List<int> Find(ReadOnlySpan<byte> text, ReadOnlySpan<byte> pattern, int maxOffset = -1)
        {
            var retVal = new List<int>();
            var m = pattern.Length;
            var n = text.Length;

            // make bad char table
            var badChar = new int[256];
            for (var i = 0; i < 256; i++)
                badChar[i] = -1;
            for (var i = 0; i < pattern.Length; i++)
                badChar[pattern[i]] = i;

            if (maxOffset == -1) maxOffset = Int32.MaxValue;

            var s = 0;
            while (s <= (n - m) && s < maxOffset)
            {
                var j = m - 1;

                while (j >= 0 && pattern[j] == text[s + j])
                    --j;

                if (j < 0)
                {
                    retVal.Add(s);
                    s += (s + m < n) ? m - badChar[text[s + m]] : 1;
                }
                else
                {
                    s += Math.Max(1, j - badChar[text[s + j]]);
                }
            }

            return retVal;
        }

    }

}
