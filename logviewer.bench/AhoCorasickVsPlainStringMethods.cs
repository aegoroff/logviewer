using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using logviewer.engine.strings;

namespace logviewer.bench
{
    [Config(typeof(Config))]
    public class AhoCorasickVsPlainStringMethods
    {
        private const string TestString =
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

        private readonly string[] keywords = { "Apple", "Chrome", "Safari" };

        private readonly AhoCorasickTree tree;

        public AhoCorasickVsPlainStringMethods()
        {
            this.tree = new AhoCorasickTree(this.keywords);
        }

        [Benchmark]
        public bool Contains_AhoCorasick()
        {
            return this.tree.Contains(TestString);
        }

        [Benchmark]
        public bool Contains_Plain()
        {
            for (int i = 0; i < this.keywords.Length; i++)
            {
                if (TestString.Contains(this.keywords[i]))
                {
                    return true;
                }
            }

            return false;
        }

        [Benchmark]
        public IEnumerable<string> FindAll_AhoCorasick()
        {
            return this.tree.FindAll(TestString);
        }

        [Benchmark]
        public IEnumerable<string> FindAll_Plain()
        {
            for (int i = 0; i < this.keywords.Length; i++)
            {
                if (TestString.Contains(this.keywords[i]))
                {
                    yield return this.keywords[i];
                }
            }
        }

        private class Config : ManualConfig
        {
            private const int InvocationCount = 10000000;
            private const int LaunchCount = 1;
            private const int WarmupCount = 2;
            private const int TargetCount = 10;

            public Config()
            {
                this.Add(Jit.RyuJit, RunStrategy.Throughput);
                this.Add(Jit.RyuJit, RunStrategy.ColdStart);
                this.Add(Jit.RyuJit, RunStrategy.Monitoring);
            }

            private void Add(Jit jit, RunStrategy runStrategy)
            {
                this.Add(
                         Job.Clr
                            .With(Platform.X64)
                            .With(jit)
                            .With(Runtime.Clr)
                            .With(runStrategy)
                            .WithLaunchCount(LaunchCount)
                            .WithWarmupCount(WarmupCount)
                            .WithTargetCount(TargetCount)
                            .WithInvocationCount(InvocationCount)
                            .WithId($"{jit:G}{runStrategy:G}")); // IMPORTANT: Id assignment should be the last call in the chain or the id will be lost.
            }
        }
    }
}
