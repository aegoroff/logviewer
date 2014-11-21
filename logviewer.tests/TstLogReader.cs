// Created by: egr
// Created at: 20.11.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using logviewer.engine;
using NMock;
using Xunit;
using Xunit.Extensions;

namespace logviewer.tests
{
    public class TstLogReader : IDisposable
    {
        private const string MessageExamples =
            "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27 19:40:11,906 [5272] ERROR \nmessage body 2";

        private const string NlogGrok = @"^\[?%{TIMESTAMP_ISO8601:Occured,DateTime}\]?%{DATA}%{LOGLEVEL:Level,LogLevel}%{DATA}";


        private readonly LogReader reader;
        private readonly MemoryStream stream;
        private readonly RulesBuilder builder;
        private byte[] buffer;

        public TstLogReader()
        {
            var mockery = new MockFactory();
            var detector = mockery.CreateMock<ICharsetDetector>();
            this.stream = new MemoryStream();
            var grokMatcher = new GrokMatcher(NlogGrok);
            this.reader = new LogReader(detector.MockObject, grokMatcher);
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

        [Theory, PropertyData("ValidStreams")]
        public void LogFromStream(string data)
        {
            this.CreateStream(data);
            this.stream.Seek(0, SeekOrigin.Begin);
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                Assert.False(message.IsEmpty);
            };

            var position = this.reader.Read(this.stream, 0, onRead, () => true);
            Assert.Equal(2, count);
            Assert.Equal(this.buffer.LongLength, position);
        }

        public static IEnumerable<object[]> ValidStreams
        {
            get
            {
                return new[]
                {
                    new object[] { MessageExamples  },
                    new object[] { Environment.NewLine + MessageExamples },
                    new object[] { " " + Environment.NewLine + " " + Environment.NewLine + MessageExamples },
                    new object[] { MessageExamples + Environment.NewLine },
                    new object[] { Environment.NewLine + MessageExamples + Environment.NewLine }

                };
            }
        }

        [Fact]
        public void LogFromStreamWithCache()
        {
            this.CreateStream();
            this.stream.Seek(0, SeekOrigin.Begin);
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                Assert.False(message.IsEmpty);
                message.Cache(this.builder.Rules);
                var level = (LogLevel)message.IntegerProperty("Level");
                var date = DateTime.FromFileTime(message.IntegerProperty("Occured"));
                Assert.InRange(level, LogLevel.Info, LogLevel.Error);
                Assert.Equal(2008, date.Year);
                Assert.Equal(12, date.Month);
                Assert.Equal(27, date.Day);
            };

            

            this.reader.Read(this.stream, 0, onRead, () => true);
            Assert.Equal(2, count);
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
                Assert.True(message.IsEmpty);
            };

            this.reader.Read(this.stream, 0, onRead, () => false);
            Assert.Equal(1, count);
        }

        [Fact]
        public void LogFromStreamEnd()
        {
            this.CreateStream();
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                Assert.True(message.IsEmpty);
            };

            this.reader.Read(this.stream, 0, onRead, () => true);
            Assert.Equal(1, count);
        }
    }
}