using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace TestLomontSharp
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void RunAThing()
        {
            return;

            // count common code files
            Dictionary<string, int> counts = new();
            var t = @"C:\Users\Chris\OneDrive\Code";
            Recurse(t);

            using var outf = File.CreateText(t + "\\" + "AllSharp.txt");

            foreach (var p in counts.OrderBy(p=>p.Value))
            {
                outf.WriteLine($"{p.Value}: {p.Key}");
            }
            outf.WriteLine("THE END");

            void Recurse(string path)
            {
                foreach (var f in Directory.EnumerateFiles(path,"*.cs"))
                {
                    var fn = Path.GetFileName(f);
                    if (!counts.ContainsKey(fn))
                        counts.Add(fn, 0);
                    counts[fn]++;
                }
                foreach (var d in Directory.EnumerateDirectories(path))
                {
                    Recurse(d);
                }
            }


        }

    }
}