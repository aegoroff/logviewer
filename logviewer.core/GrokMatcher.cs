// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace logviewer.core
{
    public class GrokMatcher
    {
        private Regex regex;
        private readonly List<Semantic> messageSchema = new List<Semantic>();

        public GrokMatcher(string grok, RegexOptions options = RegexOptions.None)
        {
            this.Compile(grok, options);
        }

        private void Compile(string grok, RegexOptions options)
        {
            ICharStream inputStream = new AntlrInputStream(grok);
            var lexer = new GrokLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new GrokParser(tokenStream);
            var tree = parser.parse();

            this.CompilationFailed = parser.NumberOfSyntaxErrors > 0;

            if (this.CompilationFailed)
            {
                this.Template = grok;
            }
            else
            {
                var grokVisitor = new GrokVisitor();
                grokVisitor.Visit(tree);
                this.Template = grokVisitor.Template;
                this.messageSchema.AddRange(grokVisitor.Schema);
            }
            this.regex = new Regex(this.Template, options);
        }

        public string Template { get; private set; }

        public bool CompilationFailed { get; private set; }

        public ICollection<Semantic> MessageSchema
        {
            get { return this.messageSchema; }
        }

        public bool Match(string s)
        {
            return this.regex.IsMatch(s);
        }

        public IDictionary<Semantic, string> Parse(string s)
        {
            var match = this.regex.Match(s);
            return !match.Success ? null : this.MessageSchema.ToDictionary(semantic => semantic, semantic => match.Groups[semantic.Property].Value);
        }
    }
}