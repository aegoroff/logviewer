// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 26.04.2017
// © 2012-2018 Alexander Egorov

using System.Collections.Generic;

namespace logviewer.engine.strings
{
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
            var patterns = FindAll(text);

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
    
    /// <summary>
    /// Represents Aho-Corasick algorithm implementation
    /// </summary>
    public class AhoCorasickTree
    {
        internal AhoCorasickTreeNode Root { get; }

        /// <summary>
        /// Initializes new algorithm instance using keywords (patterns) specified
        /// </summary>
        /// <param name="keywords">Patterns to search in a string</param>
        public AhoCorasickTree(IEnumerable<string> keywords)
        {
            this.Root = new AhoCorasickTreeNode();

            if (keywords == null)
            {
                return;
            }

            foreach (var p in keywords)
            {
                this.AddPatternToTree(p);
            }

            this.SetFailureNodes();
        }

        /// <summary>
        /// Validates whether the string specified contains any pattern
        /// </summary>
        /// <param name="text">string to search within</param>
        /// <returns>True if the string contains any pattern. False otherwise</returns>
        public bool Contains(string text) => this.Contains(text, false);

        /// <summary>
        /// Validates whether the string specified starts with any pattern
        /// </summary>
        /// <param name="text">string to search within</param>
        /// <returns>True if the string starts with any pattern. False otherwise</returns>
        public bool ContainsThatStart(string text) => this.Contains(text, true);

        private bool Contains(string text, bool onlyStarts)
        {
            var pointer = this.Root;

            var len = text.Length * 2;
            var ix = 0;
            while (len > 0)
            {
                var c = text[ix++];
                len -= 2;

                var transition = this.GetTransition(c, ref pointer);

                if (transition != null)
                {
                    pointer = transition;
                }
                else if (onlyStarts)
                {
                    return false;
                }

                if (pointer.Results.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds all patterns occurrences in the string specified
        /// </summary>
        /// <param name="text">string to search within</param>
        /// <returns>All found patterns</returns>
        public IEnumerable<string> FindAll(string text)
        {
            var pointer = this.Root;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < text.Length; i++)
            {
                var transition = this.GetTransition(text[i], ref pointer);

                if (transition != null)
                {
                    pointer = transition;
                }

                var results = pointer.Results;

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var ix = 0; ix < results.Count; ix++)
                {
                    yield return results[ix];
                }
            }
        }

        private AhoCorasickTreeNode GetTransition(char c, ref AhoCorasickTreeNode pointer)
        {
            AhoCorasickTreeNode transition = null;
            while (transition == null)
            {
                transition = pointer.GetTransition(c);

                if (pointer == this.Root)
                {
                    break;
                }

                if (transition == null)
                {
                    pointer = pointer.Failure;
                }
            }

            return transition;
        }

        private void SetFailureNodes()
        {
            var nodes = this.FailToRootNode();
            this.FailUsingBfs(nodes);
            this.Root.Failure = this.Root;
        }

        private void AddPatternToTree(string pattern)
        {
            var node = this.Root;
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < pattern.Length; i++)
            {
                var c = pattern[i];
                node = node.GetTransition(c)
                       ?? node.AddTransition(c);
            }

            node.AddResult(pattern);
        }

        private List<AhoCorasickTreeNode> FailToRootNode()
        {
            var nodes = new List<AhoCorasickTreeNode>();
            foreach (var node in this.Root.Transitions)
            {
                node.Failure = this.Root;
                nodes.AddRange(node.Transitions);
            }

            return nodes;
        }

        private void FailUsingBfs(List<AhoCorasickTreeNode> nodes)
        {
            while (nodes.Count != 0)
            {
                var newNodes = new List<AhoCorasickTreeNode>();
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    var failure = node.ParentFailure;
                    var value = node.Value;

                    while (failure != null && !failure.ContainsTransition(value))
                    {
                        failure = failure.Failure;
                    }

                    if (failure == null)
                    {
                        node.Failure = this.Root;
                    }
                    else
                    {
                        node.Failure = failure.GetTransition(value);
                        node.AddResults(node.Failure.Results);
                    }

                    newNodes.AddRange(node.Transitions);
                }

                nodes = newNodes;
            }
        }
    }
}
