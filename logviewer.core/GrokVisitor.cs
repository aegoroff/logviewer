// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace logviewer.core
{
    public class GrokVisitor : GrokBaseVisitor<string>
    {
        private readonly Dictionary<string, string> templates = new Dictionary<string, string>();
        private readonly List<Semantic> schema = new List<Semantic>();
        private readonly List<string> agregattor = new List<string>();
        private readonly List<int> recompileIndexes = new List<int>();
        private readonly Dictionary<int, string> recompileProperties = new Dictionary<int, string>();
        private string compiledPattern;
        private string property;
        private bool doNotWrapCurrentIntoNamedMatchGroup;

        private const string PatternStart = "%{";
        private const string PatternStop = "}";
        private const string NamedPattern = @"(?<{0}>{1})";
        
        public GrokVisitor()
        {
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

        public string Template
        {
            get { return string.Join(string.Empty, this.agregattor); }
        }

        public bool RecompilationNeeded { get; private set; }

        public ICollection<Semantic> Schema
        {
            get { return this.schema; }
        }

        internal IList<int> RecompileIndexes
        {
            get { return this.recompileIndexes; }
        }

        internal string GetRecompile(int ix)
        {
            return this.agregattor[ix];
        }

        internal void SetRecompiled(int ix, string recompiled)
        {
            this.agregattor[ix] = this.recompileProperties.ContainsKey(ix)
                ? string.Format(NamedPattern, this.recompileProperties[ix], recompiled)
                : recompiled;
        }

        private void AddSemantic(Rule rule)
        {
            var s = new Semantic(this.property, rule);
            AddSemantic(s);
        }
        
        private void AddSemantic(Semantic s)
        {
            this.schema.Add(s);

            this.compiledPattern = this.doNotWrapCurrentIntoNamedMatchGroup
                ? this.compiledPattern
                : string.Format(NamedPattern, this.property, this.compiledPattern);
            this.agregattor.Add(this.compiledPattern);
            if (!this.doNotWrapCurrentIntoNamedMatchGroup)
            {
                return;
            }
            this.AddRecompileIndex(this.property);
        }

        private void AddRecompileIndex(string prop = null)
        {
            var recompileIx = this.agregattor.Count - 1;
            this.recompileIndexes.Add(recompileIx);
            if (prop != null)
            {
                this.recompileProperties.Add(recompileIx, prop);
            }
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
                this.AddSemantic(new Semantic(this.property));
            }
            return this.VisitChildren(context);
        }

        public override string VisitOnDefinition(GrokParser.OnDefinitionContext context)
        {
            var node = context.PATTERN().Symbol.Text;

            if (templates.ContainsKey(node))
            {
                this.compiledPattern = templates[node];

                bool continueMatch;
                bool matchFound;
                do
                {
                    matchFound = false;
                    foreach (var k in templates.Keys)
                    {
                        var link = PatternStart + k + PatternStop;
                        if (!this.compiledPattern.Contains(link))
                        {
                            continue;
                        }
                        this.compiledPattern = this.compiledPattern.Replace(link, templates[k]);
                        matchFound = true;
                    }
                    continueMatch = this.compiledPattern.Contains(PatternStart);
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
                    this.agregattor.Add(this.compiledPattern);
                    if (this.RecompilationNeeded)
                    {
                        this.AddRecompileIndex();
                    }
                }
            }
            else
            {
                this.compiledPattern = null;
                this.agregattor.Add(PatternStart);
                this.agregattor.Add(node);
                this.agregattor.Add(PatternStop);
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
            var raw = context.GetText();
            this.agregattor.Add(raw.UnescapeString());
            return this.VisitChildren(context);
        }
    }
}