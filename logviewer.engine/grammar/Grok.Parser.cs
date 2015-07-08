using System;
using System.Collections.Generic;
using System.IO;

namespace logviewer.engine.grammar
{
    internal partial class GrokParser
    {
        private readonly Action<string> customErrorOutputMethod;
        private readonly Stack<GrokRule> rulesStack = new Stack<GrokRule>();

        public GrokParser(IDictionary<string, string> templates, Action<string> customErrorOutputMethod = null) : base(null)
        {
            this.templates = templates;
            this.customErrorOutputMethod = customErrorOutputMethod ?? Console.WriteLine;
        }

        public void Parse(string s)
        {
            byte[] inputBuffer = System.Text.Encoding.Default.GetBytes(s);
            MemoryStream stream = new MemoryStream(inputBuffer);
            this.Scanner = new GrokScanner(stream) { CustomErrorOutputMethod = this.customErrorOutputMethod };
            this.Parse();
        }

        private void AddSemantic(string prop)
        {
            var semantic = new Semantic(prop);
            while (this.rulesStack.Count > 0)
            {
                semantic.CastingRules.Add(this.rulesStack.Pop());
            }
            this.AddSemantic(semantic);
        }

        private void AddSemantic(Semantic s)
        {
            this.schema.Add(s);
            var p = new NamedPattern(s.Property, this.compiledPattern);
            this.composer.Add(p);
        }

        void OnRule(string patternName)
        {
            var node = patternName;

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
                // this.compiledPattern = new CompilePattern(this.templates[node], this.compiler);

                // Semantic handlers do it later but without semantic it MUST BE done here
                //if (context.semantic() == null)
                //{
                //    this.composer.Add(this.compiledPattern);
                //}
            }
        }

        void AddRule(string parser, string pattern)
        {
            this.rulesStack.Push(new GrokRule(parser, pattern));
        }
    }
}
