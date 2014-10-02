// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using Antlr4.Runtime;

namespace logviewer.core
{
    public class GrokMatcher
    {
        public string Template { get; private set; }

        public bool Match(string s)
        {
            ICharStream inputStream = new AntlrInputStream(s);
            GrokLexer lexer = new GrokLexer(inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            GrokParser parser = new GrokParser(tokenStream);
            var tree = parser.parse();

            if (parser.NumberOfSyntaxErrors > 0)
            {
                return false;
            }

            GrokVisitor grokVisitor = new GrokVisitor();

            grokVisitor.Visit(tree);
            this.Template = grokVisitor.Template;

            return true;
        }
    }
}