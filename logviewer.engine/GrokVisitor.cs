// Created by: egr
// Created at: 02.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Linq;

namespace logviewer.engine
{
    internal class GrokVisitor : GrokBaseVisitor<string>
    {
        private readonly IDictionary<string, string> templates;
        private readonly List<Semantic> schema = new List<Semantic>();
        private readonly Composer composer = new Composer();
        private IPattern compiledPattern;
        private string property;
        private readonly Func<string, string> compiler;

        internal GrokVisitor(IDictionary<string, string> templates, Func<string, string> compiler)
        {
            this.templates = templates;
            this.compiler = compiler;
        }

        internal string Template
        {
            get { return this.composer.Content; }
        }

        internal ICollection<Semantic> Schema
        {
            get { return this.schema; }
        }

        private void AddSemantic(Rule rule)
        {
            var s = new Semantic(this.property, rule);
            this.AddSemantic(s);
        }
        
        private void AddSemantic(Semantic s)
        {
            this.schema.Add(s);
            var p = new NamedPattern(this.property, this.compiledPattern);
            this.composer.Add(p);
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
                // just use pattern itself
                var pattern = new PassthroughPattern(node);
                this.composer.Add(pattern);
            }
            else
            {
                // Rule needs rewinding
                this.compiledPattern = new CompilePattern(this.templates[node], this.compiler);

                // Semantic handlers do it later but without semantic it MUST BE done here
                if (context.semantic() == null)
                {
                    this.composer.Add(this.compiledPattern);
                }
            }
            return this.VisitChildren(context);
        }

        public override string VisitOnLiteral(GrokParser.OnLiteralContext context)
        {
            var literal = new StringLiteral(context.GetText());
            this.composer.Add(literal);
            return this.VisitChildren(context);
        }
    }
}