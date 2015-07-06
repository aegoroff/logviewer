using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace logviewer.engine.grammar
{
    internal partial class GrokParser
    {
        private readonly Action<string> customErrorOutputMethod;

        public GrokParser(IDictionary<string, string> templates, Action<string> customErrorOutputMethod = null) : base(null)
        {
            this.templates = templates;
            this.customErrorOutputMethod = customErrorOutputMethod ?? Console.WriteLine;
        }

        public void Parse(string s)
        {
            byte[] inputBuffer = System.Text.Encoding.Default.GetBytes(s);
            MemoryStream stream = new MemoryStream(inputBuffer);
            this.Scanner = new GrokScanner(stream) { CustomErrorOutputMethod = customErrorOutputMethod };
            this.Parse();
        }

        private void AddSemantic(Semantic s)
        {
            this.schema.Add(s);
            var p = new NamedPattern(this.property, this.compiledPattern);
            this.composer.Add(p);
        }

        private void OnSemantic(string prop)
        {
            if (this.compiledPattern == null)
            {
                return;
            }
            this.property = prop;
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

        void OnCastingCustomRule(string lvalue, ParserType parser)
        {
            this.schema.Last().CastingRules.Add(new GrokRule(parser.ToString(), lvalue));
        }
    }
}
