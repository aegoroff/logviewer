// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Tree;

namespace logviewer.core
{
    public class GrokVisitor : GrokBaseVisitor<string>
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();

        static readonly Dictionary<string, string> templates = new Dictionary<string, string>
        {
            { "USERNAME", "[a-zA-Z0-9._-]+" },
            { "WORD", @"\b\w+\b" },
            { "SPACE", @"\s*" },
            { "DATA", @".*?" },
            { "GREEDYDATA", @".*" },
            { "INT", @"(?:[+-]?(?:[0-9]+))" },
            { "BASE10NUM", @"(?<![0-9.+-])(?>[+-]?(?:(?:[0-9]+(?:\.[0-9]+)?)|(?:\.[0-9]+)))" },
            { "BASE16NUM", @"(?<![0-9A-Fa-f])(?:[+-]?(?:0x)?(?:[0-9A-Fa-f]+))" },
            { "BASE16FLOAT", @"\b(?<![0-9A-Fa-f.])(?:[+-]?(?:0x)?(?:(?:[0-9A-Fa-f]+(?:\.[0-9A-Fa-f]*)?)|(?:\.[0-9A-Fa-f]+)))\b" },
            { "POSINT", @"\b(?:[1-9][0-9]*)\b" },
            { "NONNEGINT", @"\b(?:[0-9]+)\b" },
            { "NOTSPACE", @"\S+" },
            { "QUOTEDSTRING", "(?>(?<!\\\\)(?>\"(?>\\\\.|[^\\\\\"]+)+\"|\"\"|(?>'(?>\\\\.|[^\\\\']+)+')|''|(?>`(?>\\\\.|[^\\\\`]+)+`)|``))" },
        }; 

        public string Template
        {
            get { return this.stringBuilder.ToString(); }
        }

        public override string VisitFind(GrokParser.FindContext ctx)
        {
            ITerminalNode node = ctx.ID();

            if (node == null)
            {
                return this.VisitChildren(ctx);
            }

            Log.Instance.TraceFormatted(node.Symbol.Text);
            Console.WriteLine("id: " + node.Symbol.Text);
            if (templates.ContainsKey(node.Symbol.Text))
            {
                this.stringBuilder.Append(templates[node.Symbol.Text]);
            }
            else
            {
                this.stringBuilder.Append("%{");
                this.stringBuilder.Append(node.Symbol.Text);
                this.stringBuilder.Append("}");
            }
            return this.VisitChildren(ctx);
        }

        public override string VisitPaste(GrokParser.PasteContext context)
        {
            Console.WriteLine("str: " + context.STRING().Symbol.Text);
            this.stringBuilder.Append(context.STRING().Symbol.Text);
            return this.VisitChildren(context);
        }
    }
}