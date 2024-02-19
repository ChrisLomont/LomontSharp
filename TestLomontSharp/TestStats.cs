using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Stats;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    public class TestStats
    {

        [Test]
        public void TestBasics()
        {
            var pts = new List<double> {1,3,4,5,7,9,11 };
            
            ClassicAssert.That(Stats.Mean(pts),Is.EqualTo(5.71429).Within(0.0001));
            
            ClassicAssert.AreEqual(Stats.Median(pts), 5);

            ClassicAssert.That(Stats.SampleVariance(pts), Is.EqualTo(12.2381).Within(0.0001));

            ClassicAssert.That(Stats.PopulationVariance(pts), Is.EqualTo(10.4898).Within(0.0001));

            ClassicAssert.That(Stats.SampleStdDev(pts), Is.EqualTo(3.4983).Within(0.0001));

            ClassicAssert.That(Stats.PopulationStdDev(pts), Is.EqualTo(3.2388).Within(0.0001));


        }
    }
}
