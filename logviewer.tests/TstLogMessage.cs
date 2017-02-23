// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 02.01.2013
// © 2012-2017 Alexander Egorov

using System;
using System.Collections.Generic;
using FluentAssertions;
using logviewer.engine;
using Xunit;

namespace logviewer.tests
{
    public class TstLogMessage
    {
        private const string H = "h";
        private const string B = "b";

        private LogMessage m;

        public TstLogMessage()
        {
            this.m = LogMessage.Create();
        }

        [Fact]
        public void AddLine_HeadAndBody_ShouldBePresent()
        {
            // Act
            this.m.AddLine(H);
            this.m.AddLine(B);

            // Assert
            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().Be(H);
            this.m.Body.Should().Be(B + "\n");
        }

        [Fact]
        public void Cache_HeadAndBody_ShouldBePresent()
        {
            // Arrange
            this.m.AddLine(H);
            this.m.AddLine(B);

            // Act
            this.m.Cache(null);

            // Assert
            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().Be(H);
            this.m.Body.Should().Be(B);
        }

        [Fact]
        public void IsEmpty_NoAddLineBefore_EmptyMessage()
        {
            // Act
            var result = this.m.IsEmpty;

            // Assert
            result.Should().BeTrue();
            this.m.Header.Should().BeEmpty();
            this.m.Body.Should().BeEmpty();
        }

        [Fact]
        public void AddLine_HeadAndBodyEmpty_MessageShouldNotBeEmpty()
        {
            // Act
            this.m.AddLine(string.Empty);
            this.m.AddLine(string.Empty);

            // Assert
            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().BeEmpty();
            this.m.Body.Should().BeEmpty();
        }

        [Fact]
        public void AddLine_OnlyHead_MessageShouldNotBeEmpty()
        {
            // Act
            this.m.AddLine(H);

            // Assert
            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().Be(H);
            this.m.Body.Should().BeEmpty();
        }

        [Fact]
        public void Cache_OnlyHead_MessageShouldNotBeEmpty()
        {
            // Arrange
            this.m.AddLine(H);

            // Act
            this.m.Cache(null);

            // Assert
            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().Be(H);
            this.m.Body.Should().BeEmpty();
        }

        [Theory]
        [InlineData("Trace", LogLevel.Trace)]
        [InlineData("TRACE", LogLevel.Trace)]
        [InlineData("Debug", LogLevel.Debug)]
        [InlineData("DEBUG", LogLevel.Debug)]
        [InlineData("Info", LogLevel.Info)]
        [InlineData("Information", LogLevel.Trace)]
        [InlineData("Informational", LogLevel.Info)]
        [InlineData("INFO", LogLevel.Info)]
        [InlineData("Warn", LogLevel.Warn)]
        [InlineData("WARN", LogLevel.Warn)]
        [InlineData("Warning", LogLevel.Warn)]
        [InlineData("Error", LogLevel.Error)]
        [InlineData("ERROR", LogLevel.Error)]
        [InlineData("Fatal", LogLevel.Fatal)]
        [InlineData("FATAL", LogLevel.Fatal)]
        [InlineData("FaTaL", LogLevel.Fatal)]
        [InlineData("FaTa", LogLevel.Trace)]
        [InlineData("alert", LogLevel.Fatal)]
        [InlineData("notice", LogLevel.Info)]
        [InlineData("severe", LogLevel.Fatal)]
        [InlineData("critical", LogLevel.Error)]
        [InlineData("emerg", LogLevel.Fatal)]
        [InlineData("emergency", LogLevel.Fatal)]
        public void Parse_LogLevel_ShouldBeAsSpecified(string input, LogLevel result)
        {
            // Act
            this.ParseTest("level", ParserType.LogLevel, ParserType.LogLevel, input);

            // Assert
            ((LogLevel) this.m.IntegerProperty("level")).Should().Be(result);
        }

        [Theory]
        [InlineData("2014-10-23 20:00:51,790", 2014, 10, 23, 20, 0, 51, 790)]
        [InlineData("2014-10-23 20:00:51.790", 2014, 10, 23, 20, 0, 51, 790)]
        [InlineData("2014-10-23 20:00:51", 2014, 10, 23, 20, 0, 51, 0)]
        public void Parse_DateTime_ShouldBeAsSpecified(string input, int y, int month, int d, int h, int min, int sec, int millisecond)
        {
            // Act
            this.ParseTest("dt", ParserType.Datetime, ParserType.Datetime, input);

            // Assert
            DateTime.FromFileTimeUtc(this.m.IntegerProperty("dt"))
                .Should()
                .Be(new DateTime(y, month, d, h, min, sec, millisecond, DateTimeKind.Utc));
        }

