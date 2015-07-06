using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace logviewer.engine.grammar
{
    internal partial class GrokParser
    {
        private readonly Action<string> customErrorOutputMethod;

        public GrokParser(Action<string> customErrorOutputMethod = null) : base(null)
        {
            this.customErrorOutputMethod = customErrorOutputMethod ?? Console.WriteLine;
        }

        public void Parse(string s)
        {
            byte[] inputBuffer = System.Text.Encoding.Default.GetBytes(s);
            MemoryStream stream = new MemoryStream(inputBuffer);
            this.Scanner = new GrokScanner(stream) { CustomErrorOutputMethod = customErrorOutputMethod };
            this.Parse();
        }
    }
}
