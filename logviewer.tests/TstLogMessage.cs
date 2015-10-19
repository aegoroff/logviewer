﻿// Created by: egr
// Created at: 02.01.2013
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using FluentAssertions;
using logviewer.engine;
using Xunit;

namespace logviewer.tests
{
    public class TstLogMessage
    {
        public TstLogMessage()
        {
            this.m = LogMessage.Create();
        }

        private LogMessage m;
        private const string H = "h";
        private const string B = "b";

        [Fact]
        public void Full()
        {
            this.m.AddLine(H);
            this.m.AddLine(B);
            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().Be(H);
            this.m.Body.Should().Be(B + "\n");
        }

        [Fact]
        public void FullCached()
        {
            this.m.AddLine(H);
            this.m.AddLine(B);
            this.m.Cache(null);

            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().Be(H);
            this.m.Body.Should().Be(B);
        }

        [Fact]
        public void IsEmpty()
        {
            this.m.IsEmpty.Should().BeTrue();
            this.m.Header.Should().BeEmpty();
            this.m.Body.Should().BeEmpty();
        }
        
        [Fact]
        public void IsEmptyAllStringsEmpty()
        {
            this.m.AddLine(string.Empty);
            this.m.AddLine(string.Empty);
            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().BeEmpty();
            this.m.Body.Should().BeEmpty();

        }

        [Fact]
        public void OnlyHead()
        {
            this.m.AddLine(H);
            this.m.IsEmpty.Should().BeFalse();
            this.m.Header.Should().Be(H);
            this.m.Body.Should().BeEmpty();
        }

        [Fact]
        public void OnlyHeadCached()
        {
            this.m.AddLine(H);
            this.m.Cache(null);
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
        public void ParseLogLevel(string input, LogLevel result)
        {
            this.ParseTest("level", ParserType.LogLevel, ParserType.LogLevel, input);
            ((LogLevel) this.m.IntegerProperty("level")).Should().Be(result);
        }

        [Theory]
        [InlineData("2014-10-23 20:00:51,790", 2014, 10, 23, 20, 0, 51, 790)]
        [InlineData("2014-10-23 20:00:51.790", 2014, 10, 23, 20, 0, 51, 790)]
        [InlineData("2014-10-23 20:00:51", 2014, 10, 23, 20, 0, 51, 0)]
        public void ParseDateTime(string input, int y, int month, int d, int h, int min, int sec, int millisecond)
        {
            this.ParseTest("dt", ParserType.Datetime, ParserType.Datetime, input);
            DateTime.FromFileTimeUtc(this.m.IntegerProperty("dt")).Should().Be(new DateTime(y, month, d, h, min, sec, millisecond, DateTimeKind.Utc));
        }
        
        [Theory]
        [InlineData("24/Oct/2014:09:34:30 +0400", 2014, 10, 24, 9, 34, 30, 0, -4)]
        [InlineData("24/Oct/2014:09:34:30 +0000", 2014, 10, 24, 9, 34, 30, 0, 0)]
        public void ParseDateTimeTimezone(string input, int y, int month, int d, int h, int min, int sec, int millisecond, int offset)
        {
            this.ParseTest("dt", ParserType.Datetime, ParserType.Datetime, input);
            DateTime.FromFileTimeUtc(this.m.IntegerProperty("dt")).Should().Be(new DateTime(y, month, d, h + offset, min, sec, millisecond, DateTimeKind.Utc));
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
        public void ParseInteger(string input, long result)
        {
            this.ParseTest("integer", ParserType.Interger, ParserType.Interger, input);
            this.m.IntegerProperty("integer").Should().Be(result);
        }

        [Theory]
        [InlineData("s", "s", ParserType.String)]
        public void ParseString(string input, string result, ParserType type)
        {
            this.ParseTest("str", type, ParserType.String, input);
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
        public void ParseLogLevelCustomRules(string input, LogLevel result)
        {
            this.ParseTest("Level", ParserType.LogLevel, input,
                new GrokRule(ParserType.LogLevel, "20", LogLevel.Info),
                new GrokRule(ParserType.LogLevel, "30", LogLevel.Warn),
                new GrokRule(ParserType.LogLevel, "40", LogLevel.Error),
                new GrokRule(ParserType.LogLevel, "41", LogLevel.Error),
                new GrokRule(ParserType.LogLevel, "50", LogLevel.Fatal),
                new GrokRule(ParserType.LogLevel, "*", LogLevel.Debug));
            ((LogLevel)this.m.IntegerProperty("Level")).Should().Be(result);
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