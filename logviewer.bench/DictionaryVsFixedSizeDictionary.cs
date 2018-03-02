using System;
using System.Collections.Generic;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using logviewer.logic.support;

namespace logviewer.bench
{
    [Config(typeof(Config))]
    public class DictionaryVsFixedSizeDictionary
    {
        private const int DictionarySize = 1000;
        private int index;
        private FixedSizeDictionary<string> fixedSize;
        private Dictionary<int, string> randomSize;

        public DictionaryVsFixedSizeDictionary()
        {
            this.index = new Random().Next(0, DictionarySize - 1);
        }

        [Setup]
        public void SetupData()
        {
            var fixture = new Fixture();
            this.fixedSize = new FixedSizeDictionary<string>(DictionarySize);
            this.randomSize = new Dictionary<int, string>(DictionarySize);
            for (var i = 0; i < DictionarySize; i++)
            {
                var str = fixture.Create<string>();
                this.fixedSize.Add(i, str);
                this.randomSize.Add(i, str);
            }
        }

        [Benchmark]
        public string TryGetValue_FixedSize()
        {
            this.fixedSize.TryGetValue(this.index, out string r);
            return r;
        }

        [Benchmark]
        public string TryGetValue_Standart()
        {
            this.randomSize.TryGetValue(this.index, out string r);
            return r;
        }

        [Benchmark]
        public bool ContainsKey_FixedSize()
        {
            return this.fixedSize.ContainsKey(this.index);
        }

        [Benchmark]
        public bool ContainsKey_Standart()
        {
            return this.randomSize.ContainsKey(this.index);
        }

        private class Config : ManualConfig
        {
            private const int InvocationCount = 100000000;
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
