using System;

namespace Lomont.Algorithms
{
    /// <summary>
    /// Common string distance metrics
    /// </summary>
    public static class StringDistance
    {
        /// <summary>
        /// Hamming distance, i.e., number of non-equal positions
        /// </summary>
        public static int Hamming(string s, string t) => Hamming(s.AsSpan(), t.AsSpan());

        /// <summary>
        /// Hamming distance, i.e., number of non-equal positions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int Hamming<T>(ReadOnlySpan<T> s, ReadOnlySpan<T> t) where T : IComparable<T>
        {
            if (s.Length != t.Length)
            {
                throw new Exception("Sequences must be equal length");
            }

            var distance = 0;
            for (var i = 0; i < s.Length; ++i)
                distance += s[i].CompareTo(t[i]) == 0 ? 0 : 1;

            return distance;
        }

        /// <summary>
        /// Compute the edit distance between two strings.
        /// Levenshtein counts as 1 unit each of insert, delete, change
        /// </summary>
        public static int Levenshtein(string s, string t) => Levenshtein(s.AsSpan(), t.AsSpan());

        /// <summary>
        /// Levenshtein in 0-1
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double LevenshteinSimilarity(string s, string t)
        {
            if (s == t) return 1.0;
            var d = Levenshtein(s.AsSpan(), t.AsSpan());
            return 1.0 - (double)d / Math.Max(s.Length,t.Length);
        }


        /// <summary>
        /// Compute the edit distance between two sequences.
        /// Levenshtein counts as 1 unit each of insert, delete, change
        /// </summary>
        public static int Levenshtein<T>(ReadOnlySpan<T> s, ReadOnlySpan<T> t) where T : IComparable<T>
        {
            // todo - rewrite using only one or two rows, also return sequence, order accesses for speed
            var (h, w) = (s.Length, t.Length);

            // empty cases
            if (h == 0) return w;
            if (w == 0) return h;

            var d = new int[h + 1, w + 1];

            // edge cases
            for (var i = 0; i <= h; i++) d[i, 0] = i;
            for (var j = 0; j <= w; j++) d[0, j] = j;

            // iterate over grid
            for (var i = 1; i <= h; i++)
            for (var j = 1; j <= w; j++)
            {
                var cost = (t[j - 1].CompareTo(s[i - 1]) == 0) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }

            // cost is lower right corner
            return d[h, w];
        }

        /// <summary>
        /// Compute the edit distance between two strings.
        /// DamerauLevenshtein counts as 1 unit each of insert, delete, change, transpose adjacent
        /// </summary>
        public static int DamerauLevenshtein(string s, string t) => DamerauLevenshtein(s.AsSpan(), t.AsSpan());

        /// <summary>
        /// Compute the edit distance between two sequences.
        /// Levenshtein counts as 1 unit each of insert, delete, change
        /// </summary>
        public static int DamerauLevenshtein<T>(ReadOnlySpan<T> s, ReadOnlySpan<T> t) where T : IComparable<T>
        {
            // todo - rewrite using only one or two rows, also return sequence, order accesses for speed

            var (h, w) = (s.Length, t.Length);
            var d = new int[h + 1, w + 1];

            for (var i = 0; i <= h; i++) d[i, 0] = i;
            for (var j = 0; j <= w; j++) d[0, j] = j;

            for (var j = 1; j <= h; j++)
            for (var i = 1; i <= w; i++)
            {
                var cost = (s[j - 1].CompareTo(t[i - 1]) == 0) ? 0 : 1;
                var insertion = d[j, i - 1] + 1;
                var deletion = d[j - 1, i] + 1;
                var substitution = d[j - 1, i - 1] + cost;

                var distance = Math.Min(insertion, Math.Min(deletion, substitution));

                if (j > 1 && i > 1 && s[j - 1].CompareTo(t[i - 2]) == 0 && s[j - 2].CompareTo(t[i - 1]) == 0)
                        distance = Math.Min(distance, d[j - 2, i - 2] + cost);

                d[j, i] = distance;
            }

            return d[h, w];
        }