        [Theory]
        [InlineData("24/Oct/2014:09:34:30 +0400", 2014, 10, 24, 9, 34, 30, 0, -4)]
        [InlineData("24/Oct/2014:09:34:30 +0000", 2014, 10, 24, 9, 34, 30, 0, 0)]
        public void Parse_DateTimeWithTimeZone_ShouldBeAsSpecified(string input, int y, int month, int d, int h, int min, int sec,
            int millisecond, int offset)
        {
            // Act
            this.ParseTest("dt", ParserType.Datetime, ParserType.Datetime, input);

            // Assert
            DateTime.FromFileTimeUtc(this.m.IntegerProperty("dt"))
                .Should()
                .Be(new DateTime(y, month, d, h + offset, min, sec, millisecond, DateTimeKind.Utc));
        }

        [Theory]
        [InlineData("-1", -1)]
        [InlineData("0", 0)]
        [InlineData("a", 0)]
        [InlineData("1", 1)]
        [InlineData("9223372036854775807", long.MaxValue)]
        [InlineData("9223372036854775807", long.MaxValue)]
        [InlineData("9223372036854775807", long.MaxValue)]
        [InlineData("9223372036854775807", long.MaxValue)]
        [InlineData("-9223372036854775808", long.MinValue)]
        [InlineData("-9223372036854775810", 0)]
        [InlineData("9223372036854775810", 0)]
        public void Parse_Integer_ShouldBeAsSpecified(string input, long result)
        {
            // Act
            this.ParseTest("integer", ParserType.Interger, ParserType.Interger, input);

            // Assert
            this.m.IntegerProperty("integer").Should().Be(result);
        }

        [Theory]
        [InlineData("s", "s", ParserType.String)]
        public void Parse_String_ShouldBeAsSpecified(string input, string result, ParserType type)
        {
            // Act
            this.ParseTest("str", type, ParserType.String, input);

            // Assert
            this.m.StringProperty("str").Should().Be(result);
        }

        [Theory]
        [InlineData("a", LogLevel.Debug)]
        [InlineData("100", LogLevel.Debug)]
        [InlineData("200", LogLevel.Info)]
        [InlineData("301", LogLevel.Warn)]
        [InlineData("404", LogLevel.Error)]
        [InlineData("410", LogLevel.Error)]
        [InlineData("416", LogLevel.Error)]
        [InlineData("500", LogLevel.Fatal)]
        public void Parse_LogLevelCustomRules_ShouldBeAsSpecified(string input, LogLevel result)
        {
            // Act
            this.ParseTest("Level", ParserType.LogLevel, input,
                new GrokRule(ParserType.LogLevel, "20", LogLevel.Info),
                new GrokRule(ParserType.LogLevel, "30", LogLevel.Warn),
                new GrokRule(ParserType.LogLevel, "40", LogLevel.Error),
                new GrokRule(ParserType.LogLevel, "41", LogLevel.Error),
                new GrokRule(ParserType.LogLevel, "50", LogLevel.Fatal),
                new GrokRule(ParserType.LogLevel, "*", LogLevel.Debug));

            // Assert
            ((LogLevel) this.m.IntegerProperty("Level")).Should().Be(result);
        }

        private void ParseTest(string prop, ParserType type, ParserType parser, string input)
        {
            this.ParseTest(prop, parser, input, new GrokRule(type));
        }

        private void ParseTest(string prop, ParserType parser, string input, params GrokRule[] rules)
        {
            this.m.AddLine(H);
            IDictionary<SemanticProperty, ISet<GrokRule>> schema = new Dictionary<SemanticProperty, ISet<GrokRule>>
            {
                {
                    new SemanticProperty(prop, parser), new HashSet<GrokRule>(rules)
                }
            };
            IDictionary<string, string> props = new Dictionary<string, string>
            {
                {
                    prop, input
                }
            };
            this.m.AddProperties(props);
            this.m.Cache(schema);
        }
    }
}