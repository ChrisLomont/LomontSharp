using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lomont.Algorithms
{
    /// <summary>
    /// Aho-Corasick pattern matcher
    /// Finds all occurrences of a set of patterns in a text
    /// </summary>
    public static class AhoCorasick
    {
        const int EmptySet = -1;

        /// <summary>
        /// Execute search on all patterns
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <param name="patterns">Patterns to search</param>
        /// <param name="automaton">Optional automaton to speed up search, reusable between calls</param>
        /// <returns></returns>
        public static List<(int offset, int patternIndex)> Find(ReadOnlySpan<byte> text, List<byte[]> patterns, Automaton automaton = null)
        {
            var ans = new List<(int, int)>();
            if (patterns == null && automaton != null)
                patterns = automaton.patterns;
            automaton ??= new Automaton(patterns); // create if not given
            Func<int, int, int> g = automaton.G;
            var f = automaton.F;
            var out1 = automaton.Out;

            var q = 0; // start node
            for (var i = 0; i < text.Length; ++i)
            {
                while (g(q, text[i]) == EmptySet)
                    q = f[q]; // follow fail
                q = g(q, text[i]); // follow a goto
                if (out1[q].Any())
                {
                    ans.AddRange(out1[q].Select(patternIndex => (i - patterns[patternIndex].Length + 1, patternIndex)));
                }
            }

            return ans;
        }

        /// <summary>
        /// Automaton for executing Aho-Corasick search
        /// </summary>
        public class Automaton

        {
            public List<byte[]> patterns;

            /// <summary>
            /// Creates an automaton for a set of patterns
            /// </summary>
            /// <param name="patterns"></param>
            public Automaton(List<byte[]> patterns)
            {
                this.patterns = patterns;
                // 0 is start
                AddNode();


                // build trie of all patterns
                for (var patternIndex = 0; patternIndex < patterns.Count; ++patternIndex)
                {
                    var pattern = patterns[patternIndex];
                    var cur = 0;
                    for (var i = 0; i < pattern.Length; ++i)
                    {
                        var b = pattern[i];
                        var nxt = nodes[cur][b];
                        if (nxt == EmptySet) 
                        {   // add node for this path
                            nxt = nodes[cur][b] = nodes.Count;
                            AddNode();
                            if (i == pattern.Length - 1)
                                Out.Last().Add(patternIndex);
                        }
                        cur = nxt;
                    }
                }
                // finish function g
                for (var a = 0; a < 256; ++a)
                    if (G(0, a) == EmptySet)
                        nodes[0][a] = 0;

                // fill in fail function, depth first search over trie
                F = new int[nodes.Count]; // note zeroed by default
                var q = new Queue<int>();
                for (var a = 0; a < 256; ++a) // iterate over alphabet
                    if (G(0, a) != 0) q.Enqueue(G(0, a)); // all depth 1 node indices

                while (q.Count > 0)
                {
                    var r = q.Dequeue(); // get node index to inspect
                    for (var a = 0; a < 256; ++a) // iterate over alphabet
                    {
                        var u = G(r, a);
                        if (u != EmptySet)
                        {
                            q.Enqueue(u);
                            var v = F[r];
                            while (G(v, a) == EmptySet)
                                v = F[v];
                            F[u] = G(v, a);
                            // union them - todo - check if possible to have dups, if not, then simply AddRange
                            foreach (var n in Out[F[u]])
                                if (!Out[u].Contains(n))
                                    Out[u].Add(n);
                            // todo - sort u[i]? would make nicer outputs
                        }
                    }
                }
            }

            void AddNode()
            {
                var b = new int[256];
                for (var i = 0; i < b.Length; ++i)
                    b[i] = EmptySet;
                nodes.Add(b);
                Out.Add(new());
            }

            List<int[]> nodes = new ();

            public int[] F; // failure function

            // patterns matched at node v, by pattern index
            public List<List<int>> Out = new();

            // goto function
            public int G(int q, int a) => nodes[q][a];
        }

        // simple sanity check
        public static bool Check()
        {
            var patternTexts = new[] { "he","she","his","hers"};
            var pats = patternTexts.Select(s => Encoding.ASCII.GetBytes(s)).ToList();
            var t = new Automaton(pats);

            // now check structure:
            Trace.Assert(t.G(0, 'h') == 1);
            Trace.Assert(t.G(0, 's') == 3);
            // todo - all other g(0,a) = 0
            Trace.Assert(t.G(1, 'e') == 2);
            Trace.Assert(t.G(1, 'i') == 6);
            // todo - all other g(1,a) = 0, etc. for all
            Trace.Assert(t.G(2, 'r') == 8);
            Trace.Assert(t.G(3, 'h') == 4);
            Trace.Assert(t.G(4, 'e') == 5);
            Trace.Assert(t.G(6, 's') == 7);
            Trace.Assert(t.G(8, 's') == 9);

            Trace.Assert(t.Out[5][0] == 1); // order will change if out1 sorted
            Trace.Assert(t.Out[5][1] == 0);

            Trace.Assert(t.F[0] == 0);
            Trace.Assert(t.F[1] == 0);
            Trace.Assert(t.F[2] == 0);
            Trace.Assert(t.F[3] == 0);
            Trace.Assert(t.F[4] == 1);
            Trace.Assert(t.F[5] == 2);
            Trace.Assert(t.F[6] == 0);
            Trace.Assert(t.F[7] == 3);
            Trace.Assert(t.F[8] == 0);
            Trace.Assert(t.F[9] == 3);


            return true;
        }


    }

}
