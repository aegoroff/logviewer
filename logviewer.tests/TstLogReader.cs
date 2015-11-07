// Created by: egr
// Created at: 20.11.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using logviewer.engine;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstLogReader : IDisposable
    {
        private const string MessageExamples =
            "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27 19:40:11,906 [5272] ERROR \nmessage body 2";

        private const string NlogGrok = @"^\[?%{TIMESTAMP_ISO8601:Occured:DateTime}\]?%{DATA}%{LOGLEVEL:Level:LogLevel}%{DATA}";


        private readonly LogReader reader;
        private readonly MemoryStream stream;
        private readonly RulesBuilder builder;
        private byte[] buffer;

        public TstLogReader()
        {
            var detector = new Mock<ICharsetDetector>();
            this.stream = new MemoryStream();
            var grokMatcher = new GrokMatcher(NlogGrok);
            this.reader = new LogReader(detector.Object, grokMatcher);
            this.builder = new RulesBuilder(grokMatcher.MessageSchema);
        }

        private void CreateStream(string data = MessageExamples)
        {
            this.buffer = Encoding.UTF8.GetBytes(data);
            this.stream.Write(this.buffer, 0, this.buffer.Length);
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }

        [Theory, MemberData("ValidStreams")]
        public void LogFromStream(string data)
        {
            this.CreateStream(data);
            this.stream.Seek(0, SeekOrigin.Begin);
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                message.IsEmpty.Should().BeFalse();
            };

            this.reader.Read(this.stream, 0, onRead, () => true);
            count.Should().Be(2);
        }

        public static IEnumerable<object[]> ValidStreams => new[]
        {
            new object[] { MessageExamples  },
            new object[] { Environment.NewLine + MessageExamples },
            new object[] { " " + Environment.NewLine + " " + Environment.NewLine + MessageExamples },
            new object[] { MessageExamples + Environment.NewLine },
            new object[] { Environment.NewLine + MessageExamples + Environment.NewLine }

        };

        [Fact]
        public void LogFromStreamWithCache()
        {
            this.CreateStream();
            this.stream.Seek(0, SeekOrigin.Begin);
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                message.IsEmpty.Should().BeFalse();
                message.Cache(this.builder.Rules);
                var level = (LogLevel)message.IntegerProperty("Level");
                var date = DateTime.FromFileTime(message.IntegerProperty("Occured"));
                Assert.InRange(level, LogLevel.Info, LogLevel.Error);
                date.Year.Should().Be(2008);
                date.Month.Should().Be(12);
                date.Day.Should().Be(27);
            };

            

            this.reader.Read(this.stream, 0, onRead, () => true);
            count.Should().Be(2);
        }

        [Fact]
        public void LogFromStreamCannotContinue()
        {
            this.CreateStream();
            this.stream.Seek(0, SeekOrigin.Begin);
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                message.IsEmpty.Should().BeTrue();
            };

            this.reader.Read(this.stream, 0, onRead, () => false);
            count.Should().Be(1);
        }

        [Fact]
        public void LogFromStreamEnd()
        {
            this.CreateStream();
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                message.IsEmpty.Should().BeTrue();
            };

            this.reader.Read(this.stream, 0, onRead, () => true);
            count.Should().Be(1);
        }
    }
}