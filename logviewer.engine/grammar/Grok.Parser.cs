using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace logviewer.engine.grammar
{
    internal partial class GrokParser
    {
        public GrokParser() : base(null) { }

        public void Parse(string s)
        {
            byte[] inputBuffer = System.Text.Encoding.Default.GetBytes(s);
            MemoryStream stream = new MemoryStream(inputBuffer);
            this.Scanner = new GrokScanner(stream);
            this.Parse();
        }
    }
}
