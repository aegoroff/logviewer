// Created by: egr
// Created at: 06.07.2015
// © 2012-2016 Alexander Egorov

using System;

namespace logviewer.engine.grammar
{
    internal partial class GrokScanner
    {
        internal Action<string> CustomErrorOutputMethod { get; set; } = Console.WriteLine;

        public override void yyerror(string format, params object[] args)
        {
            base.yyerror(format, args);
            var message = string.Format(format, args);
            var currentToken = $"current token: '{this.yytext}'";
            this.CustomErrorOutputMethod(currentToken);
            this.CustomErrorOutputMethod(string.Empty);
            throw new GrokSyntaxException(message);
        }
    }
}