// Created by: egr
// Created at: 20.11.2014
// © 2012-2014 Alexander Egorov

using System;
using System.IO;
using System.Text;
using logviewer.engine;
using NMock;
using Xunit;

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

        public TstLogReader()
        {
            MockFactory mockery = new MockFactory();
            Mock<ICharsetDetector> detector = mockery.CreateMock<ICharsetDetector>();
            this.stream = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(MessageExamples);
            this.stream.Write(buffer, 0, buffer.Length);
            GrokMatcher grokMatcher = new GrokMatcher(NlogGrok);
            this.reader = new LogReader(detector.MockObject, grokMatcher);
            this.builder = new RulesBuilder(grokMatcher.MessageSchema);
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }

        [Fact]
        public void LogFromStream()
        {
            this.stream.Seek(0, SeekOrigin.Begin);
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                Assert.False(message.IsEmpty);
            };

            this.reader.Read(this.stream, 0, onRead, () => true);
            Assert.Equal(2, count);
        }

        [Fact]
        public void LogFromStreamWithCache()
        {
            this.stream.Seek(0, SeekOrigin.Begin);
            var count = 0;
            Action<LogMessage> onRead = delegate(LogMessage message)
            {
                count++;
                Assert.False(message.IsEmpty);
                message.Cache(this.builder.Rules);
                Assert.True(message.IntegerProperty("Level") > 0);
                Assert.True(message.IntegerProperty("Occured") > 0);
            };

            this.reader.Read(this.stream, 0, onRead, () => true);
            Assert.Equal(2, count);
        }

        [Fact]
        public void LogFromStreamCannotContinue()
        {
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