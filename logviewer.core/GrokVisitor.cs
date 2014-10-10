// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace logviewer.core
{
    public class GrokVisitor : GrokBaseVisitor<string>
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();

        private const string PatternStart = "%{";
        private const string PatternStop = "}";
        const string NamedPattern = @"(?<{0}>{1})";

        static readonly Dictionary<string, string> templates = new Dictionary<string, string>
        {
            { "USERNAME", "[a-zA-Z0-9._-]+" },
            { "WORD", @"\b\w+\b" },
            { "SPACE", @"\s*" },
            { "DATA", @".*?" },
            { "GREEDYDATA", @".*" },
            { "INT", @"(?:[+-]?(?:[0-9]+))" },
            { "BASE10NUM", @"(?<![0-9.+-])(?>[+-]?(?:(?:[0-9]+(?:\.[0-9]+)?)|(?:\.[0-9]+)))" },
            { "BASE16NUM", @"(?<![0-9A-Fa-f])(?:[+-]?(?:0x)?(?:[0-9A-Fa-f]+))" },
            { "BASE16FLOAT", @"\b(?<![0-9A-Fa-f.])(?:[+-]?(?:0x)?(?:(?:[0-9A-Fa-f]+(?:\.[0-9A-Fa-f]*)?)|(?:\.[0-9A-Fa-f]+)))\b" },
            { "POSINT", @"\b(?:[1-9][0-9]*)\b" },
            { "NONNEGINT", @"\b(?:[0-9]+)\b" },
            { "NOTSPACE", @"\S+" },
            { "QUOTEDSTRING", "(?>(?<!\\\\)(?>\"(?>\\\\.|[^\\\\\"]+)+\"|\"\"|(?>'(?>\\\\.|[^\\\\']+)+')|''|(?>`(?>\\\\.|[^\\\\`]+)+`)|``))" },
            { "YEAR", @"(?>\d\d){1,2}" },
            { "HOUR", @"(?:2[0123]|[01]?[0-9])" },
            { "MINUTE", @"(?:[0-5][0-9])" },
            { "SECOND", @"(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?)" },
            { "MONTH", @"\b(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:tember)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)\b" },
            { "MONTHNUM", @"(?:0?[1-9]|1[0-2])" },
            { "MONTHDAY", @"(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])" },
            { "TIME", @"(?!<[0-9])%{HOUR}:%{MINUTE}(?::%{SECOND})(?![0-9])" },
            { "DATE_US", @"%{MONTHNUM}[/-]%{MONTHDAY}[/-]%{YEAR}" },
            { "DATE_EU", @"%{MONTHDAY}[./-]%{MONTHNUM}[./-]%{YEAR}" },
            { "ISO8601_TIMEZONE", @"(?:Z|[+-]%{HOUR}(?::?%{MINUTE}))" },
            { "ISO8601_SECOND", @"(?:%{SECOND}|60)" },
            { "TIMESTAMP_ISO8601", @"%{YEAR}-%{MONTHNUM}-%{MONTHDAY}[T ]%{HOUR}:?%{MINUTE}(?::?%{SECOND})?%{ISO8601_TIMEZONE}?" },
            { "LOGLEVEL", @"([A-a]lert|ALERT|[T|t]race|TRACE|[D|d]ebug|DEBUG|[N|n]otice|NOTICE|[I|i]nfo|INFO|[W|w]arn?(?:ing)?|WARN?(?:ING)?|[E|e]rr?(?:or)?|ERR?(?:OR)?|[C|c]rit?(?:ical)?|CRIT?(?:ICAL)?|[F|f]atal|FATAL|[S|s]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?)" },
        };

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

        private void AddSemantic(string typeName = "string", string pattern = "*")
        {
            var s = new Semantic(this.semantic, pattern, typeName);
            this.semantics.Add(s);
            this.regexp = string.Format(NamedPattern, this.semantic, this.regexp);
            this.stringBuilder.Append(this.regexp);
        }

        public override string VisitOnCastingCustomRule(GrokParser.OnCastingCustomRuleContext context)
        {
            var pattern = context.QUOTED_STR().Symbol.Text.Trim('\'', '"');
            var value = context.target().GetText();
             this.semantics.Last().CastingRules.Add(pattern, value);
            return this.VisitChildren(context);
        }

        public override string VisitOnCasting(GrokParser.OnCastingContext context)
        {
            if (context.TYPE_NAME() == null)
            {
                this.AddSemantic(null, null);
                return this.VisitChildren(context);
            }
            var typeName = context.TYPE_NAME().Symbol.Text;

            this.AddSemantic(typeName);

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
                this.AddSemantic();
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
            this.stringBuilder.Append(context.GetText());
            return this.VisitChildren(context);
        }
    }
}