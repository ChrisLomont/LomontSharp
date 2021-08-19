using System.Collections.Generic;

namespace Lomont.Stats
{
    /// <summary>
    /// Tally items by type
    /// </summary>
    /// <typeparam name="T">Type of things to count</typeparam>
    public class Tally<T> : Dictionary<T, long>
    {
        /// <summary>
        /// Add an item to tally
        /// </summary>
        /// <param name="key"></param>
        public void Add(T key)
        {
            if (!this.ContainsKey(key))
                this.Add(key, 0);
            this[key]++;
        }
    }
}
