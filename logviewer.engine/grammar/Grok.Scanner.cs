using System;

namespace logviewer.engine.grammar
{
    internal partial class GrokScanner
    {
        private Action<string> customErrorOutputMethod = Console.WriteLine;

        internal Action<string> CustomErrorOutputMethod
        {
            get { return this.customErrorOutputMethod; }
            set { this.customErrorOutputMethod = value; }
        }

        public override void yyerror(string format, params object[] args)
        {
            base.yyerror(format, args);
            var message = string.Format(format, args);
            this.CustomErrorOutputMethod(message);
            this.CustomErrorOutputMethod(string.Empty);
        }
    }
}