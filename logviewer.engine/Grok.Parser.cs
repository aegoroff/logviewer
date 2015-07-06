using System.IO;
using System.Text;

namespace logviewer.engine
{
    internal partial class GrokParser
    {
        public GrokParser() : base(null)
        {
        }

        public void Parse(string s)
        {
            var inputBuffer = Encoding.Default.GetBytes(s);
            var stream = new MemoryStream(inputBuffer);
            Scanner = new GrokScanner(stream);
            Parse();
        }
    }
}