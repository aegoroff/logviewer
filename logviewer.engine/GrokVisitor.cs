// Created by: egr
// Created at: 02.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using logviewer.engine.Tree;

namespace logviewer.engine
{
    internal class GrokVisitor : GrokBaseVisitor<string>
    {
        private readonly Dictionary<string, string> templates = new Dictionary<string, string>();
        private readonly List<Semantic> schema = new List<Semantic>();
        private readonly Composer composer = new Composer();
        private readonly List<int> recompileIndexes = new List<int>();
        private readonly Dictionary<int, string> recompileProperties = new Dictionary<int, string>();
        private string compiledPattern;
        private string property;
        private bool doNotWrapCurrentIntoNamedMatchGroup;
        readonly BinaryTree<string> tree = new BinaryTree<string>();
        private BinaryTreeNode<string> lastNode;


        internal GrokVisitor()
        {
            this.tree.Root = new BinaryTreeNode<string>(string.Empty);
            this.lastNode = this.tree.Root;
            const string pattern = "*.patterns";
            var patternFiles = Directory.GetFiles(Extensions.AssemblyDirectory, pattern, SearchOption.TopDirectoryOnly);
            if (patternFiles.Length == 0)
            {
                patternFiles = Directory.GetFiles(".", pattern, SearchOption.TopDirectoryOnly);
            }
            foreach (var file in patternFiles)
            {
                this.AddTemplates(file);
            }
        }

        private void AddTemplates(string fullPath)
        {
            var patterns = File.ReadAllLines(fullPath);
            foreach (var pattern in patterns)
            {
                var parts = pattern.Split(new[] { ' ' }, StringSplitOptions.None);
                if (parts.Length < 2)
                {
                    continue;
                }
                var template = parts[0];
                if (string.IsNullOrWhiteSpace(template) || template.StartsWith("#") || this.templates.ContainsKey(template))
                {
                    continue;
                }
                this.templates.Add(template, pattern.Substring(template.Length).Trim());
            }
        }

        internal string Template
        {
            get { return this.composer.Content; }
        }

        internal bool RecompilationNeeded { get; private set; }

        internal ICollection<Semantic> Schema
        {
            get { return this.schema; }
        }

        internal IList<int> RecompileIndexes
        {
            get { return this.recompileIndexes; }
        }

        internal string GetRecompile(int ix)
        {
            return this.composer.GetPattern(ix);
        }

        internal void SetRecompiled(int ix, string recompiled)
        {
            var p = this.recompileProperties.ContainsKey(ix)
                ? (IPattern) new NamedPattern(this.recompileProperties[ix], recompiled)
                : new Pattern(recompiled);
            this.composer.SetPattern(ix, p);
        }

        private void AddSemantic(Rule rule)
        {
            var s = new Semantic(this.property, rule);
            AddSemantic(s);
        }
        
        private void AddSemantic(Semantic s)
        {
            this.schema.Add(s);

            var p = this.doNotWrapCurrentIntoNamedMatchGroup
                ? new Pattern(this.compiledPattern)
                : (IPattern)new NamedPattern(this.property, this.compiledPattern);
            this.compiledPattern = p.Content;
            this.composer.Add(p);
            if (!this.doNotWrapCurrentIntoNamedMatchGroup)
            {
                return;
            }
            this.AddRecompileIndex(this.property);
        }

        private void AddRecompileIndex(string prop = null)
        {
            var recompileIx = this.composer.Count - 1;
            this.recompileIndexes.Add(recompileIx);
            prop.Do(s => this.recompileProperties.Add(recompileIx, s));
        }

        public override string VisitOnCastingCustomRule(GrokParser.OnCastingCustomRuleContext context)
        {
            var pattern = context.QUOTED_STR().Symbol.Text.UnescapeString();
            var value = context.target().GetText();
             this.schema.Last().CastingRules.Add(new Rule(value, pattern));
            return this.VisitChildren(context);
        }

