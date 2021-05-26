// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 26.04.2017
// © 2012-2018 Alexander Egorov

using System;
using System.Collections.Generic;

namespace logviewer.engine.strings
{
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
        /// <param name="comparer">String comparer</param>
        public AhoCorasickTree(IEnumerable<string> keywords, StringComparer comparer)
        {
            this.Root = new AhoCorasickTreeNode(comparer);

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

                foreach (var result in pointer.Results)
                {
                    yield return result;
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
