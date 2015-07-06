using System;
using System.Collections.Generic;
using System.Text;

namespace logviewer.engine.grammar
{
    internal partial class GrokScanner
    {
        private Action<string> customErrorOutputMethod = Console.WriteLine;

        internal Action<string> CustomErrorOutputMethod
        {
            get { return customErrorOutputMethod; }
            set { customErrorOutputMethod = value; }
        }

        public override void yyerror(string format, params object[] args)
		{
			base.yyerror(format, args);
            this.CustomErrorOutputMethod(string.Format(format, args));
            CustomErrorOutputMethod(string.Empty);
		}
    }
}
