using BenchmarkDotNet.Running;

namespace logviewer.bench
{
    public class BenchmarkTests
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DictionaryVsFixedSizeDictionary>();
        }
    }
}