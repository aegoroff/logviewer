using System;

namespace logviewer.engine
{
    internal partial class GrokScanner
    {
        public override void yyerror(string format, params object[] args)
        {
            base.yyerror(format, args);
            Console.WriteLine(format, args);
            Console.WriteLine();
        }
    }
}