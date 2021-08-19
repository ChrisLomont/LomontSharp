using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomont.Stats
{
    public class Histogram
    {
        // track how many times each input sample is seen
        readonly Dictionary<int, int> counts = new();

        /// <summary>
        /// Add a sample
        /// </summary>
        /// <param name="sample"></param>
        public void AddSample(int sample)
        {
            if (!counts.ContainsKey(sample))
                counts.Add(sample, 0);
            counts[sample]++;
            TotalSamples += sample;
            TotalCounts++;
        }

        /// <summary>
        /// Number of distinct entries
        /// </summary>
        public int DistinctSamples => counts.Count;

        /// <summary>
        /// Sum of all counts
        /// </summary>
        public int TotalCounts { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public double AverageCount => (double)TotalCounts / DistinctSamples;
        public int MinCount => counts.Min(p => p.Value);
        public int MaxCount => counts.Max(p => p.Value);

        /// <summary>
        /// Sum of all samples (each times their count)
        /// </summary>
        public int TotalSamples { get; private set; }
        public double AverageSample => (double)TotalSamples / DistinctSamples;
        public int MinSample => counts.Min(p => p.Key);
        public int MaxSample => counts.Max(p => p.Key);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Count: [{MinCount},{MaxCount}: {AverageCount:F2}], Samples: [{MinSample},{MaxSample}: {AverageSample:F2}], ");
            foreach (var key in counts.Keys.OrderBy(v => v))
                sb.Append($"[{key},{counts[key]}] ");
            return sb.ToString();
        }


    }
}