        public static (int, string) LongestCommonSubsequence(string s, string t)
        {
            var (c, ans) = LongestCommonSubsequence(s.AsSpan(), t.AsSpan());
            return (c, new string(ans));
        }


        public static (int, T[]) LongestCommonSubsequence<T>(ReadOnlySpan<T> s, ReadOnlySpan<T> t)
            where T : IComparable<T>
        {
            // todo - rewrite using only one or two rows, also return sequence, order accesses for speed

            int w = s.Length;
            int h = t.Length;
            var d = new int[(w + 1), (h + 1)];

            for (var i = 1; i <= w; ++i)
            for (var j = 1; j <= h; ++j)
            {
                if (s[i - 1].CompareTo(t[j - 1]) == 0)
                    d[i, j] = d[i - 1, j - 1] + 1;
                else
                    d[i, j] = Math.Max(d[i - 1, j], d[i, j - 1]);
            }

            var tt = d[w, h];
            T[] output = new T[tt];

            for (int i = w, j = h, k = tt - 1; k >= 0; /* nothing*/)
            {
                if (s[i - 1].CompareTo(t[j - 1]) == 0)
                {
                    output[k] = s[i - 1];
                    --i;
                    --j;
                    --k;
                }
                else if (d[i, j - 1] > d[i - 1, j])
                    --j;
                else
                    --i;
            }

            return (tt, output);

        }

        /// <summary>
        /// Jaro similarity of two strings.
        /// 1.0 means equal, else down to 0 = nothing alike
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static double JaroSimilarity(string s1, string s2)
        {
            // If the strings are equal
            if (s1 == s2)
                return 1.0;

            // Length of two strings
            int len1 = s1.Length,
                len2 = s2.Length;

            if (len1 == 0 || len2 == 0)
                return 0.0;

            // Maximum distance upto which matching
            // is allowed
            int max_dist = (int)Math.Floor((double)
                Math.Max(len1, len2) / 2) - 1;

            // Count of matches
            int match = 0;

            // Hash for matches
            int[] hash_s1 = new int[s1.Length];
            int[] hash_s2 = new int[s2.Length];

            // Traverse through the first string
            for (int i = 0; i < len1; i++)
            {

                // Check if there is any matches
                for (int j = Math.Max(0, i - max_dist);
                        j < Math.Min(len2, i + max_dist + 1);
                        j++)

                    // If there is a match
                    if (s1[i] == s2[j] &&
                        hash_s2[j] == 0)
                    {
                        hash_s1[i] = 1;
                        hash_s2[j] = 1;
                        match++;
                        break;
                    }
            }

            // If there is no match
            if (match == 0)
                return 0.0;

            // Number of transpositions
            double t = 0;

            int point = 0;

            // Count number of occurrences
            // where two characters match but
            // there is a third matched character
            // in between the indices
            for (int i = 0; i < len1; i++)
                if (hash_s1[i] == 1)
                {

                    // Find the next matched character
                    // in second string
                    while (hash_s2[point] == 0)
                        point++;

                    if (s1[i] != s2[point++])
                        t++;
                }

            t /= 2;

            // Return the Jaro Similarity
            return (((double)match) / ((double)len1)
                    + ((double)match) / ((double)len2)
                    + ((double)match - t) / ((double)match))
                   / 3.0;
        }

        /// <summary>
        /// Jaro-Winkler string similarity
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static double JaroWinklerSimilarity(string s1, string s2)
        {
            double jaro_dist = JaroSimilarity(s1, s2);

            // If the jaro Similarity is above a threshold
            if (jaro_dist > 0.7)
            {

                // Find the length of common prefix
                int prefix = 0;

                for (int i = 0;
                    i < Math.Min(s1.Length,
                        s2.Length);
                    i++)
                {

                    // If the characters match
                    if (s1[i] == s2[i])
                        prefix++;

                    // Else break
                    else
                        break;
                }

                // Maximum of 4 characters are allowed in prefix
                prefix = Math.Min(4, prefix);

                // Calculate jaro winkler Similarity
                jaro_dist += 0.1 * prefix * (1 - jaro_dist);
            }

            return jaro_dist;
        }
    }
}
