// Created by: egr
// Created at: 10.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using logviewer.engine;
using logviewer.logic;
using logviewer.logic.models;
using logviewer.logic.storage;
using logviewer.logic.ui;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstLogProvider
    {
        private readonly MemoryStream stream;
        private readonly LogStore store;
        private static readonly string dbPath = Path.GetTempFileName();
        private readonly LogProvider provider;
        private readonly LogReader reader;

        public TstLogProvider()
        {
            var settings = new SqliteSettingsProvider(dbPath, 100, 2);
            var detector = new Mock<ICharsetDetector>();
            this.stream = new MemoryStream();
            var grokMatcher = new GrokMatcher(TstLogReader.NlogGrok);
            this.reader = new LogReader(detector.Object, grokMatcher);
            this.store = new LogStore(schema: grokMatcher.MessageSchema);

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
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                message.Ix = ix++;
                this.store.AddMessage(message);
            };

            this.reader.Read(this.stream, 0, onRead);
        }

        private void CreateStream(string data = TstLogReader.MessageExamples)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            this.stream.Write(buffer, 0, buffer.Length);
            this.stream.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            this.stream.Dispose();
            this.provider.Dispose();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        public static IEnumerable<object[]> FilterCases => new[]
        {
            new object[] { new MessageFilterModel(), 2  },
            new object[] { new MessageFilterModel { Max = LogLevel.Info }, 1  },
            new object[] { new MessageFilterModel { Min = LogLevel.Error }, 1  },
            new object[] { new MessageFilterModel { Filter = "body 1" }, 1  },
            new object[] { new MessageFilterModel { Filter = @"body\s+(\d).*", UseRegexp = true }, 2  },
            new object[] { new MessageFilterModel { Filter = @"body\s+(\d).*", UseRegexp = false }, 0  },
            new object[] { new MessageFilterModel { Max = LogLevel.Debug }, 0  },
            new object[] { new MessageFilterModel { Min = LogLevel.Fatal }, 0  },
            new object[] { new MessageFilterModel { Start = DateTime.Parse("2008-12-27 19:40") }, 1  },
            new object[] { new MessageFilterModel { Finish = DateTime.Parse("2008-12-27 19:40") }, 1  },
            new object[] { new MessageFilterModel { Start = DateTime.MaxValue }, 0  },
            new object[] { new MessageFilterModel { Finish = new DateTime(2000, 1, 1) }, 0  }
        };

        [Theory, MemberData("FilterCases")]
        public void FetchCount(MessageFilterModel filterModel, int expectation)
        {
            this.FillStore();
            this.provider.FilterModel = filterModel;
            this.provider.FetchCount().Should().Be(expectation);
        }

        [Theory, MemberData("FilterCases")]
        public void FetchRangeFiltered(MessageFilterModel filterModel, int expectation)
        {
            this.FillStore();
            this.provider.FilterModel = filterModel;
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
            this.provider.FilterModel = new MessageFilterModel();
            var result = this.provider.FetchRange(0, 1);
            result.Length.Should().Be(1);
            result[0].Should().MatchRegex("message body 1");
        }

        [Fact]
        public void FetchNotZeroOffset()
        {
            this.FillStore();
            this.provider.FilterModel = new MessageFilterModel();
            var result = this.provider.FetchRange(1, 2);
            result.Length.Should().Be(2);
            result[0].Should().MatchRegex("message body 2");
            result[1].Should().BeNullOrEmpty();
        }

        [Fact]
        public void FetchAll()
        {
            this.FillStore();
            this.provider.FilterModel = new MessageFilterModel();
            var result = this.provider.FetchRange(0, 2);
            result.Length.Should().Be(2);
            result[0].Should().MatchRegex("message body 1");
            result[1].Should().MatchRegex("message body 2");
        }

        [Fact]
        public void FetchFilterLevelMin()
        {
            this.FillStore();
            this.provider.FilterModel = new MessageFilterModel { Min = LogLevel.Error };
            var result = this.provider.FetchRange(0, 2);
            result.Length.Should().Be(2);
            result[0].Should().MatchRegex("message body 2");
            result[1].Should().BeNullOrEmpty();
        }

        [Fact]
        public void FetchFilterLevelMax()
        {
            this.FillStore();
            this.provider.FilterModel = new MessageFilterModel { Max = LogLevel.Info };
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
            this.provider.FilterModel = new MessageFilterModel();
            var result = this.provider.FetchRange(0, 2);
            result.Length.Should().Be(2);
            result[0].Should().NotMatchRegex("message body 1");
            result[1].Should().NotMatchRegex("message body 2");
        }
    }
}