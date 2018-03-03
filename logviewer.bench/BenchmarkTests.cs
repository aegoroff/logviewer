using BenchmarkDotNet.Running;

namespace logviewer.bench
{
    public class BenchmarkTests
    {
        public static void Main(string[] args)
        {
            var summary1 = BenchmarkRunner.Run<DictionaryVsFixedSizeDictionary>();
            var summary2 = BenchmarkRunner.Run<AhoCorasickVsPlainStringMethods>();
        }
    }
}
