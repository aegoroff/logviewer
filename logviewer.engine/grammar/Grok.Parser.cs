// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 06.07.2015
// © 2012-2018 Alexander Egorov

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
        private ReferencePattern currentPattern;

        public GrokParser(Action<string> customErrorOutputMethod = null) : base(null)
        {
            this.definitionsTable = new Dictionary<string, IPattern>();
            this.customErrorOutputMethod = customErrorOutputMethod ?? Console.WriteLine;
        }

        public void Parse(string s)
        {
            var inputBuffer = System.Text.Encoding.UTF8.GetBytes(s);
            var stream = new MemoryStream(inputBuffer);
            using (stream)
            {
                this.Parse(stream);
            }
        }

        public void Parse(Stream stream)
        {
            this.Scanner = new GrokScanner(stream) { CustomErrorOutputMethod = this.customErrorOutputMethod };
            this.Parse();
        }

        private void OnLiteral(string text)
        {
            var pattern = new Pattern(text);
            this.composer.Add(pattern);
        }


        private void OnPatternDefinition(string patternName)
        {
            this.composer = new Composer();
            this.definitionsTable.Add(patternName, this.composer);
        }

        private void OnPattern(string patternName)
        {
            this.currentPattern = new ReferencePattern(patternName, this.definitionsTable);
            this.composer.Add(this.currentPattern);
        }

        public IDictionary<string, IPattern> DefinitionsTable => this.definitionsTable;

        private void OnSemantic(string property)
        {
            this.currentPattern.Property = property;
            this.currentPattern.Schema = new Semantic(property);
        }

        private void OnRule(ParserType parser, string pattern)
        {
            var rule = new GrokRule(parser, pattern);
            this.AddRule(rule);
        }

        private void OnRule(ParserType parser, string pattern, LogLevel level)
        {
            var rule = new GrokRule(parser, pattern.Unescape(), level);
            this.AddRule(rule);
        }

        private void AddRule(GrokRule rule)
        {
            this.currentPattern.Schema.CastingRules.Add(rule);
        }
    }
}
