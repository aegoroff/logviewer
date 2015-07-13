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
        private readonly IDictionary<string, IPattern> definitionsTable;
        private readonly List<Semantic> schema = new List<Semantic>();
        
        private readonly Action<string> customErrorOutputMethod;
        private readonly Stack<Semantic> propertiesStack = new Stack<Semantic>();
        private readonly Stack<Composer> stackFrames = new Stack<Composer>();

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
            this.CurrentStackFrame.Add(pattern);
        }

        private Composer CurrentStackFrame
        {
            get { return this.stackFrames.Peek(); }
        }


        void OnPatternDefinition(string patternName)
        {
            var currentComposer = new Composer();
            this.definitionsTable.Add(patternName, currentComposer);
            this.stackFrames.Push(currentComposer);
        }
        
        void SaveSchema()
        {
            if (this.propertiesStack.Count > 0)
            {
                this.schema.Add(this.propertiesStack.Pop());
            }
        }


        void OnPattern(string patternName)
        {
            IPattern pattern = new CompilePattern(patternName);
            this.CurrentStackFrame.Add(pattern);
            //if (this.templates.ContainsKey(patternName))
            //{
            //    pattern = new CompilePattern(this.templates[patternName], this.compiler);
            //    if (this.propertiesStack.Count > 0)
            //    {
            //        var property = this.propertiesStack.Pop();
            //        pattern = new NamedPattern(property, pattern);
            //    }
            //}
            //else
            //{
            //    // just use pattern itself
            //    pattern = new PassthroughPattern(patternName);
            //}
            //this.composer.Add(pattern);
        }

        internal bool IsPropertyStackEmpty
        {
            get { return this.propertiesStack.Count == 0; }
        }

        private void OnSemantic(string property)
        {
            var last = this.CurrentStackFrame[this.CurrentStackFrame.Count - 1];
            this.CurrentStackFrame[this.CurrentStackFrame.Count - 1] = new NamedPattern(property, last);
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
