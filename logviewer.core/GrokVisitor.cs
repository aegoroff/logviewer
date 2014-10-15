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

        private readonly StringBuilder stringBuilder = new StringBuilder();

        private const string PatternStart = "%{";
        private const string PatternStop = "}";
        const string NamedPattern = @"(?<{0}>{1})";

        readonly Dictionary<string, string> templates = new Dictionary<string, string>();

        readonly List<Semantic> schema = new List<Semantic>();
        string regexp;
        string property;

        public string Template
        {
            get { return this.stringBuilder.ToString(); }
        }

        public bool RecompilationNeeded { get;private set ;}

        public ICollection<Semantic> Schema
        {
            get { return this.schema; }
        }

        private void AddSemantic(Rule rule)
        {
            var s = new Semantic(this.property, rule);
            AddSemantic(s);
        }
        
        private void AddSemantic(Semantic s)
        {
            this.schema.Add(s);
            this.regexp = string.Format(NamedPattern, this.property, this.regexp);
            this.stringBuilder.Append(this.regexp);
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
            if (this.regexp == null)
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
            string node = context.PATTERN().Symbol.Text;

            Log.Instance.TraceFormatted(node);
            if (templates.ContainsKey(node))
            {
                this.regexp = templates[node];

                bool continueMatch;
                bool matchFound;
                do
                {
                    matchFound = false;
                    foreach (string k in templates.Keys)
                    {
                        string link = PatternStart + k + PatternStop;
                        if (!this.regexp.Contains(link))
                        {
                            continue;
                        }
                        this.regexp = this.regexp.Replace(link, templates[k]);
                        matchFound = true;
                    }
                    continueMatch = this.regexp.Contains(PatternStart);
                } while (continueMatch && matchFound);

                if (!this.RecompilationNeeded)
                {
                    var r = new Regex("(" + PatternStart + @"[^:]+?:\S+?\" + PatternStop + ")");
                    var m = r.Match(this.regexp);
                    this.RecompilationNeeded = m.Success;

                    if (this.RecompilationNeeded)
                    {
                        var ix = 0;
                        var sb = new StringBuilder();
                        do
                        {
                            var capture = m.Groups[1];
                            if (capture.Index > 0)
                            {
                                var substr = this.regexp.Substring(ix, capture.Index - ix);
                                Escape(sb, substr);
                            }
                            ix = capture.Index + capture.Length;
                            sb.Append(capture.Value);
                            m = m.NextMatch();
                            if (m.Success)
                            {
                                continue;
                            }
                            var tailLength = this.regexp.Length - ix;
                            if (tailLength <= 0)
                            {
                                continue;
                            }
                            var tail = this.regexp.Substring(ix, tailLength);
                            Escape(sb, tail);
                        } while (m.Success);
                        this.regexp = sb.ToString();
                    }
                }
                
                // Semantic handlers do it later but without semantic it MUST BE done here
                if (context.semantic() == null)
                {
                    this.stringBuilder.Append(this.regexp);
                }
            }
            else
            {
                this.regexp = null;
                this.stringBuilder.Append(PatternStart);
                this.stringBuilder.Append(node);
                this.stringBuilder.Append(PatternStop);
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
            this.stringBuilder.Append(raw.UnescapeString());
            return this.VisitChildren(context);
        }
    }
}