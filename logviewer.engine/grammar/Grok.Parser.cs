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
        private Composer composer;
        private readonly IDictionary<string, IPattern> definitionsTable;
        
        private readonly Action<string> customErrorOutputMethod;
        private readonly Stack<Semantic> propertiesStack = new Stack<Semantic>();
        private ReferencePattern current;

        public GrokParser(Action<string> customErrorOutputMethod = null) : base(null)
        {
            this.definitionsTable = new Dictionary<string, IPattern>();
            this.customErrorOutputMethod = customErrorOutputMethod ?? Console.WriteLine;
        }

        public void Parse(string s)
        {
            var inputBuffer = System.Text.Encoding.Default.GetBytes(s);
            var stream = new MemoryStream(inputBuffer);
            this.Scanner = new GrokScanner(stream) { CustomErrorOutputMethod = this.customErrorOutputMethod };
            this.Parse();
        }

        void OnLiteral(string text)
        {
            var pattern = new StringLiteral(text);
            this.composer.Add(pattern);
        }


        void OnPatternDefinition(string patternName)
        {
            var currentComposer = new Composer();
            this.definitionsTable.Add(patternName, currentComposer);
            this.composer = currentComposer;
        }
        
        void SaveSchema()
        {
            if (this.propertiesStack.Count > 0)
            {
                this.current.Schema.Add(this.propertiesStack.Pop());
            }
        }

        void OnPattern(string patternName)
        {
            this.current = new ReferencePattern(patternName, this.definitionsTable);
            this.composer.Add(this.current);
        }

        internal bool IsPropertyStackEmpty
        {
            get { return this.propertiesStack.Count == 0; }
        }

        public IDictionary<string, IPattern> DefinitionsTable
        {
            get { return this.definitionsTable; }
        }

        private void OnSemantic(string property)
        {
            this.current.Property = property;
            var semantic = new Semantic(property);
            this.propertiesStack.Push(semantic); // push property into stack to wrap pattern later
        }

        void OnRule(ParserType parser, string pattern)
        {
            var rule = new GrokRule(parser, pattern);
            this.AddRule(rule);
        }
        
        void OnRule(ParserType parser, string pattern, LogLevel level)
        {
            var rule = new GrokRule(parser, pattern.UnescapeString(), level);
            this.AddRule(rule);
        }

        private void AddRule(GrokRule rule)
        {
            this.propertiesStack.Peek().CastingRules.Add(rule);
        }
    }
}
