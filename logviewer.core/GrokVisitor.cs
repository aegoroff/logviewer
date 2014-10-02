using Antlr4.Runtime.Tree;

namespace logviewer.core
{
    public class GrokVisitor : IGrokVisitor<string>
    {
        public string VisitFind(GrokParser.FindContext ctx)
        {
            ITerminalNode node = ctx.ID();

            Log.Instance.TraceFormatted(node.Symbol.Text);
            return node.Symbol.Text;
        }

        public string VisitBuild(GrokParser.BuildContext ctx)
        {
            return string.Empty;
        }

        public string Visit(IParseTree tree)
        {
            return string.Empty;
        }

        public string VisitChildren(IRuleNode node)
        {
            return string.Empty;
        }

        public string VisitErrorNode(IErrorNode node)
        {
            return string.Empty;
        }

        public string VisitTerminal(ITerminalNode node)
        {
            return string.Empty;
        }
    }
}