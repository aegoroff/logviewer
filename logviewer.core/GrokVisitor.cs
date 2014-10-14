// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace logviewer.core
{
    public class GrokVisitor : GrokBaseVisitor<string>
    {
        public GrokVisitor()
        {
            const string localPath = "grok.patterns";
            var fullPath = Path.Combine(Extensions.AssemblyDirectory, localPath);

            var patterns = File.ReadAllLines(File.Exists(fullPath) ? fullPath : localPath);
            foreach (var pattern in patterns)
            {
                var parts = pattern.Split(new[] { ' ' }, StringSplitOptions.None);
                if (parts.Length < 2)
                {
                    continue;
                }
                var template = parts[0];
                if (string.IsNullOrWhiteSpace(template) || template.StartsWith("#") || templates.ContainsKey(template))
                {
                    continue;
                }
                templates.Add(template, pattern.Substring(template.Length).Trim());
            }
        }
        
        private readonly StringBuilder stringBuilder = new StringBuilder();

        private const string PatternStart = "%{";
        private const string PatternStop = "}";
        const string NamedPattern = @"(?<{0}>{1})";

        readonly Dictionary<string, string> templates = new Dictionary<string, string>();

        readonly List<Semantic> semantics = new List<Semantic>();
        string regexp;
        string semantic;

        public string Template
        {
            get { return this.stringBuilder.ToString(); }
        }

        public IEnumerable<Semantic> Semantics
        {
            get { return this.semantics; }
        }

        private void AddSemantic(Rule rule)
        {
            var s = new Semantic(this.semantic, rule);
            AddSemantic(s);
        }
        
        private void AddSemantic(Semantic s)
        {
            this.semantics.Add(s);
            this.regexp = string.Format(NamedPattern, this.semantic, this.regexp);
            this.stringBuilder.Append(this.regexp);
        }

        public override string VisitOnCastingCustomRule(GrokParser.OnCastingCustomRuleContext context)
        {
            var pattern = context.QUOTED_STR().Symbol.Text.Trim('\'', '"');
            var value = context.target().GetText();
             this.semantics.Last().CastingRules.Add(new Rule(value, pattern));
            return this.VisitChildren(context);
        }

        public override string VisitOnCasting(GrokParser.OnCastingContext context)
        {
            if (context.TYPE_NAME() == null)
            {
                this.AddSemantic(new Rule());
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
            this.semantic = context.PROPERTY().GetText();

            if (context.casting() == null)
            {
                this.AddSemantic(new Semantic(this.semantic));
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

        public override string VisitOnLiteral(GrokParser.OnLiteralContext context)
        {
            var raw = context.GetText();
            if (raw.Length > 1 && (raw.StartsWith("'") && raw.EndsWith("'") || raw.StartsWith("\"") && raw.EndsWith("\"")))
            {
                raw = raw.Substring(1, raw.Length - 2).Replace("\\\"", "\"").Replace("\\'", "'");
            }
            this.stringBuilder.Append(raw);
            return this.VisitChildren(context);
        }
    }
}