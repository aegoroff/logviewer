﻿// Created by: egr
// Created at: 10.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using logviewer.core;
using logviewer.engine;
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

        public TstLogProvider()
        {
            var settings = new SqliteSettingsProvider(dbPath, 100, 2);
            var detector = new Mock<ICharsetDetector>();
            this.stream = new MemoryStream();
            var grokMatcher = new GrokMatcher(TstLogReader.NlogGrok);
            var reader = new LogReader(detector.Object, grokMatcher);
            this.store = new LogStore(schema: grokMatcher.MessageSchema);

            this.CreateStream();
            this.stream.Seek(0, SeekOrigin.Begin);
            long ix = 0;
            Action<LogMessage> onRead = delegate (LogMessage message)
            {
                message.Ix = ix++;
                this.store.AddMessage(message);
            };

            reader.Read(this.stream, 0, onRead, () => true);

            this.provider = new LogProvider(this.store, settings);
        }

        private void CreateStream(string data = TstLogReader.MessageExamples)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            this.stream.Write(buffer, 0, buffer.Length);
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

        public static IEnumerable<object[]> FilterCount => new[]
        {
            new object[] { new MessageFilter(), 2  },
            new object[] { new MessageFilter { Max = LogLevel.Info }, 1  },
            new object[] { new MessageFilter { Min = LogLevel.Error }, 1  },
            new object[] { new MessageFilter { Max = LogLevel.Debug }, 0  },
            new object[] { new MessageFilter { Min = LogLevel.Fatal }, 0  },
            new object[] { new MessageFilter { Start = DateTime.Parse("2008-12-27 19:40") }, 1  },
            new object[] { new MessageFilter { Finish = DateTime.Parse("2008-12-27 19:40") }, 1  },
            new object[] { new MessageFilter { Start = DateTime.MaxValue }, 0  },
            new object[] { new MessageFilter { Finish = new DateTime(2000, 1, 1) }, 0  },
        };

        [Theory, MemberData("FilterCount")]
        public void FetchCount(MessageFilter filter, int expectation)
        {
            this.provider.Filter = filter;
            this.provider.FetchCount().Should().Be(expectation);
        }

        [Fact]
        public void FetchLimit()
        {
            this.provider.Filter = new MessageFilter();
            var result = this.provider.FetchRange(0, 1);
            result.Count.Should().Be(1);
            result[0].Should().MatchRegex("message body 1");
        }

        [Fact]
        public void FetchNotZeroOffset()
        {
            this.provider.Filter = new MessageFilter();
            var result = this.provider.FetchRange(1, 2);
            result.Count.Should().Be(1);
            result[0].Should().MatchRegex("message body 2");
        }

        [Fact]
        public void FetchAll()
        {
            this.provider.Filter = new MessageFilter();
            var result = this.provider.FetchRange(0, 2);
            result.Count.Should().Be(2);
            result[0].Should().MatchRegex("message body 1");
            result[1].Should().MatchRegex("message body 2");
        }

        [Fact]
        public void FetchFilterLevelMin()
        {
            this.provider.Filter = new MessageFilter { Min = LogLevel.Error };
            var result = this.provider.FetchRange(0, 2);
            result.Count.Should().Be(1);
            result[0].Should().MatchRegex("message body 2");
        }

        [Fact]
        public void FetchFilterLevelMax()
        {
            this.provider.Filter = new MessageFilter { Max = LogLevel.Info };
            var result = this.provider.FetchRange(0, 2);
            result.Count.Should().Be(1);
            result[0].Should().MatchRegex("message body 1");
        }
    }
}