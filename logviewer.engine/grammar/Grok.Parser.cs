// Created by: egr
// Created at: 06.07.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;

namespace logviewer.engine.grammar
{
    internal partial class GrokParser
    {
        private readonly Composer composer = new Composer();
        private readonly IDictionary<string, string> templates;
        private readonly Func<string, string> compiler;
        private readonly List<Semantic> schema = new List<Semantic>();
        
        private readonly Action<string> customErrorOutputMethod;
        private readonly Stack<GrokRule> rulesStack = new Stack<GrokRule>();
        private readonly Stack<string> propertiesStack = new Stack<string>();

        public GrokParser(IDictionary<string, string> templates, Func<string, string> compiler, Action<string> customErrorOutputMethod = null) : base(null)
        {
            this.templates = templates;
            this.compiler = compiler;
            this.customErrorOutputMethod = customErrorOutputMethod ?? Console.WriteLine;
        }

        public void Parse(string s)
        {
            var inputBuffer = System.Text.Encoding.Default.GetBytes(s);
            var stream = new MemoryStream(inputBuffer);
            this.Scanner = new GrokScanner(stream) { CustomErrorOutputMethod = this.customErrorOutputMethod };
            this.Parse();
        }

        internal string Template
        {
            get { return this.composer.Content; }
        }

        internal List<Semantic> Schema
        {
            get { return this.schema; }
        }

        void OnLiteral(string text)
        {
            var pattern = new StringLiteral(text);
            this.composer.Add(pattern);
        }


        void OnPattern(string patternName)
        {
            IPattern pattern;
            if (this.templates.ContainsKey(patternName))
            {
                pattern = new CompilePattern(this.templates[patternName], this.compiler);
                if (this.propertiesStack.Count > 0)
                {
                    var property = this.propertiesStack.Pop();
                    pattern = new NamedPattern(property, pattern);
                }
            }
            else
            {
                // just use pattern itself
                pattern = new PassthroughPattern(patternName);
            }
            this.composer.Add(pattern);
        }

        internal bool IsPropertyStackEmpty
        {
            get { return this.propertiesStack.Count == 0; }
        }

        private void OnSemantic(string property)
        {
            var semantic = new Semantic(property);

            if (rulesStack.Count == 0)
            {
                OnRule(ParserType.String, GrokRule.DefaultPattern); // No schema case. Add generic string rule
            }

            while (this.rulesStack.Count > 0)
            {
                semantic.CastingRules.Add(this.rulesStack.Pop());
            }
            this.schema.Add(semantic);
            this.propertiesStack.Push(property); // push property into stack to wrap pattern later
        }

        void OnRule(ParserType parser, string pattern)
        {
            this.rulesStack.Push(new GrokRule(parser, pattern));
        }
        
        void OnRule(ParserType parser, string pattern, LogLevel level)
        {
            this.rulesStack.Push(new GrokRule(parser, pattern.UnescapeString(), level));
        }
    }
}
