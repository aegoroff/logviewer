using System;

namespace logviewer.engine
{
    /// <summary>
    /// Represents grok template compiler
    /// </summary>
    public class GrokCompiler
    {
        private readonly Action<string> customErrorOutputMethod;

        /// <summary>
        /// Creates new compiler instance using custom error method output if necessary
        /// </summary>
        /// <param name="customErrorOutputMethod"></param>
        public GrokCompiler(Action<string> customErrorOutputMethod = null)
        {
            this.customErrorOutputMethod = customErrorOutputMethod;
        }
        
        /// <summary>
        /// Compiles grok specified
        /// </summary>
        /// <param name="grok"></param>
        public void Compile(string grok)
        {
            var parser = new grammar.GrokParser(this.customErrorOutputMethod);
            parser.Parse(grok);
        }
    }
}