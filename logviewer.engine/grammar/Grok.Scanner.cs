using System;
using System.Collections.Generic;
using System.Text;

namespace logviewer.engine.grammar
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
