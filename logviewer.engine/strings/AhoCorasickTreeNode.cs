// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 26.04.2017
// © 2012-2018 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace logviewer.engine.strings
{
    [DebuggerDisplay("Value = {Value}, TransitionCount = {transitionsDictionary.Count}")]
    internal class AhoCorasickTreeNode
    {
        private readonly StringComparer comparer;
        public char Value { get; }

        public AhoCorasickTreeNode Failure { get; set; }

        private readonly HashSet<string> results;

        private readonly CharKeyDictionary<AhoCorasickTreeNode> transitionsDictionary;

        private readonly AhoCorasickTreeNode parent;

        public HashSet<string> Results => this.results;

        public AhoCorasickTreeNode ParentFailure => this.parent?.Failure;

        public IEnumerable<AhoCorasickTreeNode> Transitions => this.transitionsDictionary.Values;

        public AhoCorasickTreeNode(StringComparer comparer) : this(null, comparer, ' ')
        {
        }

        private AhoCorasickTreeNode(AhoCorasickTreeNode parent, StringComparer comparer, char value)
        {
            this.Value = value;
            this.parent = parent;
            this.comparer = comparer;

            this.results = new HashSet<string>(comparer);
            this.transitionsDictionary = new CharKeyDictionary<AhoCorasickTreeNode>();
        }

        public void AddResult(string result)
        {
            this.results.Add(result);
        }

        public void AddResults(IEnumerable<string> items)
        {
            foreach (var result in items)
            {
                this.AddResult(result);
            }
        }

        public AhoCorasickTreeNode AddTransition(char c)
        {
            var node = new AhoCorasickTreeNode(this, this.comparer, c);
            this.transitionsDictionary.Add(node.Value, node);

            return node;
        }

        public AhoCorasickTreeNode GetTransition(char c)
        {
            this.transitionsDictionary.TryGetValue(c, out AhoCorasickTreeNode r);
            return r;
        }

        public bool ContainsTransition(char c) => this.transitionsDictionary.ContainsKey(c);
    }
}
