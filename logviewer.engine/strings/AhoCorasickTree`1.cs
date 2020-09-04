using System.Collections.Generic;
using System.Linq;

namespace logviewer.engine.strings
{
    /// <summary>
    /// Represents Aho-Corasick algorithm implementation
    /// </summary>
    public class AhoCorasickTree<T> : AhoCorasickTree
    {
        private readonly Dictionary<string, List<T>> mapping;
        
        /// <summary>
        /// Initializes new algorithm instance using key-value pairs specified
        /// </summary>
        /// <param name="pairs">Patterns to search values by a string</param>
        public AhoCorasickTree(IEnumerable<KeyValuePair<string, T>> pairs) : base(pairs?.Select(x => x.Key))
        {
            if (pairs == null)
            {
                return;
            }

            this.mapping = new Dictionary<string, List<T>>();

            foreach (var p in pairs)
            {
                if (this.mapping.TryGetValue(p.Key, out var values))
                {
                    values.Add(p.Value);
                }
                else
                {
                    this.mapping[p.Key] = new List<T> { p.Value };
                }
            }
        }
        
        /// <summary>
        /// Finds all patterns values occurrences in the string specified
        /// </summary>
        /// <param name="text">string to search within</param>
        /// <returns>All found values</returns>
        public IEnumerable<T> FindAllValues(string text)
        {
            var patterns = this.FindAll(text);

            foreach (var pattern in patterns)
            {
                if (!this.mapping.TryGetValue(pattern, out var values))
                {
                    continue;
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var j = 0; j < values.Count; j++)
                {
                    yield return values[j];                            
                }
            }
        }
    }
}
