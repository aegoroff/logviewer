using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using logviewer.logic.support;
using Ploeh.AutoFixture;

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
        public string FixedSize()
        {
            this.fixedSize.TryGetValue(this.index, out string r);
            return r;
        }

        [Benchmark(Baseline = true)]
        public string Standart()
        {
            this.randomSize.TryGetValue(this.index, out string r);
            return r;
        }

        private class Config : ManualConfig
        {
            public Config()
            {
                this.Add(
                    Job.Clr
                    .With(Platform.X64)
                    .With(Jit.RyuJit)
                    .With(Runtime.Clr)
                    .With(RunStrategy.Throughput)
                    .WithLaunchCount(1)
                    .WithWarmupCount(3)
                    .WithTargetCount(10)
                    .WithInvocationCount(100000000)
                    .WithId("RyuJitThroughput")); // IMPORTANT: Id assignment should be the last call in the chain or the id will be lost.

                this.Add(
                    Job.Clr
                    .With(Platform.X64)
                    .With(Jit.RyuJit)
                    .With(Runtime.Clr)
                    .With(RunStrategy.ColdStart)
                    .WithLaunchCount(1)
                    .WithWarmupCount(3)
                    .WithTargetCount(10)
                    .WithInvocationCount(100000000)
                    .WithId("RyuJitColdStart")); // IMPORTANT: Id assignment should be the last call in the chain or the id will be lost.

                this.Add(
                    Job.Clr
                    .With(Platform.X64)
                    .With(Jit.RyuJit)
                    .With(Runtime.Clr)
                    .With(RunStrategy.Monitoring)
                    .WithLaunchCount(1)
                    .WithWarmupCount(3)
                    .WithTargetCount(10)
                    .WithInvocationCount(100000000)
                    .WithId("RyuJitMonitoring")); // IMPORTANT: Id assignment should be the last call in the chain or the id will be lost.

                this.Add(
                    Job.Clr
                    .With(Platform.X64)
                    .With(Jit.LegacyJit)
                    .With(Runtime.Clr)
                    .With(RunStrategy.Throughput)
                    .WithLaunchCount(1)
                    .WithWarmupCount(3)
                    .WithTargetCount(10)
                    .WithInvocationCount(100000000)
                    .WithId("LegacyJitThroughput")); // IMPORTANT: Id assignment should be the last call in the chain or the id will be lost.

                this.Add(
                    Job.Clr
                    .With(Platform.X64)
                    .With(Jit.LegacyJit)
                    .With(Runtime.Clr)
                    .With(RunStrategy.ColdStart)
                    .WithLaunchCount(1)
                    .WithWarmupCount(3)
                    .WithTargetCount(10)
                    .WithInvocationCount(100000000)
                    .WithId("LegacyJitColdStart")); // IMPORTANT: Id assignment should be the last call in the chain or the id will be lost.

                this.Add(
                    Job.Clr
                    .With(Platform.X64)
                    .With(Jit.LegacyJit)
                    .With(Runtime.Clr)
                    .With(RunStrategy.Monitoring)
                    .WithLaunchCount(1)
                    .WithWarmupCount(3)
                    .WithTargetCount(10)
                    .WithInvocationCount(100000000)
                    .WithId("LegacyJitMonitoring")); // IMPORTANT: Id assignment should be the last call in the chain or the id will be lost.
            }
        }
    }
}