// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System;
using Antlr4.Runtime.Tree;

namespace logviewer.core
{
    public class GrokVisitor : GrokBaseVisitor<string>
    {
        public override string VisitFind(GrokParser.FindContext ctx)
        {
            ITerminalNode node = ctx.ID();

            Log.Instance.TraceFormatted(node.Symbol.Text);
            Console.WriteLine(node.Symbol.Text);
            return VisitChildren(ctx);
        }
    }
}