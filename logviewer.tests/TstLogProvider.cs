// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 10.11.2015
// © 2012-2017 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using logviewer.engine;
using logviewer.logic.Annotations;
using logviewer.logic.models;
using logviewer.logic.storage;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstLogProvider //-V3072 //-V3074
    {
        private readonly MemoryStream stream;
        private readonly LogStore store;
        private static readonly string dbPath = Path.GetTempFileName();
        private readonly LogProvider provider;
        private readonly LogReader reader;

        public TstLogProvider()
        {
            var settings = new LocalDbSettingsProvider(dbPath, 100, 2);
            var detector = new Mock<ICharsetDetector>();
            this.stream = new MemoryStream();
            var grokMatcher = new GrokMatcher(TstLogReader.NlogGrok);
            this.reader = new LogReader(detector.Object, grokMatcher);
            this.store = new LogStore(grokMatcher.MessageSchema);

            this.provider = new LogProvider(this.store, settings);
        }

        private void FillStore()
        {
            this.CreateStream();
            this.ReadFromStream();
        }

        private void ReadFromStream()
        {
            long ix = 0;

            var enumerator = this.reader.Read(this.stream, 0).GetEnumerator();

            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    var message = enumerator.Current;
                    message.Ix = ix++;
                    this.store.AddMessage(message);
                }
            }
        }

        private void CreateStream(string data = TstLogReader.MessageExamples)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            this.stream.Write(buffer, 0, buffer.Length);
            this.stream.Seek(0, SeekOrigin.Begin);
        }

        [PublicAPI]
        public void Dispose()
        {
            this.stream.Dispose();
            this.provider.Dispose();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        [PublicAPI]
        public static IEnumerable<object[]> FilterCases => new[]
        {
            new object[] { new MessageFilterViewModel(), 2  },
            new object[] { new MessageFilterViewModel { Max = LogLevel.Info }, 1  },
            new object[] { new MessageFilterViewModel { Min = LogLevel.Error }, 1  },
            new object[] { new MessageFilterViewModel { Filter = "body 1" }, 1  },
            new object[] { new MessageFilterViewModel { Filter = @"body\s+(\d).*", UseRegexp = true }, 2  },
            new object[] { new MessageFilterViewModel { Filter = @"body\s+(\d).*", UseRegexp = false }, 0  },
            new object[] { new MessageFilterViewModel { Max = LogLevel.Debug }, 0  },
            new object[] { new MessageFilterViewModel { Min = LogLevel.Fatal }, 0  },
            new object[] { new MessageFilterViewModel { Start = DateTime.Parse("2008-12-27 19:40") }, 1  },
            new object[] { new MessageFilterViewModel { Finish = DateTime.Parse("2008-12-27 19:40") }, 1  },
            new object[] { new MessageFilterViewModel { Start = DateTime.MaxValue }, 0  },
            new object[] { new MessageFilterViewModel { Finish = new DateTime(2000, 1, 1) }, 0  }
        };

        [Theory, MemberData(nameof(FilterCases))]
        public void FetchCount(MessageFilterViewModel filterViewModel, int expectation)
        {
            this.FillStore();
            this.provider.FilterViewModel = filterViewModel;
            this.provider.FetchCount().Should().Be(expectation);
        }

        [Theory, MemberData(nameof(FilterCases))]
        public void FetchRangeFiltered(MessageFilterViewModel filterViewModel, int expectation)
        {
            this.FillStore();
            this.provider.FilterViewModel = filterViewModel;
            var result = this.provider.FetchRange(0, 2);
            var lastIndex = expectation - 1;
            if (lastIndex < 0)
            {
                result[0].Should().BeNullOrEmpty();
            }
            else
            {
                result[lastIndex].Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void FetchLimit()
        {
            this.FillStore();
            this.provider.FilterViewModel = new MessageFilterViewModel();
            var result = this.provider.FetchRange(0, 1);
            result.Length.Should().Be(1);
            result[0].Should().MatchRegex("message body 1");
        }

        [Fact]
        public void FetchNotZeroOffset()
        {
            this.FillStore();
            this.provider.FilterViewModel = new MessageFilterViewModel();
            var result = this.provider.FetchRange(1, 2);
            result.Length.Should().Be(2);
            result[0].Should().MatchRegex("message body 2");
            result[1].Should().BeNullOrEmpty();
        }

        [Fact]
        public void FetchAll()
        {
            this.FillStore();
            this.provider.FilterViewModel = new MessageFilterViewModel();
            var result = this.provider.FetchRange(0, 2);
            result.Length.Should().Be(2);
            result[0].Should().MatchRegex("message body 1");
            result[1].Should().MatchRegex("message body 2");
        }

        [Fact]
        public void FetchFilterLevelMin()
        {
            this.FillStore();
            this.provider.FilterViewModel = new MessageFilterViewModel { Min = LogLevel.Error };
            var result = this.provider.FetchRange(0, 2);
            result.Length.Should().Be(2);
            result[0].Should().MatchRegex("message body 2");
            result[1].Should().BeNullOrEmpty();
        }

        [Fact]
        public void FetchFilterLevelMax()
        {
            this.FillStore();
            this.provider.FilterViewModel = new MessageFilterViewModel { Max = LogLevel.Info };
            var result = this.provider.FetchRange(0, 2);
            result.Length.Should().Be(2);
            result[0].Should().MatchRegex("message body 1");
            result[1].Should().BeNullOrEmpty();
        }

        [Fact]
        public void FetchWithoutBody()
        {
            this.CreateStream("2008-12-27 19:31:47,250 [4688] INFO h1\n2008-12-27 19:40:11,906 [5272] ERROR h2");
            this.ReadFromStream();
            this.provider.FilterViewModel = new MessageFilterViewModel();
            var result = this.provider.FetchRange(0, 2);
            result.Length.Should().Be(2);
            result[0].Should().NotMatchRegex("message body 1");
            result[1].Should().NotMatchRegex("message body 2");
        }
    }
}