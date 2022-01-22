using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Lomont.Formats;

namespace TestLomontSharp
{
    class TestTreeTextFormatter
    {
        public class Node1
        {
            public Node1(string name, params Node1[] children)
            {
                Name = name;
                foreach (var child in children)
                    Children.Add(child);
            }
            public string Name;
            public List<Node1> Children = new List<Node1>();
        }

        [Test]

        public void TestFormat()
        {
            var tree = new Node1(
                "0",
                new Node1("1",
                    new Node1("1.1"),
                    new Node1("1.2",
                        new Node1("1.2.1")
                    ),
                    new Node1("1.3")
                ),
                new Node1("2"),
                new Node1("3",
                    new Node1("3.1",
                        new Node1("3.1.1"),
                        new Node1("3.1.2",
                            new Node1("3.1.2.1",
                                new Node1("3.1.2.1.1",
                                    new Node1("3.1.2.1.1.1")
                                )
                            ),
                            new Node1("3.1.3")
                        ),
                        new Node1("3.2")
                    )
                ),
                new Node1("4")
            );

            var sw1 = new StringWriter();
            TreeTextFormatter.Format(
                sw1, tree,
                    n => n.Children,
                    n => n.Name,
                    TreeTextFormatter.Style.Ascii
                );
            Assert.AreEqual(sw1.ToString(),
@"0
 +--1
 |   +--1.1
 |   +--1.2
 |   |   \--1.2.1
 |   \--1.3
 +--2
 +--3
 |   \--3.1
 |       +--3.1.1
 |       +--3.1.2
 |       |   +--3.1.2.1
 |       |   |   \--3.1.2.1.1
 |       |   |       \--3.1.2.1.1.1
 |       |   \--3.1.3
 |       \--3.2
 \--4
"
                );

            var sw2 = new StringWriter();
            TreeTextFormatter.Format(
                sw2, tree,
                    n => n.Children,
                    n => n.Name,
                    TreeTextFormatter.Style.Unicode
                );
            Assert.AreEqual(sw2.ToString(),
@"0
 ├─1
 │  ├─1.1
 │  ├─1.2
 │  │  └─1.2.1
 │  └─1.3
 ├─2
 ├─3
 │  └─3.1
 │     ├─3.1.1
 │     ├─3.1.2
 │     │  ├─3.1.2.1
 │     │  │  └─3.1.2.1.1
 │     │  │     └─3.1.2.1.1.1
 │     │  └─3.1.3
 │     └─3.2
 └─4
");

        }
    }
}
