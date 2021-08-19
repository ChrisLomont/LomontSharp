using System;
using System.Collections.Generic;

namespace Lomont.Stats
{
    /// <summary>
    /// Track counts of which things appear with which things
    /// </summary>
    public class CorrelationGrid
    {
        Dictionary<string, Dictionary<string, int>> counts = new ();
        public void Add(string key1, string key2)
        {
            // keep upper triangular via sorting keys for space
            if (0 < String.Compare(key1, key2, StringComparison.Ordinal))
            {
                var temp = key1;
                key1 = key2;
                key2 = temp;
            }


            if (!counts.ContainsKey(key1))
                counts.Add(key1,new());
            if (!counts[key1].ContainsKey(key2))
                counts[key1].Add(key2,0);
            counts[key1][key2]++;
        }

    }
}
