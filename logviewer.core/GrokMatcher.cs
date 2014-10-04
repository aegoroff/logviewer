// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace logviewer.core
{
    public class GrokMatcher
    {
        private Regex regex;
        private readonly List<Semantic> semantics = new List<Semantic>();

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
            var tree = TryCompile(parser, tokenStream);

            if (parser.NumberOfSyntaxErrors > 0)
            {
                this.Template = grok;
            }
            else
            {
                var grokVisitor = new GrokVisitor();
                grokVisitor.Visit(tree);
                this.Template = grokVisitor.Template;
                this.semantics.AddRange(grokVisitor.Semantics);
            }
            this.regex = new Regex(this.Template, options);
        }

        private static GrokParser.ParseContext TryCompile(GrokParser parser, BufferedTokenStream tokens)
        {
            try
            {
                parser.Interpreter.PredictionMode = PredictionMode.Sll;
                parser.RemoveErrorListeners();
                parser.ErrorHandler = new BailErrorStrategy();
            }
            catch (ParseCanceledException e)
            {
                tokens.Reset();
                parser.Reset();
                Log.Instance.Debug(e);
                parser.ErrorListeners.Add(ConsoleErrorListener<IToken>.Instance);
                parser.ErrorHandler = new DefaultErrorStrategy();
                parser.Interpreter.PredictionMode = PredictionMode.Ll;
            }
            return parser.parse();
        }

        public string Template { get; private set; }

        public bool Match(string s)
        {
            return this.regex.IsMatch(s);
        }

        public IDictionary<Semantic, string> Parse(string s)
        {
            var match = this.regex.Match(s);
            return !match.Success ? null : this.semantics.ToDictionary(semantic => semantic, semantic => match.Groups[semantic.Name].Value);
        }
    }
}