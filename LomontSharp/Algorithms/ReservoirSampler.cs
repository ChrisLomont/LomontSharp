using System;
using System.Collections.Generic;

namespace Lomont.Algorithms
{
    /// <summary>
    /// Sample N items uniformly from a possibly large stream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReservoirSampler<T>
    {

        public ReservoirSampler(int sampleCount = 100, int randomSeed = 1234)
        {
            this.sampleCount = sampleCount;
            samplesProcessed = 0;
            rand = new Random(1234);
        }

        readonly int sampleCount;
        readonly Random rand;
        int samplesProcessed;

        /// <summary>
        /// Walk the source item, and gather samples
        /// </summary>
        /// <param name="source"></param>
        public List<T> Sample(IEnumerable<T> source)
        {
            Samples.Clear();
            samplesProcessed = 0;
            foreach (var s in source)
                ProcessItem(s);
            return Samples;
        }


        /// <summary>
        /// Process an item
        /// Return true if it was selected for inclusion
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns></returns>
        public bool ProcessItem(T item)
        {
            // i = number read from source
            ++samplesProcessed;

            if (Samples.Count < sampleCount)
            {
                Samples.Add(item);
                return true;
            }

            var j = rand.Next(0, samplesProcessed); // returns 0 to (i-1) inclusive
            if (j < sampleCount)
            {
                Samples[j] = item;
                return true;
            }
            return false; // not added
        }


        /// <summary>
        /// Current sample
        /// </summary>
        public List<T> Samples { get;  }= new();



    }
}
