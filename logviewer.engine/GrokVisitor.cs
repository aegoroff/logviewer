// Created by: egr
// Created at: 02.10.2014
// © 2012-2015 Alexander Egorov

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using logviewer.engine.Tree;

namespace logviewer.engine
{
    internal class GrokVisitor : GrokBaseVisitor<string>
    {
        private readonly IDictionary<string, string> templates;
        private readonly List<Semantic> schema = new List<Semantic>();
        private readonly Composer composer = new Composer();
        private readonly List<int> recompileIndexes = new List<int>();
        private readonly Dictionary<int, string> recompileProperties = new Dictionary<int, string>();
        private string compiledPattern;
        private string property;
        private bool doNotWrapCurrentIntoNamedMatchGroup;
        readonly BinaryTree<Pattern> tree = new BinaryTree<Pattern>();
        private BinaryTreeNode<Pattern> lastNode;


        internal GrokVisitor(IDictionary<string, string> templates)
        {
            this.templates = templates;
            this.tree.Root = new BinaryTreeNode<Pattern>(new StringLiteral(string.Empty));
            this.lastNode = this.tree.Root;
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

            if (!this.templates.ContainsKey(node))
            {
                this.compiledPattern = null;
                
                var pattern = new PassthroughPattern(node);
                
                var ruleNode = new BinaryTreeNode<Pattern>(pattern);
                this.lastNode.Left = ruleNode;
                this.lastNode = ruleNode;

                this.composer.Add(pattern);
            }
            else
            {
                // Rule needs rewinding
                this.compiledPattern = this.templates[node];

                bool continueMatch;
                bool matchFound;
                do
                {
                    matchFound = false;
                    foreach (var k in this.templates.Keys)
                    {
                        var pp = new PassthroughPattern(k);

                        var link = pp.Content;
                        if (!this.compiledPattern.Contains(link))
                        {
                            continue;
                        }
                        var ruleNode = new BinaryTreeNode<Pattern>(pp);
                        this.lastNode.Right = ruleNode;
                        this.lastNode = ruleNode;

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
                    var p = new Pattern(this.compiledPattern);
                    var ruleNode = new BinaryTreeNode<Pattern>(p);
                    this.lastNode.Right = ruleNode;
                    this.lastNode = ruleNode;

                    this.composer.Add(p);

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

            var literalNode = new BinaryTreeNode<Pattern>(literal);
            this.lastNode.Left = literalNode;
            this.lastNode = literalNode;
            return this.VisitChildren(context);
        }
    }
}