        public override string VisitOnCasting(GrokParser.OnCastingContext context)
        {
            if (context.TYPE_NAME() == null)
            {
                this.AddSemantic(new Semantic(this.property));
                return this.VisitChildren(context);
            }
            var typeName = context.TYPE_NAME().Symbol.Text;

            this.AddSemantic(new Rule(typeName));

            return this.VisitChildren(context);
        }

        public override string VisitOnSemantic(GrokParser.OnSemanticContext context)
        {
            if (this.compiledPattern == null)
            {
                return this.VisitChildren(context);
            }
            this.property = context.PROPERTY().GetText();

            if (context.casting() == null)
            {
                this.AddSemantic(new Semantic(this.property, new Rule("string")));
            }
            return this.VisitChildren(context);
        }

        public override string VisitOnRule(GrokParser.OnRuleContext context)
        {
            var node = context.PATTERN().Symbol.Text;
            
            var ruleNode = new BinaryTreeNode<string>(node);
            this.lastNode.Right = ruleNode;
            this.lastNode = ruleNode;

            if (!this.templates.ContainsKey(node))
            {
                this.compiledPattern = null;
                this.composer.Add(new PassthroughPattern(node));
            }
            else
            {
                this.compiledPattern = this.templates[node];

                bool continueMatch;
                bool matchFound;
                do
                {
                    matchFound = false;
                    foreach (var k in this.templates.Keys)
                    {
                        var link = new PassthroughPattern(k).Content;
                        if (!this.compiledPattern.Contains(link))
                        {
                            continue;
                        }
                        this.compiledPattern = this.compiledPattern.Replace(link, this.templates[k]);
                        matchFound = true;
                    }
                    continueMatch = this.compiledPattern.Contains(PassthroughPattern.Start);
                } while (continueMatch && matchFound);


                var r = new Regex(@"(%\{[^:]+?:\S+?\})");
                var m = r.Match(this.compiledPattern);

                this.doNotWrapCurrentIntoNamedMatchGroup = m.Success;
                this.RecompilationNeeded |= this.doNotWrapCurrentIntoNamedMatchGroup;


                if (this.doNotWrapCurrentIntoNamedMatchGroup)
                {
                    var ix = 0;
                    var sb = new StringBuilder();
                    do
                    {
                        var capture = m.Groups[1];
                        if (capture.Index > 0)
                        {
                            var substr = this.compiledPattern.Substring(ix, capture.Index - ix);
                            Escape(sb, substr);
                        }
                        ix = capture.Index + capture.Length;
                        sb.Append(capture.Value);
                        m = m.NextMatch();
                        if (m.Success)
                        {
                            continue;
                        }
                        var tailLength = this.compiledPattern.Length - ix;
                        if (tailLength <= 0)
                        {
                            continue;
                        }
                        var tail = this.compiledPattern.Substring(ix, tailLength);
                        Escape(sb, tail);
                    } while (m.Success);
                    this.compiledPattern = sb.ToString();
                }

                // Semantic handlers do it later but without semantic it MUST BE done here
                if (context.semantic() == null)
                {
                    this.composer.Add(new Pattern(this.compiledPattern));
                    if (this.RecompilationNeeded)
                    {
                        this.AddRecompileIndex();
                    }
                }
            }
            return this.VisitChildren(context);
        }

        private static void Escape(StringBuilder sb, string str)
        {
            sb.Append("'");
            sb.Append(str.Replace("'", @"\'"));
            sb.Append("'");
        }

        public override string VisitOnLiteral(GrokParser.OnLiteralContext context)
        {
            var literal = new StringLiteral(context.GetText());
            this.composer.Add(literal);

            var literalNode = new BinaryTreeNode<string>(literal.Content);
            this.lastNode.Left = literalNode;
            this.lastNode = literalNode;
            return this.VisitChildren(context);
        }
    }
}