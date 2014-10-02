// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace logviewer.core
{
    public class GrokMatcher
    {
        private readonly Regex regex;
        readonly List<string> semantics = new List<string>();

        public GrokMatcher(string grok, RegexOptions options = RegexOptions.None)
        {
            ICharStream inputStream = new AntlrInputStream(grok);
            var lexer = new GrokLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new GrokParser(tokenStream);
            var tree = parser.parse();

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
        
        public string Template { get; private set; }

        public bool Match(string s)
        {
            return this.regex.IsMatch(s);
        }
        
        public IDictionary<string, object> Parse(string s)
        {
            var result = new Dictionary<string, object>();
            var match = regex.Match(s);
            if (!match.Success)
            {
                return result;
            }
            foreach (var semantic in semantics)
            {
                result.Add(semantic, match.Groups[semantic]);
            }
            return result;
        }
    }
}